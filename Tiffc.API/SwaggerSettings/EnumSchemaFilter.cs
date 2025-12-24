using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Tiffc.API.SwaggerSettings;

/// <summary>
/// 在 Swagger 顯示 Enum 的描述與值
/// </summary>
public class EnumSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (!context.Type.IsEnum) return;
        
        var enumDescriptions = Enum.GetValues(context.Type)
            .Cast<Enum>()
            .ToDictionary(e => e.ToString(), e => e.GetHashCode());
        var possibleValues = string.Join(", ", enumDescriptions.Select(kv => $"{kv.Key} = {kv.Value}"));
            
        // 保留現有的 XML 註解描述，並補充自定義資訊
        if (!string.IsNullOrEmpty(schema.Description))
        {
            schema.Description += Environment.NewLine + Environment.NewLine + "Possible values: " + possibleValues;
        }
        else
        {
            schema.Description = "Possible values: " + possibleValues;
        }
    }
}