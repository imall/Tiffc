using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Tiffc.Repository.Entities;

[Table("exchange_rates")]
public class ExchangeRate : BaseModel
{
    [PrimaryKey("id")]
    public Guid Id { get; set; }

    [Column("source")]
    public string Source { get; set; } = string.Empty;

    [Column("currency")]
    public string Currency { get; set; } = string.Empty;

    [Column("rate")]
    public decimal Rate { get; set; }

    [Column("crawled_at")]
    public DateTime CrawledAt { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }
}
