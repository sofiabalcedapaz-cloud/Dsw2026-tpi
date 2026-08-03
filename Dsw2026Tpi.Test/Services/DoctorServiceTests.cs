using System.Linq.Expressions;
using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Services;
using Dsw2026Tpi.CrossCutting.Exceptions;
using Dsw2026Tpi.Domain.Entities;
using Dsw2026Tpi.Domain.Interfaces;
using NSubstitute;

namespace Dsw2026Tpi.Test.Services;

public class DoctorServiceTests
{
    // El mock de la "cocina": lo recreamos en cada test a través del constructor
    // (xUnit crea una instancia nueva de esta clase por cada [Fact], así que no hay
    // riesgo de que un test "ensucie" a otro).
    private readonly IPersistence _mockPersistence = Substitute.For<IPersistence>();
    private readonly DoctorService _service;

    public DoctorServiceTests()
    {
        _service = new DoctorService(_mockPersistence);
    }

    [Fact]
    public async Task Create_ConDatosValidos_EntoncesCreaElDoctor()
    {
        // Arrange
        var speciality = new Speciality("Cardiología", "Especialidad del corazón");
        var request = new DoctorModel.Request("Juan Perez", "MP1234", speciality.Id);

        // "Cuando busques si ya existe un doctor con esta matrícula, no encontraste ninguno"
        _mockPersistence
            .First<Doctor>(Arg.Any<Expression<Func<Doctor, bool>>>())
            .Returns((Doctor?)null);

        // "Cuando busques la especialidad por Id, encontrala"
        _mockPersistence
            .GetById<Speciality>(speciality.Id)
            .Returns(speciality);

        // "Cuando te pidan guardar (Add), devolvé el mismo doctor que te pasaron"
        _mockPersistence
            .Add(Arg.Any<Doctor>())
            .Returns(callInfo => callInfo.Arg<Doctor>());

        // Act
        var response = await _service.Create(request);

        // Assert
        Assert.Equal(request.Name, response.Name);
        Assert.Equal(request.LicenseNumber, response.LicenseNumber);
        Assert.Equal(speciality.Id, response.Speciality?.SpecialityId);

        // Verificamos que el service efectivamente le haya pedido a la "cocina" que guarde
        await _mockPersistence.Received(1).Add(Arg.Any<Doctor>());
    }

    [Fact]
    public async Task Create_CuandoLaEspecialidadNoExiste_EntoncesLanzaEntityNotFoundException()
    {
        // Arrange
        var request = new DoctorModel.Request("Juan Perez", "MP1234", Guid.NewGuid());

        _mockPersistence
            .First<Doctor>(Arg.Any<Expression<Func<Doctor, bool>>>())
            .Returns((Doctor?)null);

        // "Cuando busques la especialidad, no la encontraste"
        _mockPersistence
            .GetById<Speciality>(Arg.Any<Guid>())
            .Returns((Speciality?)null);

        // Act - Assert
        await Assert.ThrowsAsync<EntityNotFoundException>(() => _service.Create(request));

        // Como la especialidad no existía, nunca debería haber intentado guardar
        await _mockPersistence.DidNotReceive().Add(Arg.Any<Doctor>());
    }

    [Fact]
    public async Task Create_CuandoYaExisteUnDoctorConEsaMatricula_EntoncesLanzaConflictException()
    {
        // Arrange
        var speciality = new Speciality("Cardiología", "Especialidad del corazón");
        var request = new DoctorModel.Request("Juan Perez", "MP1234", speciality.Id);

        var doctorExistente = new Doctor("Otro Doctor", "MP1234", speciality);

        // "Cuando busques si ya existe un doctor con esta matrícula, encontraste este otro"
        _mockPersistence
            .First<Doctor>(Arg.Any<Expression<Func<Doctor, bool>>>())
            .Returns(doctorExistente);

        // Act - Assert
        await Assert.ThrowsAsync<ConflictException>(() => _service.Create(request));

        await _mockPersistence.DidNotReceive().Add(Arg.Any<Doctor>());
    }

    [Theory]
    [InlineData("", "MP1234")]                 // nombre vacío
    [InlineData("Jo", "MP1234")]                // nombre muy corto (< 3)
    [InlineData("Juan Perez", "")]              // matrícula vacía
    public async Task Create_ConDatosInvalidos_EntoncesLanzaValidationException(string nombre, string matricula)
    {
        // Arrange
        var request = new DoctorModel.Request(nombre, matricula, Guid.NewGuid());

        // Act - Assert
        await Assert.ThrowsAsync<ValidationException>(() => _service.Create(request));

        // La validación de formato falla ANTES de tocar la persistencia
        await _mockPersistence.DidNotReceive().Add(Arg.Any<Doctor>());
    }
}