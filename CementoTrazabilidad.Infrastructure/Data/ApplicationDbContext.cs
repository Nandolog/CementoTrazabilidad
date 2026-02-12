using CementoTrazabilidad.Core.Entidades;
using Microsoft.EntityFrameworkCore;

namespace CementoTrazabilidad.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Personal> Personal { get; set; }
        public DbSet<TurnoProduccion> TurnosProduccion { get; set; }
        public DbSet<ProduccionMaterial> ProduccionMaterial { get; set; }
        public DbSet<Material> Materiales { get; set; }
        public DbSet<Parada> Paradas { get; set; }
        public DbSet<Despacho> Despachos { get; set; }
        public DbSet<ProveedorBolsa> ProveedoresBolsa { get; set; }
        public DbSet<ConsumoBolsas> ConsumoBolsas { get; set; }
        public DbSet<PersonalTurno> PersonalTurno { get; set; }
        public DbSet<EventoCarga> EventosCarga { get; set; }
        public DbSet<LoteProveedorBolsa> LotesProveedorBolsa { get; set; }
        public DbSet<RegistroDefectoBolsa> RegistrosDefectosBolsas { get; set; }
        public DbSet<LoteProduccion> LotesProduccion { get; set; } // Sin 's' al final
        public DbSet<RegistroStockPalets> RegistrosStockPalets { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Configurar nombres de tabla
            modelBuilder.Entity<Usuario>().ToTable("Usuario");
            modelBuilder.Entity<Personal>().ToTable("Personal");
            modelBuilder.Entity<TurnoProduccion>().ToTable("TurnoProduccion");
            modelBuilder.Entity<Material>().ToTable("Material");
            modelBuilder.Entity<ProveedorBolsa>().ToTable("ProveedoresBolsa");
            modelBuilder.Entity<ConsumoBolsas>().ToTable("ConsumoBolsas");
            modelBuilder.Entity<Despacho>().ToTable("Despacho");
            modelBuilder.Entity<Parada>().ToTable("Parada");
            modelBuilder.Entity<PersonalTurno>().ToTable("PersonalTurno");
            modelBuilder.Entity<ProduccionMaterial>().ToTable("ProduccionMaterial");
            modelBuilder.Entity<LoteProduccion>().ToTable("LotesProduccion");
            
            // ✅ NUEVAS TABLAS - AGREGAR ESTAS LÍNEAS
            modelBuilder.Entity<LoteProveedorBolsa>().ToTable("LoteProveedorBolsa");
            modelBuilder.Entity<EventoCarga>().ToTable("EventosCarga"); // Plural en BD
            modelBuilder.Entity<RegistroDefectoBolsa>().ToTable("RegistroDefectoBolsa");

            // ✅ CONFIGURACIÓN CRÍTICA: Mapear entidad Parada a columnas reales de BD
            modelBuilder.Entity<Parada>(entity =>
            {
                entity.ToTable("Parada");
                entity.HasKey(e => e.ParadaID);
                
                // Mapear propiedades a columnas con nombres diferentes
                entity.Property(e => e.ParadaID).HasColumnName("ParadaID");
                entity.Property(e => e.TurnoProduccionID).HasColumnName("TurnoProduccionID");
                entity.Property(e => e.TipoParada).HasColumnName("Tipo");
                entity.Property(e => e.Descripcion).HasColumnName("Decripcion");
                entity.Property(e => e.FechaHoraInicio).HasColumnName("FechaHoraInicio");
                entity.Property(e => e.FechaHoraFin).HasColumnName("FechaHoraFin");
                
                // Ignorar propiedades que NO existen en la base de datos
                entity.Ignore(e => e.Motivo);
                entity.Ignore(e => e.Estado);
                entity.Ignore(e => e.PersonalResponsableID);
                entity.Ignore(e => e.Duracion);
                entity.Ignore(e => e.PersonalResponsable);
                
                // Relación con Turno
                entity.HasOne(e => e.Turno)
                    .WithMany(t => t.Paradas)
                    .HasForeignKey(e => e.TurnoProduccionID)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ✅ NUEVA CONFIGURACIÓN: Mapear entidad Material a columnas reales de BD
            modelBuilder.Entity<Material>(entity =>
            {
                entity.ToTable("Material");
                entity.HasKey(e => e.MaterialID);
                
                // Mapear propiedades a columnas con nombres diferentes
                entity.Property(e => e.MaterialID).HasColumnName("MaterialID");
                entity.Property(e => e.Codigo).HasColumnName("Codigo");
                entity.Property(e => e.Descripcion).HasColumnName("descripcion"); // ✅ minúscula en BD
                entity.Property(e => e.PesoBolsa).HasColumnName("PesoPorBolsa").HasPrecision(18, 2); // ✅ nombre diferente
                entity.Property(e => e.DensidadKGm3).HasColumnName("DensildadKGm3").HasPrecision(18, 2); // ← AGREGAR
                entity.Property(e => e.Activo).HasColumnName("Activo");
                
                // ✅ Ignorar propiedades que NO existen en la BD
                entity.Ignore(e => e.Nombre); // No existe en BD, solo "descripcion"
                entity.Ignore(e => e.UnidadMedida); // No existe en BD
                entity.Ignore(e => e.FechaCreacion); // No existe en BD
            });

            // Configuraciones de entidades
            modelBuilder.Entity<TurnoProduccion>()
                .HasIndex(t => new { t.Fecha, t.TurnoNumero })
                .IsUnique();

            modelBuilder.Entity<Usuario>()
                .HasIndex(u => u.Legajo)
                .IsUnique();

            modelBuilder.Entity<Personal>()
                .HasIndex(p => p.Legajo)
                .IsUnique();

            modelBuilder.Entity<Despacho>(entity =>
            {
                entity.ToTable("Despacho");
                entity.HasKey(e => e.DespachoID);
                
                // Mapear explícitamente todas las columnas
                entity.Property(e => e.DespachoID).HasColumnName("DespachoID");
                entity.Property(e => e.TurnoProduccionID).HasColumnName("TurnoProduccionID");
                entity.Property(e => e.MaterialID).HasColumnName("MaterialID");
                
                // ✅ IMPORTANTE: Modalidad tiene CHECK constraint en BD: IN ('Granel', 'Pallet', 'Anden')
                entity.Property(e => e.Modalidad)
                    .HasMaxLength(20)
                    .HasColumnType("varchar(20)")
                    .HasColumnName("Modalidad");
                
                entity.Property(e => e.Destino)
                    .HasMaxLength(50)
                    .HasColumnType("varchar(50)")
                    .HasColumnName("Destino");
                
                entity.Property(e => e.Bolsas).HasColumnName("Bolsas");
                
                entity.Property(e => e.Toneladas)
                    .HasColumnType("decimal(18, 2)")
                    .HasColumnName("Toneladas");
                
                entity.Property(e => e.Camiones).HasColumnName("Camiones");
                
                entity.Property(e => e.FechaHoraDespacho)
                    .HasColumnType("datetime2")
                    .HasColumnName("FechaHoraDespacho");
                
                // ✅ Relación con Turno
                entity.HasOne(e => e.Turno)
                    .WithMany()
                    .HasForeignKey(e => e.TurnoProduccionID)
                    .OnDelete(DeleteBehavior.SetNull)
                    .IsRequired(false);
            });

            // Especificar precisión para propiedades decimales
            modelBuilder.Entity<ProduccionMaterial>()
                .Property(p => p.HorasMarcha)
                .HasPrecision(18, 2);

            modelBuilder.Entity<EventoCarga>(entity =>
            {
                entity.HasKey(e => e.EventoCargaID);
                
                entity.HasOne(e => e.Turno)
                    .WithMany()
                    .HasForeignKey(e => e.TurnoProduccionID)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configurar ConsumoBolsas
            modelBuilder.Entity<ConsumoBolsas>(entity =>
            {
                entity.HasOne(e => e.ProveedorBolsa)
                    .WithMany(p => p.Consumos)
                    .HasForeignKey(e => e.ProveedorBolsaID)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.TurnoProduccion)
                    .WithMany(t => t.ConsumosBolsas)
                    .HasForeignKey(e => e.TurnoProduccionID)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.ProduccionMaterial)
                    .WithMany(p => p.ConsumoBolsas)
                    .HasForeignKey(e => e.ProduccionMaterialID)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configurar LoteProveedorBolsa
            modelBuilder.Entity<LoteProveedorBolsa>(entity =>
            {
                entity.HasIndex(e => new { e.ProveedorBolsaID, e.NumeroLote }).IsUnique();

                entity.HasOne(e => e.ProveedorBolsa)
                    .WithMany(p => p.Lotes)
                    .HasForeignKey(e => e.ProveedorBolsaID)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configurar RegistroDefectoBolsa
            modelBuilder.Entity<RegistroDefectoBolsa>(entity =>
            {
                entity.ToTable("RegistroDefectoBolsa");
                
                // ✅ PK: RegistroDefectoBolsaID en C# → RegistroDefectoID en BD
                entity.HasKey(e => e.RegistroDefectoBolsaID);
                entity.Property(e => e.RegistroDefectoBolsaID).HasColumnName("RegistroDefectoID");
                
                // ✅ Foreign Keys
                entity.Property(e => e.TurnoProduccionID).HasColumnName("TurnoProduccionID");
                entity.Property(e => e.ProduccionMaterialID).HasColumnName("ProduccionMaterialID");
                entity.Property(e => e.LoteProveedorBolsaID).HasColumnName("LoteProveedorBolsaID");
                
                // ✅ Cantidad en C# → CantidadDefectuosa en BD
                entity.Property(e => e.Cantidad).HasColumnName("CantidadDefectuosa");
                
                // ✅ TipoDefecto existe en ambos con el mismo nombre
                entity.Property(e => e.TipoDefecto).HasColumnName("TipoDefecto").HasMaxLength(100);
                
                // ✅ FechaHoraRegistro en C# → FechaRegistro en BD
                entity.Property(e => e.FechaHoraRegistro).HasColumnName("FechaRegistro");
                
                // ✅ Ignorar propiedades que NO existen en la BD
                entity.Ignore(e => e.Descripcion);         // No existe en BD (solo en tabla tenemos Observaciones)
                entity.Ignore(e => e.RegistradoPor);       // No existe en BD
                entity.Ignore(e => e.Gravedad);            // No existe en BD
                entity.Ignore(e => e.ProveedorNotificado); // No existe en BD
                entity.Ignore(e => e.FechaNotificacion);   // No existe en BD
                
                // ✅ Relaciones
                entity.HasOne(e => e.LoteProveedorBolsa)
                    .WithMany(l => l.Defectos)
                    .HasForeignKey(e => e.LoteProveedorBolsaID)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.TurnoProduccion)
                    .WithMany(t => t.DefectosBolsas)
                    .HasForeignKey(e => e.TurnoProduccionID)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.ProduccionMaterial)
                    .WithMany(p => p.DefectosBolsas)
                    .HasForeignKey(e => e.ProduccionMaterialID)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configurar LoteProduccion
            modelBuilder.Entity<LoteProduccion>(entity =>
            {
                entity.HasKey(e => e.LoteID);
                entity.Property(e => e.NumeroLote).HasMaxLength(50).IsRequired();
                entity.Property(e => e.TipoRegistro).HasMaxLength(20).IsRequired();
                
                // ✅ CORRECCIÓN CRÍTICA: Especificar que TurnoID apunta a TurnoProduccionID
                entity.HasOne(e => e.Turno)
                      .WithMany()
                      .HasForeignKey(e => e.TurnoID)
                      .HasPrincipalKey(t => t.TurnoProduccionID) // ✅ LÍNEA CRÍTICA
                      .OnDelete(DeleteBehavior.Restrict);
                
                // ✅ Material - sin HasPrincipalKey porque MaterialID ya es la PK convencional
                entity.HasOne(e => e.Material)
                      .WithMany()
                      .HasForeignKey(e => e.MaterialID)
                      .OnDelete(DeleteBehavior.Restrict);
                
                entity.HasIndex(e => e.FechaHoraInicio);
                entity.HasIndex(e => e.FechaHoraFin);
                entity.HasIndex(e => e.NumeroLote).IsUnique();
            });

            // Configurar RegistroStockPalets
            modelBuilder.Entity<RegistroStockPalets>(entity =>
            {
                entity.HasKey(e => e.RegistroStockPaletsID);
                
                entity.HasOne(e => e.TurnoProduccion)
                    .WithMany()
                    .HasForeignKey(e => e.TurnoProduccionID)
                    .OnDelete(DeleteBehavior.Restrict);
                
                entity.HasOne(e => e.PersonalRegistro)
                    .WithMany()
                    .HasForeignKey(e => e.PersonalID)
                    .OnDelete(DeleteBehavior.Restrict);
                
                entity.Property(e => e.ObservacionesInicio)
                    .HasMaxLength(500);
                
                entity.Property(e => e.ObservacionesFin)
                    .HasMaxLength(500);
                
                // Índices para búsquedas rápidas
                entity.HasIndex(e => new { e.TurnoProduccionID, e.Activo });
            });

            // ✅ Datos semilla para proveedores
            modelBuilder.Entity<ProveedorBolsa>().HasData(
                new ProveedorBolsa { ProveedorBolsaID = 1, Nombre = "Coselapa", Descripcion = "Cooperativa de cemento", Activo = true },
                new ProveedorBolsa { ProveedorBolsaID = 2, Nombre = "Primo Tedesco", Descripcion = "Fabricante de bolsas", Activo = true },
                new ProveedorBolsa { ProveedorBolsaID = 3, Nombre = "Bolpar", Descripcion = "Bolsas para cemento", Activo = true }
            );
        }
    }
}
