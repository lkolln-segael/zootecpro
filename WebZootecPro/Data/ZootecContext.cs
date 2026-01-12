using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace WebZootecPro.Data;

public partial class ZootecContext : DbContext
{
    public ZootecContext()
    {
    }

    public ZootecContext(DbContextOptions<ZootecContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Aborto> Abortos { get; set; }

    public virtual DbSet<Alimentacion> Alimentacions { get; set; }

    public virtual DbSet<Animal> Animals { get; set; }

    public virtual DbSet<Calidad> Calidads { get; set; }

    public virtual DbSet<CalidadDiariaHato> CalidadDiariaHatos { get; set; }

    public virtual DbSet<CampaniaLechera> CampaniaLecheras { get; set; }

    public virtual DbSet<CategoriaAnimal> CategoriaAnimals { get; set; }

    public virtual DbSet<CausaAborto> CausaAbortos { get; set; }

    public virtual DbSet<CentroCosto> CentroCostos { get; set; }

    public virtual DbSet<Colaborador> Colaboradors { get; set; }

    public virtual DbSet<ConfirmacionPrenez> ConfirmacionPrenezs { get; set; }

    public virtual DbSet<DesarrolloCrecimiento> DesarrolloCrecimientos { get; set; }

    public virtual DbSet<Empresa> Empresas { get; set; }

    public virtual DbSet<Enfermedad> Enfermedads { get; set; }

    public virtual DbSet<Especialidad> Especialidads { get; set; }

    public virtual DbSet<Establo> Establos { get; set; }

    public virtual DbSet<EstadoAnimal> EstadoAnimals { get; set; }

    public virtual DbSet<EstadoCrium> EstadoCria { get; set; }

    public virtual DbSet<EstadoProductivo> EstadoProductivos { get; set; }

    public virtual DbSet<EventoGeneral> EventoGenerals { get; set; }

    public virtual DbSet<Hato> Hatos { get; set; }

    public virtual DbSet<IngredienteNutriente> IngredienteNutrientes { get; set; }

    public virtual DbSet<LecturaMedidorLeche> LecturaMedidorLeches { get; set; }

    public virtual DbSet<MovimientoCosto> MovimientoCostos { get; set; }

    public virtual DbSet<Nutriente> Nutrientes { get; set; }

    public virtual DbSet<Parto> Partos { get; set; }

    public virtual DbSet<PlanLicencium> PlanLicencia { get; set; }

    public virtual DbSet<Prenez> Prenezs { get; set; }

    public virtual DbSet<ProcedenciaAnimal> ProcedenciaAnimals { get; set; }

    public virtual DbSet<PropositoAnimal> PropositoAnimals { get; set; }

    public virtual DbSet<Raza> Razas { get; set; }

    public virtual DbSet<RegistroIngreso> RegistroIngresos { get; set; }

    public virtual DbSet<RegistroNacimiento> RegistroNacimientos { get; set; }

    public virtual DbSet<RegistroProduccionLeche> RegistroProduccionLeches { get; set; }

    public virtual DbSet<RegistroReproduccion> RegistroReproduccions { get; set; }

    public virtual DbSet<RegistroSalidum> RegistroSalida { get; set; }

    public virtual DbSet<ReporteIndustriaLeche> ReporteIndustriaLeches { get; set; }

    public virtual DbSet<RequerimientoNutricional> RequerimientoNutricionals { get; set; }

    public virtual DbSet<Rol> Rols { get; set; }

    public virtual DbSet<RtmEntrega> RtmEntregas { get; set; }

    public virtual DbSet<RtmFormula> RtmFormulas { get; set; }

    public virtual DbSet<RtmFormulaDetalle> RtmFormulaDetalles { get; set; }

    public virtual DbSet<RtmIngrediente> RtmIngredientes { get; set; }

    public virtual DbSet<RtmRacionCorral> RtmRacionCorrals { get; set; }

    public virtual DbSet<Seca> Secas { get; set; }

    public virtual DbSet<SexoCrium> SexoCria { get; set; }

    public virtual DbSet<Sintoma> Sintomas { get; set; }

    public virtual DbSet<TipoAlimento> TipoAlimentos { get; set; }

    public virtual DbSet<TipoCosto> TipoCostos { get; set; }

    public virtual DbSet<TipoEnfermedade> TipoEnfermedades { get; set; }

    public virtual DbSet<TipoParto> TipoPartos { get; set; }

    public virtual DbSet<TipoTratamiento> TipoTratamientos { get; set; }

    public virtual DbSet<Tratamiento> Tratamientos { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    public virtual DbSet<vw_TratamientosEnfermerium> vw_TratamientosEnfermeria { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Aborto>(entity =>
        {
            entity.HasOne(d => d.idCausaAbortoNavigation).WithMany(p => p.Abortos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Aborto_CausaAborto");

            entity.HasOne(d => d.idHatoNavigation).WithMany(p => p.Abortos).HasConstraintName("FK_Aborto_Hato");

            entity.HasOne(d => d.idRegistroReproduccionNavigation).WithMany(p => p.Abortos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Aborto_RegistroReproduccion");
        });

        modelBuilder.Entity<Alimentacion>(entity =>
        {
            entity.HasOne(d => d.idAnimalNavigation).WithMany(p => p.Alimentacions)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Alimentacion_Animal");

            entity.HasOne(d => d.idTipoAlimentoNavigation).WithMany(p => p.Alimentacions)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Alimentacion_TipoAlimento");
        });

        modelBuilder.Entity<Animal>(entity =>
        {
            entity.HasIndex(e => e.naab, "UX_Animal_NAAB")
                .IsUnique()
                .HasFilter("([naab] IS NOT NULL)");

            entity.HasIndex(e => e.arete, "UX_Animal_arete")
                .IsUnique()
                .HasFilter("([arete] IS NOT NULL)");

            entity.HasOne(d => d.IdCategoriaAnimalNavigation).WithMany(p => p.Animals).HasConstraintName("FK_Animal_CategoriaAnimal");

            entity.HasOne(d => d.estado).WithMany(p => p.Animals).HasConstraintName("FK_Animal_EstadoAnimal");

            entity.HasOne(d => d.estadoProductivo).WithMany(p => p.Animals).HasConstraintName("FK_Animal_EstadoProductivo");

            entity.HasOne(d => d.idHatoNavigation).WithMany(p => p.Animals)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Animal_Hato");

            entity.HasOne(d => d.idMadreNavigation).WithMany(p => p.InverseidMadreNavigation).HasConstraintName("FK_Animal_Madre");

            entity.HasOne(d => d.idPadreNavigation).WithMany(p => p.InverseidPadreNavigation).HasConstraintName("FK_Animal_Padre");

            entity.HasOne(d => d.idRazaNavigation).WithMany(p => p.Animals).HasConstraintName("FK_Animal_Raza");

            entity.HasOne(d => d.idUltimoCrecimientoNavigation).WithMany(p => p.Animals)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_Animal_UltimoCrecimiento");

            entity.HasOne(d => d.procedencia).WithMany(p => p.Animals).HasConstraintName("FK_Animal_ProcedenciaAnimal");

            entity.HasOne(d => d.proposito).WithMany(p => p.Animals).HasConstraintName("FK_Animal_PropositoAnimal");
        });

        modelBuilder.Entity<Calidad>(entity =>
        {
            entity.HasOne(d => d.idRegistroProduccionLecheNavigation).WithMany(p => p.Calidads)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Calidad_RegistroProduccionLeche");
        });

        modelBuilder.Entity<CalidadDiariaHato>(entity =>
        {
            entity.Property(e => e.fechaRegistro).HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.idHatoNavigation).WithMany(p => p.CalidadDiariaHatos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CalidadDiariaHato_Hato");
        });

        modelBuilder.Entity<CampaniaLechera>(entity =>
        {
            entity.Property(e => e.activa).HasDefaultValue(true);

            entity.HasOne(d => d.Establo).WithMany(p => p.CampaniaLecheras)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CampaniaLechera_Establo");
        });

        modelBuilder.Entity<CentroCosto>(entity =>
        {
            entity.Property(e => e.Activo).HasDefaultValue(true);
        });

        modelBuilder.Entity<Colaborador>(entity =>
        {
            entity.Property(e => e.DNI).IsFixedLength();

            entity.HasOne(d => d.Empresa).WithMany(p => p.Colaboradors).HasConstraintName("FK_Colaborador_Empresa");

            entity.HasOne(d => d.Especialidad).WithMany(p => p.Colaboradors)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Colaborador_Especialidad");

            entity.HasOne(d => d.idUsuarioNavigation).WithMany(p => p.Colaboradors)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Colaborador_Usuario");
        });

        modelBuilder.Entity<ConfirmacionPrenez>(entity =>
        {
            entity.HasOne(d => d.idHatoNavigation).WithMany(p => p.ConfirmacionPrenezs).HasConstraintName("FK_ConfirmacionPrenez_Hato");

            entity.HasOne(d => d.idRegistroReproduccionNavigation).WithMany(p => p.ConfirmacionPrenezs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ConfirmacionPrenez_RegistroReproduccion");
        });

        modelBuilder.Entity<DesarrolloCrecimiento>(entity =>
        {
            entity.HasOne(d => d.idAnimalNavigation).WithMany(p => p.DesarrolloCrecimientos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DesarrolloCrecimiento_Animal");
        });

        modelBuilder.Entity<Empresa>(entity =>
        {
            entity.Property(e => e.NombreEmpresa).HasDefaultValue("");
            entity.Property(e => e.ruc).IsFixedLength();

            entity.HasOne(d => d.Plan).WithMany(p => p.Empresas).HasConstraintName("FK_Empresa_PlanLicencia");

            entity.HasOne(d => d.usuario).WithMany(p => p.Empresas)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Empresa_Usuario");
        });

        modelBuilder.Entity<Enfermedad>(entity =>
        {
            entity.HasOne(d => d.idAnimalNavigation).WithMany(p => p.Enfermedads)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Enfermedad_Animal");

            entity.HasOne(d => d.idTipoEnfermedadNavigation).WithMany(p => p.Enfermedads)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Enfermedad_TipoEnfermedades");

            entity.HasOne(d => d.idVeterinarioNavigation).WithMany(p => p.Enfermedads)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Enfermedad_UsuarioVeterinario");
        });

        modelBuilder.Entity<Establo>(entity =>
        {
            entity.Property(e => e.pveDias).HasDefaultValue(60);

            entity.HasOne(d => d.Empresa).WithMany(p => p.Establos).HasConstraintName("FK_Establo_Empresa");
        });

        modelBuilder.Entity<EstadoAnimal>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__EstadoAn__3214EC07850FD416");
        });

        modelBuilder.Entity<EstadoCrium>(entity =>
        {
            entity.Property(e => e.Activo).HasDefaultValue(true);
        });

        modelBuilder.Entity<EstadoProductivo>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__EstadoPr__3214EC07CD73119E");
        });

        modelBuilder.Entity<EventoGeneral>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__EventoGe__3214EC074FD345D9");

            entity.HasOne(d => d.idAnimalNavigation).WithMany(p => p.EventoGenerals)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_EventoGeneral_Animal");
        });

        modelBuilder.Entity<Hato>(entity =>
        {
            entity.HasOne(d => d.Establo).WithMany(p => p.Hatos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Hato_Establo");
        });

        modelBuilder.Entity<MovimientoCosto>(entity =>
        {
            entity.Property(e => e.FechaRegistro).HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.IdCentroCostoNavigation).WithMany(p => p.MovimientoCostos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MovimientoCosto_CentroCosto");

            entity.HasOne(d => d.IdTipoCostoNavigation).WithMany(p => p.MovimientoCostos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MovimientoCosto_TipoCosto");
        });

        modelBuilder.Entity<Parto>(entity =>
        {
            entity.HasOne(d => d.idEstadoCriaNavigation).WithMany(p => p.Partos).HasConstraintName("FK_Parto_EstadoCria");

            entity.HasOne(d => d.idHatoNavigation).WithMany(p => p.Partos).HasConstraintName("FK_Parto_Hato");

            entity.HasOne(d => d.idRegistroReproduccionNavigation).WithMany(p => p.Partos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Parto_RegistroReproduccion");

            entity.HasOne(d => d.idSexoCriaNavigation).WithMany(p => p.Partos).HasConstraintName("FK_Parto_SexoCria");

            entity.HasOne(d => d.idTipoPartoNavigation).WithMany(p => p.Partos).HasConstraintName("FK_Parto_TipoParto");
        });

        modelBuilder.Entity<PlanLicencium>(entity =>
        {
            entity.Property(e => e.Activo).HasDefaultValue(true);
            entity.Property(e => e.FechaRegistro).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Moneda)
                .HasDefaultValue("PEN")
                .IsFixedLength();
        });

        modelBuilder.Entity<Prenez>(entity =>
        {
            entity.HasOne(d => d.idHatoNavigation).WithMany(p => p.Prenezs).HasConstraintName("FK_Prenez_Hato");

            entity.HasOne(d => d.idMadreAnimalNavigation).WithMany(p => p.PrenezidMadreAnimalNavigations).HasConstraintName("FK_Prenez_MadreAnimal");

            entity.HasOne(d => d.idPadreAnimalNavigation).WithMany(p => p.PrenezidPadreAnimalNavigations).HasConstraintName("FK_Prenez_PadreAnimal");

            entity.HasOne(d => d.idRegistroReproduccionNavigation).WithMany(p => p.Prenezs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Prenez_RegistroReproduccion");
        });

        modelBuilder.Entity<ProcedenciaAnimal>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Proceden__3214EC07F451371E");
        });

        modelBuilder.Entity<PropositoAnimal>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Proposit__3214EC0768A318A1");
        });

        modelBuilder.Entity<Raza>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Raza__3214EC07EF123631");
        });

        modelBuilder.Entity<RegistroIngreso>(entity =>
        {
            entity.Property(e => e.fechaIngreso).HasDefaultValueSql("(CONVERT([date],getdate()))");

            entity.HasOne(d => d.idAnimalNavigation).WithMany(p => p.RegistroIngresos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RegistroIngreso_Animal");
        });

        modelBuilder.Entity<RegistroNacimiento>(entity =>
        {
            entity.HasOne(d => d.idAnimalNavigation).WithMany(p => p.RegistroNacimientos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RegistroNacimiento_Animal");

            entity.HasOne(d => d.idRegistroReproduccionNavigation).WithMany(p => p.RegistroNacimientos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RegistroNacimiento_RegistroReproduccion");
        });

        modelBuilder.Entity<RegistroProduccionLeche>(entity =>
        {
            entity.Property(e => e.turno).HasDefaultValue("MAÑANA");

            entity.HasOne(d => d.idAnimalNavigation).WithMany(p => p.RegistroProduccionLeches)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RegistroProduccionLeche_Animal");
        });

        modelBuilder.Entity<RegistroReproduccion>(entity =>
        {
            entity.HasOne(d => d.idAnimalNavigation).WithMany(p => p.RegistroReproduccions)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RegistroReproduccion_Animal");

            entity.HasOne(d => d.idHatoNavigation).WithMany(p => p.RegistroReproduccions).HasConstraintName("FK_RegistroReproduccion_Hato");
        });

        modelBuilder.Entity<RegistroSalidum>(entity =>
        {
            entity.Property(e => e.fechaSalida).HasDefaultValueSql("(CONVERT([date],getdate()))");

            entity.HasOne(d => d.idAnimalNavigation).WithMany(p => p.RegistroSalida)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RegistroSalida_Animal");
        });

        modelBuilder.Entity<ReporteIndustriaLeche>(entity =>
        {
            entity.Property(e => e.fechaRegistro).HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.idHatoNavigation).WithMany(p => p.ReporteIndustriaLeches)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ReporteIndustriaLeche_Hato");
        });

        modelBuilder.Entity<RequerimientoNutricional>(entity =>
        {
            entity.HasOne(d => d.IdCategoriaAnimalNavigation).WithMany(p => p.RequerimientoNutricionals)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RequerimientoNutricional_Categoria");

            entity.HasOne(d => d.IdNutrienteNavigation).WithMany(p => p.RequerimientoNutricionals)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RequerimientoNutricional_Nutriente");
        });

        modelBuilder.Entity<RtmEntrega>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__RtmEntre__3214EC0724675ECE");

            entity.HasOne(d => d.formula).WithMany(p => p.RtmEntregas)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RtmEntrega_Formula");

            entity.HasOne(d => d.hato).WithMany(p => p.RtmEntregas)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RtmEntrega_Hato");

            entity.HasOne(d => d.idUsuarioNavigation).WithMany(p => p.RtmEntregas).HasConstraintName("FK_RtmEntrega_Usuario");
        });

        modelBuilder.Entity<RtmFormula>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__RtmFormu__3214EC07BC6A4F5C");

            entity.Property(e => e.activo).HasDefaultValue(true);
            entity.Property(e => e.fechaCreacion).HasDefaultValueSql("(sysdatetime())");
        });

        modelBuilder.Entity<RtmFormulaDetalle>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__RtmFormu__3214EC07B9767AE0");

            entity.HasOne(d => d.formula).WithMany(p => p.RtmFormulaDetalles)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RtmFormulaDetalle_Formula");

            entity.HasOne(d => d.ingrediente).WithMany(p => p.RtmFormulaDetalles)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RtmFormulaDetalle_Ingrediente");
        });

        modelBuilder.Entity<RtmIngrediente>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__RtmIngre__3214EC0795CD5A77");

            entity.Property(e => e.activo).HasDefaultValue(true);
        });

        modelBuilder.Entity<RtmRacionCorral>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__RtmRacio__3214EC07375CD8E4");

            entity.Property(e => e.activo).HasDefaultValue(true);

            entity.HasOne(d => d.formula).WithMany(p => p.RtmRacionCorrals)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RtmRacionCorral_Formula");

            entity.HasOne(d => d.hato).WithMany(p => p.RtmRacionCorrals)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RtmRacionCorral_Hato");
        });

        modelBuilder.Entity<Seca>(entity =>
        {
            entity.HasOne(d => d.idHatoNavigation).WithMany(p => p.Secas).HasConstraintName("FK_Seca_Hato");

            entity.HasOne(d => d.idRegistroReproduccionNavigation).WithMany(p => p.Secas)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Seca_RegistroReproduccion");
        });

        modelBuilder.Entity<SexoCrium>(entity =>
        {
            entity.Property(e => e.Activo).HasDefaultValue(true);
        });

        modelBuilder.Entity<Sintoma>(entity =>
        {
            entity.HasOne(d => d.idTipoEnfermedadNavigation).WithMany(p => p.Sintomas)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Sintomas_TipoEnfermedades");
        });

        modelBuilder.Entity<TipoAlimento>(entity =>
        {
            entity.HasOne(d => d.idAnimalNavigation).WithMany(p => p.TipoAlimentos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TipoAlimento_Animal");
        });

        modelBuilder.Entity<TipoParto>(entity =>
        {
            entity.Property(e => e.Activo).HasDefaultValue(true);
        });

        modelBuilder.Entity<TipoTratamiento>(entity =>
        {
            entity.HasOne(d => d.idTipoEnfermedadNavigation).WithMany(p => p.TipoTratamientos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TipoTratamiento_TipoEnfermedades");
        });

        modelBuilder.Entity<Tratamiento>(entity =>
        {
            entity.HasOne(d => d.idEnfermedadNavigation).WithMany(p => p.Tratamientos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Tratamiento_Enfermedad");

            entity.HasOne(d => d.idTipoTratamientoNavigation).WithMany(p => p.Tratamientos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Tratamiento_TipoTratamiento");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasOne(d => d.Rol).WithMany(p => p.Usuarios)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Usuario_Rol");

            entity.HasOne(d => d.idEstabloNavigation).WithMany(p => p.Usuarios)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_Usuario_Establo");

            entity.HasOne(d => d.idHatoNavigation).WithMany(p => p.Usuarios)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_Usuario_Hato");
        });

        modelBuilder.Entity<vw_TratamientosEnfermerium>(entity =>
        {
            entity.ToView("vw_TratamientosEnfermeria");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
