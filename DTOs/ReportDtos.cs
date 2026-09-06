namespace ProductCatalogApi.DTOs
{
    public class InventoryValueDto
    {
        public decimal TotalValue { get; set; }
    }

    public class MostExpensiveProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
    }

    public class OutOfStockProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Stock { get; set; }
    }
}