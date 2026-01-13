USE [master]
GO
/****** Object:  Database [ZootecPro]    Script Date: 12/01/2026 11:22:44 ******/
CREATE DATABASE [ZootecPro]
GO
USE [ZootecPro]
GO
CREATE TABLE [dbo].[Enfermedad](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[fechaDiagnostico] [datetime2](0) NOT NULL,
	[fechaRecuperacion] [datetime2](0) NULL,
	[idVeterinario] [int] NOT NULL,
	[idTipoEnfermedad] [int] NOT NULL,
	[idAnimal] [int] NOT NULL,
 CONSTRAINT [PK_Enfermedad] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Tratamiento]    Script Date: 12/01/2026 11:22:44 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Tratamiento](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[fechaInicio] [datetime2](0) NOT NULL,
	[fechaFinalEstimada] [datetime2](0) NULL,
	[costoEstimado] [decimal](12, 2) NULL,
	[observaciones] [nvarchar](600) NULL,
	[idTipoTratamiento] [int] NOT NULL,
	[idEnfermedad] [int] NOT NULL,
 CONSTRAINT [PK_Tratamiento] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  View [dbo].[vw_TratamientosEnfermeria]    Script Date: 12/01/2026 11:22:44 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE VIEW [dbo].[vw_TratamientosEnfermeria]
AS
SELECT
    t.Id                   AS IdTratamiento,
    e.idAnimal             AS IdAnimal,
    t.idEnfermedad,
    t.idTipoTratamiento,

    t.fechaInicio,
    t.fechaFinalEstimada,

    DiasEnEnfermeria = DATEDIFF(
        DAY,
        t.fechaInicio,
        ISNULL(t.fechaFinalEstimada, CAST(SYSDATETIME() AS DATE))
    ),

    EstaEnEnfermeria = CASE 
                           WHEN t.fechaFinalEstimada IS NULL THEN 1 
                           ELSE 0 
                       END
FROM dbo.Tratamiento t
INNER JOIN dbo.Enfermedad e 
    ON t.idEnfermedad = e.Id;
GO
/****** Object:  Table [dbo].[Aborto]    Script Date: 12/01/2026 11:22:44 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Aborto](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[fechaRegistro] [datetime2](0) NOT NULL,
	[idRegistroReproduccion] [int] NOT NULL,
	[idCausaAborto] [int] NOT NULL,
	[diasATermino] [int] NULL,
 CONSTRAINT [PK_Aborto] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Alimentacion]    Script Date: 12/01/2026 11:22:44 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Alimentacion](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[estado] [nvarchar](50) NULL,
	[idAnimal] [int] NOT NULL,
	[idTipoAlimento] [int] NOT NULL,
	[cantidad] [decimal](12, 2) NULL,
	[fecha] [date] NOT NULL,
 CONSTRAINT [PK_Alimentacion] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Animal]    Script Date: 12/01/2026 11:22:44 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Animal](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[nombre] [nvarchar](150) NOT NULL,
	[sexo] [nvarchar](20) NULL,
	[codigo] [nvarchar](60) NULL,
	[IdentificadorElectronico] [nvarchar](80) NULL,
	[OtroIdentificador] [nvarchar](80) NULL,
	[color] [nvarchar](50) NULL,
	[fechaNacimiento] [date] NULL,
	[observaciones] [nvarchar](600) NULL,
	[idHato] [int] NOT NULL,
	[idPadre] [int] NULL,
	[idMadre] [int] NULL,
	[idUltimoCrecimiento] [int] NULL,
	[estadoId] [int] NULL,
	[propositoId] [int] NULL,
	[idRaza] [int] NULL,
	[procedenciaId] [int] NULL,
	[nacimientoEstimado] [bit] NOT NULL,
	[estadoProductivoId] [int] NULL,
	[IdCategoriaAnimal] [int] NULL,
	[arete] [nvarchar](30) NULL,
	[naab] [nvarchar](20) NULL,
 CONSTRAINT [PK_Animal] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Calidad]    Script Date: 12/01/2026 11:22:44 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Calidad](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[grasa] [decimal](6, 2) NULL,
	[proteina] [decimal](6, 2) NULL,
	[solidosTotales] [decimal](6, 2) NULL,
	[urea] [decimal](8, 2) NULL,
	[idRegistroProduccionLeche] [int] NOT NULL,
	[fechaRegistro] [datetime2](0) NOT NULL,
	[rcs] [int] NULL,
 CONSTRAINT [PK_Calidad] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[CalidadDiariaHato]    Script Date: 12/01/2026 11:22:44 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CalidadDiariaHato](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[fecha] [date] NOT NULL,
	[idHato] [int] NOT NULL,
	[fuente] [varchar](30) NOT NULL,
	[grasa] [decimal](5, 2) NULL,
	[proteina] [decimal](5, 2) NULL,
	[solidosTotales] [decimal](5, 2) NULL,
	[urea] [decimal](6, 2) NULL,
	[rcs] [int] NULL,
	[observaciones] [nvarchar](400) NULL,
	[fechaRegistro] [datetime2](0) NOT NULL,
 CONSTRAINT [PK_CalidadDiariaHato] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[CampaniaLechera]    Script Date: 12/01/2026 11:22:44 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CampaniaLechera](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[EstabloId] [int] NOT NULL,
	[nombre] [nvarchar](150) NOT NULL,
	[fechaInicio] [date] NOT NULL,
	[fechaFin] [date] NOT NULL,
	[activa] [bit] NOT NULL,
	[observaciones] [nvarchar](400) NULL,
 CONSTRAINT [PK_CampaniaLechera] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[CategoriaAnimal]    Script Date: 12/01/2026 11:22:44 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CategoriaAnimal](
	[IdCategoriaAnimal] [int] IDENTITY(1,1) NOT NULL,
	[Nombre] [nvarchar](100) NOT NULL,
 CONSTRAINT [PK_CategoriaAnimal] PRIMARY KEY CLUSTERED 
(
	[IdCategoriaAnimal] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[CausaAborto]    Script Date: 12/01/2026 11:22:44 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CausaAborto](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Nombre] [nvarchar](80) NOT NULL,
	[Oculto] [bit] NOT NULL,
 CONSTRAINT [PK_CausaAborto] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[CentroCosto]    Script Date: 12/01/2026 11:22:44 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CentroCosto](
	[IdCentroCosto] [int] IDENTITY(1,1) NOT NULL,
	[Nombre] [nvarchar](100) NOT NULL,
	[Descripcion] [nvarchar](250) NULL,
	[Activo] [bit] NOT NULL,
 CONSTRAINT [PK_CentroCosto] PRIMARY KEY CLUSTERED 
(
	[IdCentroCosto] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Colaborador]    Script Date: 12/01/2026 11:22:44 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Colaborador](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[nombre] [nvarchar](120) NOT NULL,
	[DNI] [char](8) NOT NULL,
	[EspecialidadId] [int] NOT NULL,
	[idUsuario] [int] NOT NULL,
	[EmpresaId] [int] NULL,
	[Apellido] [nvarchar](120) NULL,
	[Direccion] [nvarchar](200) NULL,
	[Provincia] [nvarchar](100) NULL,
	[Localidad] [nvarchar](100) NULL,
	[CodigoPostal] [nvarchar](10) NULL,
	[Telefono] [nvarchar](30) NULL,
 CONSTRAINT [PK_Colaborador] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ConfirmacionPrenez]    Script Date: 12/01/2026 11:22:44 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ConfirmacionPrenez](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[tipo] [nvarchar](20) NOT NULL,
	[fechaRegistro] [datetime2](0) NOT NULL,
	[idRegistroReproduccion] [int] NOT NULL,
	[observacion] [nvarchar](600) NULL,
	[metodo] [nvarchar](20) NULL,
 CONSTRAINT [PK_ConfirmacionPrenez] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[DesarrolloCrecimiento]    Script Date: 12/01/2026 11:22:44 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DesarrolloCrecimiento](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[estado] [nvarchar](50) NULL,
	[fechaRegistro] [datetime2](0) NOT NULL,
	[pesoActual] [decimal](10, 2) NULL,
	[tamano] [decimal](10, 2) NULL,
	[condicionCorporal] [nvarchar](50) NULL,
	[unidadesAnimal] [nvarchar](50) NULL,
	[idAnimal] [int] NOT NULL,
 CONSTRAINT [PK_DesarrolloCrecimiento] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Empresa]    Script Date: 12/01/2026 11:22:44 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Empresa](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[usuarioID] [int] NOT NULL,
	[ruc] [char](11) NOT NULL,
	[logo] [nvarchar](260) NULL,
	[capacidadMaxima] [int] NULL,
	[areaTotal] [decimal](12, 2) NULL,
	[areaPasto] [decimal](12, 2) NULL,
	[areaBosque] [decimal](12, 2) NULL,
	[areaCultivos] [decimal](12, 2) NULL,
	[areaConstruida] [decimal](12, 2) NULL,
	[ubicacion] [nvarchar](200) NULL,
	[NombreEmpresa] [nvarchar](150) NOT NULL,
	[PlanId] [int] NULL,
 CONSTRAINT [PK_Empresa] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Especialidad]    Script Date: 12/01/2026 11:22:44 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Especialidad](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Nombre] [nvarchar](100) NOT NULL,
 CONSTRAINT [PK_Especialidad] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Establo]    Script Date: 12/01/2026 11:22:44 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Establo](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[nombre] [nvarchar](150) NOT NULL,
	[logo] [nvarchar](260) NULL,
	[capacidadMaxima] [int] NULL,
	[areaTotal] [decimal](12, 2) NULL,
	[areaPasto] [decimal](12, 2) NULL,
	[areaBosque] [decimal](12, 2) NULL,
	[areaCultivos] [decimal](12, 2) NULL,
	[areaConstruida] [decimal](12, 2) NULL,
	[ubicacion] [nvarchar](200) NULL,
	[EmpresaId] [int] NOT NULL,
	[pveDias] [int] NULL,
 CONSTRAINT [PK_Establo] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[EstadoAnimal]    Script Date: 12/01/2026 11:22:44 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[EstadoAnimal](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[nombre] [nvarchar](50) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[EstadoCria]    Script Date: 12/01/2026 11:22:44 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[EstadoCria](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Nombre] [nvarchar](40) NOT NULL,
	[Activo] [bit] NOT NULL,
	[Orden] [int] NOT NULL,
 CONSTRAINT [PK_EstadoCria] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[EstadoProductivo]    Script Date: 12/01/2026 11:22:44 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[EstadoProductivo](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[nombre] [nvarchar](50) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[EventoGeneral]    Script Date: 12/01/2026 11:22:44 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[EventoGeneral](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[idAnimal] [int] NOT NULL,
	[fechaEvento] [datetime2](0) NOT NULL,
	[tipoEvento] [varchar](50) NOT NULL,
	[tipoAnalisis] [varchar](150) NULL,
	[resultado] [varchar](150) NULL,
	[descripcion] [nvarchar](600) NULL,
	[idHato] [int] NULL,
	[usuarioId] [int] NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Hato]    Script Date: 12/01/2026 11:22:44 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Hato](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[nombre] [nvarchar](150) NOT NULL,
	[sistemaProduccion] [nvarchar](150) NULL,
	[ubicacion] [nvarchar](200) NULL,
	[EstabloId] [int] NOT NULL,
 CONSTRAINT [PK_Hato] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[IngredienteNutriente]    Script Date: 12/01/2026 11:22:44 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[IngredienteNutriente](
	[IdIngredienteNutriente] [int] IDENTITY(1,1) NOT NULL,
	[IdRtmIngrediente] [int] NOT NULL,
	[IdNutriente] [int] NOT NULL,
	[ValorPorMS] [decimal](10, 4) NOT NULL,
 CONSTRAINT [PK_IngredienteNutriente] PRIMARY KEY CLUSTERED 
(
	[IdIngredienteNutriente] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[LecturaMedidorLeche]    Script Date: 12/01/2026 11:22:44 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[LecturaMedidorLeche](
	[IdLecturaMedidorLeche] [int] IDENTITY(1,1) NOT NULL,
	[CodigoMedidor] [nvarchar](50) NOT NULL,
	[CodigoAnimal] [nvarchar](50) NULL,
	[IdAnimal] [int] NULL,
	[FechaHoraLectura] [datetime2](7) NOT NULL,
	[PesoLecheKg] [decimal](10, 3) NOT NULL,
	[NumeroOrdeno] [tinyint] NOT NULL,
	[Procesado] [bit] NOT NULL,
	[IdRegistroProduccionLeche] [int] NULL,
	[Observacion] [nvarchar](200) NULL,
 CONSTRAINT [PK_LecturaMedidorLeche] PRIMARY KEY CLUSTERED 
(
	[IdLecturaMedidorLeche] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[MovimientoCosto]    Script Date: 12/01/2026 11:22:44 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MovimientoCosto](
	[IdMovimientoCosto] [int] IDENTITY(1,1) NOT NULL,
	[Fecha] [date] NOT NULL,
	[IdCentroCosto] [int] NOT NULL,
	[IdTipoCosto] [int] NOT NULL,
	[MontoTotal] [decimal](18, 2) NOT NULL,
	[Descripcion] [nvarchar](250) NULL,
	[IdEstablo] [int] NULL,
	[IdCorral] [int] NULL,
	[IdAnimal] [int] NULL,
	[IdRegistroProduccionLeche] [int] NULL,
	[FechaRegistro] [datetime2](7) NOT NULL,
 CONSTRAINT [PK_MovimientoCosto] PRIMARY KEY CLUSTERED 
(
	[IdMovimientoCosto] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Nutriente]    Script Date: 12/01/2026 11:22:44 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Nutriente](
	[IdNutriente] [int] IDENTITY(1,1) NOT NULL,
	[Nombre] [nvarchar](100) NOT NULL,
	[Unidad] [nvarchar](20) NOT NULL,
 CONSTRAINT [PK_Nutriente] PRIMARY KEY CLUSTERED 
(
	[IdNutriente] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Parto]    Script Date: 12/01/2026 11:22:44 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Parto](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[tipo] [nvarchar](80) NOT NULL,
	[fechaRegistro] [datetime2](0) NOT NULL,
	[idRegistroReproduccion] [int] NOT NULL,
	[idSexoCria] [int] NULL,
	[idTipoParto] [int] NULL,
	[idEstadoCria] [int] NULL,
	[nombreCria1] [nvarchar](150) NULL,
	[nombreCria2] [nvarchar](150) NULL,
	[horaParto] [time](0) NULL,
	[pveDias] [int] NULL,
	[fechaFinPve] [date] NULL,
	[areteCria1] [nvarchar](50) NULL,
	[areteCria2] [nvarchar](50) NULL,
 CONSTRAINT [PK_Parto] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PlanLicencia]    Script Date: 12/01/2026 11:22:44 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PlanLicencia](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Codigo] [varchar](30) NOT NULL,
	[Nombre] [varchar](120) NOT NULL,
	[Precio] [decimal](12, 2) NOT NULL,
	[Moneda] [char](3) NOT NULL,
	[EsIndefinido] [bit] NOT NULL,
	[MaxAnimales] [int] NULL,
	[MaxEstablos] [int] NULL,
	[Activo] [bit] NOT NULL,
	[FechaRegistro] [datetime2](0) NOT NULL,
 CONSTRAINT [PK_PlanLicencia] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Prenez]    Script Date: 12/01/2026 11:22:44 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Prenez](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[fechaCelo] [date] NULL,
	[fechaInseminacion] [date] NULL,
	[fechaDiagnostico] [date] NULL,
	[idPadreAnimal] [int] NULL,
	[idMadreAnimal] [int] NULL,
	[idRegistroReproduccion] [int] NOT NULL,
	[observacion] [nvarchar](600) NULL,
	[horaServicio] [time](0) NULL,
	[numeroServicio] [int] NULL,
	[nombreToro] [nvarchar](150) NULL,
	[codigoNaab] [nvarchar](20) NULL,
	[protocolo] [nvarchar](50) NULL,
	[fechaProbableParto] [date] NULL,
	[fechaProbableSeca] [date] NULL,
 CONSTRAINT [PK_Prenez] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ProcedenciaAnimal]    Script Date: 12/01/2026 11:22:44 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ProcedenciaAnimal](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[nombre] [nvarchar](80) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PropositoAnimal]    Script Date: 12/01/2026 11:22:44 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PropositoAnimal](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[nombre] [nvarchar](80) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Raza]    Script Date: 12/01/2026 11:22:44 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Raza](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[nombre] [nvarchar](80) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RegistroIngreso]    Script Date: 12/01/2026 11:22:44 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RegistroIngreso](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[codigoIngreso] [nvarchar](60) NOT NULL,
	[tipoIngreso] [nvarchar](80) NOT NULL,
	[idAnimal] [int] NOT NULL,
	[fechaIngreso] [date] NOT NULL,
	[idHato] [int] NULL,
	[origen] [nvarchar](200) NULL,
	[usuarioId] [int] NULL,
	[observacion] [nvarchar](600) NULL,
 CONSTRAINT [PK_RegistroIngreso] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RegistroNacimiento]    Script Date: 12/01/2026 11:22:44 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RegistroNacimiento](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[observacionesNacimiento] [nvarchar](600) NULL,
	[pesoNacimiento] [decimal](10, 2) NULL,
	[altitud] [decimal](10, 2) NULL,
	[ubicacion] [nvarchar](200) NULL,
	[fecha] [date] NOT NULL,
	[temperatura] [decimal](5, 2) NULL,
	[idAnimal] [int] NOT NULL,
	[idRegistroReproduccion] [int] NOT NULL,
 CONSTRAINT [PK_RegistroNacimiento] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RegistroProduccionLeche]    Script Date: 12/01/2026 11:22:44 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RegistroProduccionLeche](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[pesoOrdeno] [decimal](10, 2) NULL,
	[fechaPreparacion] [datetime2](0) NULL,
	[fechaLimpieza] [datetime2](0) NULL,
	[fechaDespunte] [datetime2](0) NULL,
	[fechaColocacionPezoneras] [datetime2](0) NULL,
	[fechaOrdeno] [datetime2](0) NULL,
	[fechaRetirada] [datetime2](0) NULL,
	[idAnimal] [int] NOT NULL,
	[fechaRegistro] [datetime2](0) NOT NULL,
	[turno] [varchar](20) NOT NULL,
	[cantidadIndustria] [decimal](10, 2) NULL,
	[cantidadTerneros] [decimal](10, 2) NULL,
	[cantidadDescartada] [decimal](10, 2) NULL,
	[cantidadVentaDirecta] [decimal](10, 2) NULL,
	[tieneAntibiotico] [bit] NOT NULL,
	[motivoDescarte] [nvarchar](200) NULL,
	[diasEnLeche] [int] NULL,
	[fuente] [varchar](50) NULL,
 CONSTRAINT [PK_RegistroProduccionLeche] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RegistroReproduccion]    Script Date: 12/01/2026 11:22:44 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RegistroReproduccion](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[fotoVaca] [nvarchar](260) NULL,
	[fechaRegistro] [datetime2](0) NOT NULL,
	[idAnimal] [int] NOT NULL,
 CONSTRAINT [PK_RegistroReproduccion] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RegistroSalida]    Script Date: 12/01/2026 11:22:44 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RegistroSalida](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[nombre] [nvarchar](150) NOT NULL,
	[tipoSalida] [nvarchar](80) NOT NULL,
	[idAnimal] [int] NOT NULL,
	[fechaSalida] [date] NOT NULL,
	[idHato] [int] NULL,
	[destino] [nvarchar](200) NULL,
	[usuarioId] [int] NULL,
	[observacion] [nvarchar](600) NULL,
 CONSTRAINT [PK_RegistroSalida] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ReporteIndustriaLeche]    Script Date: 12/01/2026 11:22:44 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ReporteIndustriaLeche](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[fecha] [date] NOT NULL,
	[turno] [varchar](20) NOT NULL,
	[idHato] [int] NOT NULL,
	[pesoReportado] [decimal](10, 2) NOT NULL,
	[observacion] [varchar](200) NULL,
	[fechaRegistro] [datetime2](0) NOT NULL,
 CONSTRAINT [PK_ReporteIndustriaLeche] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RequerimientoNutricional]    Script Date: 12/01/2026 11:22:44 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RequerimientoNutricional](
	[IdRequerimientoNutricional] [int] IDENTITY(1,1) NOT NULL,
	[IdCategoriaAnimal] [int] NOT NULL,
	[ProduccionMinLitros] [decimal](10, 2) NULL,
	[ProduccionMaxLitros] [decimal](10, 2) NULL,
	[IdNutriente] [int] NOT NULL,
	[ValorMin] [decimal](10, 4) NULL,
	[ValorMax] [decimal](10, 4) NULL,
 CONSTRAINT [PK_RequerimientoNutricional] PRIMARY KEY CLUSTERED 
(
	[IdRequerimientoNutricional] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Rol]    Script Date: 12/01/2026 11:22:44 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Rol](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Nombre] [nvarchar](100) NOT NULL,
 CONSTRAINT [PK_Rol] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RtmEntrega]    Script Date: 12/01/2026 11:22:44 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RtmEntrega](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[hatoId] [int] NOT NULL,
	[formulaId] [int] NOT NULL,
	[fecha] [date] NOT NULL,
	[hora] [time](0) NOT NULL,
	[kgTotal] [decimal](12, 2) NOT NULL,
	[numeroVacas] [int] NOT NULL,
	[kgPorVaca] [decimal](12, 4) NOT NULL,
	[idUsuario] [int] NULL,
	[observacion] [nvarchar](250) NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RtmFormula]    Script Date: 12/01/2026 11:22:44 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RtmFormula](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[nombre] [nvarchar](120) NOT NULL,
	[descripcion] [nvarchar](300) NULL,
	[activo] [bit] NOT NULL,
	[fechaCreacion] [datetime2](0) NOT NULL,
	[costoKg] [decimal](12, 4) NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RtmFormulaDetalle]    Script Date: 12/01/2026 11:22:44 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RtmFormulaDetalle](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[formulaId] [int] NOT NULL,
	[ingredienteId] [int] NOT NULL,
	[porcentaje] [decimal](7, 4) NOT NULL,
	[observacion] [nvarchar](200) NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RtmIngrediente]    Script Date: 12/01/2026 11:22:44 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RtmIngrediente](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[nombre] [nvarchar](120) NOT NULL,
	[unidad] [nvarchar](20) NULL,
	[costoKg] [decimal](12, 4) NULL,
	[msPct] [decimal](5, 2) NULL,
	[activo] [bit] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RtmRacionCorral]    Script Date: 12/01/2026 11:22:44 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RtmRacionCorral](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[hatoId] [int] NOT NULL,
	[formulaId] [int] NOT NULL,
	[hora] [time](0) NOT NULL,
	[kgRtmPorVaca] [decimal](12, 2) NOT NULL,
	[activo] [bit] NOT NULL,
	[observacion] [nvarchar](200) NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Seca]    Script Date: 12/01/2026 11:22:44 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Seca](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[motivo] [nvarchar](250) NULL,
	[idRegistroReproduccion] [int] NOT NULL,
	[fechaSeca] [datetime2](0) NULL,
	[diasSecaReal] [int] NULL,
 CONSTRAINT [PK_Seca] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[SexoCria]    Script Date: 12/01/2026 11:22:44 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SexoCria](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Nombre] [nvarchar](60) NOT NULL,
	[Activo] [bit] NOT NULL,
	[Orden] [int] NOT NULL,
 CONSTRAINT [PK_SexoCria] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Sintomas]    Script Date: 12/01/2026 11:22:44 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Sintomas](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[nombre] [nvarchar](150) NOT NULL,
	[idTipoEnfermedad] [int] NOT NULL,
 CONSTRAINT [PK_Sintomas] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[TipoAlimento]    Script Date: 12/01/2026 11:22:44 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TipoAlimento](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[estado] [nvarchar](50) NULL,
	[idAnimal] [int] NOT NULL,
 CONSTRAINT [PK_TipoAlimento] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[TipoCosto]    Script Date: 12/01/2026 11:22:44 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TipoCosto](
	[IdTipoCosto] [int] IDENTITY(1,1) NOT NULL,
	[Nombre] [nvarchar](100) NOT NULL,
	[EsVariable] [bit] NOT NULL,
 CONSTRAINT [PK_TipoCosto] PRIMARY KEY CLUSTERED 
(
	[IdTipoCosto] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[TipoEnfermedades]    Script Date: 12/01/2026 11:22:44 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TipoEnfermedades](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[nombre] [nvarchar](120) NOT NULL,
 CONSTRAINT [PK_TipoEnfermedades] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[TipoParto]    Script Date: 12/01/2026 11:22:44 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TipoParto](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Nombre] [nvarchar](60) NOT NULL,
	[Activo] [bit] NOT NULL,
	[Orden] [int] NOT NULL,
 CONSTRAINT [PK_TipoParto] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[TipoTratamiento]    Script Date: 12/01/2026 11:22:44 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TipoTratamiento](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[nombre] [nvarchar](150) NOT NULL,
	[costo] [decimal](12, 2) NULL,
	[cantidad] [decimal](12, 2) NULL,
	[unidad] [nvarchar](50) NULL,
	[idTipoEnfermedad] [int] NOT NULL,
	[retiroLecheDias] [int] NULL,
 CONSTRAINT [PK_TipoTratamiento] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Usuario]    Script Date: 12/01/2026 11:22:44 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Usuario](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[nombreUsuario] [nvarchar](60) NOT NULL,
	[nombre] [nvarchar](120) NOT NULL,
	[idEstablo] [int] NULL,
	[idHato] [int] NULL,
	[contrasena] [nvarchar](255) NOT NULL,
	[RolId] [int] NOT NULL,
 CONSTRAINT [PK_Usuario] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
SET IDENTITY_INSERT [dbo].[Animal] ON 

INSERT [dbo].[Animal] ([Id], [nombre], [sexo], [codigo], [IdentificadorElectronico], [OtroIdentificador], [color], [fechaNacimiento], [observaciones], [idHato], [idPadre], [idMadre], [idUltimoCrecimiento], [estadoId], [propositoId], [idRaza], [procedenciaId], [nacimientoEstimado], [estadoProductivoId], [IdCategoriaAnimal], [arete], [naab]) VALUES (1, N'ZEUS', N'MACHO', N'TOR-0001', N'EID-TOR-0001', N'CHIP-TOR-0001', N'NEGRO CON BLANCO', CAST(N'2015-03-10' AS Date), NULL, 2, NULL, NULL, NULL, 1, 2, 1, 3, 0, 1, NULL, NULL, NULL)
INSERT [dbo].[Animal] ([Id], [nombre], [sexo], [codigo], [IdentificadorElectronico], [OtroIdentificador], [color], [fechaNacimiento], [observaciones], [idHato], [idPadre], [idMadre], [idUltimoCrecimiento], [estadoId], [propositoId], [idRaza], [procedenciaId], [nacimientoEstimado], [estadoProductivoId], [IdCategoriaAnimal], [arete], [naab]) VALUES (2, N'LILA', N'HEMBRA', N'VAC-0001', N'EID-VAC-0001', N'CHIP-VAC-0001', N'NEGRO CON BLANCO', CAST(N'2016-01-05' AS Date), NULL, 2, NULL, NULL, NULL, 1, 3, 1, 3, 0, 1, NULL, NULL, NULL)
INSERT [dbo].[Animal] ([Id], [nombre], [sexo], [codigo], [IdentificadorElectronico], [OtroIdentificador], [color], [fechaNacimiento], [observaciones], [idHato], [idPadre], [idMadre], [idUltimoCrecimiento], [estadoId], [propositoId], [idRaza], [procedenciaId], [nacimientoEstimado], [estadoProductivoId], [IdCategoriaAnimal], [arete], [naab]) VALUES (3, N'TITAN', N'MACHO', N'TOR-0002', N'EID-TOR-0002', N'EID-TOR-0002', N'ROJO CON BLANCO', CAST(N'2014-08-22' AS Date), NULL, 2, NULL, NULL, NULL, 1, 2, 1, 3, 0, 1, NULL, NULL, NULL)
INSERT [dbo].[Animal] ([Id], [nombre], [sexo], [codigo], [IdentificadorElectronico], [OtroIdentificador], [color], [fechaNacimiento], [observaciones], [idHato], [idPadre], [idMadre], [idUltimoCrecimiento], [estadoId], [propositoId], [idRaza], [procedenciaId], [nacimientoEstimado], [estadoProductivoId], [IdCategoriaAnimal], [arete], [naab]) VALUES (4, N'SARA', N'HEMBRA', N'VAC-0002', N'EID-VAC-0002', N'CHIP-VAC-0002', N'NEGRO CON BLANCO', CAST(N'2016-07-12' AS Date), NULL, 2, NULL, NULL, NULL, 1, 3, 1, 3, 0, 1, NULL, NULL, NULL)
INSERT [dbo].[Animal] ([Id], [nombre], [sexo], [codigo], [IdentificadorElectronico], [OtroIdentificador], [color], [fechaNacimiento], [observaciones], [idHato], [idPadre], [idMadre], [idUltimoCrecimiento], [estadoId], [propositoId], [idRaza], [procedenciaId], [nacimientoEstimado], [estadoProductivoId], [IdCategoriaAnimal], [arete], [naab]) VALUES (5, N'GOLIAT', N'MACHO', N'TOR-0100', NULL, NULL, N'NEGRO CON BLANCO', CAST(N'2019-02-14' AS Date), NULL, 2, 1, 2, NULL, 1, 2, 1, 2, 0, 1, NULL, NULL, NULL)
INSERT [dbo].[Animal] ([Id], [nombre], [sexo], [codigo], [IdentificadorElectronico], [OtroIdentificador], [color], [fechaNacimiento], [observaciones], [idHato], [idPadre], [idMadre], [idUltimoCrecimiento], [estadoId], [propositoId], [idRaza], [procedenciaId], [nacimientoEstimado], [estadoProductivoId], [IdCategoriaAnimal], [arete], [naab]) VALUES (6, N'BRUNO', N'MACHO', N'TOR-0200', NULL, NULL, N'ROJO CON BLANCO', CAST(N'2019-05-30' AS Date), NULL, 2, 3, 4, NULL, 1, 2, 1, 2, 0, 1, NULL, NULL, NULL)
INSERT [dbo].[Animal] ([Id], [nombre], [sexo], [codigo], [IdentificadorElectronico], [OtroIdentificador], [color], [fechaNacimiento], [observaciones], [idHato], [idPadre], [idMadre], [idUltimoCrecimiento], [estadoId], [propositoId], [idRaza], [procedenciaId], [nacimientoEstimado], [estadoProductivoId], [IdCategoriaAnimal], [arete], [naab]) VALUES (7, N'DULCE', N'HEMBRA', N'VAC-0101', NULL, NULL, N'NEGRO CON BLANCO', CAST(N'2019-06-15' AS Date), NULL, 2, 3, 4, NULL, 1, 3, 1, 2, 0, 1, NULL, NULL, NULL)
INSERT [dbo].[Animal] ([Id], [nombre], [sexo], [codigo], [IdentificadorElectronico], [OtroIdentificador], [color], [fechaNacimiento], [observaciones], [idHato], [idPadre], [idMadre], [idUltimoCrecimiento], [estadoId], [propositoId], [idRaza], [procedenciaId], [nacimientoEstimado], [estadoProductivoId], [IdCategoriaAnimal], [arete], [naab]) VALUES (8, N'NINA', N'HEMBRA', N'VAC-0201', NULL, NULL, NULL, CAST(N'2019-09-20' AS Date), NULL, 2, 1, 2, NULL, 1, 3, 1, 2, 0, 1, NULL, NULL, NULL)
INSERT [dbo].[Animal] ([Id], [nombre], [sexo], [codigo], [IdentificadorElectronico], [OtroIdentificador], [color], [fechaNacimiento], [observaciones], [idHato], [idPadre], [idMadre], [idUltimoCrecimiento], [estadoId], [propositoId], [idRaza], [procedenciaId], [nacimientoEstimado], [estadoProductivoId], [IdCategoriaAnimal], [arete], [naab]) VALUES (9, N'MAX', N'MACHO', N'TOR-0300', NULL, NULL, N'NEGRO CON BLANCO', CAST(N'2021-03-18' AS Date), NULL, 2, 5, 7, NULL, 1, 2, 1, 2, 0, 1, NULL, NULL, NULL)
INSERT [dbo].[Animal] ([Id], [nombre], [sexo], [codigo], [IdentificadorElectronico], [OtroIdentificador], [color], [fechaNacimiento], [observaciones], [idHato], [idPadre], [idMadre], [idUltimoCrecimiento], [estadoId], [propositoId], [idRaza], [procedenciaId], [nacimientoEstimado], [estadoProductivoId], [IdCategoriaAnimal], [arete], [naab]) VALUES (10, N'LUCERA', N'HEMBRA', N'VAC-0301', NULL, NULL, N'NEGRO CON BLANCO', CAST(N'2021-10-04' AS Date), NULL, 2, 6, 8, NULL, 1, 2, 1, 2, 0, 3, NULL, NULL, NULL)
INSERT [dbo].[Animal] ([Id], [nombre], [sexo], [codigo], [IdentificadorElectronico], [OtroIdentificador], [color], [fechaNacimiento], [observaciones], [idHato], [idPadre], [idMadre], [idUltimoCrecimiento], [estadoId], [propositoId], [idRaza], [procedenciaId], [nacimientoEstimado], [estadoProductivoId], [IdCategoriaAnimal], [arete], [naab]) VALUES (11, N'SOL', N'HEMBRA', N'CR-0400', NULL, NULL, NULL, CAST(N'2025-12-17' AS Date), NULL, 2, 9, 10, NULL, 1, 2, 1, 3, 0, NULL, 0, N'PE-000123', NULL)
SET IDENTITY_INSERT [dbo].[Animal] OFF
GO
SET IDENTITY_INSERT [dbo].[Calidad] ON 

INSERT [dbo].[Calidad] ([Id], [grasa], [proteina], [solidosTotales], [urea], [idRegistroProduccionLeche], [fechaRegistro], [rcs]) VALUES (0, CAST(38.00 AS Decimal(6, 2)), CAST(32.00 AS Decimal(6, 2)), CAST(124.00 AS Decimal(6, 2)), CAST(22.00 AS Decimal(8, 2)), 2, CAST(N'2026-01-04T22:05:58.0000000' AS DateTime2), 180)
INSERT [dbo].[Calidad] ([Id], [grasa], [proteina], [solidosTotales], [urea], [idRegistroProduccionLeche], [fechaRegistro], [rcs]) VALUES (1, CAST(375.00 AS Decimal(6, 2)), CAST(320.00 AS Decimal(6, 2)), CAST(1230.00 AS Decimal(6, 2)), CAST(21.00 AS Decimal(8, 2)), 1, CAST(N'2026-01-04T22:06:21.0000000' AS DateTime2), 160)
INSERT [dbo].[Calidad] ([Id], [grasa], [proteina], [solidosTotales], [urea], [idRegistroProduccionLeche], [fechaRegistro], [rcs]) VALUES (2, CAST(385.00 AS Decimal(6, 2)), CAST(325.00 AS Decimal(6, 2)), CAST(1245.00 AS Decimal(6, 2)), CAST(22.00 AS Decimal(8, 2)), 0, CAST(N'2026-01-04T22:06:45.0000000' AS DateTime2), 170)
SET IDENTITY_INSERT [dbo].[Calidad] OFF
GO
SET IDENTITY_INSERT [dbo].[CalidadDiariaHato] ON 

INSERT [dbo].[CalidadDiariaHato] ([Id], [fecha], [idHato], [fuente], [grasa], [proteina], [solidosTotales], [urea], [rcs], [observaciones], [fechaRegistro]) VALUES (0, CAST(N'2025-12-18' AS Date), 2, N'GLORIA', CAST(3.60 AS Decimal(5, 2)), CAST(3.20 AS Decimal(5, 2)), CAST(12.20 AS Decimal(5, 2)), CAST(21.00 AS Decimal(6, 2)), 180000, N'TANQUE-GLORIA', CAST(N'2026-01-05T01:04:42.0000000' AS DateTime2))
INSERT [dbo].[CalidadDiariaHato] ([Id], [fecha], [idHato], [fuente], [grasa], [proteina], [solidosTotales], [urea], [rcs], [observaciones], [fechaRegistro]) VALUES (1, CAST(N'2025-12-19' AS Date), 2, N'GLORIA', CAST(3.55 AS Decimal(5, 2)), CAST(3.18 AS Decimal(5, 2)), CAST(12.10 AS Decimal(5, 2)), CAST(20.50 AS Decimal(6, 2)), 190000, NULL, CAST(N'2026-01-05T01:05:13.0000000' AS DateTime2))
INSERT [dbo].[CalidadDiariaHato] ([Id], [fecha], [idHato], [fuente], [grasa], [proteina], [solidosTotales], [urea], [rcs], [observaciones], [fechaRegistro]) VALUES (2, CAST(N'2025-12-20' AS Date), 2, N'GLORIA', CAST(3.50 AS Decimal(5, 2)), CAST(3.15 AS Decimal(5, 2)), CAST(12.00 AS Decimal(5, 2)), CAST(19.80 AS Decimal(6, 2)), 170000, NULL, CAST(N'2026-01-05T01:05:35.0000000' AS DateTime2))
SET IDENTITY_INSERT [dbo].[CalidadDiariaHato] OFF
GO
SET IDENTITY_INSERT [dbo].[CategoriaAnimal] ON 

INSERT [dbo].[CategoriaAnimal] ([IdCategoriaAnimal], [Nombre]) VALUES (0, N'Ternera')
INSERT [dbo].[CategoriaAnimal] ([IdCategoriaAnimal], [Nombre]) VALUES (1, N'Ternero')
INSERT [dbo].[CategoriaAnimal] ([IdCategoriaAnimal], [Nombre]) VALUES (2, N'Novilla')
INSERT [dbo].[CategoriaAnimal] ([IdCategoriaAnimal], [Nombre]) VALUES (3, N'Vaca en lactancia')
INSERT [dbo].[CategoriaAnimal] ([IdCategoriaAnimal], [Nombre]) VALUES (4, N'Vaca seca')
SET IDENTITY_INSERT [dbo].[CategoriaAnimal] OFF
GO
SET IDENTITY_INSERT [dbo].[CausaAborto] ON 

INSERT [dbo].[CausaAborto] ([Id], [Nombre], [Oculto]) VALUES (1, N'DESCONOCIDA', 0)
INSERT [dbo].[CausaAborto] ([Id], [Nombre], [Oculto]) VALUES (2, N'INFECCIOSA', 0)
SET IDENTITY_INSERT [dbo].[CausaAborto] OFF
GO
SET IDENTITY_INSERT [dbo].[CentroCosto] ON 

INSERT [dbo].[CentroCosto] ([IdCentroCosto], [Nombre], [Descripcion], [Activo]) VALUES (0, N'ALIMENTACION', N'Raciones/insumos', 1)
INSERT [dbo].[CentroCosto] ([IdCentroCosto], [Nombre], [Descripcion], [Activo]) VALUES (1, N'SANIDAD', N'Medicinas/veterinaria', 1)
SET IDENTITY_INSERT [dbo].[CentroCosto] OFF
GO
SET IDENTITY_INSERT [dbo].[Colaborador] ON 

INSERT [dbo].[Colaborador] ([Id], [nombre], [DNI], [EspecialidadId], [idUsuario], [EmpresaId], [Apellido], [Direccion], [Provincia], [Localidad], [CodigoPostal], [Telefono]) VALUES (5, N'marco ', N'12345678', 1, 8, 3, N'gonzales', N'av quilca 366', N'callao', N'callao', N'001', N'123456789')
INSERT [dbo].[Colaborador] ([Id], [nombre], [DNI], [EspecialidadId], [idUsuario], [EmpresaId], [Apellido], [Direccion], [Provincia], [Localidad], [CodigoPostal], [Telefono]) VALUES (6, N'jose', N'12345679', 2, 9, 3, N'iberico', N'av angeles 4564', N'lima', N'lima', N'002', N'789456123')
INSERT [dbo].[Colaborador] ([Id], [nombre], [DNI], [EspecialidadId], [idUsuario], [EmpresaId], [Apellido], [Direccion], [Provincia], [Localidad], [CodigoPostal], [Telefono]) VALUES (7, N'luciano', N'45678912', 3, 10, 3, N'tas', N'av quilca 456', N'callao', N'callao', N'5465', N'789456123')
INSERT [dbo].[Colaborador] ([Id], [nombre], [DNI], [EspecialidadId], [idUsuario], [EmpresaId], [Apellido], [Direccion], [Provincia], [Localidad], [CodigoPostal], [Telefono]) VALUES (9, N'juan', N'45678945', 4, 12, 3, N'divos', N'av giron los maares 768', N'lima', N'lima', N'465', N'789654123')
INSERT [dbo].[Colaborador] ([Id], [nombre], [DNI], [EspecialidadId], [idUsuario], [EmpresaId], [Apellido], [Direccion], [Provincia], [Localidad], [CodigoPostal], [Telefono]) VALUES (10, N'luis', N'45612345', 5, 13, 3, N'davila', N'av racmon casilltaa 6565', N'callao', N'lima', N'456', N'987989798')
SET IDENTITY_INSERT [dbo].[Colaborador] OFF
GO
SET IDENTITY_INSERT [dbo].[ConfirmacionPrenez] ON 

INSERT [dbo].[ConfirmacionPrenez] ([Id], [tipo], [fechaRegistro], [idRegistroReproduccion], [observacion], [metodo]) VALUES (1, N'POSITIVA', CAST(N'2025-12-10T00:00:00.0000000' AS DateTime2), 1, N'Preñez confirmada por ecografía', N'ECOGRAFIA')
SET IDENTITY_INSERT [dbo].[ConfirmacionPrenez] OFF
GO
SET IDENTITY_INSERT [dbo].[Empresa] ON 

INSERT [dbo].[Empresa] ([Id], [usuarioID], [ruc], [logo], [capacidadMaxima], [areaTotal], [areaPasto], [areaBosque], [areaCultivos], [areaConstruida], [ubicacion], [NombreEmpresa], [PlanId]) VALUES (3, 8, N'12345678910', NULL, NULL, NULL, NULL, NULL, NULL, NULL, N'av quilca 366', N'zoteproc sac', 1)
SET IDENTITY_INSERT [dbo].[Empresa] OFF
GO
SET IDENTITY_INSERT [dbo].[Especialidad] ON 

INSERT [dbo].[Especialidad] ([Id], [Nombre]) VALUES (1, N'ADMINISTRACION')
INSERT [dbo].[Especialidad] ([Id], [Nombre]) VALUES (3, N'INSPECTOR')
INSERT [dbo].[Especialidad] ([Id], [Nombre]) VALUES (4, N'LABORATORISTA')
INSERT [dbo].[Especialidad] ([Id], [Nombre]) VALUES (5, N'USUARIO_EMPRESA')
INSERT [dbo].[Especialidad] ([Id], [Nombre]) VALUES (2, N'VETERINARIO')
SET IDENTITY_INSERT [dbo].[Especialidad] OFF
GO
SET IDENTITY_INSERT [dbo].[Establo] ON 

INSERT [dbo].[Establo] ([Id], [nombre], [logo], [capacidadMaxima], [areaTotal], [areaPasto], [areaBosque], [areaCultivos], [areaConstruida], [ubicacion], [EmpresaId], [pveDias]) VALUES (2, N'Establo Santa Rosa', NULL, 250, CAST(60.00 AS Decimal(12, 2)), NULL, NULL, NULL, NULL, N'Huacho - Lima, Perú', 3, 60)
SET IDENTITY_INSERT [dbo].[Establo] OFF
GO
SET IDENTITY_INSERT [dbo].[EstadoAnimal] ON 

INSERT [dbo].[EstadoAnimal] ([Id], [nombre]) VALUES (1, N'ACTIVO')
INSERT [dbo].[EstadoAnimal] ([Id], [nombre]) VALUES (2, N'INACTIVO')
SET IDENTITY_INSERT [dbo].[EstadoAnimal] OFF
GO
SET IDENTITY_INSERT [dbo].[EstadoCria] ON 

INSERT [dbo].[EstadoCria] ([Id], [Nombre], [Activo], [Orden]) VALUES (1, N'VIVA', 1, 1)
INSERT [dbo].[EstadoCria] ([Id], [Nombre], [Activo], [Orden]) VALUES (2, N'MUERTA', 1, 2)
SET IDENTITY_INSERT [dbo].[EstadoCria] OFF
GO
SET IDENTITY_INSERT [dbo].[EstadoProductivo] ON 

INSERT [dbo].[EstadoProductivo] ([Id], [nombre]) VALUES (3, N'LACTANDO')
INSERT [dbo].[EstadoProductivo] ([Id], [nombre]) VALUES (2, N'PREÑADA')
INSERT [dbo].[EstadoProductivo] ([Id], [nombre]) VALUES (4, N'SECA')
INSERT [dbo].[EstadoProductivo] ([Id], [nombre]) VALUES (1, N'VACIA')
SET IDENTITY_INSERT [dbo].[EstadoProductivo] OFF
GO
SET IDENTITY_INSERT [dbo].[Hato] ON 

INSERT [dbo].[Hato] ([Id], [nombre], [sistemaProduccion], [ubicacion], [EstabloId]) VALUES (2, N'Hato Vaquillonas', N'Semi-intensivo (Pastoreo)', N'Corral B - Vaquillonas', 2)
SET IDENTITY_INSERT [dbo].[Hato] OFF
GO
SET IDENTITY_INSERT [dbo].[Parto] ON 

INSERT [dbo].[Parto] ([Id], [tipo], [fechaRegistro], [idRegistroReproduccion], [idSexoCria], [idTipoParto], [idEstadoCria], [nombreCria1], [nombreCria2], [horaParto], [pveDias], [fechaFinPve], [areteCria1], [areteCria2]) VALUES (1, N'NORMAL', CAST(N'2025-12-17T02:30:00.0000000' AS DateTime2), 1, 1, 1, 1, N'SOL', NULL, CAST(N'02:30:00' AS Time), NULL, NULL, NULL, NULL)
SET IDENTITY_INSERT [dbo].[Parto] OFF
GO
SET IDENTITY_INSERT [dbo].[PlanLicencia] ON 

INSERT [dbo].[PlanLicencia] ([Id], [Codigo], [Nombre], [Precio], [Moneda], [EsIndefinido], [MaxAnimales], [MaxEstablos], [Activo], [FechaRegistro]) VALUES (1, N'PRO_INDEF', N'Plan Pro Indefinido', CAST(2000.00 AS Decimal(12, 2)), N'PEN', 1, 10000, 1, 1, CAST(N'2026-01-08T02:11:28.0000000' AS DateTime2))
INSERT [dbo].[PlanLicencia] ([Id], [Codigo], [Nombre], [Precio], [Moneda], [EsIndefinido], [MaxAnimales], [MaxEstablos], [Activo], [FechaRegistro]) VALUES (2, N'FREE_1000', N'Plan Gratuito', CAST(0.00 AS Decimal(12, 2)), N'PEN', 1, 1000, 1, 1, CAST(N'2026-01-12T01:02:52.0000000' AS DateTime2))
SET IDENTITY_INSERT [dbo].[PlanLicencia] OFF
GO
SET IDENTITY_INSERT [dbo].[Prenez] ON 

INSERT [dbo].[Prenez] ([Id], [fechaCelo], [fechaInseminacion], [fechaDiagnostico], [idPadreAnimal], [idMadreAnimal], [idRegistroReproduccion], [observacion], [horaServicio], [numeroServicio], [nombreToro], [codigoNaab], [protocolo], [fechaProbableParto], [fechaProbableSeca]) VALUES (1, NULL, CAST(N'2025-11-01' AS Date), NULL, 9, 10, 1, N'“Inseminación prueba LUCERA con MAX', CAST(N'08:00:00' AS Time), 1, NULL, N'7HO12345', NULL, CAST(N'2026-08-08' AS Date), CAST(N'2026-06-09' AS Date))
SET IDENTITY_INSERT [dbo].[Prenez] OFF
GO
SET IDENTITY_INSERT [dbo].[ProcedenciaAnimal] ON 

INSERT [dbo].[ProcedenciaAnimal] ([Id], [nombre]) VALUES (3, N'COMPRA')
INSERT [dbo].[ProcedenciaAnimal] ([Id], [nombre]) VALUES (1, N'DESCONOCIDA')
INSERT [dbo].[ProcedenciaAnimal] ([Id], [nombre]) VALUES (2, N'NACIMIENTO')
SET IDENTITY_INSERT [dbo].[ProcedenciaAnimal] OFF
GO
SET IDENTITY_INSERT [dbo].[PropositoAnimal] ON 

INSERT [dbo].[PropositoAnimal] ([Id], [nombre]) VALUES (1, N'LECHE')
INSERT [dbo].[PropositoAnimal] ([Id], [nombre]) VALUES (3, N'PRODUCCION')
INSERT [dbo].[PropositoAnimal] ([Id], [nombre]) VALUES (2, N'REPRODUCCIÓN')
SET IDENTITY_INSERT [dbo].[PropositoAnimal] OFF
GO
SET IDENTITY_INSERT [dbo].[Raza] ON 

INSERT [dbo].[Raza] ([Id], [nombre]) VALUES (1, N'HOLSTEIN')
SET IDENTITY_INSERT [dbo].[Raza] OFF
GO
SET IDENTITY_INSERT [dbo].[RegistroIngreso] ON 

INSERT [dbo].[RegistroIngreso] ([Id], [codigoIngreso], [tipoIngreso], [idAnimal], [fechaIngreso], [idHato], [origen], [usuarioId], [observacion]) VALUES (1, N'ING-20260103213807-1', N'ALTA', 1, CAST(N'2026-01-03' AS Date), 2, NULL, 8, N'Alta automática al crear el animal')
INSERT [dbo].[RegistroIngreso] ([Id], [codigoIngreso], [tipoIngreso], [idAnimal], [fechaIngreso], [idHato], [origen], [usuarioId], [observacion]) VALUES (2, N'ING-20260103214205-2', N'ALTA', 2, CAST(N'2026-01-03' AS Date), 2, NULL, 8, N'Alta automática al crear el animal')
INSERT [dbo].[RegistroIngreso] ([Id], [codigoIngreso], [tipoIngreso], [idAnimal], [fechaIngreso], [idHato], [origen], [usuarioId], [observacion]) VALUES (3, N'ING-20260103214314-3', N'ALTA', 3, CAST(N'2026-01-03' AS Date), 2, NULL, 8, N'Alta automática al crear el animal')
INSERT [dbo].[RegistroIngreso] ([Id], [codigoIngreso], [tipoIngreso], [idAnimal], [fechaIngreso], [idHato], [origen], [usuarioId], [observacion]) VALUES (4, N'ING-20260103214434-4', N'ALTA', 4, CAST(N'2026-01-03' AS Date), 2, NULL, 8, N'Alta automática al crear el animal')
INSERT [dbo].[RegistroIngreso] ([Id], [codigoIngreso], [tipoIngreso], [idAnimal], [fechaIngreso], [idHato], [origen], [usuarioId], [observacion]) VALUES (5, N'ING-20260103214606-5', N'ALTA', 5, CAST(N'2026-01-03' AS Date), 2, NULL, 8, N'Alta automática al crear el animal')
INSERT [dbo].[RegistroIngreso] ([Id], [codigoIngreso], [tipoIngreso], [idAnimal], [fechaIngreso], [idHato], [origen], [usuarioId], [observacion]) VALUES (6, N'ING-20260103214710-6', N'ALTA', 6, CAST(N'2026-01-03' AS Date), 2, NULL, 8, N'Alta automática al crear el animal')
INSERT [dbo].[RegistroIngreso] ([Id], [codigoIngreso], [tipoIngreso], [idAnimal], [fechaIngreso], [idHato], [origen], [usuarioId], [observacion]) VALUES (7, N'ING-20260103214847-7', N'ALTA', 7, CAST(N'2026-01-03' AS Date), 2, NULL, 8, N'Alta automática al crear el animal')
INSERT [dbo].[RegistroIngreso] ([Id], [codigoIngreso], [tipoIngreso], [idAnimal], [fechaIngreso], [idHato], [origen], [usuarioId], [observacion]) VALUES (8, N'ING-20260103214950-8', N'ALTA', 8, CAST(N'2026-01-03' AS Date), 2, NULL, 8, N'Alta automática al crear el animal')
INSERT [dbo].[RegistroIngreso] ([Id], [codigoIngreso], [tipoIngreso], [idAnimal], [fechaIngreso], [idHato], [origen], [usuarioId], [observacion]) VALUES (9, N'ING-20260103215051-9', N'ALTA', 9, CAST(N'2026-01-03' AS Date), 2, NULL, 8, N'Alta automática al crear el animal')
INSERT [dbo].[RegistroIngreso] ([Id], [codigoIngreso], [tipoIngreso], [idAnimal], [fechaIngreso], [idHato], [origen], [usuarioId], [observacion]) VALUES (10, N'ING-20260103215144-10', N'ALTA', 10, CAST(N'2026-01-03' AS Date), 2, NULL, 8, N'Alta automática al crear el animal')
INSERT [dbo].[RegistroIngreso] ([Id], [codigoIngreso], [tipoIngreso], [idAnimal], [fechaIngreso], [idHato], [origen], [usuarioId], [observacion]) VALUES (11, N'ING-20260104203926-11', N'ALTA', 11, CAST(N'2025-12-17' AS Date), 2, N'NACIMIENTO', 13, N'Alta automática por parto')
SET IDENTITY_INSERT [dbo].[RegistroIngreso] OFF
GO
SET IDENTITY_INSERT [dbo].[RegistroNacimiento] ON 

INSERT [dbo].[RegistroNacimiento] ([Id], [observacionesNacimiento], [pesoNacimiento], [altitud], [ubicacion], [fecha], [temperatura], [idAnimal], [idRegistroReproduccion]) VALUES (0, N'Nacimiento automático por PARTO', NULL, NULL, NULL, CAST(N'2025-12-17' AS Date), NULL, 11, 1)
SET IDENTITY_INSERT [dbo].[RegistroNacimiento] OFF
GO
SET IDENTITY_INSERT [dbo].[RegistroProduccionLeche] ON 

INSERT [dbo].[RegistroProduccionLeche] ([Id], [pesoOrdeno], [fechaPreparacion], [fechaLimpieza], [fechaDespunte], [fechaColocacionPezoneras], [fechaOrdeno], [fechaRetirada], [idAnimal], [fechaRegistro], [turno], [cantidadIndustria], [cantidadTerneros], [cantidadDescartada], [cantidadVentaDirecta], [tieneAntibiotico], [motivoDescarte], [diasEnLeche], [fuente]) VALUES (0, CAST(22.00 AS Decimal(10, 2)), CAST(N'2025-12-18T05:50:00.0000000' AS DateTime2), CAST(N'2025-12-18T05:55:00.0000000' AS DateTime2), CAST(N'2025-12-18T05:57:00.0000000' AS DateTime2), CAST(N'2025-12-18T06:00:00.0000000' AS DateTime2), CAST(N'2025-12-18T06:01:00.0000000' AS DateTime2), CAST(N'2025-12-18T06:09:00.0000000' AS DateTime2), 10, CAST(N'2026-01-04T20:48:27.0000000' AS DateTime2), N'MAÑANA', CAST(20.00 AS Decimal(10, 2)), CAST(2.00 AS Decimal(10, 2)), CAST(0.00 AS Decimal(10, 2)), CAST(0.00 AS Decimal(10, 2)), 0, NULL, 1, NULL)
INSERT [dbo].[RegistroProduccionLeche] ([Id], [pesoOrdeno], [fechaPreparacion], [fechaLimpieza], [fechaDespunte], [fechaColocacionPezoneras], [fechaOrdeno], [fechaRetirada], [idAnimal], [fechaRegistro], [turno], [cantidadIndustria], [cantidadTerneros], [cantidadDescartada], [cantidadVentaDirecta], [tieneAntibiotico], [motivoDescarte], [diasEnLeche], [fuente]) VALUES (1, CAST(24.00 AS Decimal(10, 2)), CAST(N'2025-12-19T05:50:00.0000000' AS DateTime2), CAST(N'2025-12-19T05:55:00.0000000' AS DateTime2), CAST(N'2025-12-19T05:57:00.0000000' AS DateTime2), CAST(N'2025-12-19T06:00:00.0000000' AS DateTime2), CAST(N'2025-12-19T06:01:00.0000000' AS DateTime2), CAST(N'2025-12-19T06:10:00.0000000' AS DateTime2), 10, CAST(N'2026-01-04T20:50:44.0000000' AS DateTime2), N'MAÑANA', CAST(22.00 AS Decimal(10, 2)), CAST(2.00 AS Decimal(10, 2)), CAST(0.00 AS Decimal(10, 2)), CAST(0.00 AS Decimal(10, 2)), 0, NULL, 2, NULL)
INSERT [dbo].[RegistroProduccionLeche] ([Id], [pesoOrdeno], [fechaPreparacion], [fechaLimpieza], [fechaDespunte], [fechaColocacionPezoneras], [fechaOrdeno], [fechaRetirada], [idAnimal], [fechaRegistro], [turno], [cantidadIndustria], [cantidadTerneros], [cantidadDescartada], [cantidadVentaDirecta], [tieneAntibiotico], [motivoDescarte], [diasEnLeche], [fuente]) VALUES (2, CAST(23.00 AS Decimal(10, 2)), CAST(N'2025-12-20T05:48:00.0000000' AS DateTime2), CAST(N'2025-12-20T05:53:00.0000000' AS DateTime2), CAST(N'2025-12-20T05:55:00.0000000' AS DateTime2), CAST(N'2025-12-20T05:58:00.0000000' AS DateTime2), CAST(N'2025-12-20T05:59:00.0000000' AS DateTime2), CAST(N'2025-12-20T06:07:00.0000000' AS DateTime2), 10, CAST(N'2026-01-04T20:51:53.0000000' AS DateTime2), N'MAÑANA', CAST(21.00 AS Decimal(10, 2)), CAST(2.00 AS Decimal(10, 2)), CAST(0.00 AS Decimal(10, 2)), CAST(0.00 AS Decimal(10, 2)), 0, NULL, 3, NULL)
SET IDENTITY_INSERT [dbo].[RegistroProduccionLeche] OFF
GO
SET IDENTITY_INSERT [dbo].[RegistroReproduccion] ON 

INSERT [dbo].[RegistroReproduccion] ([Id], [fotoVaca], [fechaRegistro], [idAnimal]) VALUES (1, NULL, CAST(N'2025-11-01T00:00:00.0000000' AS DateTime2), 10)
SET IDENTITY_INSERT [dbo].[RegistroReproduccion] OFF
GO
SET IDENTITY_INSERT [dbo].[ReporteIndustriaLeche] ON 

INSERT [dbo].[ReporteIndustriaLeche] ([Id], [fecha], [turno], [idHato], [pesoReportado], [observacion], [fechaRegistro]) VALUES (1, CAST(N'2025-12-18' AS Date), N'MAÑANA', 2, CAST(20.00 AS Decimal(10, 2)), NULL, CAST(N'2026-01-04T21:27:20.0000000' AS DateTime2))
INSERT [dbo].[ReporteIndustriaLeche] ([Id], [fecha], [turno], [idHato], [pesoReportado], [observacion], [fechaRegistro]) VALUES (2, CAST(N'2025-12-19' AS Date), N'MAÑANA', 2, CAST(21.00 AS Decimal(10, 2)), NULL, CAST(N'2026-01-04T21:27:30.0000000' AS DateTime2))
INSERT [dbo].[ReporteIndustriaLeche] ([Id], [fecha], [turno], [idHato], [pesoReportado], [observacion], [fechaRegistro]) VALUES (3, CAST(N'2025-12-20' AS Date), N'MAÑANA', 2, CAST(21.00 AS Decimal(10, 2)), NULL, CAST(N'2026-01-04T21:27:47.0000000' AS DateTime2))
SET IDENTITY_INSERT [dbo].[ReporteIndustriaLeche] OFF
GO
SET IDENTITY_INSERT [dbo].[Rol] ON 

INSERT [dbo].[Rol] ([Id], [Nombre]) VALUES (2, N'ADMIN_EMPRESA')
INSERT [dbo].[Rol] ([Id], [Nombre]) VALUES (5, N'INSPECTOR')
INSERT [dbo].[Rol] ([Id], [Nombre]) VALUES (4, N'LABORATORIO_EMPRESA')
INSERT [dbo].[Rol] ([Id], [Nombre]) VALUES (1, N'SUPERADMIN')
INSERT [dbo].[Rol] ([Id], [Nombre]) VALUES (3, N'USUARIO_EMPRESA')
INSERT [dbo].[Rol] ([Id], [Nombre]) VALUES (6, N'VETERINARIO')
SET IDENTITY_INSERT [dbo].[Rol] OFF
GO
SET IDENTITY_INSERT [dbo].[Seca] ON 

INSERT [dbo].[Seca] ([Id], [motivo], [idRegistroReproduccion], [fechaSeca], [diasSecaReal]) VALUES (1, N'Seca programada preparto', 1, CAST(N'2025-12-15T00:00:00.0000000' AS DateTime2), NULL)
SET IDENTITY_INSERT [dbo].[Seca] OFF
GO
SET IDENTITY_INSERT [dbo].[SexoCria] ON 

INSERT [dbo].[SexoCria] ([Id], [Nombre], [Activo], [Orden]) VALUES (1, N'HEMBRA', 1, 1)
INSERT [dbo].[SexoCria] ([Id], [Nombre], [Activo], [Orden]) VALUES (2, N'MACHO', 1, 2)
INSERT [dbo].[SexoCria] ([Id], [Nombre], [Activo], [Orden]) VALUES (3, N'HEMBRA-HEMBRA', 1, 3)
INSERT [dbo].[SexoCria] ([Id], [Nombre], [Activo], [Orden]) VALUES (4, N'HEMBRA-MACHO', 1, 4)
INSERT [dbo].[SexoCria] ([Id], [Nombre], [Activo], [Orden]) VALUES (5, N'MACHO-HEMBRA', 1, 5)
INSERT [dbo].[SexoCria] ([Id], [Nombre], [Activo], [Orden]) VALUES (6, N'MACHO-MACHO', 1, 6)
SET IDENTITY_INSERT [dbo].[SexoCria] OFF
GO
SET IDENTITY_INSERT [dbo].[TipoCosto] ON 

INSERT [dbo].[TipoCosto] ([IdTipoCosto], [Nombre], [EsVariable]) VALUES (0, N'INSUMO', 1)
INSERT [dbo].[TipoCosto] ([IdTipoCosto], [Nombre], [EsVariable]) VALUES (1, N'SERVICIO', 1)
SET IDENTITY_INSERT [dbo].[TipoCosto] OFF
GO
SET IDENTITY_INSERT [dbo].[TipoEnfermedades] ON 

INSERT [dbo].[TipoEnfermedades] ([Id], [nombre]) VALUES (1, N'ABCESO')
SET IDENTITY_INSERT [dbo].[TipoEnfermedades] OFF
GO
SET IDENTITY_INSERT [dbo].[TipoParto] ON 

INSERT [dbo].[TipoParto] ([Id], [Nombre], [Activo], [Orden]) VALUES (1, N'NORMAL', 1, 1)
INSERT [dbo].[TipoParto] ([Id], [Nombre], [Activo], [Orden]) VALUES (2, N'DISTOCIA', 1, 2)
SET IDENTITY_INSERT [dbo].[TipoParto] OFF
GO
SET IDENTITY_INSERT [dbo].[Usuario] ON 

INSERT [dbo].[Usuario] ([Id], [nombreUsuario], [nombre], [idEstablo], [idHato], [contrasena], [RolId]) VALUES (6, N'kuric001', N'deyb kuric', NULL, NULL, N'AQAAAAIAAYagAAAAELducy9GhxubE4Dl7ETaFkbhjd5dRNnFgWEvAzWMwFcLPQIHjgNHRrS9pAR2+WK8GQ==', 1)
INSERT [dbo].[Usuario] ([Id], [nombreUsuario], [nombre], [idEstablo], [idHato], [contrasena], [RolId]) VALUES (8, N'marco001', N'marco ', NULL, NULL, N'AQAAAAIAAYagAAAAEOSP3eg7dDPZ6c0k7Tge/AVffCLNYXP9AwKHcXLvssPGeNXs2RpKdwMkmIY72tKsQA==', 2)
INSERT [dbo].[Usuario] ([Id], [nombreUsuario], [nombre], [idEstablo], [idHato], [contrasena], [RolId]) VALUES (9, N'jose', N'jose', 2, NULL, N'AQAAAAIAAYagAAAAEIEEg8dyioVzZQSAd6KLO97jV1ZHWykiHaR2TMCtasfYb/Y0+vkhIqD91YaRTBJNjg==', 6)
INSERT [dbo].[Usuario] ([Id], [nombreUsuario], [nombre], [idEstablo], [idHato], [contrasena], [RolId]) VALUES (10, N'luciano001', N'luciano', 2, NULL, N'AQAAAAIAAYagAAAAEDfvD0X7nFAYaXJ6ZaBXsdQ7VPVGkl+dz+2IKxyJEfntLy+YemIn3cDCi+4WZeJx4A==', 5)
INSERT [dbo].[Usuario] ([Id], [nombreUsuario], [nombre], [idEstablo], [idHato], [contrasena], [RolId]) VALUES (12, N'juan001', N'juan', 2, NULL, N'AQAAAAIAAYagAAAAELpyl6d+C4l6A8uMjcgbEYmfhNypCTcy8gFYrTW6PUbQIbN6DCW1lTTACyKKSazDlw==', 4)
INSERT [dbo].[Usuario] ([Id], [nombreUsuario], [nombre], [idEstablo], [idHato], [contrasena], [RolId]) VALUES (13, N'luis001', N'luis', 2, NULL, N'AQAAAAIAAYagAAAAEKPdIypg3XBBkY3Nm+LxxfNDGyFDCrJkGrozJHLhNkUH645WYR78tJqfSi0IhBilew==', 3)
SET IDENTITY_INSERT [dbo].[Usuario] OFF
GO
/****** Object:  Index [IX_Aborto_idCausaAborto]    Script Date: 12/01/2026 11:22:45 ******/
CREATE NONCLUSTERED INDEX [IX_Aborto_idCausaAborto] ON [dbo].[Aborto]
(
	[idCausaAborto] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Aborto_idRegistroReproduccion]    Script Date: 12/01/2026 11:22:45 ******/
CREATE NONCLUSTERED INDEX [IX_Aborto_idRegistroReproduccion] ON [dbo].[Aborto]
(
	[idRegistroReproduccion] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Alimentacion_idAnimal]    Script Date: 12/01/2026 11:22:45 ******/
CREATE NONCLUSTERED INDEX [IX_Alimentacion_idAnimal] ON [dbo].[Alimentacion]
(
	[idAnimal] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Alimentacion_idTipoAlimento]    Script Date: 12/01/2026 11:22:45 ******/
CREATE NONCLUSTERED INDEX [IX_Alimentacion_idTipoAlimento] ON [dbo].[Alimentacion]
(
	[idTipoAlimento] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_Animal_codigo]    Script Date: 12/01/2026 11:22:45 ******/
CREATE NONCLUSTERED INDEX [IX_Animal_codigo] ON [dbo].[Animal]
(
	[codigo] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Animal_IdCategoriaAnimal]    Script Date: 12/01/2026 11:22:45 ******/
CREATE NONCLUSTERED INDEX [IX_Animal_IdCategoriaAnimal] ON [dbo].[Animal]
(
	[IdCategoriaAnimal] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Animal_idHato]    Script Date: 12/01/2026 11:22:45 ******/
CREATE NONCLUSTERED INDEX [IX_Animal_idHato] ON [dbo].[Animal]
(
	[idHato] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Animal_idMadre]    Script Date: 12/01/2026 11:22:45 ******/
CREATE NONCLUSTERED INDEX [IX_Animal_idMadre] ON [dbo].[Animal]
(
	[idMadre] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Animal_idPadre]    Script Date: 12/01/2026 11:22:45 ******/
CREATE NONCLUSTERED INDEX [IX_Animal_idPadre] ON [dbo].[Animal]
(
	[idPadre] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UX_Animal_arete]    Script Date: 12/01/2026 11:22:45 ******/
CREATE UNIQUE NONCLUSTERED INDEX [UX_Animal_arete] ON [dbo].[Animal]
(
	[arete] ASC
)
WHERE ([arete] IS NOT NULL)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UX_Animal_NAAB]    Script Date: 12/01/2026 11:22:45 ******/
CREATE UNIQUE NONCLUSTERED INDEX [UX_Animal_NAAB] ON [dbo].[Animal]
(
	[naab] ASC
)
WHERE ([naab] IS NOT NULL)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Calidad_idRegistroProduccionLeche]    Script Date: 12/01/2026 11:22:45 ******/
CREATE NONCLUSTERED INDEX [IX_Calidad_idRegistroProduccionLeche] ON [dbo].[Calidad]
(
	[idRegistroProduccionLeche] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_CalidadDiariaHato_Hato_Fecha]    Script Date: 12/01/2026 11:22:45 ******/
CREATE NONCLUSTERED INDEX [IX_CalidadDiariaHato_Hato_Fecha] ON [dbo].[CalidadDiariaHato]
(
	[idHato] ASC,
	[fecha] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UX_CalidadDiariaHato_Fecha_Hato_Fuente]    Script Date: 12/01/2026 11:22:45 ******/
CREATE UNIQUE NONCLUSTERED INDEX [UX_CalidadDiariaHato_Fecha_Hato_Fuente] ON [dbo].[CalidadDiariaHato]
(
	[fecha] ASC,
	[idHato] ASC,
	[fuente] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_CampaniaLechera_Establo_Fechas]    Script Date: 12/01/2026 11:22:45 ******/
CREATE NONCLUSTERED INDEX [IX_CampaniaLechera_Establo_Fechas] ON [dbo].[CampaniaLechera]
(
	[EstabloId] ASC,
	[fechaInicio] ASC,
	[fechaFin] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UX_CampaniaLechera_Establo_Nombre]    Script Date: 12/01/2026 11:22:45 ******/
CREATE UNIQUE NONCLUSTERED INDEX [UX_CampaniaLechera_Establo_Nombre] ON [dbo].[CampaniaLechera]
(
	[EstabloId] ASC,
	[nombre] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UQ_CausaAborto_Nombre]    Script Date: 12/01/2026 11:22:45 ******/
ALTER TABLE [dbo].[CausaAborto] ADD  CONSTRAINT [UQ_CausaAborto_Nombre] UNIQUE NONCLUSTERED 
(
	[Nombre] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UQ_Colaborador_DNI]    Script Date: 12/01/2026 11:22:45 ******/
ALTER TABLE [dbo].[Colaborador] ADD  CONSTRAINT [UQ_Colaborador_DNI] UNIQUE NONCLUSTERED 
(
	[DNI] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Colaborador_EmpresaId]    Script Date: 12/01/2026 11:22:45 ******/
CREATE NONCLUSTERED INDEX [IX_Colaborador_EmpresaId] ON [dbo].[Colaborador]
(
	[EmpresaId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_ConfirmacionPrenez_idRegistroReproduccion]    Script Date: 12/01/2026 11:22:45 ******/
CREATE NONCLUSTERED INDEX [IX_ConfirmacionPrenez_idRegistroReproduccion] ON [dbo].[ConfirmacionPrenez]
(
	[idRegistroReproduccion] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_DesarrolloCrecimiento_idAnimal]    Script Date: 12/01/2026 11:22:45 ******/
CREATE NONCLUSTERED INDEX [IX_DesarrolloCrecimiento_idAnimal] ON [dbo].[DesarrolloCrecimiento]
(
	[idAnimal] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UQ_Empresa_Ruc]    Script Date: 12/01/2026 11:22:45 ******/
ALTER TABLE [dbo].[Empresa] ADD  CONSTRAINT [UQ_Empresa_Ruc] UNIQUE NONCLUSTERED 
(
	[ruc] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Enfermedad_idAnimal]    Script Date: 12/01/2026 11:22:45 ******/
CREATE NONCLUSTERED INDEX [IX_Enfermedad_idAnimal] ON [dbo].[Enfermedad]
(
	[idAnimal] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Enfermedad_idTipoEnfermedad]    Script Date: 12/01/2026 11:22:45 ******/
CREATE NONCLUSTERED INDEX [IX_Enfermedad_idTipoEnfermedad] ON [dbo].[Enfermedad]
(
	[idTipoEnfermedad] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Enfermedad_idVeterinario]    Script Date: 12/01/2026 11:22:45 ******/
CREATE NONCLUSTERED INDEX [IX_Enfermedad_idVeterinario] ON [dbo].[Enfermedad]
(
	[idVeterinario] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UQ_Especialidad_Nombre]    Script Date: 12/01/2026 11:22:45 ******/
ALTER TABLE [dbo].[Especialidad] ADD  CONSTRAINT [UQ_Especialidad_Nombre] UNIQUE NONCLUSTERED 
(
	[Nombre] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Establo_EmpresaId]    Script Date: 12/01/2026 11:22:45 ******/
CREATE NONCLUSTERED INDEX [IX_Establo_EmpresaId] ON [dbo].[Establo]
(
	[EmpresaId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UQ__EstadoAn__72AFBCC6DE71D88B]    Script Date: 12/01/2026 11:22:45 ******/
ALTER TABLE [dbo].[EstadoAnimal] ADD UNIQUE NONCLUSTERED 
(
	[nombre] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UQ_EstadoCria_Nombre]    Script Date: 12/01/2026 11:22:45 ******/
ALTER TABLE [dbo].[EstadoCria] ADD  CONSTRAINT [UQ_EstadoCria_Nombre] UNIQUE NONCLUSTERED 
(
	[Nombre] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UQ__EstadoPr__72AFBCC630A86A0B]    Script Date: 12/01/2026 11:22:45 ******/
ALTER TABLE [dbo].[EstadoProductivo] ADD UNIQUE NONCLUSTERED 
(
	[nombre] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_EventoGeneral_idAnimal_fecha]    Script Date: 12/01/2026 11:22:45 ******/
CREATE NONCLUSTERED INDEX [IX_EventoGeneral_idAnimal_fecha] ON [dbo].[EventoGeneral]
(
	[idAnimal] ASC,
	[fechaEvento] DESC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Hato_EstabloId]    Script Date: 12/01/2026 11:22:45 ******/
CREATE NONCLUSTERED INDEX [IX_Hato_EstabloId] ON [dbo].[Hato]
(
	[EstabloId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_LecturaMedidor_CodigoMedidor_Fecha]    Script Date: 12/01/2026 11:22:45 ******/
CREATE NONCLUSTERED INDEX [IX_LecturaMedidor_CodigoMedidor_Fecha] ON [dbo].[LecturaMedidorLeche]
(
	[CodigoMedidor] ASC,
	[FechaHoraLectura] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_LecturaMedidor_Procesado]    Script Date: 12/01/2026 11:22:45 ******/
CREATE NONCLUSTERED INDEX [IX_LecturaMedidor_Procesado] ON [dbo].[LecturaMedidorLeche]
(
	[Procesado] ASC,
	[FechaHoraLectura] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_MovimientoCosto_Fecha]    Script Date: 12/01/2026 11:22:45 ******/
CREATE NONCLUSTERED INDEX [IX_MovimientoCosto_Fecha] ON [dbo].[MovimientoCosto]
(
	[Fecha] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_MovimientoCosto_IdAnimal]    Script Date: 12/01/2026 11:22:45 ******/
CREATE NONCLUSTERED INDEX [IX_MovimientoCosto_IdAnimal] ON [dbo].[MovimientoCosto]
(
	[IdAnimal] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Parto_idRegistroReproduccion]    Script Date: 12/01/2026 11:22:45 ******/
CREATE NONCLUSTERED INDEX [IX_Parto_idRegistroReproduccion] ON [dbo].[Parto]
(
	[idRegistroReproduccion] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UX_PlanLicencia_Codigo]    Script Date: 12/01/2026 11:22:45 ******/
CREATE UNIQUE NONCLUSTERED INDEX [UX_PlanLicencia_Codigo] ON [dbo].[PlanLicencia]
(
	[Codigo] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Prenez_idMadreAnimal]    Script Date: 12/01/2026 11:22:45 ******/
CREATE NONCLUSTERED INDEX [IX_Prenez_idMadreAnimal] ON [dbo].[Prenez]
(
	[idMadreAnimal] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Prenez_idPadreAnimal]    Script Date: 12/01/2026 11:22:45 ******/
CREATE NONCLUSTERED INDEX [IX_Prenez_idPadreAnimal] ON [dbo].[Prenez]
(
	[idPadreAnimal] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Prenez_idRegistroReproduccion]    Script Date: 12/01/2026 11:22:45 ******/
CREATE NONCLUSTERED INDEX [IX_Prenez_idRegistroReproduccion] ON [dbo].[Prenez]
(
	[idRegistroReproduccion] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UQ__Proceden__72AFBCC639988B77]    Script Date: 12/01/2026 11:22:45 ******/
ALTER TABLE [dbo].[ProcedenciaAnimal] ADD UNIQUE NONCLUSTERED 
(
	[nombre] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UQ__Proposit__72AFBCC6C403BB3A]    Script Date: 12/01/2026 11:22:45 ******/
ALTER TABLE [dbo].[PropositoAnimal] ADD UNIQUE NONCLUSTERED 
(
	[nombre] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UQ__Raza__72AFBCC6C6A94B6B]    Script Date: 12/01/2026 11:22:45 ******/
ALTER TABLE [dbo].[Raza] ADD UNIQUE NONCLUSTERED 
(
	[nombre] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UQ_RegistroIngreso_codigo]    Script Date: 12/01/2026 11:22:45 ******/
ALTER TABLE [dbo].[RegistroIngreso] ADD  CONSTRAINT [UQ_RegistroIngreso_codigo] UNIQUE NONCLUSTERED 
(
	[codigoIngreso] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_RegistroIngreso_idAnimal]    Script Date: 12/01/2026 11:22:45 ******/
CREATE NONCLUSTERED INDEX [IX_RegistroIngreso_idAnimal] ON [dbo].[RegistroIngreso]
(
	[idAnimal] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_RegistroNacimiento_idAnimal]    Script Date: 12/01/2026 11:22:45 ******/
CREATE NONCLUSTERED INDEX [IX_RegistroNacimiento_idAnimal] ON [dbo].[RegistroNacimiento]
(
	[idAnimal] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_RegistroNacimiento_idRegistroReproduccion]    Script Date: 12/01/2026 11:22:45 ******/
CREATE NONCLUSTERED INDEX [IX_RegistroNacimiento_idRegistroReproduccion] ON [dbo].[RegistroNacimiento]
(
	[idRegistroReproduccion] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_RegistroProduccionLeche_idAnimal]    Script Date: 12/01/2026 11:22:45 ******/
CREATE NONCLUSTERED INDEX [IX_RegistroProduccionLeche_idAnimal] ON [dbo].[RegistroProduccionLeche]
(
	[idAnimal] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_RegistroReproduccion_idAnimal]    Script Date: 12/01/2026 11:22:45 ******/
CREATE NONCLUSTERED INDEX [IX_RegistroReproduccion_idAnimal] ON [dbo].[RegistroReproduccion]
(
	[idAnimal] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_RegistroSalida_idAnimal]    Script Date: 12/01/2026 11:22:45 ******/
CREATE NONCLUSTERED INDEX [IX_RegistroSalida_idAnimal] ON [dbo].[RegistroSalida]
(
	[idAnimal] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_ReporteIndustriaLeche_fecha]    Script Date: 12/01/2026 11:22:45 ******/
CREATE NONCLUSTERED INDEX [IX_ReporteIndustriaLeche_fecha] ON [dbo].[ReporteIndustriaLeche]
(
	[fecha] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UX_ReporteIndustriaLeche_FechaTurnoHato]    Script Date: 12/01/2026 11:22:45 ******/
CREATE UNIQUE NONCLUSTERED INDEX [UX_ReporteIndustriaLeche_FechaTurnoHato] ON [dbo].[ReporteIndustriaLeche]
(
	[fecha] ASC,
	[turno] ASC,
	[idHato] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UQ_Rol_Nombre]    Script Date: 12/01/2026 11:22:45 ******/
ALTER TABLE [dbo].[Rol] ADD  CONSTRAINT [UQ_Rol_Nombre] UNIQUE NONCLUSTERED 
(
	[Nombre] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_RtmEntrega_fecha_hato]    Script Date: 12/01/2026 11:22:45 ******/
CREATE NONCLUSTERED INDEX [IX_RtmEntrega_fecha_hato] ON [dbo].[RtmEntrega]
(
	[fecha] ASC,
	[hatoId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_RtmFormulaDetalle_formulaId]    Script Date: 12/01/2026 11:22:45 ******/
CREATE NONCLUSTERED INDEX [IX_RtmFormulaDetalle_formulaId] ON [dbo].[RtmFormulaDetalle]
(
	[formulaId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_RtmRacionCorral_hatoId]    Script Date: 12/01/2026 11:22:45 ******/
CREATE NONCLUSTERED INDEX [IX_RtmRacionCorral_hatoId] ON [dbo].[RtmRacionCorral]
(
	[hatoId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Seca_idRegistroReproduccion]    Script Date: 12/01/2026 11:22:45 ******/
CREATE NONCLUSTERED INDEX [IX_Seca_idRegistroReproduccion] ON [dbo].[Seca]
(
	[idRegistroReproduccion] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UQ_SexoCria_Nombre]    Script Date: 12/01/2026 11:22:45 ******/
ALTER TABLE [dbo].[SexoCria] ADD  CONSTRAINT [UQ_SexoCria_Nombre] UNIQUE NONCLUSTERED 
(
	[Nombre] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Sintomas_idTipoEnfermedad]    Script Date: 12/01/2026 11:22:45 ******/
CREATE NONCLUSTERED INDEX [IX_Sintomas_idTipoEnfermedad] ON [dbo].[Sintomas]
(
	[idTipoEnfermedad] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_TipoAlimento_idAnimal]    Script Date: 12/01/2026 11:22:45 ******/
CREATE NONCLUSTERED INDEX [IX_TipoAlimento_idAnimal] ON [dbo].[TipoAlimento]
(
	[idAnimal] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UQ_TipoEnfermedades_nombre]    Script Date: 12/01/2026 11:22:45 ******/
ALTER TABLE [dbo].[TipoEnfermedades] ADD  CONSTRAINT [UQ_TipoEnfermedades_nombre] UNIQUE NONCLUSTERED 
(
	[nombre] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UQ_TipoParto_Nombre]    Script Date: 12/01/2026 11:22:45 ******/
ALTER TABLE [dbo].[TipoParto] ADD  CONSTRAINT [UQ_TipoParto_Nombre] UNIQUE NONCLUSTERED 
(
	[Nombre] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_TipoTratamiento_idTipoEnfermedad]    Script Date: 12/01/2026 11:22:45 ******/
CREATE NONCLUSTERED INDEX [IX_TipoTratamiento_idTipoEnfermedad] ON [dbo].[TipoTratamiento]
(
	[idTipoEnfermedad] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Tratamiento_idEnfermedad]    Script Date: 12/01/2026 11:22:45 ******/
CREATE NONCLUSTERED INDEX [IX_Tratamiento_idEnfermedad] ON [dbo].[Tratamiento]
(
	[idEnfermedad] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Tratamiento_idTipoTratamiento]    Script Date: 12/01/2026 11:22:45 ******/
CREATE NONCLUSTERED INDEX [IX_Tratamiento_idTipoTratamiento] ON [dbo].[Tratamiento]
(
	[idTipoTratamiento] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UQ_Usuario_nombreUsuario]    Script Date: 12/01/2026 11:22:45 ******/
ALTER TABLE [dbo].[Usuario] ADD  CONSTRAINT [UQ_Usuario_nombreUsuario] UNIQUE NONCLUSTERED 
(
	[nombreUsuario] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Usuario_idEstablo]    Script Date: 12/01/2026 11:22:45 ******/
CREATE NONCLUSTERED INDEX [IX_Usuario_idEstablo] ON [dbo].[Usuario]
(
	[idEstablo] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Usuario_idHato]    Script Date: 12/01/2026 11:22:45 ******/
CREATE NONCLUSTERED INDEX [IX_Usuario_idHato] ON [dbo].[Usuario]
(
	[idHato] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
ALTER TABLE [dbo].[Animal] ADD  CONSTRAINT [DF_Animal_nacEst]  DEFAULT ((0)) FOR [nacimientoEstimado]
GO
ALTER TABLE [dbo].[CalidadDiariaHato] ADD  CONSTRAINT [DF_CalidadDiariaHato_fechaRegistro]  DEFAULT (sysutcdatetime()) FOR [fechaRegistro]
GO
ALTER TABLE [dbo].[CampaniaLechera] ADD  CONSTRAINT [DF_CampaniaLechera_activa]  DEFAULT ((1)) FOR [activa]
GO
ALTER TABLE [dbo].[CausaAborto] ADD  CONSTRAINT [DF_CausaAborto_Oculto]  DEFAULT ((0)) FOR [Oculto]
GO
ALTER TABLE [dbo].[CentroCosto] ADD  CONSTRAINT [DF_CentroCosto_Activo]  DEFAULT ((1)) FOR [Activo]
GO
ALTER TABLE [dbo].[Empresa] ADD  CONSTRAINT [DF_Empresa_NombreEmpresa]  DEFAULT (N'') FOR [NombreEmpresa]
GO
ALTER TABLE [dbo].[Establo] ADD  CONSTRAINT [DF_Establo_pveDias]  DEFAULT ((60)) FOR [pveDias]
GO
ALTER TABLE [dbo].[EstadoCria] ADD  CONSTRAINT [DF_EstadoCria_Activo]  DEFAULT ((1)) FOR [Activo]
GO
ALTER TABLE [dbo].[EstadoCria] ADD  CONSTRAINT [DF_EstadoCria_Orden]  DEFAULT ((0)) FOR [Orden]
GO
ALTER TABLE [dbo].[LecturaMedidorLeche] ADD  CONSTRAINT [DF_LecturaMedidor_Procesado]  DEFAULT ((0)) FOR [Procesado]
GO
ALTER TABLE [dbo].[MovimientoCosto] ADD  CONSTRAINT [DF_MovimientoCosto_FechaRegistro]  DEFAULT (sysdatetime()) FOR [FechaRegistro]
GO
ALTER TABLE [dbo].[PlanLicencia] ADD  CONSTRAINT [DF_PlanLicencia_Moneda]  DEFAULT ('PEN') FOR [Moneda]
GO
ALTER TABLE [dbo].[PlanLicencia] ADD  CONSTRAINT [DF_PlanLicencia_EsIndefinido]  DEFAULT ((0)) FOR [EsIndefinido]
GO
ALTER TABLE [dbo].[PlanLicencia] ADD  CONSTRAINT [DF_PlanLicencia_Activo]  DEFAULT ((1)) FOR [Activo]
GO
ALTER TABLE [dbo].[PlanLicencia] ADD  CONSTRAINT [DF_PlanLicencia_FechaRegistro]  DEFAULT (sysutcdatetime()) FOR [FechaRegistro]
GO
ALTER TABLE [dbo].[RegistroIngreso] ADD  CONSTRAINT [DF_RegistroIngreso_fecha]  DEFAULT (CONVERT([date],getdate())) FOR [fechaIngreso]
GO
ALTER TABLE [dbo].[RegistroProduccionLeche] ADD  CONSTRAINT [DF_RegistroProduccionLeche_turno]  DEFAULT ('MAÑANA') FOR [turno]
GO
ALTER TABLE [dbo].[RegistroProduccionLeche] ADD  CONSTRAINT [DF_RegistroProduccionLeche_tieneAntibiotico]  DEFAULT ((0)) FOR [tieneAntibiotico]
GO
ALTER TABLE [dbo].[RegistroSalida] ADD  CONSTRAINT [DF_RegistroSalida_fecha]  DEFAULT (CONVERT([date],getdate())) FOR [fechaSalida]
GO
ALTER TABLE [dbo].[ReporteIndustriaLeche] ADD  CONSTRAINT [DF_ReporteIndustriaLeche_fechaRegistro]  DEFAULT (sysdatetime()) FOR [fechaRegistro]
GO
ALTER TABLE [dbo].[RtmFormula] ADD  CONSTRAINT [DF_RtmFormula_activo]  DEFAULT ((1)) FOR [activo]
GO
ALTER TABLE [dbo].[RtmFormula] ADD  CONSTRAINT [DF_RtmFormula_fecha]  DEFAULT (sysdatetime()) FOR [fechaCreacion]
GO
ALTER TABLE [dbo].[RtmIngrediente] ADD  CONSTRAINT [DF_RtmIngrediente_activo]  DEFAULT ((1)) FOR [activo]
GO
ALTER TABLE [dbo].[RtmRacionCorral] ADD  CONSTRAINT [DF_RtmRacionCorral_activo]  DEFAULT ((1)) FOR [activo]
GO
ALTER TABLE [dbo].[SexoCria] ADD  CONSTRAINT [DF_SexoCria_Activo]  DEFAULT ((1)) FOR [Activo]
GO
ALTER TABLE [dbo].[SexoCria] ADD  CONSTRAINT [DF_SexoCria_Orden]  DEFAULT ((0)) FOR [Orden]
GO
ALTER TABLE [dbo].[TipoCosto] ADD  CONSTRAINT [DF_TipoCosto_EsVariable]  DEFAULT ((0)) FOR [EsVariable]
GO
ALTER TABLE [dbo].[TipoParto] ADD  CONSTRAINT [DF_TipoParto_Activo]  DEFAULT ((1)) FOR [Activo]
GO
ALTER TABLE [dbo].[TipoParto] ADD  CONSTRAINT [DF_TipoParto_Orden]  DEFAULT ((0)) FOR [Orden]
GO
ALTER TABLE [dbo].[Aborto]  WITH CHECK ADD  CONSTRAINT [FK_Aborto_CausaAborto] FOREIGN KEY([idCausaAborto])
REFERENCES [dbo].[CausaAborto] ([Id])
GO
ALTER TABLE [dbo].[Aborto] CHECK CONSTRAINT [FK_Aborto_CausaAborto]
GO
ALTER TABLE [dbo].[Aborto]  WITH CHECK ADD  CONSTRAINT [FK_Aborto_RegistroReproduccion] FOREIGN KEY([idRegistroReproduccion])
REFERENCES [dbo].[RegistroReproduccion] ([Id])
GO
ALTER TABLE [dbo].[Aborto] CHECK CONSTRAINT [FK_Aborto_RegistroReproduccion]
GO
ALTER TABLE [dbo].[Alimentacion]  WITH CHECK ADD  CONSTRAINT [FK_Alimentacion_Animal] FOREIGN KEY([idAnimal])
REFERENCES [dbo].[Animal] ([Id])
GO
ALTER TABLE [dbo].[Alimentacion] CHECK CONSTRAINT [FK_Alimentacion_Animal]
GO
ALTER TABLE [dbo].[Alimentacion]  WITH CHECK ADD  CONSTRAINT [FK_Alimentacion_TipoAlimento] FOREIGN KEY([idTipoAlimento])
REFERENCES [dbo].[TipoAlimento] ([Id])
GO
ALTER TABLE [dbo].[Alimentacion] CHECK CONSTRAINT [FK_Alimentacion_TipoAlimento]
GO
ALTER TABLE [dbo].[Animal]  WITH CHECK ADD  CONSTRAINT [FK_Animal_CategoriaAnimal] FOREIGN KEY([IdCategoriaAnimal])
REFERENCES [dbo].[CategoriaAnimal] ([IdCategoriaAnimal])
GO
ALTER TABLE [dbo].[Animal] CHECK CONSTRAINT [FK_Animal_CategoriaAnimal]
GO
ALTER TABLE [dbo].[Animal]  WITH CHECK ADD  CONSTRAINT [FK_Animal_EstadoAnimal] FOREIGN KEY([estadoId])
REFERENCES [dbo].[EstadoAnimal] ([Id])
GO
ALTER TABLE [dbo].[Animal] CHECK CONSTRAINT [FK_Animal_EstadoAnimal]
GO
ALTER TABLE [dbo].[Animal]  WITH CHECK ADD  CONSTRAINT [FK_Animal_EstadoProductivo] FOREIGN KEY([estadoProductivoId])
REFERENCES [dbo].[EstadoProductivo] ([Id])
GO
ALTER TABLE [dbo].[Animal] CHECK CONSTRAINT [FK_Animal_EstadoProductivo]
GO
ALTER TABLE [dbo].[Animal]  WITH CHECK ADD  CONSTRAINT [FK_Animal_Hato] FOREIGN KEY([idHato])
REFERENCES [dbo].[Hato] ([Id])
GO
ALTER TABLE [dbo].[Animal] CHECK CONSTRAINT [FK_Animal_Hato]
GO
ALTER TABLE [dbo].[Animal]  WITH CHECK ADD  CONSTRAINT [FK_Animal_Madre] FOREIGN KEY([idMadre])
REFERENCES [dbo].[Animal] ([Id])
GO
ALTER TABLE [dbo].[Animal] CHECK CONSTRAINT [FK_Animal_Madre]
GO
ALTER TABLE [dbo].[Animal]  WITH CHECK ADD  CONSTRAINT [FK_Animal_Padre] FOREIGN KEY([idPadre])
REFERENCES [dbo].[Animal] ([Id])
GO
ALTER TABLE [dbo].[Animal] CHECK CONSTRAINT [FK_Animal_Padre]
GO
ALTER TABLE [dbo].[Animal]  WITH CHECK ADD  CONSTRAINT [FK_Animal_ProcedenciaAnimal] FOREIGN KEY([procedenciaId])
REFERENCES [dbo].[ProcedenciaAnimal] ([Id])
GO
ALTER TABLE [dbo].[Animal] CHECK CONSTRAINT [FK_Animal_ProcedenciaAnimal]
GO
ALTER TABLE [dbo].[Animal]  WITH CHECK ADD  CONSTRAINT [FK_Animal_PropositoAnimal] FOREIGN KEY([propositoId])
REFERENCES [dbo].[PropositoAnimal] ([Id])
GO
ALTER TABLE [dbo].[Animal] CHECK CONSTRAINT [FK_Animal_PropositoAnimal]
GO
ALTER TABLE [dbo].[Animal]  WITH CHECK ADD  CONSTRAINT [FK_Animal_Raza] FOREIGN KEY([idRaza])
REFERENCES [dbo].[Raza] ([Id])
GO
ALTER TABLE [dbo].[Animal] CHECK CONSTRAINT [FK_Animal_Raza]
GO
ALTER TABLE [dbo].[Animal]  WITH CHECK ADD  CONSTRAINT [FK_Animal_UltimoCrecimiento] FOREIGN KEY([idUltimoCrecimiento])
REFERENCES [dbo].[DesarrolloCrecimiento] ([Id])
ON DELETE SET NULL
GO
ALTER TABLE [dbo].[Animal] CHECK CONSTRAINT [FK_Animal_UltimoCrecimiento]
GO
ALTER TABLE [dbo].[Calidad]  WITH CHECK ADD  CONSTRAINT [FK_Calidad_RegistroProduccionLeche] FOREIGN KEY([idRegistroProduccionLeche])
REFERENCES [dbo].[RegistroProduccionLeche] ([Id])
GO
ALTER TABLE [dbo].[Calidad] CHECK CONSTRAINT [FK_Calidad_RegistroProduccionLeche]
GO
ALTER TABLE [dbo].[CalidadDiariaHato]  WITH CHECK ADD  CONSTRAINT [FK_CalidadDiariaHato_Hato] FOREIGN KEY([idHato])
REFERENCES [dbo].[Hato] ([Id])
GO
ALTER TABLE [dbo].[CalidadDiariaHato] CHECK CONSTRAINT [FK_CalidadDiariaHato_Hato]
GO
ALTER TABLE [dbo].[CampaniaLechera]  WITH CHECK ADD  CONSTRAINT [FK_CampaniaLechera_Establo] FOREIGN KEY([EstabloId])
REFERENCES [dbo].[Establo] ([Id])
GO
ALTER TABLE [dbo].[CampaniaLechera] CHECK CONSTRAINT [FK_CampaniaLechera_Establo]
GO
ALTER TABLE [dbo].[Colaborador]  WITH CHECK ADD  CONSTRAINT [FK_Colaborador_Empresa] FOREIGN KEY([EmpresaId])
REFERENCES [dbo].[Empresa] ([Id])
GO
ALTER TABLE [dbo].[Colaborador] CHECK CONSTRAINT [FK_Colaborador_Empresa]
GO
ALTER TABLE [dbo].[Colaborador]  WITH CHECK ADD  CONSTRAINT [FK_Colaborador_Especialidad] FOREIGN KEY([EspecialidadId])
REFERENCES [dbo].[Especialidad] ([Id])
GO
ALTER TABLE [dbo].[Colaborador] CHECK CONSTRAINT [FK_Colaborador_Especialidad]
GO
ALTER TABLE [dbo].[Colaborador]  WITH CHECK ADD  CONSTRAINT [FK_Colaborador_Usuario] FOREIGN KEY([idUsuario])
REFERENCES [dbo].[Usuario] ([Id])
GO
ALTER TABLE [dbo].[Colaborador] CHECK CONSTRAINT [FK_Colaborador_Usuario]
GO
ALTER TABLE [dbo].[ConfirmacionPrenez]  WITH CHECK ADD  CONSTRAINT [FK_ConfirmacionPrenez_RegistroReproduccion] FOREIGN KEY([idRegistroReproduccion])
REFERENCES [dbo].[RegistroReproduccion] ([Id])
GO
ALTER TABLE [dbo].[ConfirmacionPrenez] CHECK CONSTRAINT [FK_ConfirmacionPrenez_RegistroReproduccion]
GO
ALTER TABLE [dbo].[DesarrolloCrecimiento]  WITH CHECK ADD  CONSTRAINT [FK_DesarrolloCrecimiento_Animal] FOREIGN KEY([idAnimal])
REFERENCES [dbo].[Animal] ([Id])
GO
ALTER TABLE [dbo].[DesarrolloCrecimiento] CHECK CONSTRAINT [FK_DesarrolloCrecimiento_Animal]
GO
ALTER TABLE [dbo].[Empresa]  WITH CHECK ADD  CONSTRAINT [FK_Empresa_PlanLicencia] FOREIGN KEY([PlanId])
REFERENCES [dbo].[PlanLicencia] ([Id])
GO
ALTER TABLE [dbo].[Empresa] CHECK CONSTRAINT [FK_Empresa_PlanLicencia]
GO
ALTER TABLE [dbo].[Empresa]  WITH CHECK ADD  CONSTRAINT [FK_Empresa_Usuario] FOREIGN KEY([usuarioID])
REFERENCES [dbo].[Usuario] ([Id])
GO
ALTER TABLE [dbo].[Empresa] CHECK CONSTRAINT [FK_Empresa_Usuario]
GO
ALTER TABLE [dbo].[Enfermedad]  WITH CHECK ADD  CONSTRAINT [FK_Enfermedad_Animal] FOREIGN KEY([idAnimal])
REFERENCES [dbo].[Animal] ([Id])
GO
ALTER TABLE [dbo].[Enfermedad] CHECK CONSTRAINT [FK_Enfermedad_Animal]
GO
ALTER TABLE [dbo].[Enfermedad]  WITH CHECK ADD  CONSTRAINT [FK_Enfermedad_TipoEnfermedades] FOREIGN KEY([idTipoEnfermedad])
REFERENCES [dbo].[TipoEnfermedades] ([Id])
GO
ALTER TABLE [dbo].[Enfermedad] CHECK CONSTRAINT [FK_Enfermedad_TipoEnfermedades]
GO
ALTER TABLE [dbo].[Enfermedad]  WITH CHECK ADD  CONSTRAINT [FK_Enfermedad_UsuarioVeterinario] FOREIGN KEY([idVeterinario])
REFERENCES [dbo].[Usuario] ([Id])
GO
ALTER TABLE [dbo].[Enfermedad] CHECK CONSTRAINT [FK_Enfermedad_UsuarioVeterinario]
GO
ALTER TABLE [dbo].[Establo]  WITH CHECK ADD  CONSTRAINT [FK_Establo_Empresa] FOREIGN KEY([EmpresaId])
REFERENCES [dbo].[Empresa] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Establo] CHECK CONSTRAINT [FK_Establo_Empresa]
GO
ALTER TABLE [dbo].[EventoGeneral]  WITH CHECK ADD  CONSTRAINT [FK_EventoGeneral_Animal] FOREIGN KEY([idAnimal])
REFERENCES [dbo].[Animal] ([Id])
GO
ALTER TABLE [dbo].[EventoGeneral] CHECK CONSTRAINT [FK_EventoGeneral_Animal]
GO
ALTER TABLE [dbo].[Hato]  WITH CHECK ADD  CONSTRAINT [FK_Hato_Establo] FOREIGN KEY([EstabloId])
REFERENCES [dbo].[Establo] ([Id])
GO
ALTER TABLE [dbo].[Hato] CHECK CONSTRAINT [FK_Hato_Establo]
GO
ALTER TABLE [dbo].[MovimientoCosto]  WITH CHECK ADD  CONSTRAINT [FK_MovimientoCosto_CentroCosto] FOREIGN KEY([IdCentroCosto])
REFERENCES [dbo].[CentroCosto] ([IdCentroCosto])
GO
ALTER TABLE [dbo].[MovimientoCosto] CHECK CONSTRAINT [FK_MovimientoCosto_CentroCosto]
GO
ALTER TABLE [dbo].[MovimientoCosto]  WITH CHECK ADD  CONSTRAINT [FK_MovimientoCosto_TipoCosto] FOREIGN KEY([IdTipoCosto])
REFERENCES [dbo].[TipoCosto] ([IdTipoCosto])
GO
ALTER TABLE [dbo].[MovimientoCosto] CHECK CONSTRAINT [FK_MovimientoCosto_TipoCosto]
GO
ALTER TABLE [dbo].[Parto]  WITH CHECK ADD  CONSTRAINT [FK_Parto_EstadoCria] FOREIGN KEY([idEstadoCria])
REFERENCES [dbo].[EstadoCria] ([Id])
GO
ALTER TABLE [dbo].[Parto] CHECK CONSTRAINT [FK_Parto_EstadoCria]
GO
ALTER TABLE [dbo].[Parto]  WITH CHECK ADD  CONSTRAINT [FK_Parto_RegistroReproduccion] FOREIGN KEY([idRegistroReproduccion])
REFERENCES [dbo].[RegistroReproduccion] ([Id])
GO
ALTER TABLE [dbo].[Parto] CHECK CONSTRAINT [FK_Parto_RegistroReproduccion]
GO
ALTER TABLE [dbo].[Parto]  WITH CHECK ADD  CONSTRAINT [FK_Parto_SexoCria] FOREIGN KEY([idSexoCria])
REFERENCES [dbo].[SexoCria] ([Id])
GO
ALTER TABLE [dbo].[Parto] CHECK CONSTRAINT [FK_Parto_SexoCria]
GO
ALTER TABLE [dbo].[Parto]  WITH CHECK ADD  CONSTRAINT [FK_Parto_TipoParto] FOREIGN KEY([idTipoParto])
REFERENCES [dbo].[TipoParto] ([Id])
GO
ALTER TABLE [dbo].[Parto] CHECK CONSTRAINT [FK_Parto_TipoParto]
GO
ALTER TABLE [dbo].[Prenez]  WITH CHECK ADD  CONSTRAINT [FK_Prenez_MadreAnimal] FOREIGN KEY([idMadreAnimal])
REFERENCES [dbo].[Animal] ([Id])
GO
ALTER TABLE [dbo].[Prenez] CHECK CONSTRAINT [FK_Prenez_MadreAnimal]
GO
ALTER TABLE [dbo].[Prenez]  WITH CHECK ADD  CONSTRAINT [FK_Prenez_PadreAnimal] FOREIGN KEY([idPadreAnimal])
REFERENCES [dbo].[Animal] ([Id])
GO
ALTER TABLE [dbo].[Prenez] CHECK CONSTRAINT [FK_Prenez_PadreAnimal]
GO
ALTER TABLE [dbo].[Prenez]  WITH CHECK ADD  CONSTRAINT [FK_Prenez_RegistroReproduccion] FOREIGN KEY([idRegistroReproduccion])
REFERENCES [dbo].[RegistroReproduccion] ([Id])
GO
ALTER TABLE [dbo].[Prenez] CHECK CONSTRAINT [FK_Prenez_RegistroReproduccion]
GO
ALTER TABLE [dbo].[RegistroIngreso]  WITH CHECK ADD  CONSTRAINT [FK_RegistroIngreso_Animal] FOREIGN KEY([idAnimal])
REFERENCES [dbo].[Animal] ([Id])
GO
ALTER TABLE [dbo].[RegistroIngreso] CHECK CONSTRAINT [FK_RegistroIngreso_Animal]
GO
ALTER TABLE [dbo].[RegistroNacimiento]  WITH CHECK ADD  CONSTRAINT [FK_RegistroNacimiento_Animal] FOREIGN KEY([idAnimal])
REFERENCES [dbo].[Animal] ([Id])
GO
ALTER TABLE [dbo].[RegistroNacimiento] CHECK CONSTRAINT [FK_RegistroNacimiento_Animal]
GO
ALTER TABLE [dbo].[RegistroNacimiento]  WITH CHECK ADD  CONSTRAINT [FK_RegistroNacimiento_RegistroReproduccion] FOREIGN KEY([idRegistroReproduccion])
REFERENCES [dbo].[RegistroReproduccion] ([Id])
GO
ALTER TABLE [dbo].[RegistroNacimiento] CHECK CONSTRAINT [FK_RegistroNacimiento_RegistroReproduccion]
GO
ALTER TABLE [dbo].[RegistroProduccionLeche]  WITH CHECK ADD  CONSTRAINT [FK_RegistroProduccionLeche_Animal] FOREIGN KEY([idAnimal])
REFERENCES [dbo].[Animal] ([Id])
GO
ALTER TABLE [dbo].[RegistroProduccionLeche] CHECK CONSTRAINT [FK_RegistroProduccionLeche_Animal]
GO
ALTER TABLE [dbo].[RegistroReproduccion]  WITH CHECK ADD  CONSTRAINT [FK_RegistroReproduccion_Animal] FOREIGN KEY([idAnimal])
REFERENCES [dbo].[Animal] ([Id])
GO
ALTER TABLE [dbo].[RegistroReproduccion] CHECK CONSTRAINT [FK_RegistroReproduccion_Animal]
GO
ALTER TABLE [dbo].[RegistroSalida]  WITH CHECK ADD  CONSTRAINT [FK_RegistroSalida_Animal] FOREIGN KEY([idAnimal])
REFERENCES [dbo].[Animal] ([Id])
GO
ALTER TABLE [dbo].[RegistroSalida] CHECK CONSTRAINT [FK_RegistroSalida_Animal]
GO
ALTER TABLE [dbo].[ReporteIndustriaLeche]  WITH CHECK ADD  CONSTRAINT [FK_ReporteIndustriaLeche_Hato] FOREIGN KEY([idHato])
REFERENCES [dbo].[Hato] ([Id])
GO
ALTER TABLE [dbo].[ReporteIndustriaLeche] CHECK CONSTRAINT [FK_ReporteIndustriaLeche_Hato]
GO
ALTER TABLE [dbo].[RequerimientoNutricional]  WITH CHECK ADD  CONSTRAINT [FK_RequerimientoNutricional_Categoria] FOREIGN KEY([IdCategoriaAnimal])
REFERENCES [dbo].[CategoriaAnimal] ([IdCategoriaAnimal])
GO
ALTER TABLE [dbo].[RequerimientoNutricional] CHECK CONSTRAINT [FK_RequerimientoNutricional_Categoria]
GO
ALTER TABLE [dbo].[RequerimientoNutricional]  WITH CHECK ADD  CONSTRAINT [FK_RequerimientoNutricional_Nutriente] FOREIGN KEY([IdNutriente])
REFERENCES [dbo].[Nutriente] ([IdNutriente])
GO
ALTER TABLE [dbo].[RequerimientoNutricional] CHECK CONSTRAINT [FK_RequerimientoNutricional_Nutriente]
GO
ALTER TABLE [dbo].[RtmEntrega]  WITH CHECK ADD  CONSTRAINT [FK_RtmEntrega_Formula] FOREIGN KEY([formulaId])
REFERENCES [dbo].[RtmFormula] ([Id])
GO
ALTER TABLE [dbo].[RtmEntrega] CHECK CONSTRAINT [FK_RtmEntrega_Formula]
GO
ALTER TABLE [dbo].[RtmEntrega]  WITH CHECK ADD  CONSTRAINT [FK_RtmEntrega_Hato] FOREIGN KEY([hatoId])
REFERENCES [dbo].[Hato] ([Id])
GO
ALTER TABLE [dbo].[RtmEntrega] CHECK CONSTRAINT [FK_RtmEntrega_Hato]
GO
ALTER TABLE [dbo].[RtmEntrega]  WITH CHECK ADD  CONSTRAINT [FK_RtmEntrega_Usuario] FOREIGN KEY([idUsuario])
REFERENCES [dbo].[Usuario] ([Id])
GO
ALTER TABLE [dbo].[RtmEntrega] CHECK CONSTRAINT [FK_RtmEntrega_Usuario]
GO
ALTER TABLE [dbo].[RtmFormulaDetalle]  WITH CHECK ADD  CONSTRAINT [FK_RtmFormulaDetalle_Formula] FOREIGN KEY([formulaId])
REFERENCES [dbo].[RtmFormula] ([Id])
GO
ALTER TABLE [dbo].[RtmFormulaDetalle] CHECK CONSTRAINT [FK_RtmFormulaDetalle_Formula]
GO
ALTER TABLE [dbo].[RtmFormulaDetalle]  WITH CHECK ADD  CONSTRAINT [FK_RtmFormulaDetalle_Ingrediente] FOREIGN KEY([ingredienteId])
REFERENCES [dbo].[RtmIngrediente] ([Id])
GO
ALTER TABLE [dbo].[RtmFormulaDetalle] CHECK CONSTRAINT [FK_RtmFormulaDetalle_Ingrediente]
GO
ALTER TABLE [dbo].[RtmRacionCorral]  WITH CHECK ADD  CONSTRAINT [FK_RtmRacionCorral_Formula] FOREIGN KEY([formulaId])
REFERENCES [dbo].[RtmFormula] ([Id])
GO
ALTER TABLE [dbo].[RtmRacionCorral] CHECK CONSTRAINT [FK_RtmRacionCorral_Formula]
GO
ALTER TABLE [dbo].[RtmRacionCorral]  WITH CHECK ADD  CONSTRAINT [FK_RtmRacionCorral_Hato] FOREIGN KEY([hatoId])
REFERENCES [dbo].[Hato] ([Id])
GO
ALTER TABLE [dbo].[RtmRacionCorral] CHECK CONSTRAINT [FK_RtmRacionCorral_Hato]
GO
ALTER TABLE [dbo].[Seca]  WITH CHECK ADD  CONSTRAINT [FK_Seca_RegistroReproduccion] FOREIGN KEY([idRegistroReproduccion])
REFERENCES [dbo].[RegistroReproduccion] ([Id])
GO
ALTER TABLE [dbo].[Seca] CHECK CONSTRAINT [FK_Seca_RegistroReproduccion]
GO
ALTER TABLE [dbo].[Sintomas]  WITH CHECK ADD  CONSTRAINT [FK_Sintomas_TipoEnfermedades] FOREIGN KEY([idTipoEnfermedad])
REFERENCES [dbo].[TipoEnfermedades] ([Id])
GO
ALTER TABLE [dbo].[Sintomas] CHECK CONSTRAINT [FK_Sintomas_TipoEnfermedades]
GO
ALTER TABLE [dbo].[TipoAlimento]  WITH CHECK ADD  CONSTRAINT [FK_TipoAlimento_Animal] FOREIGN KEY([idAnimal])
REFERENCES [dbo].[Animal] ([Id])
GO
ALTER TABLE [dbo].[TipoAlimento] CHECK CONSTRAINT [FK_TipoAlimento_Animal]
GO
ALTER TABLE [dbo].[TipoTratamiento]  WITH CHECK ADD  CONSTRAINT [FK_TipoTratamiento_TipoEnfermedades] FOREIGN KEY([idTipoEnfermedad])
REFERENCES [dbo].[TipoEnfermedades] ([Id])
GO
ALTER TABLE [dbo].[TipoTratamiento] CHECK CONSTRAINT [FK_TipoTratamiento_TipoEnfermedades]
GO
ALTER TABLE [dbo].[Tratamiento]  WITH CHECK ADD  CONSTRAINT [FK_Tratamiento_Enfermedad] FOREIGN KEY([idEnfermedad])
REFERENCES [dbo].[Enfermedad] ([Id])
GO
ALTER TABLE [dbo].[Tratamiento] CHECK CONSTRAINT [FK_Tratamiento_Enfermedad]
GO
ALTER TABLE [dbo].[Tratamiento]  WITH CHECK ADD  CONSTRAINT [FK_Tratamiento_TipoTratamiento] FOREIGN KEY([idTipoTratamiento])
REFERENCES [dbo].[TipoTratamiento] ([Id])
GO
ALTER TABLE [dbo].[Tratamiento] CHECK CONSTRAINT [FK_Tratamiento_TipoTratamiento]
GO
ALTER TABLE [dbo].[Usuario]  WITH CHECK ADD  CONSTRAINT [FK_Usuario_Establo] FOREIGN KEY([idEstablo])
REFERENCES [dbo].[Establo] ([Id])
ON DELETE SET NULL
GO
ALTER TABLE [dbo].[Usuario] CHECK CONSTRAINT [FK_Usuario_Establo]
GO
ALTER TABLE [dbo].[Usuario]  WITH CHECK ADD  CONSTRAINT [FK_Usuario_Hato] FOREIGN KEY([idHato])
REFERENCES [dbo].[Hato] ([Id])
ON DELETE SET NULL
GO
ALTER TABLE [dbo].[Usuario] CHECK CONSTRAINT [FK_Usuario_Hato]
GO
ALTER TABLE [dbo].[Usuario]  WITH CHECK ADD  CONSTRAINT [FK_Usuario_Rol] FOREIGN KEY([RolId])
REFERENCES [dbo].[Rol] ([Id])
GO
ALTER TABLE [dbo].[Usuario] CHECK CONSTRAINT [FK_Usuario_Rol]
GO
ALTER TABLE [dbo].[CampaniaLechera]  WITH CHECK ADD  CONSTRAINT [CK_CampaniaLechera_Fechas] CHECK  (([fechaFin]>=[fechaInicio]))
GO
ALTER TABLE [dbo].[CampaniaLechera] CHECK CONSTRAINT [CK_CampaniaLechera_Fechas]
GO
ALTER TABLE [dbo].[Colaborador]  WITH CHECK ADD  CONSTRAINT [CK_Colaborador_DNI_SoloDigitos] CHECK  ((NOT [DNI] like '%[^0-9]%'))
GO
ALTER TABLE [dbo].[Colaborador] CHECK CONSTRAINT [CK_Colaborador_DNI_SoloDigitos]
GO
ALTER TABLE [dbo].[ConfirmacionPrenez]  WITH CHECK ADD  CONSTRAINT [CK_ConfirmacionPrenez_Tipo] CHECK  (([tipo]='NEGATIVA' OR [tipo]='POSITIVA'))
GO
ALTER TABLE [dbo].[ConfirmacionPrenez] CHECK CONSTRAINT [CK_ConfirmacionPrenez_Tipo]
GO
ALTER TABLE [dbo].[Empresa]  WITH CHECK ADD  CONSTRAINT [CK_Empresa_Ruc_SoloDigitos] CHECK  ((NOT [ruc] like '%[^0-9]%'))
GO
ALTER TABLE [dbo].[Empresa] CHECK CONSTRAINT [CK_Empresa_Ruc_SoloDigitos]
GO
USE [master]
GO
ALTER DATABASE [ZootecPro] SET  READ_WRITE 
GO
