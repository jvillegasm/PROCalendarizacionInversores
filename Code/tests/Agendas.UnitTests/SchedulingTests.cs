using Agendas.Application.DTOs;
using Agendas.Application.Engine;
using Agendas.Application.Interfaces;
using Agendas.Application.UseCases;
using Agendas.Domain.Entities;
using Agendas.Domain.Enums;
using Agendas.Domain.Exceptions;
using FluentAssertions;
using Moq;

namespace Agendas.UnitTests;

// ═══════════════════════════════════════════════════════════════
// UT-01: TravelTimeResolver — devuelve minutos correctos
// ═══════════════════════════════════════════════════════════════
public class TravelTimeResolverTests
{
    private readonly TravelTimeResolver _sut = new();
    private readonly Guid _a = Guid.NewGuid();
    private readonly Guid _b = Guid.NewGuid();
    private readonly Guid _c = Guid.NewGuid();

    [Fact(DisplayName = "UT-01a: devuelve el tiempo registrado en la matriz")]
    public void Resolver_DebeRetornarTiempoRegistrado()
    {
        var matriz = new Dictionary<(Guid, Guid), int> { [(_a, _b)] = 25 };

        var resultado = _sut.Resolver(_a, _b, matriz);

        resultado.Should().Be(25);
    }

    [Fact(DisplayName = "UT-01b: devuelve 0 cuando las oficinas son la misma")]
    public void Resolver_MismaOficina_RetornasCero()
    {
        var resultado = _sut.Resolver(_a, _a, new Dictionary<(Guid, Guid), int>());

        resultado.Should().Be(0);
    }

    [Fact(DisplayName = "UT-01c: devuelve 0 cuando el par no existe en la matriz")]
    public void Resolver_ParNoExistente_RetornaCero()
    {
        var resultado = _sut.Resolver(_a, _c, new Dictionary<(Guid, Guid), int>());

        resultado.Should().Be(0);
    }
}

// ═══════════════════════════════════════════════════════════════
// UT-02: SchedulingEngine — genera reuniones sin violar restricciones
// ═══════════════════════════════════════════════════════════════
public class SchedulingEngineTests
{
    private static readonly Guid IdiomaEsp = Guid.NewGuid();
    private static readonly Guid OficinaA = Guid.NewGuid();
    private static readonly Guid OficinaB = Guid.NewGuid();
    private static readonly DateTime Fecha = new(2025, 6, 20);

    // Helper: crea un candidato con un bloque de disponibilidad completo (08:00–17:00)
    private static CandidatoAgenda CrearCandidato(string nombre, Guid oficina) =>
        new(
            Participante: new ParticipanteCatalogoDto(
                Id: Guid.NewGuid(), NombreCompleto: nombre, Cargo: "Gerente",
                OficinaId: oficina, OficinaNombre: "Oficina",
                Activo: true,
                Idiomas: [new IdiomaCatalogoDto(IdiomaEsp, "Español", "ES")],
                Disponibilidades: [new DisponibilidadCatalogoDto(Guid.NewGuid(), Fecha, new TimeSpan(8,0,0), new TimeSpan(17,0,0))]),
            SlotsDisponibles: [new SlotDisponible(new TimeSpan(8,0,0), new TimeSpan(12,0,0)),
                                new SlotDisponible(new TimeSpan(13,0,0), new TimeSpan(17,0,0))]);

    private static OficinaCatalogoDto CrearOficina(Guid id, string nombre) =>
        new(id, nombre, "Dirección física", null);

    [Fact(DisplayName = "UT-02: genera 3 reuniones sin violar horarios ni almuerzo")]
    public void Generar_TresCandidatos_AgendaConTresReuniones()
    {
        var engine = new SchedulingEngine(new TravelTimeResolver());
        var candidatos = new List<CandidatoAgenda>
        {
            CrearCandidato("Ana Pérez", OficinaA),
            CrearCandidato("Luis Mora", OficinaA),
            CrearCandidato("María Soto", OficinaB),
        };
        var oficinas = new[] { CrearOficina(OficinaA, "Oficina Central"), CrearOficina(OficinaB, "Sede Norte") };
        var matriz = new Dictionary<(Guid, Guid), int> { [(OficinaA, OficinaB)] = 10, [(OficinaB, OficinaA)] = 10 };

        var result = engine.Generar(candidatos, duracionMinutos: 60, metaReuniones: 3,
            ultimaOficinaId: OficinaA, matrizTraslados: matriz, oficinas: oficinas);

        result.ReunionesLogradas.Should().Be(3);
        result.Completa.Should().BeTrue();
        // Ninguna reunión debe cruzar el bloque de almuerzo 12:00–13:00
        result.Reuniones.Should().NotContain(r => r.HoraInicio < new TimeSpan(13, 0, 0) && r.HoraFin > new TimeSpan(12, 0, 0));
        // Todas dentro del horario laboral
        result.Reuniones.Should().OnlyContain(r => r.HoraInicio >= new TimeSpan(8, 0, 0) && r.HoraFin <= new TimeSpan(17, 0, 0));
    }
}

// ═══════════════════════════════════════════════════════════════
// UT-03: AnularAgendaHandler — cambia estado a Anulada sin eliminar
// ═══════════════════════════════════════════════════════════════
public class AnularAgendaHandlerTests
{
    [Fact(DisplayName = "UT-03: anulación lógica cambia estado a Anulada y persiste")]
    public async Task HandleAsync_AgendaActiva_CambiaAAnulada()
    {
        // Arrange
        var agendaId = Guid.NewGuid();
        var agenda = new Agenda
        {
            Id = agendaId,
            InversorId = Guid.NewGuid(),
            Fecha = DateTime.Today,
            Estado = EstadoAgenda.Activa,
            FechaGeneracion = DateTime.UtcNow
        };

        Agenda? agendaActualizada = null;
        var repoMock = new Mock<IAgendaRepository>();
        repoMock.Setup(r => r.GetByIdAsync(agendaId)).ReturnsAsync(agenda);
        repoMock.Setup(r => r.UpdateAsync(It.IsAny<Agenda>()))
                .Callback<Agenda>(a => agendaActualizada = a)
                .Returns(Task.CompletedTask);

        var handler = new AnularAgendaHandler(repoMock.Object);

        // Act
        await handler.HandleAsync(agendaId);

        // Assert
        repoMock.Verify(r => r.UpdateAsync(agenda), Times.Once);
        agendaActualizada!.Estado.Should().Be(EstadoAgenda.Anulada);
        agendaActualizada.FechaAnulacion.Should().NotBeNull();
        // El registro fue actualizado, no creado de nuevo ni reemplazado (RN-15: anulación lógica)
        repoMock.Verify(r => r.AddAsync(It.IsAny<Agenda>()), Times.Never);
    }

    [Fact(DisplayName = "UT-03b: anular agenda ya anulada lanza AgendaYaAnuladaException (HTTP 409)")]
    public async Task HandleAsync_AgendaYaAnulada_LanzaExcepcion()
    {
        var agendaId = Guid.NewGuid();
        var agenda = new Agenda { Id = agendaId, Estado = EstadoAgenda.Anulada };

        var repoMock = new Mock<IAgendaRepository>();
        repoMock.Setup(r => r.GetByIdAsync(agendaId)).ReturnsAsync(agenda);

        var handler = new AnularAgendaHandler(repoMock.Object);

        Func<Task> act = () => handler.HandleAsync(agendaId);

        await act.Should().ThrowAsync<AgendaYaAnuladaException>();
    }
}

// ═══════════════════════════════════════════════════════════════
// UT-04: SchedulingEngine — idioma incompatible → 0 reuniones (RN-12)
// ═══════════════════════════════════════════════════════════════
public class SchedulingEngineIdiomaTests
{
    [Fact(DisplayName = "UT-04: sin candidatos compatibles el motor retorna 0 reuniones")]
    public void Generar_SinCandidatosCompatibles_CeroReuniones()
    {
        // El motor en sí NO lanza IdiomaIncompatibleException — eso lo hace GenerarAgendaHandler.
        // El motor simplemente devuelve 0 reuniones si no recibe candidatos válidos (lista vacía).
        var engine = new SchedulingEngine(new TravelTimeResolver());

        var result = engine.Generar(
            candidatos: new List<CandidatoAgenda>(), // lista vacía = ningún candidato compatible
            duracionMinutos: 60,
            metaReuniones: 3,
            ultimaOficinaId: Guid.NewGuid(),
            matrizTraslados: new Dictionary<(Guid, Guid), int>(),
            oficinas: Array.Empty<OficinaCatalogoDto>());

        result.ReunionesLogradas.Should().Be(0);
        result.Completa.Should().BeFalse();
    }

    [Fact(DisplayName = "UT-04b: LanguageCompatibilityFilter excluye candidatos sin idioma compartido (RN-12)")]
    public void Filtrar_SinIdiomaCompartido_RetornaListaVacia()
    {
        var filtro = new LanguageCompatibilityFilter();
        var idiomaInversor = Guid.NewGuid();
        var idiomaDistinto = Guid.NewGuid();

        var candidatos = new List<ParticipanteCatalogoDto>
        {
            new(Guid.NewGuid(), "Pedro López", "Director", Guid.NewGuid(), "Oficina", true,
                Idiomas: [new IdiomaCatalogoDto(idiomaDistinto, "Mandarín", "ZH")],
                Disponibilidades: [])
        };

        var compatibles = filtro.Filtrar(candidatos, new[] { idiomaInversor });

        compatibles.Should().BeEmpty("ningún candidato comparte idioma con el inversor (RN-12)");
    }
}

// ═══════════════════════════════════════════════════════════════
// UT-05: AvailabilitySlotBuilder — fecha fuera de rango → sin slots (RN-08/RN-09)
// ═══════════════════════════════════════════════════════════════
public class AvailabilitySlotBuilderTests
{
    [Fact(DisplayName = "UT-05: fecha sin disponibilidades registradas retorna lista vacía")]
    public void Construir_FechaSinDisponibilidad_RetornaListaVacia()
    {
        var builder = new AvailabilitySlotBuilder();
        var participante = new ParticipanteCatalogoDto(
            Id: Guid.NewGuid(), NombreCompleto: "Ana", Cargo: "Gerente",
            OficinaId: Guid.NewGuid(), OficinaNombre: "HQ",
            Activo: true,
            Idiomas: [],
            Disponibilidades: []); // Sin ningún bloque registrado

        var fechaFueraDeRango = new DateTime(2099, 12, 31);

        var slots = builder.Construir(participante, fechaFueraDeRango);

        slots.Should().BeEmpty("la fecha no tiene disponibilidades registradas (equivalente a RN-08 fuera de rango)");
    }

    [Fact(DisplayName = "UT-05b: bloque que cruza mediodía se parte en dos slots (RN-11)")]
    public void Construir_BloqueQueCruzaAlmuerzo_SeDivideEnDosSlots()
    {
        var builder = new AvailabilitySlotBuilder();
        var fecha = new DateTime(2025, 6, 20);
        var participante = new ParticipanteCatalogoDto(
            Id: Guid.NewGuid(), NombreCompleto: "Lucía", Cargo: "Analista",
            OficinaId: Guid.NewGuid(), OficinaNombre: "Sede",
            Activo: true,
            Idiomas: [],
            Disponibilidades: [new DisponibilidadCatalogoDto(Guid.NewGuid(), fecha, new TimeSpan(8, 0, 0), new TimeSpan(17, 0, 0))]);

        var slots = builder.Construir(participante, fecha);

        slots.Should().HaveCount(2, "el bloque 08:00–17:00 debe partirse por el almuerzo 12:00–13:00 (RN-11)");
        slots[0].Fin.Should().Be(new TimeSpan(12, 0, 0));
        slots[1].Inicio.Should().Be(new TimeSpan(13, 0, 0));
    }
}
