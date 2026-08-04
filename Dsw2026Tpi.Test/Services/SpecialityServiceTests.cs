using System.Linq.Expressions;
using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Services;
using Dsw2026Tpi.CrossCutting.Exceptions;
using Dsw2026Tpi.Domain.Entities;
using Dsw2026Tpi.Domain.Interfaces;
using NSubstitute;

namespace Dsw2026Tpi.Test.Services;

public class SpecialityServiceTests
{
    private readonly IPersistence _mockPersistence = Substitute.For<IPersistence>();
    private readonly SpecialityService _service;

    public SpecialityServiceTests()
    {
        _service = new SpecialityService(_mockPersistence);
    }

    [Fact]
    public async Task Create_ConDatosValidos_EntoncesCreaLaEspecialidad()
    {
        
        var request = new SpecialityModel.Request("Cardiología", "Especialidad del corazón");

        _mockPersistence
            .First<Speciality>(Arg.Any<Expression<Func<Speciality, bool>>>())
            .Returns((Speciality?)null);

        _mockPersistence
            .Add(Arg.Any<Speciality>())
            .Returns(callInfo => callInfo.Arg<Speciality>());

      
        var response = await _service.Create(request);

        
        Assert.Equal(request.Name, response.Name);
        Assert.Equal(request.Description, response.Description);
        await _mockPersistence.Received(1).Add(Arg.Any<Speciality>());
    }

    [Fact]
    public async Task Create_CuandoYaExisteUnaEspecialidadConEseNombre_EntoncesLanzaConflictException()
    {
      
        var request = new SpecialityModel.Request("Cardiología", "Especialidad del corazón");
        var existente = new Speciality("Cardiología", "Otra descripción");

        _mockPersistence
            .First<Speciality>(Arg.Any<Expression<Func<Speciality, bool>>>())
            .Returns(existente);

      
        await Assert.ThrowsAsync<ConflictException>(() => _service.Create(request));
        await _mockPersistence.DidNotReceive().Add(Arg.Any<Speciality>());
    }

    [Theory]
    [InlineData("", "Descripción válida y larga")]     
    [InlineData("Ca", "Descripción válida y larga")]    
    [InlineData("Cardiología", "corta")]                
    public async Task Create_ConDatosInvalidos_EntoncesLanzaValidationException(string nombre, string descripcion)
    {
        
        var request = new SpecialityModel.Request(nombre, descripcion);

       
        await Assert.ThrowsAsync<ValidationException>(() => _service.Create(request));
        await _mockPersistence.DidNotReceive().Add(Arg.Any<Speciality>());
    }

    [Fact]
    public async Task Update_CuandoLaEspecialidadNoExiste_EntoncesLanzaEntityNotFoundException()
    {
        
        var id = Guid.NewGuid();
        var request = new SpecialityModel.Request("Cardiología", "Especialidad del corazón");

        _mockPersistence
            .GetById<Speciality>(id)
            .Returns((Speciality?)null);

        
        await Assert.ThrowsAsync<EntityNotFoundException>(() => _service.Update(id, request));
    }

    [Fact]
    public async Task Update_ConDatosValidos_EntoncesActualizaLaEspecialidad()
    {
       
        var speciality = new Speciality("Cardiología", "Descripción original larga");
        var request = new SpecialityModel.Request("Traumatología", "Descripción nueva y larga");

        _mockPersistence
            .GetById<Speciality>(speciality.Id)
            .Returns(speciality);

        _mockPersistence
            .First<Speciality>(Arg.Any<Expression<Func<Speciality, bool>>>())
            .Returns((Speciality?)null);

       
        var response = await _service.Update(speciality.Id, request);

        
        Assert.Equal(request.Name, response.Name);
        Assert.Equal(request.Description, response.Description);
        await _mockPersistence.Received(1).Update(Arg.Any<Speciality>());
    }

    [Fact]
    public async Task Delete_CuandoLaEspecialidadNoExiste_EntoncesLanzaEntityNotFoundException()
    {
        
        var id = Guid.NewGuid();
        _mockPersistence
            .GetById<Speciality>(id)
            .Returns((Speciality?)null);

       
        await Assert.ThrowsAsync<EntityNotFoundException>(() => _service.Delete(id));
    }

    [Fact]
    public async Task Delete_CuandoLaEspecialidadExiste_EntoncesLaElimina()
    {
       
        var speciality = new Speciality("Cardiología", "Descripción original larga");
        _mockPersistence
            .GetById<Speciality>(speciality.Id)
            .Returns(speciality);

       
        await _service.Delete(speciality.Id);

        
        await _mockPersistence.Received(1).Delete(speciality);
    }
}