using System.ComponentModel.DataAnnotations;

namespace ProductCatalogApi.DTOs
{
    public class CategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class CreateCategoryDto
    {
        [Required]
        [MaxLength(50)]
        [MinLength(5, ErrorMessage = "Name must be at least 5 characters long.")]
        public string Name { get; set; } = string.Empty;

        [MaxLength(250)]
        public string Description { get; set; } = string.Empty;
    }

    public class UpdateCategoryDto
    {
        [Required]
        [MaxLength(50)]
        [MinLength(5, ErrorMessage = "Name must be at least 5 characters long.")]
        public string Name { get; set; } = string.Empty;

        [MaxLength(250)]
        public string Description { get; set; } = string.Empty;
    }
}