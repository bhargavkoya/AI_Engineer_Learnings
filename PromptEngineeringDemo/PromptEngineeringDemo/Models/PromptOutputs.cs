using System.Text.Json.Serialization;

namespace PromptEngineeringDemo.Models
{
    public record SummaryOutput(
    [property: JsonPropertyName("bullet_points")] List<string> BulletPoints
);

    public record ClassificationOutput(
        [property: JsonPropertyName("label")] string Label,        // positive | negative | neutral
        [property: JsonPropertyName("confidence")] string Confidence // high | medium | low
    );

    public record DocumentExtraction(
        [property: JsonPropertyName("customer_name")] string? CustomerName,
        [property: JsonPropertyName("order_total")] decimal? OrderTotal,
        [property: JsonPropertyName("delivery_date")] string? DeliveryDate,
        [property: JsonPropertyName("items")] List<LineItem> Items
    );

    public record LineItem(
        [property: JsonPropertyName("description")] string Description,
        [property: JsonPropertyName("quantity")] int Quantity,
        [property: JsonPropertyName("unit_price")] decimal UnitPrice
    );
}
