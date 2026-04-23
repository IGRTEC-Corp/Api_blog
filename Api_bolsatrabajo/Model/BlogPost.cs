using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api_blog.Model
{
    [Table("BlogPosts")]
    public class BlogPost
    {
        [Key]
        public int BlogPostId { get; set; }

        // =========================
        // Contenido
        // =========================

        [Required]
        [MaxLength(200)]
        public string Title { get; set; }

        [Required]
        [MaxLength(200)]
        public string Slug { get; set; }

        [MaxLength(500)]
        public string? Summary { get; set; }

        [Required]
        public string Content { get; set; }

        // =========================
        // SEO
        // =========================

        [MaxLength(200)]
        public string? MetaTitle { get; set; }

        [MaxLength(300)]
        public string? MetaDescription { get; set; }

        [MaxLength(300)]
        public string? MetaKeywords { get; set; }

        // =========================
        // Imagen destacada
        // =========================

        [MaxLength(300)]
        public string? FeaturedImageUrl { get; set; }

        // =========================
        // Estado de publicación
        // =========================

        public bool IsPublished { get; set; } = false;

        public DateTime? PublishedAt { get; set; }

        // =========================
        // Autor
        // =========================

        [Required]
        [MaxLength(150)]
        public string AuthorName { get; set; }

        // =========================
        // Métricas
        // =========================

        public int Views { get; set; } = 0;

        // =========================
        // Auditoría
        // =========================

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }
    }
}
