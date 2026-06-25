namespace ProductMcpServer.Models
{
    public record Product(
        string Sku,
        string Name,
        string Category,
        decimal Price,
        int StockLevel);
}
