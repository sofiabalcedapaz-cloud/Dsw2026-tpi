using System.Linq.Expressions;
using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Services;
using Dsw2026Tpi.CrossCutting.Exceptions;
using Dsw2026Tpi.Domain.Entities;
using Dsw2026Tpi.Domain.Interfaces;
using NSubstitute;

namespace Dsw2026Tpi.Test.Services;

public class AvailabilityRuleServiceTests
{
    private readonly IPersistence _mockPersistence = Substitute.For<IPersistence>();
    private readonly AvailabilityRuleService _service;

    public AvailabilityRuleServiceTests()
    {
        _service = new AvailabilityRuleService(_mockPersistence);
    }

    // ---------- GetByDoctor ----------

    [Fact]
    public async Task GetByDoctor_CuandoElDoctorNoExiste_EntoncesLanzaEntityNotFoundException()
    {
        // Arrange
        var doctorId = Guid.NewGuid();
        _mockPersistence.GetById<Doctor>(doctorId).Returns((Doctor?)null);

        // Act - Assert
        await Assert.ThrowsAsync<EntityNotFoundException>(() => _service.GetByDoctor(doctorId));
    }

    [Fact]
    public async Task GetByDoctor_CuandoElDoctorExiste_EntoncesDevuelveLasReglas()
    {
        // Arrange
        var speciality = new Speciality("Cardiología", "Especialidad del corazón");
        var doctor = new Doctor("Juan Perez", "MP1234", speciality);

        var rule = new AvailabilityRule(doctor.Id, 1, 2026, 1, new TimeOnly(9, 0), new TimeOnly(12, 0));

        _mockPersistence.GetById<Doctor>(doctor.Id).Returns(doctor);
        _mockPersistence
            .GetFiltered<AvailabilityRule>(Arg.Any<Expression<Func<AvailabilityRule, bool>>>())
            .Returns(new List<AvailabilityRule> { rule });

        // Act
        var result = await _service.GetByDoctor(doctor.Id);

        // Assert
        Assert.Single(result);
        Assert.Equal(rule.Id, result.First().Id);
    }

    // ---------- Create ----------

    [Fact]
    public async Task Create_CuandoElDoctorNoExiste_EntoncesLanzaEntityNotFoundException()
    {
        // Arrange
        var request = new AvailabilityRuleModel.Request(
            Guid.NewGuid(),
            [new AvailabilityRuleModel.DayScheduleDto(1, "09:00", "12:00")]);

        _mockPersistence.GetById<Doctor>(request.DoctorId).Returns((Doctor?)null);

        // Act - Assert
        await Assert.ThrowsAsync<EntityNotFoundException>(() => _service.Create(request));
    }

    [Fact]
    public async Task Create_CuandoElHorarioEsInvalido_EntoncesLanzaValidationException()
    {
        // Arrange: hora de inicio posterior a la de fin
        var speciality = new Speciality("Cardiología", "Especialidad del corazón");
        var doctor = new Doctor("Juan Perez", "MP1234", speciality);
        var request = new AvailabilityRuleModel.Request(
            doctor.Id,
            [new AvailabilityRuleModel.DayScheduleDto(1, "12:00", "09:00")]);

        _mockPersistence.GetById<Doctor>(doctor.Id).Returns(doctor);

        // Act - Assert
        await Assert.ThrowsAsync<ValidationException>(() => _service.Create(request));
    }

    [Fact]
    public async Task Create_CuandoHaySolapamientoDeHorarios_EntoncesLanzaConflictException()
    {
        // Arrange
        var speciality = new Speciality("Cardiología", "Especialidad del corazón");
        var doctor = new Doctor("Juan Perez", "MP1234", speciality);
        var request = new AvailabilityRuleModel.Request(
            doctor.Id,
            [new AvailabilityRuleModel.DayScheduleDto(1, "09:00", "12:00")]);

        var reglaExistente = new AvailabilityRule(doctor.Id, 1, 2026, 1, new TimeOnly(10, 0), new TimeOnly(11, 0));

        _mockPersistence.GetById<Doctor>(doctor.Id).Returns(doctor);
        // "Ya existe una regla que se solapa con el horario pedido"
        _mockPersistence
            .First<AvailabilityRule>(Arg.Any<Expression<Func<AvailabilityRule, bool>>>())
            .Returns(reglaExistente);

        // Act - Assert
        await Assert.ThrowsAsync<ConflictException>(() => _service.Create(request));
    }

    // ---------- Update ----------

    [Fact]
    public async Task Update_CuandoElDoctorNoExiste_EntoncesLanzaEntityNotFoundException()
    {
        // Arrange
        var doctorId = Guid.NewGuid();
        var request = new AvailabilityRuleModel.Request(
            doctorId,
            [new AvailabilityRuleModel.DayScheduleDto(1, "09:00", "12:00")]);

        _mockPersistence.GetById<Doctor>(doctorId).Returns((Doctor?)null);

        // Act - Assert
        await Assert.ThrowsAsync<EntityNotFoundException>(() => _service.Update(doctorId, request));
    }
}