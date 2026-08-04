using System.Linq.Expressions;
using System.Reflection;
using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Services;
using Dsw2026Tpi.CrossCutting.Exceptions;
using Dsw2026Tpi.CrossCutting.Identity;
using Dsw2026Tpi.Domain.Entities;
using Dsw2026Tpi.Domain.Interfaces;
using NSubstitute;

namespace Dsw2026Tpi.Test.Services;

public class AppointmentServiceTests
{
    private readonly IPersistence _mockPersistence = Substitute.For<IPersistence>();
    private readonly AppointmentService _service;

    public AppointmentServiceTests()
    {
        _service = new AppointmentService(_mockPersistence);
    }

    // Helper: asigna a la fuerza una propiedad de navegación con setter privado
    // (Doctor, AvailabilityRule, etc.), simulando lo que EF Core haría al hacer
    // un Include(...) contra la base real. Solo se usa acá en el test, no toca
    // ninguna entidad de producción.
    private static void SetNavigation<TEntity>(TEntity entity, string propertyName, object? value)
        where TEntity : class
    {
        var prop = typeof(TEntity).GetProperty(
            propertyName,
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            ?? throw new InvalidOperationException($"No se encontró la propiedad {propertyName}");
        prop.SetValue(entity, value);
    }

    // Arma un slot "listo para reservar": disponible, en el futuro, con su
    // AvailabilityRule (y opcionalmente el Doctor) ya "cargados".
    private static AvailabilitySlot BuildSlotConReglaYDoctor(Guid doctorId, bool incluirDoctor = false)
    {
        var slot = new AvailabilitySlot(
            Guid.NewGuid(),
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
            new TimeOnly(10, 0),
            new TimeOnly(10, 30));

        var rule = new AvailabilityRule(doctorId, 1, 2026, 1, new TimeOnly(9, 0), new TimeOnly(12, 0));

        if (incluirDoctor)
        {
            var speciality = new Speciality("Cardiología", "Especialidad del corazón");
            var doctor = new Doctor("Juan Perez", "MP1234", speciality, doctorId);
            SetNavigation(rule, nameof(AvailabilityRule.Doctor), doctor);
        }

        SetNavigation(slot, nameof(AvailabilitySlot.AvailabilityRule), rule);
        return slot;
    }

    private static AppointmentModel.Request BuildRequestValido(Guid doctorId, Guid slotId, long dni = 30123456) =>
        new(doctorId, slotId, new AppointmentModel.PatientRequestDto(dni), "Control de rutina");

    [Fact]
    public async Task Create_ConDatosValidos_EntoncesCreaLaCita()
    {
        // Arrange
        var doctorId = Guid.NewGuid();
        var patient = new Patient(Guid.NewGuid(), 30123456, "Ana Gomez");
        var slot = BuildSlotConReglaYDoctor(doctorId, incluirDoctor: true);
        var request = BuildRequestValido(doctorId, slot.Id, patient.Dni);

        _mockPersistence.First<Patient>(Arg.Any<Expression<Func<Patient, bool>>>()).Returns(patient);
        _mockPersistence.GetById<AvailabilitySlot>(Arg.Any<Guid>(), Arg.Any<string[]>()).Returns(slot);
        _mockPersistence.Update(Arg.Any<AvailabilitySlot>()).Returns(callInfo => callInfo.Arg<AvailabilitySlot>());
        _mockPersistence.Add(Arg.Any<Appointment>()).Returns(callInfo => callInfo.Arg<Appointment>());

        // Act
        var response = await _service.Create(request);

        // Assert
        Assert.Equal(request.Reason, response.Reason);
        Assert.Equal(AppointmentStatus.Booked, response.Status);
        Assert.Equal(patient.Dni, response.Patient.Dni);
        await _mockPersistence.Received(1).Update(Arg.Any<AvailabilitySlot>());
        await _mockPersistence.Received(1).Add(Arg.Any<Appointment>());
    }

    [Fact]
    public async Task Create_CuandoElMotivoEsInvalido_EntoncesLanzaValidationException()
    {
        // Arrange
        var request = new AppointmentModel.Request(
            Guid.NewGuid(), Guid.NewGuid(), new AppointmentModel.PatientRequestDto(30123456), "hi");

        // Act - Assert
        await Assert.ThrowsAsync<ValidationException>(() => _service.Create(request));

        // Ni siquiera debería haber intentado buscar al paciente
        await _mockPersistence.DidNotReceive().First<Patient>(Arg.Any<Expression<Func<Patient, bool>>>());
    }

    [Fact]
    public async Task Create_CuandoElPacienteNoExiste_EntoncesLanzaEntityNotFoundException()
    {
        // Arrange
        var request = BuildRequestValido(Guid.NewGuid(), Guid.NewGuid());

        _mockPersistence.First<Patient>(Arg.Any<Expression<Func<Patient, bool>>>()).Returns((Patient?)null);

        // Act - Assert
        await Assert.ThrowsAsync<EntityNotFoundException>(() => _service.Create(request));
    }

    [Fact]
    public async Task Create_CuandoElSlotNoExiste_EntoncesLanzaEntityNotFoundException()
    {
        // Arrange
        var patient = new Patient(Guid.NewGuid(), 30123456, "Ana Gomez");
        var request = BuildRequestValido(Guid.NewGuid(), Guid.NewGuid(), patient.Dni);

        _mockPersistence.First<Patient>(Arg.Any<Expression<Func<Patient, bool>>>()).Returns(patient);
        _mockPersistence.GetById<AvailabilitySlot>(Arg.Any<Guid>(), Arg.Any<string[]>()).Returns((AvailabilitySlot?)null);

        // Act - Assert
        await Assert.ThrowsAsync<EntityNotFoundException>(() => _service.Create(request));
    }

    [Fact]
    public async Task Create_CuandoElSlotNoPerteneceAlMedico_EntoncesLanzaValidationException()
    {
        // Arrange
        var doctorIdDelSlot = Guid.NewGuid();
        var otroDoctorIdEnElRequest = Guid.NewGuid();

        var patient = new Patient(Guid.NewGuid(), 30123456, "Ana Gomez");
        var slot = BuildSlotConReglaYDoctor(doctorIdDelSlot);
        var request = BuildRequestValido(otroDoctorIdEnElRequest, slot.Id, patient.Dni);

        _mockPersistence.First<Patient>(Arg.Any<Expression<Func<Patient, bool>>>()).Returns(patient);
        _mockPersistence.GetById<AvailabilitySlot>(Arg.Any<Guid>(), Arg.Any<string[]>()).Returns(slot);

        // Act - Assert
        await Assert.ThrowsAsync<ValidationException>(() => _service.Create(request));
    }

    [Fact]
    public async Task Create_CuandoElSlotNoEstaDisponible_EntoncesLanzaConflictException()
    {
        // Arrange
        var doctorId = Guid.NewGuid();
        var patient = new Patient(Guid.NewGuid(), 30123456, "Ana Gomez");
        var slot = BuildSlotConReglaYDoctor(doctorId);
        slot.Book(); // ya no está disponible

        var request = BuildRequestValido(doctorId, slot.Id, patient.Dni);

        _mockPersistence.First<Patient>(Arg.Any<Expression<Func<Patient, bool>>>()).Returns(patient);
        _mockPersistence.GetById<AvailabilitySlot>(Arg.Any<Guid>(), Arg.Any<string[]>()).Returns(slot);

        // Act - Assert
        await Assert.ThrowsAsync<ConflictException>(() => _service.Create(request));
    }

    [Fact]
    public async Task Create_CuandoElTurnoEsEnElPasado_EntoncesLanzaValidationException()
    {
        // Arrange
        var doctorId = Guid.NewGuid();
        var patient = new Patient(Guid.NewGuid(), 30123456, "Ana Gomez");

        var slot = new AvailabilitySlot(
            Guid.NewGuid(),
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)), // ayer
            new TimeOnly(10, 0),
            new TimeOnly(10, 30));
        var rule = new AvailabilityRule(doctorId, 1, 2026, 1, new TimeOnly(9, 0), new TimeOnly(12, 0));
        SetNavigation(slot, nameof(AvailabilitySlot.AvailabilityRule), rule);

        var request = BuildRequestValido(doctorId, slot.Id, patient.Dni);

        _mockPersistence.First<Patient>(Arg.Any<Expression<Func<Patient, bool>>>()).Returns(patient);
        _mockPersistence.GetById<AvailabilitySlot>(Arg.Any<Guid>(), Arg.Any<string[]>()).Returns(slot);

        // Act - Assert
        await Assert.ThrowsAsync<ValidationException>(() => _service.Create(request));
    }

    [Fact]
    public async Task Cancel_CuandoLaCitaNoExiste_EntoncesLanzaEntityNotFoundException()
    {
        // Arrange
        _mockPersistence.GetById<Appointment>(Arg.Any<Guid>(), Arg.Any<string[]>()).Returns((Appointment?)null);

        // Act - Assert
        await Assert.ThrowsAsync<EntityNotFoundException>(() => _service.Cancel(Guid.NewGuid()));
    }

    [Fact]
    public async Task Cancel_CuandoLaCitaNoEstaReservada_EntoncesLanzaConflictException()
    {
        // Arrange
        var appointment = new Appointment(Guid.NewGuid(), Guid.NewGuid(), "Control de rutina");
        appointment.Cancel(); // ya está cancelada, no debería poder cancelarse otra vez

        _mockPersistence.GetById<Appointment>(Arg.Any<Guid>(), Arg.Any<string[]>()).Returns(appointment);

        // Act - Assert
        await Assert.ThrowsAsync<ConflictException>(() => _service.Cancel(appointment.Id));
    }

    [Fact]
    public async Task Cancel_ConDatosValidos_EntoncesCancelaLaCitaYLiberaElSlot()
    {
        // Arrange
        var slot = new AvailabilitySlot(
            Guid.NewGuid(), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)), new TimeOnly(10, 0), new TimeOnly(10, 30));
        slot.Book(); // el slot está reservado, como corresponde a una cita activa

        var appointment = new Appointment(slot.Id, Guid.NewGuid(), "Control de rutina");
        SetNavigation(appointment, nameof(Appointment.AvailabilitySlot), slot);

        _mockPersistence.GetById<Appointment>(Arg.Any<Guid>(), Arg.Any<string[]>()).Returns(appointment);

        // Act
        await _service.Cancel(appointment.Id);

        // Assert
        Assert.Equal(AppointmentStatus.Cancelled, appointment.Status);
        Assert.Equal(AvailabilitySlotStatus.Available, slot.Status);
        await _mockPersistence.Received(1).Update(slot);
        await _mockPersistence.Received(1).Update(appointment);
    }
}