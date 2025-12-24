using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Tiffc.Repository.Entities;

[Table("exchange_rates")]
public class ExchangeRate : BaseModel
{
    [Column("source")]
    [PrimaryKey("source", false)]
    public string Source { get; set; } = string.Empty;

    [Column("currency")]
    [PrimaryKey("currency", false)]
    public string Currency { get; set; } = string.Empty;

    [Column("rate")]
    public decimal Rate { get; set; }

    [Column("crawled_at")]
    public DateTime CrawledAt { get; set; } = DateTime.Now;
}
