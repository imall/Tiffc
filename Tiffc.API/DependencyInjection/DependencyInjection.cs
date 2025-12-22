using Supabase;
using Tiffc.API.Configuration;

namespace Tiffc.API.DependencyInjection;

/// <summary>
/// 依賴注入擴展方法
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// 標記 Hangfire 是否已成功初始化
    /// </summary>
    private static bool IsHangfireEnabled { get; set; } = false;

    /// <summary>
    /// 新增 supabase 服務
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static IServiceCollection AddSupabase(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddSingleton<Client>(_ =>
        {
            var supabaseSettings = new SupabaseSettings();
            configuration.GetSection("Supabase").Bind(supabaseSettings);

            if (string.IsNullOrEmpty(supabaseSettings.Url))
                throw new InvalidOperationException("Supabase URL 未設定");

            if (string.IsNullOrEmpty(supabaseSettings.AnonKey))
                throw new InvalidOperationException("Supabase AnonKey 未設定");

            var options = new SupabaseOptions
            {
                AutoConnectRealtime = false
            };

            var apiKey = supabaseSettings.GetApiKey();
            var client = new Client(supabaseSettings.Url, apiKey, options);
            client.InitializeAsync().Wait();
            return client;
        });

        return services;
    }
    
}
