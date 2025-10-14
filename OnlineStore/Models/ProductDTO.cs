using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace OnlineStore.Models
{
    public class ProductDTO
    {
        [Required, MaxLength(20)]
        public string Name { get; set; } = "";
        [Required, MaxLength(20)]
        public string Brand { get; set; } = "";
        [Required, MaxLength(20)]
        public string Category { get; set; } = "";
        [Required]
        public decimal Price { get; set; }
        [Required]
        public string Description { get; set; } = "";
        public IFormFile? ImageFile { get; set; }
    }
}
 