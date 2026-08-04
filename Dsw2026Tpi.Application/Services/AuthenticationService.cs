using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.CrossCutting.Exceptions;
using Dsw2026Tpi.CrossCutting.Helpers;
using Dsw2026Tpi.CrossCutting.Identity;
using Dsw2026Tpi.CrossCutting.Resources;
using Dsw2026Tpi.Data.Identity;
using Dsw2026Tpi.Domain.Entities;
using Dsw2026Tpi.Domain.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Dsw2026Tpi.Application.Services;

public class AuthenticationService : IAuthenticationService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ISignInService _signInManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly JwtService _jwtService;
    private readonly ILogger<AuthenticationService> _logger;
    private readonly IPersistence _persistence;

    public AuthenticationService(UserManager<ApplicationUser> userManager,
        ISignInService signInManager,
        RoleManager<IdentityRole> roleManager,
        JwtService jwtService,
        ILogger<AuthenticationService> logger, IPersistence persistence)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _roleManager = roleManager;
        _jwtService = jwtService;
        _logger = logger;
        _persistence = persistence;
    }

    public async Task<LoginAdminModel.Response> LoginAdmin(LoginAdminModel.Request request)
    {
        if (!request.Email.IsEmailValid()) throw new AuthenticationException();
        var user = await _userManager.FindByEmailAsync(request.Email) ?? throw new AuthenticationException();
        var result = await _signInManager.CheckPassword(user, request.Password);

        if (!result)
        {
            _logger.LogError("Intento de login fallido para: {Email}", request.Email);
            throw new AuthenticationException();
        }

        var role = (await _userManager.GetRolesAsync(user)).FirstOrDefault();

        var token  = _jwtService.GenerateToken(user.UserName!, role);

        return new LoginAdminModel.Response(
            token,
            role
        );
    }

    public async Task<LoginPatientModel.Response> LoginPatient(LoginPatientModel.Request request)
    {
        if (!request.Email.IsEmailValid()) throw new AuthenticationException();
        var user = await _userManager.FindByEmailAsync(request.Email);
        var dniAsPassword = "Dni#" + request.Dni.ToString();

        if (user == null)
        {
            var dniExists = await _persistence.First<Patient>(p => p.Dni == request.Dni);
            if (dniExists != null)
            {
                _logger.LogError("Intento con DNI ya existente : {Dni}", request.Dni);
                throw new ConflictException(nameof(ErrorCodes.PATIENT_DNI_CONFLICT), ErrorCodes.PATIENT_DNI_CONFLICT);
            }
            user = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var createResult = await _userManager.CreateAsync(user, dniAsPassword); 
            if (!createResult.Succeeded) throw new AuthenticationException();

            await _userManager.AddToRoleAsync(user, Roles.Patient);

            var patient = new Patient(Guid.Parse(user.Id), request.Dni)
            {
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            await _persistence.Add(patient);
        }
        else
        {
            var roles = await _userManager.GetRolesAsync(user);
            if (!roles.Contains(Roles.Patient)) throw new AuthenticationException();

            var patient = await _persistence.First<Patient>(p => p.UserId == Guid.Parse(user.Id));
            if(patient == null || patient.Dni != request.Dni)
            {
                _logger.LogError("Intento de loging fallido para: {Email}", request.Email);
                throw new AuthenticationException();
            }
        }
        var token = _jwtService.GenerateToken(user.UserName!, Roles.Patient);

        return new LoginPatientModel.Response(token, Roles.Patient);
    }

    
}
