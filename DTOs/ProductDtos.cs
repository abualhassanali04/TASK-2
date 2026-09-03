using System.ComponentModel.DataAnnotations;

namespace ProductCatalogApi.DTOs
{
    public class ProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public DateTime CreatedAt { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
    }

    public class CreateProductDto
    {
        [Required]
        [MaxLength(100)]
        [MinLength(5, ErrorMessage = "Name must be at least 5 characters long.")]
        public string Name { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0.")]
        public decimal Price { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Stock cannot be negative.")]
        public int Stock { get; set; }

        [Required]
        public int CategoryId { get; set; }
    }

    public class UpdateProductDto
    {
        [Required]
        [MaxLength(100)]
        [MinLength(5, ErrorMessage = "Name must be at least 5 characters long.")]
        public string Name { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0.")]
        public decimal Price { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Stock cannot be negative.")]
        public int Stock { get; set; }

        [Required]
        public int CategoryId { get; set; }
    }
}