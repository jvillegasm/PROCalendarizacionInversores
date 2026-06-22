-- ============================================================
-- Schema.sql
-- Sistema de Calendarización de Inversores · PROCOMER-CALEND-2026
-- Compatible: SQL Server 2019+ / Azure SQL Database
-- Fuente de verdad: Código fuente EF Core
--   · Catalogo.Infrastructure.Persistence.CatalogoDbContext
--   · Agendas.Infrastructure.Persistence.AgendasDbContext
-- Generado: 2026-06-22
-- ============================================================
--
-- Mapeos EF Core → SQL Server utilizados:
--   C# Guid          → UNIQUEIDENTIFIER
--   C# string        → NVARCHAR(n)
--   C# DateTime      → DATETIME2        (EF Core default para DateTime)
--   C# DateTime?     → DATETIME2 NULL
--   C# TimeSpan      → TIME             (EF Core default para TimeSpan)
--   C# bool          → BIT
--   C# int           → INT
--
-- Comportamiento de cascada según Fluent API:
--   OnDelete(DeleteBehavior.Cascade)  → ON DELETE CASCADE
--   OnDelete(DeleteBehavior.Restrict) → ON DELETE NO ACTION
--
-- INSTRUCCIONES:
--   1. Ejecutar sobre la base de datos de destino (vacía o existente).
--   2. El script es idempotente: usa IF NOT EXISTS en todas las tablas.
--   3. El bloque CATCH hace ROLLBACK automático ante cualquier error.
-- ============================================================

SET NOCOUNT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

BEGIN TRY
    BEGIN TRANSACTION;

    PRINT '══════════════════════════════════════════════════════════';
    PRINT '  Creando schema PROCalendarizacionInversores...';
    PRINT '══════════════════════════════════════════════════════════';

    -- ────────────────────────────────────────────────────────────
    -- 1. Idiomas
    --    Catálogo de idiomas disponibles en el sistema.
    --    Fuente: CatalogoDbContext → modelBuilder.Entity<Idioma>
    --      e.ToTable("Idiomas")
    --      e.Property(x => x.Nombre).IsRequired().HasMaxLength(100)
    --      e.Property(x => x.Codigo).IsRequired().HasMaxLength(10)
    -- ────────────────────────────────────────────────────────────
    IF NOT EXISTS (
        SELECT 1 FROM sys.tables
        WHERE name = 'Idiomas' AND schema_id = SCHEMA_ID('dbo')
    )
    BEGIN
        CREATE TABLE dbo.Idiomas (
            Id      UNIQUEIDENTIFIER    NOT NULL,   -- PK
            Nombre  NVARCHAR(100)       NOT NULL,   -- HasMaxLength(100), IsRequired
            Codigo  NVARCHAR(10)        NOT NULL,   -- HasMaxLength(10),  IsRequired

            CONSTRAINT PK_Idiomas PRIMARY KEY CLUSTERED (Id)
        );
        PRINT '  [OK] Tabla Idiomas creada.';
    END
    ELSE
        PRINT '  [--] Tabla Idiomas ya existe.';

    -- ────────────────────────────────────────────────────────────
    -- 2. Inversores
    --    Inversores extranjeros que visitan Costa Rica.
    --    Fuente: CatalogoDbContext → modelBuilder.Entity<Inversor>
    --      e.Property(x => x.NombreCompleto).IsRequired().HasMaxLength(200)
    --      e.Property(x => x.Empresa).IsRequired().HasMaxLength(200)
    --      e.Property(x => x.PaisOrigen).IsRequired().HasMaxLength(100)
    --      e.Property(x => x.LugarHospedaje).IsRequired().HasMaxLength(300)
    --      e.Property(x => x.FechaInicioVisita).IsRequired()
    --      e.Property(x => x.FechaFinVisita).IsRequired()
    --    CK_Inversores_Fechas refuerza RN-02 (FechaFin >= FechaInicio)
    -- ────────────────────────────────────────────────────────────
    IF NOT EXISTS (
        SELECT 1 FROM sys.tables
        WHERE name = 'Inversores' AND schema_id = SCHEMA_ID('dbo')
    )
    BEGIN
        CREATE TABLE dbo.Inversores (
            Id                  UNIQUEIDENTIFIER    NOT NULL,
            NombreCompleto      NVARCHAR(200)       NOT NULL,   -- HasMaxLength(200)
            Empresa             NVARCHAR(200)       NOT NULL,   -- HasMaxLength(200)
            PaisOrigen          NVARCHAR(100)       NOT NULL,   -- HasMaxLength(100)
            LugarHospedaje      NVARCHAR(300)       NOT NULL,   -- HasMaxLength(300)
            FechaInicioVisita   DATETIME2           NOT NULL,   -- IsRequired
            FechaFinVisita      DATETIME2           NOT NULL,   -- IsRequired; RN-02

            CONSTRAINT PK_Inversores        PRIMARY KEY CLUSTERED (Id),
            CONSTRAINT CK_Inversores_Fechas CHECK (FechaFinVisita >= FechaInicioVisita)
        );
        PRINT '  [OK] Tabla Inversores creada.';
    END
    ELSE
        PRINT '  [--] Tabla Inversores ya existe.';

    -- ────────────────────────────────────────────────────────────
    -- 3. Oficinas
    --    Oficinas físicas donde se realizan las reuniones.
    --    Fuente: CatalogoDbContext → modelBuilder.Entity<Oficina>
    --      e.Property(x => x.Nombre).IsRequired().HasMaxLength(200)
    --      e.Property(x => x.DireccionFisica).IsRequired().HasMaxLength(400)
    --      e.Property(x => x.Coordenadas).HasMaxLength(100)  ← nullable
    -- ────────────────────────────────────────────────────────────
    IF NOT EXISTS (
        SELECT 1 FROM sys.tables
        WHERE name = 'Oficinas' AND schema_id = SCHEMA_ID('dbo')
    )
    BEGIN
        CREATE TABLE dbo.Oficinas (
            Id              UNIQUEIDENTIFIER    NOT NULL,
            Nombre          NVARCHAR(200)       NOT NULL,   -- HasMaxLength(200)
            DireccionFisica NVARCHAR(400)       NOT NULL,   -- HasMaxLength(400)
            Coordenadas     NVARCHAR(100)       NULL,       -- HasMaxLength(100); opcional (string?)

            CONSTRAINT PK_Oficinas PRIMARY KEY CLUSTERED (Id)
        );
        PRINT '  [OK] Tabla Oficinas creada.';
    END
    ELSE
        PRINT '  [--] Tabla Oficinas ya existe.';

    -- ────────────────────────────────────────────────────────────
    -- 4. Participantes
    --    Funcionarios y aliados convocables a reuniones.
    --    Fuente: CatalogoDbContext → modelBuilder.Entity<Participante>
    --      e.Property(x => x.NombreCompleto).IsRequired().HasMaxLength(200)
    --      e.Property(x => x.Cargo).IsRequired().HasMaxLength(200)
    --      e.Property(x => x.Activo).IsRequired()
    --      e.HasOne(x => x.Oficina)...OnDelete(DeleteBehavior.Restrict)
    --    DF_Participantes_Activo proviene de: public bool Activo { get; set; } = true
    -- ────────────────────────────────────────────────────────────
    IF NOT EXISTS (
        SELECT 1 FROM sys.tables
        WHERE name = 'Participantes' AND schema_id = SCHEMA_ID('dbo')
    )
    BEGIN
        CREATE TABLE dbo.Participantes (
            Id              UNIQUEIDENTIFIER    NOT NULL,
            NombreCompleto  NVARCHAR(200)       NOT NULL,   -- HasMaxLength(200)
            Cargo           NVARCHAR(200)       NOT NULL,   -- HasMaxLength(200)
            OficinaId       UNIQUEIDENTIFIER    NOT NULL,   -- FK → Oficinas
            Activo          BIT                 NOT NULL    -- IsRequired; C# default = true
                            CONSTRAINT DF_Participantes_Activo DEFAULT (1),

            CONSTRAINT PK_Participantes PRIMARY KEY CLUSTERED (Id),
            CONSTRAINT FK_Participantes_Oficinas
                FOREIGN KEY (OficinaId) REFERENCES dbo.Oficinas (Id)
                ON DELETE NO ACTION     -- OnDelete(DeleteBehavior.Restrict)
                ON UPDATE NO ACTION
        );
        PRINT '  [OK] Tabla Participantes creada.';
    END
    ELSE
        PRINT '  [--] Tabla Participantes ya existe.';

    -- ────────────────────────────────────────────────────────────
    -- 5. InversoresIdiomas  (tabla de unión N:M)
    --    Fuente: CatalogoDbContext → modelBuilder.Entity<InversorIdioma>
    --      e.HasKey(x => new { x.InversorId, x.IdiomaId })  ← PK compuesta
    --      InversorId → ON DELETE CASCADE
    --      IdiomaId   → ON DELETE NO ACTION (Restrict)
    --    RN-01: al menos un registro por inversor (reforzado en Application)
    -- ────────────────────────────────────────────────────────────
    IF NOT EXISTS (
        SELECT 1 FROM sys.tables
        WHERE name = 'InversoresIdiomas' AND schema_id = SCHEMA_ID('dbo')
    )
    BEGIN
        CREATE TABLE dbo.InversoresIdiomas (
            InversorId  UNIQUEIDENTIFIER    NOT NULL,   -- FK → Inversores; PK
            IdiomaId    UNIQUEIDENTIFIER    NOT NULL,   -- FK → Idiomas;    PK

            CONSTRAINT PK_InversoresIdiomas PRIMARY KEY CLUSTERED (InversorId, IdiomaId),
            CONSTRAINT FK_InversoresIdiomas_Inversores
                FOREIGN KEY (InversorId) REFERENCES dbo.Inversores (Id)
                ON DELETE CASCADE       -- OnDelete(DeleteBehavior.Cascade)
                ON UPDATE NO ACTION,
            CONSTRAINT FK_InversoresIdiomas_Idiomas
                FOREIGN KEY (IdiomaId) REFERENCES dbo.Idiomas (Id)
                ON DELETE NO ACTION     -- OnDelete(DeleteBehavior.Restrict)
                ON UPDATE NO ACTION
        );
        PRINT '  [OK] Tabla InversoresIdiomas creada.';
    END
    ELSE
        PRINT '  [--] Tabla InversoresIdiomas ya existe.';

    -- ────────────────────────────────────────────────────────────
    -- 6. ParticipantesIdiomas  (tabla de unión N:M)
    --    Fuente: CatalogoDbContext → modelBuilder.Entity<ParticipanteIdioma>
    --      e.HasKey(x => new { x.ParticipanteId, x.IdiomaId })  ← PK compuesta
    --      ParticipanteId → ON DELETE CASCADE
    --      IdiomaId       → ON DELETE NO ACTION (Restrict)
    --    RN-04: al menos un registro por participante (reforzado en Application)
    -- ────────────────────────────────────────────────────────────
    IF NOT EXISTS (
        SELECT 1 FROM sys.tables
        WHERE name = 'ParticipantesIdiomas' AND schema_id = SCHEMA_ID('dbo')
    )
    BEGIN
        CREATE TABLE dbo.ParticipantesIdiomas (
            ParticipanteId  UNIQUEIDENTIFIER    NOT NULL,   -- FK → Participantes; PK
            IdiomaId        UNIQUEIDENTIFIER    NOT NULL,   -- FK → Idiomas;        PK

            CONSTRAINT PK_ParticipantesIdiomas PRIMARY KEY CLUSTERED (ParticipanteId, IdiomaId),
            CONSTRAINT FK_ParticipantesIdiomas_Participantes
                FOREIGN KEY (ParticipanteId) REFERENCES dbo.Participantes (Id)
                ON DELETE CASCADE       -- OnDelete(DeleteBehavior.Cascade)
                ON UPDATE NO ACTION,
            CONSTRAINT FK_ParticipantesIdiomas_Idiomas
                FOREIGN KEY (IdiomaId) REFERENCES dbo.Idiomas (Id)
                ON DELETE NO ACTION     -- OnDelete(DeleteBehavior.Restrict)
                ON UPDATE NO ACTION
        );
        PRINT '  [OK] Tabla ParticipantesIdiomas creada.';
    END
    ELSE
        PRINT '  [--] Tabla ParticipantesIdiomas ya existe.';

    -- ────────────────────────────────────────────────────────────
    -- 7. DisponibilidadParticipantes
    --    Bloques horarios de disponibilidad por participante y fecha.
    --    Fuente: CatalogoDbContext → modelBuilder.Entity<DisponibilidadParticipante>
    --      e.Property(x => x.Fecha).IsRequired()
    --      e.Property(x => x.HoraInicio).IsRequired()  ← TimeSpan → TIME
    --      e.Property(x => x.HoraFin).IsRequired()     ← TimeSpan → TIME
    --      ParticipanteId → ON DELETE CASCADE
    -- ────────────────────────────────────────────────────────────
    IF NOT EXISTS (
        SELECT 1 FROM sys.tables
        WHERE name = 'DisponibilidadParticipantes' AND schema_id = SCHEMA_ID('dbo')
    )
    BEGIN
        CREATE TABLE dbo.DisponibilidadParticipantes (
            Id              UNIQUEIDENTIFIER    NOT NULL,
            ParticipanteId  UNIQUEIDENTIFIER    NOT NULL,   -- FK → Participantes
            Fecha           DATETIME2           NOT NULL,   -- IsRequired; AvailabilitySlotBuilder usa .Date
            HoraInicio      TIME                NOT NULL,   -- TimeSpan → TIME; RN-09 >= 08:00
            HoraFin         TIME                NOT NULL,   -- TimeSpan → TIME; RN-10 <= 17:00

            CONSTRAINT PK_DisponibilidadParticipantes PRIMARY KEY CLUSTERED (Id),
            CONSTRAINT FK_DisponibilidadParticipantes_Participantes
                FOREIGN KEY (ParticipanteId) REFERENCES dbo.Participantes (Id)
                ON DELETE CASCADE       -- OnDelete(DeleteBehavior.Cascade)
                ON UPDATE NO ACTION
        );
        PRINT '  [OK] Tabla DisponibilidadParticipantes creada.';
    END
    ELSE
        PRINT '  [--] Tabla DisponibilidadParticipantes ya existe.';

    -- ────────────────────────────────────────────────────────────
    -- 8. MatrizTraslados
    --    Tiempos de desplazamiento entre cada par de oficinas.
    --    Fuente: CatalogoDbContext → modelBuilder.Entity<MatrizTraslado>
    --      e.Property(x => x.TiempoMinutos).IsRequired()
    --      OficinaOrigenId  → ON DELETE NO ACTION (Restrict)
    --      OficinaDestinoId → ON DELETE NO ACTION (Restrict)
    --    RN-07: simetría A→B = B→A garantizada por Application
    --    UQ_MatrizTraslados_Par: un par A→B solo puede existir una vez
    --    CK_MatrizTraslados_TiempoMinutos: valor positivo
    --    CK_MatrizTraslados_DistintasOficinas: origen ≠ destino
    -- ────────────────────────────────────────────────────────────
    IF NOT EXISTS (
        SELECT 1 FROM sys.tables
        WHERE name = 'MatrizTraslados' AND schema_id = SCHEMA_ID('dbo')
    )
    BEGIN
        CREATE TABLE dbo.MatrizTraslados (
            Id                  UNIQUEIDENTIFIER    NOT NULL,
            OficinaOrigenId     UNIQUEIDENTIFIER    NOT NULL,   -- FK → Oficinas
            OficinaDestinoId    UNIQUEIDENTIFIER    NOT NULL,   -- FK → Oficinas
            TiempoMinutos       INT                 NOT NULL,   -- IsRequired

            CONSTRAINT PK_MatrizTraslados               PRIMARY KEY CLUSTERED (Id),
            CONSTRAINT UQ_MatrizTraslados_Par            UNIQUE (OficinaOrigenId, OficinaDestinoId),
            CONSTRAINT CK_MatrizTraslados_TiempoMinutos  CHECK (TiempoMinutos > 0),
            CONSTRAINT CK_MatrizTraslados_DistintasPar   CHECK (OficinaOrigenId <> OficinaDestinoId),
            CONSTRAINT FK_MatrizTraslados_OficinaOrigen
                FOREIGN KEY (OficinaOrigenId) REFERENCES dbo.Oficinas (Id)
                ON DELETE NO ACTION     -- OnDelete(DeleteBehavior.Restrict)
                ON UPDATE NO ACTION,
            CONSTRAINT FK_MatrizTraslados_OficinaDestino
                FOREIGN KEY (OficinaDestinoId) REFERENCES dbo.Oficinas (Id)
                ON DELETE NO ACTION     -- OnDelete(DeleteBehavior.Restrict)
                ON UPDATE NO ACTION
        );
        PRINT '  [OK] Tabla MatrizTraslados creada.';
    END
    ELSE
        PRINT '  [--] Tabla MatrizTraslados ya existe.';

    -- ────────────────────────────────────────────────────────────
    -- 9. Agendas
    --    Agendas diarias generadas para un inversor.
    --    Fuente: AgendasDbContext → modelBuilder.Entity<Agenda>
    --      Estado almacenado como NVARCHAR(20) via HasConversion(enum→string)
    --      InversorId: referencia cruzada (sin FK entre Agendas y Inversores,
    --        los servicios son independientes y comparten la misma BD)
    --    DF_Agendas_Estado: C# default = EstadoAgenda.Activa
    --    CK_Agendas_Estado: solo acepta los valores del enum EstadoAgenda
    --    RN-15: FechaAnulacion se registra en la anulación lógica
    -- ────────────────────────────────────────────────────────────
    IF NOT EXISTS (
        SELECT 1 FROM sys.tables
        WHERE name = 'Agendas' AND schema_id = SCHEMA_ID('dbo')
    )
    BEGIN
        CREATE TABLE dbo.Agendas (
            Id              UNIQUEIDENTIFIER    NOT NULL,
            InversorId      UNIQUEIDENTIFIER    NOT NULL,   -- cross-service ref; sin FK explícita
            Fecha           DATETIME2           NOT NULL,   -- IsRequired; jornada de la agenda
            Estado          NVARCHAR(20)        NOT NULL    -- HasConversion(EstadoAgenda→string)
                            CONSTRAINT DF_Agendas_Estado DEFAULT (N'Activa'),
            FechaGeneracion DATETIME2           NOT NULL,   -- IsRequired
            FechaAnulacion  DATETIME2           NULL,       -- nullable; se asigna al anular (RN-15)

            CONSTRAINT PK_Agendas       PRIMARY KEY CLUSTERED (Id),
            CONSTRAINT CK_Agendas_Estado CHECK (Estado IN (N'Activa', N'Anulada'))
        );
        PRINT '  [OK] Tabla Agendas creada.';
    END
    ELSE
        PRINT '  [--] Tabla Agendas ya existe.';

    -- ────────────────────────────────────────────────────────────
    -- 10. Reuniones
    --     Reuniones individuales que conforman una agenda.
    --     Fuente: AgendasDbContext → modelBuilder.Entity<Reunion>
    --       e.Property(x => x.IdiomaReunion).IsRequired().HasMaxLength(100)
    --       e.Property(x => x.Orden).IsRequired()
    --       e.Property(x => x.TiempoTrasladoSiguiente).IsRequired()
    --       AgendaId   → ON DELETE CASCADE
    --       ParticipanteId, OficinaId: referencias cruzadas sin FK explícita
    --     DF_Reuniones_TiempoTraslado: C# default int = 0 (última reunión del día)
    -- ────────────────────────────────────────────────────────────
    IF NOT EXISTS (
        SELECT 1 FROM sys.tables
        WHERE name = 'Reuniones' AND schema_id = SCHEMA_ID('dbo')
    )
    BEGIN
        CREATE TABLE dbo.Reuniones (
            Id                      UNIQUEIDENTIFIER    NOT NULL,
            AgendaId                UNIQUEIDENTIFIER    NOT NULL,   -- FK → Agendas
            ParticipanteId          UNIQUEIDENTIFIER    NOT NULL,   -- cross-service ref
            HoraInicio              TIME                NOT NULL,   -- TimeSpan; RN-09 >= 08:00
            HoraFin                 TIME                NOT NULL,   -- TimeSpan; RN-10 <= 17:00
            OficinaId               UNIQUEIDENTIFIER    NOT NULL,   -- cross-service ref
            IdiomaReunion           NVARCHAR(100)       NOT NULL,   -- HasMaxLength(100)
            Orden                   INT                 NOT NULL,   -- posición en el itinerario (≥ 1)
            TiempoTrasladoSiguiente INT                 NOT NULL    -- minutos; 0 = última reunión
                                    CONSTRAINT DF_Reuniones_TiempoTraslado DEFAULT (0),

            CONSTRAINT PK_Reuniones PRIMARY KEY CLUSTERED (Id),
            CONSTRAINT FK_Reuniones_Agendas
                FOREIGN KEY (AgendaId) REFERENCES dbo.Agendas (Id)
                ON DELETE CASCADE       -- OnDelete(DeleteBehavior.Cascade)
                ON UPDATE NO ACTION
        );
        PRINT '  [OK] Tabla Reuniones creada.';
    END
    ELSE
        PRINT '  [--] Tabla Reuniones ya existe.';

    -- ══════════════════════════════════════════════════════════════
    -- ÍNDICES
    -- Derivados de los patrones de consulta de los repositorios EF Core
    -- ══════════════════════════════════════════════════════════════

    -- IX_Participantes_Activo
    --   ParticipanteRepository.GetActivosAsync(): .Where(p => p.Activo)
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Participantes_Activo'
                   AND object_id = OBJECT_ID('dbo.Participantes'))
    BEGIN
        CREATE NONCLUSTERED INDEX IX_Participantes_Activo
            ON dbo.Participantes (Activo)
            INCLUDE (NombreCompleto, Cargo, OficinaId);
        PRINT '  [OK] Indice IX_Participantes_Activo creado.';
    END

    -- IX_Participantes_OficinaId_Activo
    --   ParticipanteRepository.CountActivosByOficinaAsync(): .Where(p => p.OficinaId == … && p.Activo)
    --   Usada también por OficinaRepository para validar RN-06
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Participantes_OficinaId_Activo'
                   AND object_id = OBJECT_ID('dbo.Participantes'))
    BEGIN
        CREATE NONCLUSTERED INDEX IX_Participantes_OficinaId_Activo
            ON dbo.Participantes (OficinaId, Activo);
        PRINT '  [OK] Indice IX_Participantes_OficinaId_Activo creado.';
    END

    -- IX_InversoresIdiomas_IdiomaId
    --   Lookup inverso Idioma → Inversores (ThenInclude en InversorRepository)
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_InversoresIdiomas_IdiomaId'
                   AND object_id = OBJECT_ID('dbo.InversoresIdiomas'))
    BEGIN
        CREATE NONCLUSTERED INDEX IX_InversoresIdiomas_IdiomaId
            ON dbo.InversoresIdiomas (IdiomaId);
        PRINT '  [OK] Indice IX_InversoresIdiomas_IdiomaId creado.';
    END

    -- IX_ParticipantesIdiomas_IdiomaId
    --   Lookup inverso Idioma → Participantes (ThenInclude en ParticipanteRepository)
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ParticipantesIdiomas_IdiomaId'
                   AND object_id = OBJECT_ID('dbo.ParticipantesIdiomas'))
    BEGIN
        CREATE NONCLUSTERED INDEX IX_ParticipantesIdiomas_IdiomaId
            ON dbo.ParticipantesIdiomas (IdiomaId);
        PRINT '  [OK] Indice IX_ParticipantesIdiomas_IdiomaId creado.';
    END

    -- IX_DisponibilidadParticipantes_ParticipanteId_Fecha
    --   AvailabilitySlotBuilder: filtra por participante y fecha exacta
    --   Consulta: d.Fecha.Date == fecha.Date
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_DisponibilidadParticipantes_ParticipanteId_Fecha'
                   AND object_id = OBJECT_ID('dbo.DisponibilidadParticipantes'))
    BEGIN
        CREATE NONCLUSTERED INDEX IX_DisponibilidadParticipantes_ParticipanteId_Fecha
            ON dbo.DisponibilidadParticipantes (ParticipanteId, Fecha)
            INCLUDE (HoraInicio, HoraFin);
        PRINT '  [OK] Indice IX_DisponibilidadParticipantes_ParticipanteId_Fecha creado.';
    END

    -- IX_Agendas_InversorId
    --   AgendaRepository.GetAllAsync(): .Where(a => a.InversorId == filtros.InversorId)
    --   Incluye FechaGeneracion para ORDER BY sin lookup al heap
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Agendas_InversorId'
                   AND object_id = OBJECT_ID('dbo.Agendas'))
    BEGIN
        CREATE NONCLUSTERED INDEX IX_Agendas_InversorId
            ON dbo.Agendas (InversorId)
            INCLUDE (Fecha, Estado, FechaGeneracion, FechaAnulacion);
        PRINT '  [OK] Indice IX_Agendas_InversorId creado.';
    END

    -- IX_Agendas_Fecha
    --   AgendaRepository.GetAllAsync(): .Where(a => a.Fecha.Date == filtros.Fecha)
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Agendas_Fecha'
                   AND object_id = OBJECT_ID('dbo.Agendas'))
    BEGIN
        CREATE NONCLUSTERED INDEX IX_Agendas_Fecha
            ON dbo.Agendas (Fecha)
            INCLUDE (InversorId, Estado, FechaGeneracion);
        PRINT '  [OK] Indice IX_Agendas_Fecha creado.';
    END

    -- IX_Agendas_FechaGeneracion
    --   AgendaRepository.GetAllAsync(): .OrderByDescending(a => a.FechaGeneracion)
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Agendas_FechaGeneracion'
                   AND object_id = OBJECT_ID('dbo.Agendas'))
    BEGIN
        CREATE NONCLUSTERED INDEX IX_Agendas_FechaGeneracion
            ON dbo.Agendas (FechaGeneracion DESC)
            INCLUDE (InversorId, Fecha, Estado);
        PRINT '  [OK] Indice IX_Agendas_FechaGeneracion creado.';
    END

    -- IX_Reuniones_AgendaId
    --   AgendaRepository: .Include(a => a.Reuniones) en GetAllAsync y GetByIdAsync
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Reuniones_AgendaId'
                   AND object_id = OBJECT_ID('dbo.Reuniones'))
    BEGIN
        CREATE NONCLUSTERED INDEX IX_Reuniones_AgendaId
            ON dbo.Reuniones (AgendaId, Orden)
            INCLUDE (ParticipanteId, HoraInicio, HoraFin, OficinaId,
                     IdiomaReunion, TiempoTrasladoSiguiente);
        PRINT '  [OK] Indice IX_Reuniones_AgendaId creado.';
    END

    COMMIT TRANSACTION;

    PRINT '';
    PRINT '══════════════════════════════════════════════════════════';
    PRINT '  Schema creado exitosamente.';
    PRINT '  Tablas: 10  |  Indices: 8';
    PRINT '══════════════════════════════════════════════════════════';

END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

    PRINT '';
    PRINT '██ ERROR: Schema no creado. Transacción revertida. ██';
    PRINT 'Mensaje : ' + ERROR_MESSAGE();
    PRINT 'Numero  : ' + CAST(ERROR_NUMBER()   AS NVARCHAR(10));
    PRINT 'Linea   : ' + CAST(ERROR_LINE()     AS NVARCHAR(10));
    PRINT 'Proc    : ' + ISNULL(ERROR_PROCEDURE(), 'N/A');
    THROW;
END CATCH;
GO
