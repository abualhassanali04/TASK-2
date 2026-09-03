using System.ComponentModel.DataAnnotations;

namespace ProductCatalogApi.DTOs
{
    public class CategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }

    public class CreateCategoryDto
    {
        [Required]
        [MaxLength(50)]
        [MinLength(5, ErrorMessage = "Name must be at least 5 characters long.")]
        public string Name { get; set; }

        [MaxLength(250)]
        public string Description { get; set; }
    }

    public class UpdateCategoryDto
    {
        [Required]
        [MaxLength(50)]
        [MinLength(5, ErrorMessage = "Name must be at least 5 characters long.")]
        public string Name { get; set; }

        [MaxLength(250)]
        public string Description { get; set; }
    }
}