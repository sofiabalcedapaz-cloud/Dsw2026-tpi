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

    private readonly IPersistence _mockPersistence = Substitute.For<IPersistence>();
    private readonly DoctorService _service;

    public DoctorServiceTests()
    {
        _service = new DoctorService(_mockPersistence);
    }

    [Fact]
    public async Task Create_ConDatosValidos_EntoncesCreaElDoctor()
    {

        var speciality = new Speciality("Cardiología", "Especialidad del corazón");
        var request = new DoctorModel.Request("Juan Perez", "MP1234", speciality.Id);

    
        _mockPersistence
            .First<Doctor>(Arg.Any<Expression<Func<Doctor, bool>>>())
            .Returns((Doctor?)null);


        _mockPersistence
            .GetById<Speciality>(speciality.Id)
            .Returns(speciality);


        _mockPersistence
            .Add(Arg.Any<Doctor>())
            .Returns(callInfo => callInfo.Arg<Doctor>());


        var response = await _service.Create(request);

    
        Assert.Equal(request.Name, response.Name);
        Assert.Equal(request.LicenseNumber, response.LicenseNumber);
        Assert.Equal(speciality.Id, response.Speciality?.SpecialityId);


        await _mockPersistence.Received(1).Add(Arg.Any<Doctor>());
    }

    [Fact]
    public async Task Create_CuandoLaEspecialidadNoExiste_EntoncesLanzaEntityNotFoundException()
    {
    
        var request = new DoctorModel.Request("Juan Perez", "MP1234", Guid.NewGuid());

        _mockPersistence
            .First<Doctor>(Arg.Any<Expression<Func<Doctor, bool>>>())
            .Returns((Doctor?)null);


        _mockPersistence
            .GetById<Speciality>(Arg.Any<Guid>())
            .Returns((Speciality?)null);


        await Assert.ThrowsAsync<EntityNotFoundException>(() => _service.Create(request));


        await _mockPersistence.DidNotReceive().Add(Arg.Any<Doctor>());
    }

    [Fact]
    public async Task Create_CuandoYaExisteUnDoctorConEsaMatricula_EntoncesLanzaConflictException()
    {
      
        var speciality = new Speciality("Cardiología", "Especialidad del corazón");
        var request = new DoctorModel.Request("Juan Perez", "MP1234", speciality.Id);

        var doctorExistente = new Doctor("Otro Doctor", "MP1234", speciality);

   
        _mockPersistence
            .First<Doctor>(Arg.Any<Expression<Func<Doctor, bool>>>())
            .Returns(doctorExistente);

   
        await Assert.ThrowsAsync<ConflictException>(() => _service.Create(request));

        await _mockPersistence.DidNotReceive().Add(Arg.Any<Doctor>());
    }

    [Theory]
    [InlineData("", "MP1234")]                
    [InlineData("Jo", "MP1234")]              
    [InlineData("Juan Perez", "")]             
    public async Task Create_ConDatosInvalidos_EntoncesLanzaValidationException(string nombre, string matricula)
    {
      
        var request = new DoctorModel.Request(nombre, matricula, Guid.NewGuid());

        
        await Assert.ThrowsAsync<ValidationException>(() => _service.Create(request));

      
        await _mockPersistence.DidNotReceive().Add(Arg.Any<Doctor>());
    }
}