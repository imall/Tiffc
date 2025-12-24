
namespace Tiffc.API.SwaggerSettings;

public static class SwaggerSetting
{
    /// <summary>
    /// Swagger 相關註冊內容
    /// </summary>
    public static void AddSwaggerSettings(this IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
            {
                Title = "TIFFC API",
                Version = "v1",
                Description = "TIFFC 後端 API 文件",
            });
            
            var xmlFiles = Directory.EnumerateFiles(AppContext.BaseDirectory, "*.xml", SearchOption.TopDirectoryOnly);

            foreach (var xmlFile in xmlFiles)
            {
                c.IncludeXmlComments(xmlFile, true);
            }

            c.SchemaFilter<EnumSchemaFilter>();
        });
    }
    
    /// <summary>
    /// Swagger 中介軟體設定
    /// </summary>
    public static void UseSwaggerSettings(this WebApplication app)
    {
        app.UseSwagger();
        app.UseSwaggerUI(setupAction =>
        {
            // UI 讀取 OpenAPI 規格路徑
            setupAction.SwaggerEndpoint($"/swagger/v1/swagger.json", "TIFFC API V1");
            
            // 顯示請求時間
            setupAction.DisplayRequestDuration();

            // 啟用深度連結 (錨點連結)
            setupAction.EnableDeepLinking();

            // 啟用篩選框
            setupAction.EnableFilter();
        });
    }
}
