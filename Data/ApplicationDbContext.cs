using Microsoft.EntityFrameworkCore;
using SIGRE_PYME.Models;

namespace SIGRE_PYME.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Usuario> Usuarios => Set<Usuario>();
        public DbSet<Producto> Productos => Set<Producto>();
        public DbSet<Cliente> Clientes => Set<Cliente>();
        public DbSet<MovimientoInventario> MovimientosInventario => Set<MovimientoInventario>();
        public DbSet<ContactoMensaje> ContactoMensajes => Set<ContactoMensaje>();
        public DbSet<Pedido> Pedidos => Set<Pedido>();
        public DbSet<PedidoDetalle> PedidoDetalles => Set<PedidoDetalle>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.HasIndex(x => x.NombreUsuario).IsUnique();

                entity.Property(x => x.NombreUsuario)
                    .HasColumnName("Username")
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(x => x.Contrasena)
                    .HasColumnName("Password")
                    .HasMaxLength(255)
                    .IsRequired();

                entity.Property(x => x.RolId)
                    .HasColumnName("RolId")
                    .IsRequired();

                entity.Property(x => x.IntentosFallidos)
                    .HasDefaultValue(0);

                entity.Property(x => x.Bloqueado)
                    .HasDefaultValue(false);
            });

            modelBuilder.Entity<Producto>(entity =>
            {
                entity.HasKey(x => x.ProductoId);

                entity.HasIndex(x => x.SKU).IsUnique();

                entity.Property(x => x.SKU)
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(x => x.Nombre)
                    .HasMaxLength(120)
                    .IsRequired();

                entity.Property(x => x.Categoria)
                    .HasMaxLength(60)
                    .HasDefaultValue("General")
                    .IsRequired();

                entity.Property(x => x.Precio)
                    .HasPrecision(18, 2);

                entity.Property(x => x.ImagenUrl)
                    .HasMaxLength(255)
                    .HasDefaultValue(string.Empty);
            });

            modelBuilder.Entity<MovimientoInventario>()
                .HasOne(m => m.Producto)
                .WithMany()
                .HasForeignKey(m => m.ProductoId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MovimientoInventario>()
                .HasOne(m => m.Usuario)
                .WithMany()
                .HasForeignKey(m => m.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Pedido>()
                .HasOne(p => p.Usuario)
                .WithMany()
                .HasForeignKey(p => p.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PedidoDetalle>()
                .HasOne(d => d.Pedido)
                .WithMany(p => p.PedidoDetalles)
                .HasForeignKey(d => d.PedidoId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PedidoDetalle>()
                .HasOne(d => d.Producto)
                .WithMany()
                .HasForeignKey(d => d.ProductoId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}