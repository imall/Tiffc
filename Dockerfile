# ========== 階段 1: 建置 Vue 前端 ==========
FROM node:20-alpine AS frontend-build

WORKDIR /src/frontend

# 安裝 pnpm
RUN npm install -g pnpm

# 複製 Vue 專案的 package.json
COPY ["Tiffc.Vue/package.json", "Tiffc.Vue/pnpm-lock.yaml*", "./"]

# 安裝依賴
RUN pnpm install --frozen-lockfile

# 複製 Vue 原始碼
COPY ["Tiffc.Vue/", "./"]

# 建置前端（輸出到臨時目錄）
RUN pnpm build

# 使用 .NET 8.0 SDK 作為建置階段
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# 設定工作目錄
WORKDIR /src

# 複製 global.json 和解決方案檔案
COPY ["global.json", "."]
COPY ["Tiffc.sln", "."]

# 複製專案檔案
COPY ["Tiffc.API/Tiffc.API.csproj", "Tiffc.API/"]
COPY ["Tiffc.Common/Tiffc.Common.csproj", "Tiffc.Common/"]
COPY ["Tiffc.Repository/Tiffc.Repository.csproj", "Tiffc.Repository/"]
COPY ["Tiffc.Service/Tiffc.Service.csproj", "Tiffc.Service/"]

# 還原專案依賴
RUN dotnet restore "Tiffc.API/Tiffc.API.csproj"

# 複製所有原始碼
COPY . .

# 從前端建置階段複製打包好的檔案到 wwwroot
COPY --from=frontend-build /src/frontend/dist /src/Tiffc.API/wwwroot

# 建置並發布專案
WORKDIR "/src/Tiffc.API"
RUN dotnet publish "Tiffc.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# 使用 ASP.NET Core Runtime 作為執行時期階段
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final

# 設定工作目錄
WORKDIR /app

# 暴露連接埠 (預設 ASP.NET Core 連接埠)
EXPOSE 8080
EXPOSE 8081

# 複製建置結果
COPY --from=build /app/publish .

# 設定進入點
ENTRYPOINT ["dotnet", "Tiffc.API.dll"]
