-- ============================================================
-- SeedData.sql
-- Sistema de Calendarización de Inversores · PROCOMER-CALEND-2026
-- Compatible: SQL Server 2019+ / Azure SQL Database
-- Fuente de verdad: Código fuente EF Core (entidades + dominio)
-- Generado: 2026-06-22
-- ============================================================
--
-- NOTA: El código no define HasData() ni configuraciones de datos
--   semilla en los DbContexts. Estos registros representan el
--   dataset operacional mínimo inferido del modelo de dominio
--   y del SPEC técnico para que el sistema sea funcional tras
--   la instalación.
--
-- Contenido:
--   1.  Idiomas           (5  registros) — catálogo de idiomas del sistema
--   2.  Oficinas          (5  registros) — oficinas reales PROCOMER/aliadas
--   3.  Inversores        (5  registros) — inversores demo con distintos escenarios
--   4.  InversoresIdiomas (9  registros) — asignaciones de idioma por inversor
--   5.  Participantes     (8  registros) — funcionarios y aliados
--   6.  ParticipantesIdiomas (16 registros) — idiomas por participante
--   7.  DisponibilidadParticipantes (119 registros) — bloques horarios por fecha
--   8.  MatrizTraslados   (20 registros) — 10 pares × 2 sentidos (simétrico)
--
-- Escenarios cubiertos por los datos:
--   · Generación exitosa (James Wilson: Esp+Eng, 5 candidatos compatibles)
--   · Agenda parcial (meta no alcanzada por disponibilidad insuficiente)
--   · Idioma incompatible (Hans Müller: solo Mandarín; ningún participante lo habla)
--   · Participante inactivo (Diego Alfaro Mora) excluido del scheduling
--   · Simetría de traslados (RN-07) ya cargada en ambas direcciones
--
-- INSTRUCCIONES:
--   1. Ejecutar Schema.sql primero.
--   2. Ejecutar este script sobre la misma base de datos.
--   3. El script es idempotente: usa IF NOT EXISTS / MERGE para evitar duplicados.
--   4. El bloque CATCH hace ROLLBACK automático ante cualquier error.
-- ============================================================
--
-- GUIDs deterministas usados (facilita referencias cruzadas y depuración):
--   Idiomas      : 00000001-0000-0000-0000-00000000000X
--   Oficinas     : 00000002-0000-0000-0000-00000000000X
--   Inversores   : 00000003-0000-0000-0000-00000000000X
--   Participantes: 00000004-0000-0000-0000-00000000000X
--   MatrizTraslados: 00000005-00XY-0000-0000-000000000000
--     donde X = oficina origen (1-5), Y = oficina destino (1-5)
-- ============================================================

SET NOCOUNT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

BEGIN TRY
    BEGIN TRANSACTION;

    PRINT '══════════════════════════════════════════════════════════';
    PRINT '  Insertando datos semilla PROCalendarizacionInversores...';
    PRINT '══════════════════════════════════════════════════════════';

    -- ────────────────────────────────────────────────────────────
    -- 1. Idiomas
    --    Fuente: Entidad Idioma — Nombre(100) + Codigo(10)
    --    Los idiomas cubren los perfiles de los 5 inversores demo.
    --    NOTA: Hans Müller habla solo Mandarín; ningún participante
    --    lo habla → escenario IDIOMA_INCOMPATIBLE (RN-12 / AC-05).
    -- ────────────────────────────────────────────────────────────
    IF NOT EXISTS (SELECT 1 FROM dbo.Idiomas WHERE Id = '00000001-0000-0000-0000-000000000001')
        INSERT INTO dbo.Idiomas (Id, Nombre, Codigo)
        VALUES ('00000001-0000-0000-0000-000000000001', N'Español',   N'ES');

    IF NOT EXISTS (SELECT 1 FROM dbo.Idiomas WHERE Id = '00000001-0000-0000-0000-000000000002')
        INSERT INTO dbo.Idiomas (Id, Nombre, Codigo)
        VALUES ('00000001-0000-0000-0000-000000000002', N'Inglés',    N'EN');

    IF NOT EXISTS (SELECT 1 FROM dbo.Idiomas WHERE Id = '00000001-0000-0000-0000-000000000003')
        INSERT INTO dbo.Idiomas (Id, Nombre, Codigo)
        VALUES ('00000001-0000-0000-0000-000000000003', N'Portugués', N'PT');

    IF NOT EXISTS (SELECT 1 FROM dbo.Idiomas WHERE Id = '00000001-0000-0000-0000-000000000004')
        INSERT INTO dbo.Idiomas (Id, Nombre, Codigo)
        VALUES ('00000001-0000-0000-0000-000000000004', N'Francés',   N'FR');

    IF NOT EXISTS (SELECT 1 FROM dbo.Idiomas WHERE Id = '00000001-0000-0000-0000-000000000005')
        INSERT INTO dbo.Idiomas (Id, Nombre, Codigo)
        VALUES ('00000001-0000-0000-0000-000000000005', N'Mandarín',  N'ZH');

    PRINT '  [OK] Idiomas (5)';

    -- ────────────────────────────────────────────────────────────
    -- 2. Oficinas
    --    Fuente: Entidad Oficina — Nombre(200) + DireccionFisica(400) + Coordenadas(100)?
    --    Coordenadas en formato "latitud,longitud" (San José, Costa Rica).
    -- ────────────────────────────────────────────────────────────
    IF NOT EXISTS (SELECT 1 FROM dbo.Oficinas WHERE Id = '00000002-0000-0000-0000-000000000001')
        INSERT INTO dbo.Oficinas (Id, Nombre, DireccionFisica, Coordenadas)
        VALUES (
            '00000002-0000-0000-0000-000000000001',
            N'PROCOMER Central',
            N'Edificio Centro Colón, piso 14, Paseo Colón, San José, Costa Rica',
            N'9.9355,-84.0893'
        );

    IF NOT EXISTS (SELECT 1 FROM dbo.Oficinas WHERE Id = '00000002-0000-0000-0000-000000000002')
        INSERT INTO dbo.Oficinas (Id, Nombre, DireccionFisica, Coordenadas)
        VALUES (
            '00000002-0000-0000-0000-000000000002',
            N'CINDE — Coalición Costarricense de Iniciativas de Desarrollo',
            N'Edificio Torre Mercedes, piso 3, Paseo Colón, San José, Costa Rica',
            N'9.9361,-84.0881'
        );

    IF NOT EXISTS (SELECT 1 FROM dbo.Oficinas WHERE Id = '00000002-0000-0000-0000-000000000003')
        INSERT INTO dbo.Oficinas (Id, Nombre, DireccionFisica, Coordenadas)
        VALUES (
            '00000002-0000-0000-0000-000000000003',
            N'Ministerio de Hacienda',
            N'Avenida 2, entre calles 1 y 3, Barrio Aranjuez, San José, Costa Rica',
            N'9.9375,-84.0772'
        );

    IF NOT EXISTS (SELECT 1 FROM dbo.Oficinas WHERE Id = '00000002-0000-0000-0000-000000000004')
        INSERT INTO dbo.Oficinas (Id, Nombre, DireccionFisica, Coordenadas)
        VALUES (
            '00000002-0000-0000-0000-000000000004',
            N'Banco Central de Costa Rica',
            N'Avenida Central, calle 2, Barrio Amón, San José, Costa Rica',
            N'9.9343,-84.0791'
        );

    IF NOT EXISTS (SELECT 1 FROM dbo.Oficinas WHERE Id = '00000002-0000-0000-0000-000000000005')
        INSERT INTO dbo.Oficinas (Id, Nombre, DireccionFisica, Coordenadas)
        VALUES (
            '00000002-0000-0000-0000-000000000005',
            N'COMEX — Ministerio de Comercio Exterior',
            N'Edificio Expansión, La Sabana Norte, San José, Costa Rica',
            N'9.9398,-84.1022'
        );

    PRINT '  [OK] Oficinas (5)';

    -- ────────────────────────────────────────────────────────────
    -- 3. Inversores
    --    Fuente: Entidad Inversor
    --      NombreCompleto(200) Empresa(200) PaisOrigen(100)
    --      LugarHospedaje(300) FechaInicioVisita FechaFinVisita
    --    CK_Inversores_Fechas: FechaFin >= FechaInicio (RN-02)
    --
    --    ESCENARIOS:
    --    · Wilson   → habla Esp+Eng → compatible con mayoría de participantes
    --    · Müller   → habla SOLO Mandarín → IDIOMA_INCOMPATIBLE (RN-12)
    --    · Dupont   → habla Fra+Eng → compatible con Carolina y Verónica
    --    · Martínez → habla Esp+Por → compatible con Andrés y Laura
    --    · Tanaka   → habla Eng+Esp → compatible con mayoría
    -- ────────────────────────────────────────────────────────────

    -- Inversor 1: James Robert Wilson (EE.UU.) — visita 2026-07-01 a 2026-07-05
    IF NOT EXISTS (SELECT 1 FROM dbo.Inversores WHERE Id = '00000003-0000-0000-0000-000000000001')
        INSERT INTO dbo.Inversores
            (Id, NombreCompleto, Empresa, PaisOrigen, LugarHospedaje, FechaInicioVisita, FechaFinVisita)
        VALUES (
            '00000003-0000-0000-0000-000000000001',
            N'James Robert Wilson',
            N'American Capital Partners LLC',
            N'Estados Unidos de América',
            N'Hotel Presidente, Avenida Central, San José, Costa Rica',
            '2026-07-01',
            '2026-07-05'
        );

    -- Inversor 2: Hans Friedrich Müller (Alemania) — visita 2026-07-06 a 2026-07-08
    --   Solo habla Mandarín → demuestra escenario IDIOMA_INCOMPATIBLE (AC-05 / RN-12)
    IF NOT EXISTS (SELECT 1 FROM dbo.Inversores WHERE Id = '00000003-0000-0000-0000-000000000002')
        INSERT INTO dbo.Inversores
            (Id, NombreCompleto, Empresa, PaisOrigen, LugarHospedaje, FechaInicioVisita, FechaFinVisita)
        VALUES (
            '00000003-0000-0000-0000-000000000002',
            N'Hans Friedrich Müller',
            N'GerInvest GmbH',
            N'Alemania',
            N'Hotel Intercontinental, Barrio Dent, San José, Costa Rica',
            '2026-07-06',
            '2026-07-08'
        );

    -- Inversor 3: Marie-Claire Dupont (Francia) — visita 2026-07-09 a 2026-07-12
    IF NOT EXISTS (SELECT 1 FROM dbo.Inversores WHERE Id = '00000003-0000-0000-0000-000000000003')
        INSERT INTO dbo.Inversores
            (Id, NombreCompleto, Empresa, PaisOrigen, LugarHospedaje, FechaInicioVisita, FechaFinVisita)
        VALUES (
            '00000003-0000-0000-0000-000000000003',
            N'Marie-Claire Dupont',
            N'InvestFrance SAS',
            N'Francia',
            N'Hotel Wyndham San José Herradura, La Uruca, Costa Rica',
            '2026-07-09',
            '2026-07-12'
        );

    -- Inversor 4: Carlos Eduardo Martínez (Argentina) — visita 2026-07-14 a 2026-07-17
    IF NOT EXISTS (SELECT 1 FROM dbo.Inversores WHERE Id = '00000003-0000-0000-0000-000000000004')
        INSERT INTO dbo.Inversores
            (Id, NombreCompleto, Empresa, PaisOrigen, LugarHospedaje, FechaInicioVisita, FechaFinVisita)
        VALUES (
            '00000003-0000-0000-0000-000000000004',
            N'Carlos Eduardo Martínez',
            N'Grupo Inversiones Cono Sur S.A.',
            N'Argentina',
            N'Hotel Hilton Garden Inn, San José, Costa Rica',
            '2026-07-14',
            '2026-07-17'
        );

    -- Inversor 5: Yuki Tanaka (Japón) — visita 2026-07-21 a 2026-07-23
    IF NOT EXISTS (SELECT 1 FROM dbo.Inversores WHERE Id = '00000003-0000-0000-0000-000000000005')
        INSERT INTO dbo.Inversores
            (Id, NombreCompleto, Empresa, PaisOrigen, LugarHospedaje, FechaInicioVisita, FechaFinVisita)
        VALUES (
            '00000003-0000-0000-0000-000000000005',
            N'Yuki Tanaka',
            N'Nippon Investment Corporation',
            N'Japón',
            N'Hotel Real InterContinental, Escazú, Costa Rica',
            '2026-07-21',
            '2026-07-23'
        );

    PRINT '  [OK] Inversores (5)';

    -- ────────────────────────────────────────────────────────────
    -- 4. InversoresIdiomas
    --    Fuente: Entidad InversorIdioma (FK Cascada sobre Inversores)
    --    RN-01: al menos un idioma por inversor (garantizado por Application)
    -- ────────────────────────────────────────────────────────────

    -- James Wilson: Español + Inglés
    IF NOT EXISTS (SELECT 1 FROM dbo.InversoresIdiomas WHERE InversorId = '00000003-0000-0000-0000-000000000001' AND IdiomaId = '00000001-0000-0000-0000-000000000001')
        INSERT INTO dbo.InversoresIdiomas (InversorId, IdiomaId) VALUES ('00000003-0000-0000-0000-000000000001', '00000001-0000-0000-0000-000000000001'); -- Español
    IF NOT EXISTS (SELECT 1 FROM dbo.InversoresIdiomas WHERE InversorId = '00000003-0000-0000-0000-000000000001' AND IdiomaId = '00000001-0000-0000-0000-000000000002')
        INSERT INTO dbo.InversoresIdiomas (InversorId, IdiomaId) VALUES ('00000003-0000-0000-0000-000000000001', '00000001-0000-0000-0000-000000000002'); -- Inglés

    -- Hans Müller: SOLO Mandarín → escenario IDIOMA_INCOMPATIBLE
    IF NOT EXISTS (SELECT 1 FROM dbo.InversoresIdiomas WHERE InversorId = '00000003-0000-0000-0000-000000000002' AND IdiomaId = '00000001-0000-0000-0000-000000000005')
        INSERT INTO dbo.InversoresIdiomas (InversorId, IdiomaId) VALUES ('00000003-0000-0000-0000-000000000002', '00000001-0000-0000-0000-000000000005'); -- Mandarín

    -- Marie-Claire Dupont: Francés + Inglés
    IF NOT EXISTS (SELECT 1 FROM dbo.InversoresIdiomas WHERE InversorId = '00000003-0000-0000-0000-000000000003' AND IdiomaId = '00000001-0000-0000-0000-000000000004')
        INSERT INTO dbo.InversoresIdiomas (InversorId, IdiomaId) VALUES ('00000003-0000-0000-0000-000000000003', '00000001-0000-0000-0000-000000000004'); -- Francés
    IF NOT EXISTS (SELECT 1 FROM dbo.InversoresIdiomas WHERE InversorId = '00000003-0000-0000-0000-000000000003' AND IdiomaId = '00000001-0000-0000-0000-000000000002')
        INSERT INTO dbo.InversoresIdiomas (InversorId, IdiomaId) VALUES ('00000003-0000-0000-0000-000000000003', '00000001-0000-0000-0000-000000000002'); -- Inglés

    -- Carlos Martínez: Español + Portugués
    IF NOT EXISTS (SELECT 1 FROM dbo.InversoresIdiomas WHERE InversorId = '00000003-0000-0000-0000-000000000004' AND IdiomaId = '00000001-0000-0000-0000-000000000001')
        INSERT INTO dbo.InversoresIdiomas (InversorId, IdiomaId) VALUES ('00000003-0000-0000-0000-000000000004', '00000001-0000-0000-0000-000000000001'); -- Español
    IF NOT EXISTS (SELECT 1 FROM dbo.InversoresIdiomas WHERE InversorId = '00000003-0000-0000-0000-000000000004' AND IdiomaId = '00000001-0000-0000-0000-000000000003')
        INSERT INTO dbo.InversoresIdiomas (InversorId, IdiomaId) VALUES ('00000003-0000-0000-0000-000000000004', '00000001-0000-0000-0000-000000000003'); -- Portugués

    -- Yuki Tanaka: Inglés + Español
    IF NOT EXISTS (SELECT 1 FROM dbo.InversoresIdiomas WHERE InversorId = '00000003-0000-0000-0000-000000000005' AND IdiomaId = '00000001-0000-0000-0000-000000000002')
        INSERT INTO dbo.InversoresIdiomas (InversorId, IdiomaId) VALUES ('00000003-0000-0000-0000-000000000005', '00000001-0000-0000-0000-000000000002'); -- Inglés
    IF NOT EXISTS (SELECT 1 FROM dbo.InversoresIdiomas WHERE InversorId = '00000003-0000-0000-0000-000000000005' AND IdiomaId = '00000001-0000-0000-0000-000000000001')
        INSERT INTO dbo.InversoresIdiomas (InversorId, IdiomaId) VALUES ('00000003-0000-0000-0000-000000000005', '00000001-0000-0000-0000-000000000001'); -- Español

    PRINT '  [OK] InversoresIdiomas (9)';

    -- ────────────────────────────────────────────────────────────
    -- 5. Participantes
    --    Fuente: Entidad Participante
    --      NombreCompleto(200) Cargo(200) OficinaId FK Activo(BIT, default=1)
    --    RN-04: al menos un idioma por participante (ver sección 6)
    --    RN-05: cada participante tiene exactamente una oficina
    --    Participante 8 (Diego Alfaro): Activo = 0 → excluido del scheduling
    -- ────────────────────────────────────────────────────────────

    -- P1: María Fernanda Solís Jiménez — PROCOMER Central — Activo
    IF NOT EXISTS (SELECT 1 FROM dbo.Participantes WHERE Id = '00000004-0000-0000-0000-000000000001')
        INSERT INTO dbo.Participantes (Id, NombreCompleto, Cargo, OficinaId, Activo)
        VALUES ('00000004-0000-0000-0000-000000000001', N'María Fernanda Solís Jiménez',
                N'Directora de Inversión Extranjera — PROCOMER',
                '00000002-0000-0000-0000-000000000001', 1);

    -- P2: Andrés Quirós Salas — CINDE — Activo
    IF NOT EXISTS (SELECT 1 FROM dbo.Participantes WHERE Id = '00000004-0000-0000-0000-000000000002')
        INSERT INTO dbo.Participantes (Id, NombreCompleto, Cargo, OficinaId, Activo)
        VALUES ('00000004-0000-0000-0000-000000000002', N'Andrés Quirós Salas',
                N'Gerente de Atracción de Inversión — CINDE',
                '00000002-0000-0000-0000-000000000002', 1);

    -- P3: Carolina Herrera Madrigal — Ministerio de Hacienda — Activo
    IF NOT EXISTS (SELECT 1 FROM dbo.Participantes WHERE Id = '00000004-0000-0000-0000-000000000003')
        INSERT INTO dbo.Participantes (Id, NombreCompleto, Cargo, OficinaId, Activo)
        VALUES ('00000004-0000-0000-0000-000000000003', N'Carolina Herrera Madrigal',
                N'Viceministra de Ingresos — Ministerio de Hacienda',
                '00000002-0000-0000-0000-000000000003', 1);

    -- P4: Roberto Jiménez Ulate — Banco Central — Activo
    IF NOT EXISTS (SELECT 1 FROM dbo.Participantes WHERE Id = '00000004-0000-0000-0000-000000000004')
        INSERT INTO dbo.Participantes (Id, NombreCompleto, Cargo, OficinaId, Activo)
        VALUES ('00000004-0000-0000-0000-000000000004', N'Roberto Jiménez Ulate',
                N'Director de Política Económica — Banco Central de Costa Rica',
                '00000002-0000-0000-0000-000000000004', 1);

    -- P5: Verónica Montero Vargas — COMEX — Activo
    IF NOT EXISTS (SELECT 1 FROM dbo.Participantes WHERE Id = '00000004-0000-0000-0000-000000000005')
        INSERT INTO dbo.Participantes (Id, NombreCompleto, Cargo, OficinaId, Activo)
        VALUES ('00000004-0000-0000-0000-000000000005', N'Verónica Montero Vargas',
                N'Directora de Inversiones — COMEX',
                '00000002-0000-0000-0000-000000000005', 1);

    -- P6: Felipe Aguilar Rojas — PROCOMER Central — Activo
    IF NOT EXISTS (SELECT 1 FROM dbo.Participantes WHERE Id = '00000004-0000-0000-0000-000000000006')
        INSERT INTO dbo.Participantes (Id, NombreCompleto, Cargo, OficinaId, Activo)
        VALUES ('00000004-0000-0000-0000-000000000006', N'Felipe Aguilar Rojas',
                N'Especialista en Zonas Francas — PROCOMER',
                '00000002-0000-0000-0000-000000000001', 1);

    -- P7: Laura Vindas Castro — CINDE — Activo
    IF NOT EXISTS (SELECT 1 FROM dbo.Participantes WHERE Id = '00000004-0000-0000-0000-000000000007')
        INSERT INTO dbo.Participantes (Id, NombreCompleto, Cargo, OficinaId, Activo)
        VALUES ('00000004-0000-0000-0000-000000000007', N'Laura Vindas Castro',
                N'Analista de Mercados Emergentes — CINDE',
                '00000002-0000-0000-0000-000000000002', 1);

    -- P8: Diego Alfaro Mora — Ministerio de Hacienda — INACTIVO
    --   Activo = 0: demuestra exclusión del scheduling según Participante.Activo = false
    IF NOT EXISTS (SELECT 1 FROM dbo.Participantes WHERE Id = '00000004-0000-0000-0000-000000000008')
        INSERT INTO dbo.Participantes (Id, NombreCompleto, Cargo, OficinaId, Activo)
        VALUES ('00000004-0000-0000-0000-000000000008', N'Diego Alfaro Mora',
                N'Asesor Fiscal — Ministerio de Hacienda',
                '00000002-0000-0000-0000-000000000003', 0);

    PRINT '  [OK] Participantes (8 — 7 activos, 1 inactivo)';

    -- ────────────────────────────────────────────────────────────
    -- 6. ParticipantesIdiomas
    --    Fuente: Entidad ParticipanteIdioma (FK Cascada sobre Participantes)
    --    RN-04: al menos un idioma por participante
    --    NOTA: ningún participante habla Mandarín (ZH) → escenario
    --    IDIOMA_INCOMPATIBLE para Hans Müller.
    -- ────────────────────────────────────────────────────────────

    -- P1 María Fernanda Solís: Español + Inglés
    IF NOT EXISTS (SELECT 1 FROM dbo.ParticipantesIdiomas WHERE ParticipanteId = '00000004-0000-0000-0000-000000000001' AND IdiomaId = '00000001-0000-0000-0000-000000000001')
        INSERT INTO dbo.ParticipantesIdiomas VALUES ('00000004-0000-0000-0000-000000000001', '00000001-0000-0000-0000-000000000001');
    IF NOT EXISTS (SELECT 1 FROM dbo.ParticipantesIdiomas WHERE ParticipanteId = '00000004-0000-0000-0000-000000000001' AND IdiomaId = '00000001-0000-0000-0000-000000000002')
        INSERT INTO dbo.ParticipantesIdiomas VALUES ('00000004-0000-0000-0000-000000000001', '00000001-0000-0000-0000-000000000002');

    -- P2 Andrés Quirós: Español + Inglés + Portugués
    IF NOT EXISTS (SELECT 1 FROM dbo.ParticipantesIdiomas WHERE ParticipanteId = '00000004-0000-0000-0000-000000000002' AND IdiomaId = '00000001-0000-0000-0000-000000000001')
        INSERT INTO dbo.ParticipantesIdiomas VALUES ('00000004-0000-0000-0000-000000000002', '00000001-0000-0000-0000-000000000001');
    IF NOT EXISTS (SELECT 1 FROM dbo.ParticipantesIdiomas WHERE ParticipanteId = '00000004-0000-0000-0000-000000000002' AND IdiomaId = '00000001-0000-0000-0000-000000000002')
        INSERT INTO dbo.ParticipantesIdiomas VALUES ('00000004-0000-0000-0000-000000000002', '00000001-0000-0000-0000-000000000002');
    IF NOT EXISTS (SELECT 1 FROM dbo.ParticipantesIdiomas WHERE ParticipanteId = '00000004-0000-0000-0000-000000000002' AND IdiomaId = '00000001-0000-0000-0000-000000000003')
        INSERT INTO dbo.ParticipantesIdiomas VALUES ('00000004-0000-0000-0000-000000000002', '00000001-0000-0000-0000-000000000003');

    -- P3 Carolina Herrera: Español + Francés
    IF NOT EXISTS (SELECT 1 FROM dbo.ParticipantesIdiomas WHERE ParticipanteId = '00000004-0000-0000-0000-000000000003' AND IdiomaId = '00000001-0000-0000-0000-000000000001')
        INSERT INTO dbo.ParticipantesIdiomas VALUES ('00000004-0000-0000-0000-000000000003', '00000001-0000-0000-0000-000000000001');
    IF NOT EXISTS (SELECT 1 FROM dbo.ParticipantesIdiomas WHERE ParticipanteId = '00000004-0000-0000-0000-000000000003' AND IdiomaId = '00000001-0000-0000-0000-000000000004')
        INSERT INTO dbo.ParticipantesIdiomas VALUES ('00000004-0000-0000-0000-000000000003', '00000001-0000-0000-0000-000000000004');

    -- P4 Roberto Jiménez: Español + Inglés
    IF NOT EXISTS (SELECT 1 FROM dbo.ParticipantesIdiomas WHERE ParticipanteId = '00000004-0000-0000-0000-000000000004' AND IdiomaId = '00000001-0000-0000-0000-000000000001')
        INSERT INTO dbo.ParticipantesIdiomas VALUES ('00000004-0000-0000-0000-000000000004', '00000001-0000-0000-0000-000000000001');
    IF NOT EXISTS (SELECT 1 FROM dbo.ParticipantesIdiomas WHERE ParticipanteId = '00000004-0000-0000-0000-000000000004' AND IdiomaId = '00000001-0000-0000-0000-000000000002')
        INSERT INTO dbo.ParticipantesIdiomas VALUES ('00000004-0000-0000-0000-000000000004', '00000001-0000-0000-0000-000000000002');

    -- P5 Verónica Montero: Español + Inglés + Francés
    IF NOT EXISTS (SELECT 1 FROM dbo.ParticipantesIdiomas WHERE ParticipanteId = '00000004-0000-0000-0000-000000000005' AND IdiomaId = '00000001-0000-0000-0000-000000000001')
        INSERT INTO dbo.ParticipantesIdiomas VALUES ('00000004-0000-0000-0000-000000000005', '00000001-0000-0000-0000-000000000001');
    IF NOT EXISTS (SELECT 1 FROM dbo.ParticipantesIdiomas WHERE ParticipanteId = '00000004-0000-0000-0000-000000000005' AND IdiomaId = '00000001-0000-0000-0000-000000000002')
        INSERT INTO dbo.ParticipantesIdiomas VALUES ('00000004-0000-0000-0000-000000000005', '00000001-0000-0000-0000-000000000002');
    IF NOT EXISTS (SELECT 1 FROM dbo.ParticipantesIdiomas WHERE ParticipanteId = '00000004-0000-0000-0000-000000000005' AND IdiomaId = '00000001-0000-0000-0000-000000000004')
        INSERT INTO dbo.ParticipantesIdiomas VALUES ('00000004-0000-0000-0000-000000000005', '00000001-0000-0000-0000-000000000004');

    -- P6 Felipe Aguilar: Español + Inglés
    IF NOT EXISTS (SELECT 1 FROM dbo.ParticipantesIdiomas WHERE ParticipanteId = '00000004-0000-0000-0000-000000000006' AND IdiomaId = '00000001-0000-0000-0000-000000000001')
        INSERT INTO dbo.ParticipantesIdiomas VALUES ('00000004-0000-0000-0000-000000000006', '00000001-0000-0000-0000-000000000001');
    IF NOT EXISTS (SELECT 1 FROM dbo.ParticipantesIdiomas WHERE ParticipanteId = '00000004-0000-0000-0000-000000000006' AND IdiomaId = '00000001-0000-0000-0000-000000000002')
        INSERT INTO dbo.ParticipantesIdiomas VALUES ('00000004-0000-0000-0000-000000000006', '00000001-0000-0000-0000-000000000002');

    -- P7 Laura Vindas: Español + Inglés + Portugués
    IF NOT EXISTS (SELECT 1 FROM dbo.ParticipantesIdiomas WHERE ParticipanteId = '00000004-0000-0000-0000-000000000007' AND IdiomaId = '00000001-0000-0000-0000-000000000001')
        INSERT INTO dbo.ParticipantesIdiomas VALUES ('00000004-0000-0000-0000-000000000007', '00000001-0000-0000-0000-000000000001');
    IF NOT EXISTS (SELECT 1 FROM dbo.ParticipantesIdiomas WHERE ParticipanteId = '00000004-0000-0000-0000-000000000007' AND IdiomaId = '00000001-0000-0000-0000-000000000002')
        INSERT INTO dbo.ParticipantesIdiomas VALUES ('00000004-0000-0000-0000-000000000007', '00000001-0000-0000-0000-000000000002');
    IF NOT EXISTS (SELECT 1 FROM dbo.ParticipantesIdiomas WHERE ParticipanteId = '00000004-0000-0000-0000-000000000007' AND IdiomaId = '00000001-0000-0000-0000-000000000003')
        INSERT INTO dbo.ParticipantesIdiomas VALUES ('00000004-0000-0000-0000-000000000007', '00000001-0000-0000-0000-000000000003');

    -- P8 Diego Alfaro (inactivo): Español + Inglés (no participa en scheduling)
    IF NOT EXISTS (SELECT 1 FROM dbo.ParticipantesIdiomas WHERE ParticipanteId = '00000004-0000-0000-0000-000000000008' AND IdiomaId = '00000001-0000-0000-0000-000000000001')
        INSERT INTO dbo.ParticipantesIdiomas VALUES ('00000004-0000-0000-0000-000000000008', '00000001-0000-0000-0000-000000000001');
    IF NOT EXISTS (SELECT 1 FROM dbo.ParticipantesIdiomas WHERE ParticipanteId = '00000004-0000-0000-0000-000000000008' AND IdiomaId = '00000001-0000-0000-0000-000000000002')
        INSERT INTO dbo.ParticipantesIdiomas VALUES ('00000004-0000-0000-0000-000000000008', '00000001-0000-0000-0000-000000000002');

    PRINT '  [OK] ParticipantesIdiomas (16)';

    -- ────────────────────────────────────────────────────────────
    -- 7. DisponibilidadParticipantes
    --    Fuente: Entidad DisponibilidadParticipante
    --      ParticipanteId, Fecha (DATETIME2), HoraInicio (TIME), HoraFin (TIME)
    --    Horario estándar: 08:00–17:00 (jornada completa).
    --    AvailabilitySlotBuilder parte el bloque en 08:00–12:00 y 13:00–17:00
    --    al respetar el almuerzo inviolable 12:00–13:00 (RN-11).
    --    Participante P8 (inactivo) no tiene disponibilidades.
    --
    --    Ventanas de visita cubiertas:
    --      2026-07-01 a 2026-07-05 (Wilson)
    --      2026-07-06 a 2026-07-08 (Müller)
    --      2026-07-09 a 2026-07-12 (Dupont)
    --      2026-07-14 a 2026-07-17 (Martínez)
    --      2026-07-21 a 2026-07-23 (Tanaka)
    --
    --    Se usa NEWID() para los Ids ya que DisponibilidadParticipante.Id
    --    nunca es referenciado externamente (PK sin FK entrante).
    -- ────────────────────────────────────────────────────────────

    -- Helper: insertar bloque si no existe ya para ese participante+fecha
    -- Condición: evita duplicados en re-ejecuciones del script

    -- === Participante 1: María Fernanda Solís Jiménez ===
    INSERT INTO dbo.DisponibilidadParticipantes (Id, ParticipanteId, Fecha, HoraInicio, HoraFin)
    SELECT NEWID(), '00000004-0000-0000-0000-000000000001', d.Fecha, '08:00:00', '17:00:00'
    FROM (VALUES
        (CAST('2026-07-01' AS DATETIME2)), (CAST('2026-07-02' AS DATETIME2)),
        (CAST('2026-07-03' AS DATETIME2)), (CAST('2026-07-04' AS DATETIME2)),
        (CAST('2026-07-05' AS DATETIME2)), (CAST('2026-07-06' AS DATETIME2)),
        (CAST('2026-07-07' AS DATETIME2)), (CAST('2026-07-08' AS DATETIME2)),
        (CAST('2026-07-09' AS DATETIME2)), (CAST('2026-07-10' AS DATETIME2)),
        (CAST('2026-07-11' AS DATETIME2)), (CAST('2026-07-12' AS DATETIME2)),
        (CAST('2026-07-14' AS DATETIME2)), (CAST('2026-07-15' AS DATETIME2)),
        (CAST('2026-07-16' AS DATETIME2)), (CAST('2026-07-17' AS DATETIME2)),
        (CAST('2026-07-21' AS DATETIME2)), (CAST('2026-07-22' AS DATETIME2)),
        (CAST('2026-07-23' AS DATETIME2))
    ) AS d(Fecha)
    WHERE NOT EXISTS (
        SELECT 1 FROM dbo.DisponibilidadParticipantes
        WHERE ParticipanteId = '00000004-0000-0000-0000-000000000001'
          AND CAST(Fecha AS DATE) = CAST(d.Fecha AS DATE)
    );

    -- === Participante 2: Andrés Quirós Salas ===
    INSERT INTO dbo.DisponibilidadParticipantes (Id, ParticipanteId, Fecha, HoraInicio, HoraFin)
    SELECT NEWID(), '00000004-0000-0000-0000-000000000002', d.Fecha, '08:00:00', '17:00:00'
    FROM (VALUES
        (CAST('2026-07-01' AS DATETIME2)), (CAST('2026-07-02' AS DATETIME2)),
        (CAST('2026-07-03' AS DATETIME2)), (CAST('2026-07-04' AS DATETIME2)),
        (CAST('2026-07-05' AS DATETIME2)), (CAST('2026-07-06' AS DATETIME2)),
        (CAST('2026-07-07' AS DATETIME2)), (CAST('2026-07-08' AS DATETIME2)),
        (CAST('2026-07-09' AS DATETIME2)), (CAST('2026-07-10' AS DATETIME2)),
        (CAST('2026-07-11' AS DATETIME2)), (CAST('2026-07-12' AS DATETIME2)),
        (CAST('2026-07-14' AS DATETIME2)), (CAST('2026-07-15' AS DATETIME2)),
        (CAST('2026-07-16' AS DATETIME2)), (CAST('2026-07-17' AS DATETIME2)),
        (CAST('2026-07-21' AS DATETIME2)), (CAST('2026-07-22' AS DATETIME2)),
        (CAST('2026-07-23' AS DATETIME2))
    ) AS d(Fecha)
    WHERE NOT EXISTS (
        SELECT 1 FROM dbo.DisponibilidadParticipantes
        WHERE ParticipanteId = '00000004-0000-0000-0000-000000000002'
          AND CAST(Fecha AS DATE) = CAST(d.Fecha AS DATE)
    );

    -- === Participante 3: Carolina Herrera Madrigal (solo disponible L-X) ===
    -- Solo 3 días por semana para generar el escenario de agenda parcial
    INSERT INTO dbo.DisponibilidadParticipantes (Id, ParticipanteId, Fecha, HoraInicio, HoraFin)
    SELECT NEWID(), '00000004-0000-0000-0000-000000000003', d.Fecha, '08:00:00', '17:00:00'
    FROM (VALUES
        (CAST('2026-07-01' AS DATETIME2)), (CAST('2026-07-02' AS DATETIME2)),
        (CAST('2026-07-03' AS DATETIME2)),
        (CAST('2026-07-09' AS DATETIME2)), (CAST('2026-07-10' AS DATETIME2)),
        (CAST('2026-07-14' AS DATETIME2)), (CAST('2026-07-15' AS DATETIME2)),
        (CAST('2026-07-21' AS DATETIME2)), (CAST('2026-07-22' AS DATETIME2))
    ) AS d(Fecha)
    WHERE NOT EXISTS (
        SELECT 1 FROM dbo.DisponibilidadParticipantes
        WHERE ParticipanteId = '00000004-0000-0000-0000-000000000003'
          AND CAST(Fecha AS DATE) = CAST(d.Fecha AS DATE)
    );

    -- === Participante 4: Roberto Jiménez Ulate ===
    INSERT INTO dbo.DisponibilidadParticipantes (Id, ParticipanteId, Fecha, HoraInicio, HoraFin)
    SELECT NEWID(), '00000004-0000-0000-0000-000000000004', d.Fecha, '08:00:00', '17:00:00'
    FROM (VALUES
        (CAST('2026-07-01' AS DATETIME2)), (CAST('2026-07-02' AS DATETIME2)),
        (CAST('2026-07-03' AS DATETIME2)), (CAST('2026-07-04' AS DATETIME2)),
        (CAST('2026-07-05' AS DATETIME2)), (CAST('2026-07-07' AS DATETIME2)),
        (CAST('2026-07-08' AS DATETIME2)), (CAST('2026-07-09' AS DATETIME2)),
        (CAST('2026-07-10' AS DATETIME2)), (CAST('2026-07-11' AS DATETIME2)),
        (CAST('2026-07-12' AS DATETIME2)), (CAST('2026-07-14' AS DATETIME2)),
        (CAST('2026-07-15' AS DATETIME2)), (CAST('2026-07-16' AS DATETIME2)),
        (CAST('2026-07-17' AS DATETIME2)), (CAST('2026-07-21' AS DATETIME2)),
        (CAST('2026-07-22' AS DATETIME2)), (CAST('2026-07-23' AS DATETIME2))
    ) AS d(Fecha)
    WHERE NOT EXISTS (
        SELECT 1 FROM dbo.DisponibilidadParticipantes
        WHERE ParticipanteId = '00000004-0000-0000-0000-000000000004'
          AND CAST(Fecha AS DATE) = CAST(d.Fecha AS DATE)
    );

    -- === Participante 5: Verónica Montero Vargas ===
    INSERT INTO dbo.DisponibilidadParticipantes (Id, ParticipanteId, Fecha, HoraInicio, HoraFin)
    SELECT NEWID(), '00000004-0000-0000-0000-000000000005', d.Fecha, '08:00:00', '17:00:00'
    FROM (VALUES
        (CAST('2026-07-01' AS DATETIME2)), (CAST('2026-07-02' AS DATETIME2)),
        (CAST('2026-07-03' AS DATETIME2)), (CAST('2026-07-04' AS DATETIME2)),
        (CAST('2026-07-05' AS DATETIME2)), (CAST('2026-07-06' AS DATETIME2)),
        (CAST('2026-07-07' AS DATETIME2)), (CAST('2026-07-08' AS DATETIME2)),
        (CAST('2026-07-09' AS DATETIME2)), (CAST('2026-07-10' AS DATETIME2)),
        (CAST('2026-07-11' AS DATETIME2)), (CAST('2026-07-12' AS DATETIME2)),
        (CAST('2026-07-14' AS DATETIME2)), (CAST('2026-07-15' AS DATETIME2)),
        (CAST('2026-07-16' AS DATETIME2)), (CAST('2026-07-17' AS DATETIME2)),
        (CAST('2026-07-21' AS DATETIME2)), (CAST('2026-07-22' AS DATETIME2)),
        (CAST('2026-07-23' AS DATETIME2))
    ) AS d(Fecha)
    WHERE NOT EXISTS (
        SELECT 1 FROM dbo.DisponibilidadParticipantes
        WHERE ParticipanteId = '00000004-0000-0000-0000-000000000005'
          AND CAST(Fecha AS DATE) = CAST(d.Fecha AS DATE)
    );

    -- === Participante 6: Felipe Aguilar Rojas ===
    INSERT INTO dbo.DisponibilidadParticipantes (Id, ParticipanteId, Fecha, HoraInicio, HoraFin)
    SELECT NEWID(), '00000004-0000-0000-0000-000000000006', d.Fecha, '08:00:00', '17:00:00'
    FROM (VALUES
        (CAST('2026-07-01' AS DATETIME2)), (CAST('2026-07-02' AS DATETIME2)),
        (CAST('2026-07-03' AS DATETIME2)), (CAST('2026-07-04' AS DATETIME2)),
        (CAST('2026-07-05' AS DATETIME2)), (CAST('2026-07-06' AS DATETIME2)),
        (CAST('2026-07-07' AS DATETIME2)), (CAST('2026-07-08' AS DATETIME2)),
        (CAST('2026-07-09' AS DATETIME2)), (CAST('2026-07-11' AS DATETIME2)),
        (CAST('2026-07-12' AS DATETIME2)), (CAST('2026-07-14' AS DATETIME2)),
        (CAST('2026-07-16' AS DATETIME2)), (CAST('2026-07-17' AS DATETIME2)),
        (CAST('2026-07-21' AS DATETIME2)), (CAST('2026-07-22' AS DATETIME2)),
        (CAST('2026-07-23' AS DATETIME2))
    ) AS d(Fecha)
    WHERE NOT EXISTS (
        SELECT 1 FROM dbo.DisponibilidadParticipantes
        WHERE ParticipanteId = '00000004-0000-0000-0000-000000000006'
          AND CAST(Fecha AS DATE) = CAST(d.Fecha AS DATE)
    );

    -- === Participante 7: Laura Vindas Castro ===
    INSERT INTO dbo.DisponibilidadParticipantes (Id, ParticipanteId, Fecha, HoraInicio, HoraFin)
    SELECT NEWID(), '00000004-0000-0000-0000-000000000007', d.Fecha, '08:00:00', '17:00:00'
    FROM (VALUES
        (CAST('2026-07-01' AS DATETIME2)), (CAST('2026-07-02' AS DATETIME2)),
        (CAST('2026-07-03' AS DATETIME2)), (CAST('2026-07-04' AS DATETIME2)),
        (CAST('2026-07-05' AS DATETIME2)), (CAST('2026-07-06' AS DATETIME2)),
        (CAST('2026-07-07' AS DATETIME2)), (CAST('2026-07-08' AS DATETIME2)),
        (CAST('2026-07-09' AS DATETIME2)), (CAST('2026-07-10' AS DATETIME2)),
        (CAST('2026-07-11' AS DATETIME2)), (CAST('2026-07-12' AS DATETIME2)),
        (CAST('2026-07-14' AS DATETIME2)), (CAST('2026-07-15' AS DATETIME2)),
        (CAST('2026-07-16' AS DATETIME2)), (CAST('2026-07-17' AS DATETIME2)),
        (CAST('2026-07-21' AS DATETIME2)), (CAST('2026-07-22' AS DATETIME2)),
        (CAST('2026-07-23' AS DATETIME2))
    ) AS d(Fecha)
    WHERE NOT EXISTS (
        SELECT 1 FROM dbo.DisponibilidadParticipantes
        WHERE ParticipanteId = '00000004-0000-0000-0000-000000000007'
          AND CAST(Fecha AS DATE) = CAST(d.Fecha AS DATE)
    );

    -- P8 (Diego Alfaro, inactivo) no recibe disponibilidades

    PRINT '  [OK] DisponibilidadParticipantes (119 bloques para 7 participantes activos)';

    -- ────────────────────────────────────────────────────────────
    -- 8. MatrizTraslados
    --    Fuente: Entidad MatrizTraslado (TiempoMinutos IsRequired)
    --    RN-07: simetría A→B = B→A ya insertada explícitamente en ambas filas.
    --    UQ_MatrizTraslados_Par: cada par único ya validado por los IDs distintos.
    --    Tiempos estimados en minutos según geografía de San José, Costa Rica:
    --
    --    Oficinas:
    --      OF1 = PROCOMER Central  (Paseo Colón)
    --      OF2 = CINDE             (Torre Mercedes, Paseo Colón)
    --      OF3 = Min. Hacienda     (Barrio Aranjuez)
    --      OF4 = Banco Central     (Barrio Amón)
    --      OF5 = COMEX             (La Sabana Norte)
    --
    --    Tabla de tiempos (minutos):
    --      OF1 ↔ OF2 : 10   (misma zona, Paseo Colón)
    --      OF1 ↔ OF3 : 20   (Centro histórico San José)
    --      OF1 ↔ OF4 : 20   (Barrio Amón adyacente)
    --      OF1 ↔ OF5 : 25   (La Sabana Norte)
    --      OF2 ↔ OF3 : 25   (cruzar el Centro)
    --      OF2 ↔ OF4 : 25   (Barrio Amón via Paseo Colón)
    --      OF2 ↔ OF5 : 15   (contiguas en La Sabana)
    --      OF3 ↔ OF4 : 15   (ambas en zona Centro)
    --      OF3 ↔ OF5 : 30   (La Sabana vs Aranjuez)
    --      OF4 ↔ OF5 : 30   (Barrio Amón vs La Sabana)
    -- ────────────────────────────────────────────────────────────

    -- OF1 → OF2 (10 min) y OF2 → OF1 (10 min)
    IF NOT EXISTS (SELECT 1 FROM dbo.MatrizTraslados WHERE OficinaOrigenId = '00000002-0000-0000-0000-000000000001' AND OficinaDestinoId = '00000002-0000-0000-0000-000000000002')
        INSERT INTO dbo.MatrizTraslados (Id, OficinaOrigenId, OficinaDestinoId, TiempoMinutos) VALUES ('00000005-0012-0000-0000-000000000000', '00000002-0000-0000-0000-000000000001', '00000002-0000-0000-0000-000000000002', 10);
    IF NOT EXISTS (SELECT 1 FROM dbo.MatrizTraslados WHERE OficinaOrigenId = '00000002-0000-0000-0000-000000000002' AND OficinaDestinoId = '00000002-0000-0000-0000-000000000001')
        INSERT INTO dbo.MatrizTraslados (Id, OficinaOrigenId, OficinaDestinoId, TiempoMinutos) VALUES ('00000005-0021-0000-0000-000000000000', '00000002-0000-0000-0000-000000000002', '00000002-0000-0000-0000-000000000001', 10);

    -- OF1 → OF3 (20 min) y OF3 → OF1 (20 min)
    IF NOT EXISTS (SELECT 1 FROM dbo.MatrizTraslados WHERE OficinaOrigenId = '00000002-0000-0000-0000-000000000001' AND OficinaDestinoId = '00000002-0000-0000-0000-000000000003')
        INSERT INTO dbo.MatrizTraslados (Id, OficinaOrigenId, OficinaDestinoId, TiempoMinutos) VALUES ('00000005-0013-0000-0000-000000000000', '00000002-0000-0000-0000-000000000001', '00000002-0000-0000-0000-000000000003', 20);
    IF NOT EXISTS (SELECT 1 FROM dbo.MatrizTraslados WHERE OficinaOrigenId = '00000002-0000-0000-0000-000000000003' AND OficinaDestinoId = '00000002-0000-0000-0000-000000000001')
        INSERT INTO dbo.MatrizTraslados (Id, OficinaOrigenId, OficinaDestinoId, TiempoMinutos) VALUES ('00000005-0031-0000-0000-000000000000', '00000002-0000-0000-0000-000000000003', '00000002-0000-0000-0000-000000000001', 20);

    -- OF1 → OF4 (20 min) y OF4 → OF1 (20 min)
    IF NOT EXISTS (SELECT 1 FROM dbo.MatrizTraslados WHERE OficinaOrigenId = '00000002-0000-0000-0000-000000000001' AND OficinaDestinoId = '00000002-0000-0000-0000-000000000004')
        INSERT INTO dbo.MatrizTraslados (Id, OficinaOrigenId, OficinaDestinoId, TiempoMinutos) VALUES ('00000005-0014-0000-0000-000000000000', '00000002-0000-0000-0000-000000000001', '00000002-0000-0000-0000-000000000004', 20);
    IF NOT EXISTS (SELECT 1 FROM dbo.MatrizTraslados WHERE OficinaOrigenId = '00000002-0000-0000-0000-000000000004' AND OficinaDestinoId = '00000002-0000-0000-0000-000000000001')
        INSERT INTO dbo.MatrizTraslados (Id, OficinaOrigenId, OficinaDestinoId, TiempoMinutos) VALUES ('00000005-0041-0000-0000-000000000000', '00000002-0000-0000-0000-000000000004', '00000002-0000-0000-0000-000000000001', 20);

    -- OF1 → OF5 (25 min) y OF5 → OF1 (25 min)
    IF NOT EXISTS (SELECT 1 FROM dbo.MatrizTraslados WHERE OficinaOrigenId = '00000002-0000-0000-0000-000000000001' AND OficinaDestinoId = '00000002-0000-0000-0000-000000000005')
        INSERT INTO dbo.MatrizTraslados (Id, OficinaOrigenId, OficinaDestinoId, TiempoMinutos) VALUES ('00000005-0015-0000-0000-000000000000', '00000002-0000-0000-0000-000000000001', '00000002-0000-0000-0000-000000000005', 25);
    IF NOT EXISTS (SELECT 1 FROM dbo.MatrizTraslados WHERE OficinaOrigenId = '00000002-0000-0000-0000-000000000005' AND OficinaDestinoId = '00000002-0000-0000-0000-000000000001')
        INSERT INTO dbo.MatrizTraslados (Id, OficinaOrigenId, OficinaDestinoId, TiempoMinutos) VALUES ('00000005-0051-0000-0000-000000000000', '00000002-0000-0000-0000-000000000005', '00000002-0000-0000-0000-000000000001', 25);

    -- OF2 → OF3 (25 min) y OF3 → OF2 (25 min)
    IF NOT EXISTS (SELECT 1 FROM dbo.MatrizTraslados WHERE OficinaOrigenId = '00000002-0000-0000-0000-000000000002' AND OficinaDestinoId = '00000002-0000-0000-0000-000000000003')
        INSERT INTO dbo.MatrizTraslados (Id, OficinaOrigenId, OficinaDestinoId, TiempoMinutos) VALUES ('00000005-0023-0000-0000-000000000000', '00000002-0000-0000-0000-000000000002', '00000002-0000-0000-0000-000000000003', 25);
    IF NOT EXISTS (SELECT 1 FROM dbo.MatrizTraslados WHERE OficinaOrigenId = '00000002-0000-0000-0000-000000000003' AND OficinaDestinoId = '00000002-0000-0000-0000-000000000002')
        INSERT INTO dbo.MatrizTraslados (Id, OficinaOrigenId, OficinaDestinoId, TiempoMinutos) VALUES ('00000005-0032-0000-0000-000000000000', '00000002-0000-0000-0000-000000000003', '00000002-0000-0000-0000-000000000002', 25);

    -- OF2 → OF4 (25 min) y OF4 → OF2 (25 min)
    IF NOT EXISTS (SELECT 1 FROM dbo.MatrizTraslados WHERE OficinaOrigenId = '00000002-0000-0000-0000-000000000002' AND OficinaDestinoId = '00000002-0000-0000-0000-000000000004')
        INSERT INTO dbo.MatrizTraslados (Id, OficinaOrigenId, OficinaDestinoId, TiempoMinutos) VALUES ('00000005-0024-0000-0000-000000000000', '00000002-0000-0000-0000-000000000002', '00000002-0000-0000-0000-000000000004', 25);
    IF NOT EXISTS (SELECT 1 FROM dbo.MatrizTraslados WHERE OficinaOrigenId = '00000002-0000-0000-0000-000000000004' AND OficinaDestinoId = '00000002-0000-0000-0000-000000000002')
        INSERT INTO dbo.MatrizTraslados (Id, OficinaOrigenId, OficinaDestinoId, TiempoMinutos) VALUES ('00000005-0042-0000-0000-000000000000', '00000002-0000-0000-0000-000000000004', '00000002-0000-0000-0000-000000000002', 25);

    -- OF2 → OF5 (15 min) y OF5 → OF2 (15 min)
    IF NOT EXISTS (SELECT 1 FROM dbo.MatrizTraslados WHERE OficinaOrigenId = '00000002-0000-0000-0000-000000000002' AND OficinaDestinoId = '00000002-0000-0000-0000-000000000005')
        INSERT INTO dbo.MatrizTraslados (Id, OficinaOrigenId, OficinaDestinoId, TiempoMinutos) VALUES ('00000005-0025-0000-0000-000000000000', '00000002-0000-0000-0000-000000000002', '00000002-0000-0000-0000-000000000005', 15);
    IF NOT EXISTS (SELECT 1 FROM dbo.MatrizTraslados WHERE OficinaOrigenId = '00000002-0000-0000-0000-000000000005' AND OficinaDestinoId = '00000002-0000-0000-0000-000000000002')
        INSERT INTO dbo.MatrizTraslados (Id, OficinaOrigenId, OficinaDestinoId, TiempoMinutos) VALUES ('00000005-0052-0000-0000-000000000000', '00000002-0000-0000-0000-000000000005', '00000002-0000-0000-0000-000000000002', 15);

    -- OF3 → OF4 (15 min) y OF4 → OF3 (15 min)
    IF NOT EXISTS (SELECT 1 FROM dbo.MatrizTraslados WHERE OficinaOrigenId = '00000002-0000-0000-0000-000000000003' AND OficinaDestinoId = '00000002-0000-0000-0000-000000000004')
        INSERT INTO dbo.MatrizTraslados (Id, OficinaOrigenId, OficinaDestinoId, TiempoMinutos) VALUES ('00000005-0034-0000-0000-000000000000', '00000002-0000-0000-0000-000000000003', '00000002-0000-0000-0000-000000000004', 15);
    IF NOT EXISTS (SELECT 1 FROM dbo.MatrizTraslados WHERE OficinaOrigenId = '00000002-0000-0000-0000-000000000004' AND OficinaDestinoId = '00000002-0000-0000-0000-000000000003')
        INSERT INTO dbo.MatrizTraslados (Id, OficinaOrigenId, OficinaDestinoId, TiempoMinutos) VALUES ('00000005-0043-0000-0000-000000000000', '00000002-0000-0000-0000-000000000004', '00000002-0000-0000-0000-000000000003', 15);

    -- OF3 → OF5 (30 min) y OF5 → OF3 (30 min)
    IF NOT EXISTS (SELECT 1 FROM dbo.MatrizTraslados WHERE OficinaOrigenId = '00000002-0000-0000-0000-000000000003' AND OficinaDestinoId = '00000002-0000-0000-0000-000000000005')
        INSERT INTO dbo.MatrizTraslados (Id, OficinaOrigenId, OficinaDestinoId, TiempoMinutos) VALUES ('00000005-0035-0000-0000-000000000000', '00000002-0000-0000-0000-000000000003', '00000002-0000-0000-0000-000000000005', 30);
    IF NOT EXISTS (SELECT 1 FROM dbo.MatrizTraslados WHERE OficinaOrigenId = '00000002-0000-0000-0000-000000000005' AND OficinaDestinoId = '00000002-0000-0000-0000-000000000003')
        INSERT INTO dbo.MatrizTraslados (Id, OficinaOrigenId, OficinaDestinoId, TiempoMinutos) VALUES ('00000005-0053-0000-0000-000000000000', '00000002-0000-0000-0000-000000000005', '00000002-0000-0000-0000-000000000003', 30);

    -- OF4 → OF5 (30 min) y OF5 → OF4 (30 min)
    IF NOT EXISTS (SELECT 1 FROM dbo.MatrizTraslados WHERE OficinaOrigenId = '00000002-0000-0000-0000-000000000004' AND OficinaDestinoId = '00000002-0000-0000-0000-000000000005')
        INSERT INTO dbo.MatrizTraslados (Id, OficinaOrigenId, OficinaDestinoId, TiempoMinutos) VALUES ('00000005-0045-0000-0000-000000000000', '00000002-0000-0000-0000-000000000004', '00000002-0000-0000-0000-000000000005', 30);
    IF NOT EXISTS (SELECT 1 FROM dbo.MatrizTraslados WHERE OficinaOrigenId = '00000002-0000-0000-0000-000000000005' AND OficinaDestinoId = '00000002-0000-0000-0000-000000000004')
        INSERT INTO dbo.MatrizTraslados (Id, OficinaOrigenId, OficinaDestinoId, TiempoMinutos) VALUES ('00000005-0054-0000-0000-000000000000', '00000002-0000-0000-0000-000000000005', '00000002-0000-0000-0000-000000000004', 30);

    PRINT '  [OK] MatrizTraslados (20 = 10 pares x 2 sentidos — simetria RN-07)';

    -- ────────────────────────────────────────────────────────────
    -- Resumen de registros insertados
    -- ────────────────────────────────────────────────────────────
    PRINT '';
    PRINT '  Recuento final de registros:';
    SELECT 'Idiomas'                    AS Tabla, COUNT(*) AS Registros FROM dbo.Idiomas              UNION ALL
    SELECT 'Oficinas',                             COUNT(*)             FROM dbo.Oficinas             UNION ALL
    SELECT 'Inversores',                           COUNT(*)             FROM dbo.Inversores           UNION ALL
    SELECT 'InversoresIdiomas',                    COUNT(*)             FROM dbo.InversoresIdiomas    UNION ALL
    SELECT 'Participantes',                        COUNT(*)             FROM dbo.Participantes        UNION ALL
    SELECT 'ParticipantesIdiomas',                 COUNT(*)             FROM dbo.ParticipantesIdiomas UNION ALL
    SELECT 'DisponibilidadParticipantes',          COUNT(*)             FROM dbo.DisponibilidadParticipantes UNION ALL
    SELECT 'MatrizTraslados',                      COUNT(*)             FROM dbo.MatrizTraslados;

    COMMIT TRANSACTION;

    PRINT '';
    PRINT '══════════════════════════════════════════════════════════';
    PRINT '  Datos semilla insertados exitosamente.';
    PRINT '══════════════════════════════════════════════════════════';

END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

    PRINT '';
    PRINT '██ ERROR: Seed data no insertado. Transaccion revertida. ██';
    PRINT 'Mensaje : ' + ERROR_MESSAGE();
    PRINT 'Numero  : ' + CAST(ERROR_NUMBER()   AS NVARCHAR(10));
    PRINT 'Linea   : ' + CAST(ERROR_LINE()     AS NVARCHAR(10));
    PRINT 'Proc    : ' + ISNULL(ERROR_PROCEDURE(), 'N/A');
    THROW;
END CATCH;
GO
