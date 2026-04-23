namespace Api_bolsatrabajo.Data
{
    using Api_blog.Model;
    using Api_bolsatrabajo.Model;
    using BolsaDeTrabajo.Api.Models;
    using Microsoft.EntityFrameworkCore;

    public class BolsatrabajoContext : DbContext
    {
        public BolsatrabajoContext() { }

        public BolsatrabajoContext(DbContextOptions<BolsatrabajoContext> options)
            : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("admin");

            // Collation general
            modelBuilder.HasAnnotation("Relational:Collation", "Modern-Spanish_CI_AS");


            modelBuilder.Entity<BlogPost>()
         .HasIndex(b => b.Slug)
         .IsUnique();

            modelBuilder.Entity<BlogPost>()
                .HasIndex(b => new { b.IsPublished, b.PublishedAt });
            // ================================================
            // CONFIGURACIÓN DE EMPRESA VACANTES POSTULACIONES
            // ================================================
            // =======================
            modelBuilder.Entity<EmpresaVacantesPostulaciones>(entity =>
            {
                entity.HasKey(e => e.IdPostulacion);

                // RELACIÓN Postulación -> Vacante
                entity.HasOne(e => e.IdVacanteNavigation)
                    .WithMany(v => v.EmpresaVacantesPostulaciones)
                    .HasForeignKey(e => e.IdVacante)               // <<--- FORZAMOS EL FK
                    .OnDelete(DeleteBehavior.Cascade);

                // RELACIÓN Postulación -> Usuario
                entity.HasOne(e => e.IdUsuarioNavigation)
                    .WithMany(u => u.EmpresaVacantesPostulaciones)
                    .HasForeignKey(e => e.IdUsuario)               // <<--- FORZAMOS EL FK
                    .OnDelete(DeleteBehavior.Cascade);
            });

            base.OnModelCreating(modelBuilder);
        }

        // =======================
        //   TABLAS CANDIDATOS
        // =======================
        public DbSet<BlogPost> BlogPosts { get; set; }

    }
}
