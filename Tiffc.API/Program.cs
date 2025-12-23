using Tiffc.API.DependencyInjection;
using Tiffc.Repository;
using Tiffc.Service;
using Tiffc.Service.Crawlers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddHttpClient();
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSupabase(builder.Configuration);

// CORS: 允許本地開發前端 (允許任何 localhost/loopback origin，例如含任意埠)
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "AllowLocalhosts",
        policy =>
        {
            policy.AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
        });
});

builder.Services.AddScoped<ProductService>();
builder.Services.AddScoped<ProductRepository>();
// 註冊所有爬蟲
builder.Services.AddScoped<IExchangeRateCrawler, LetaoCrawler>();

// 註冊爬蟲服務
builder.Services.AddScoped<ExchangeRateCrawlerService>();

// 註冊 Repository
builder.Services.AddScoped<ExchangeRateRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// 提供靜態文件服務
app.UseDefaultFiles();
app.UseStaticFiles();

// 啟用 CORS（需在 Authorization 之前）
app.UseCors("AllowLocalhosts");

app.UseAuthorization();

app.MapControllers();

// SPA fallback - 所有非 API 路由都返回 index.html
app.MapFallbackToFile("index.html");

app.Run();
