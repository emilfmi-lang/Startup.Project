using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Startup_Project.Application.Abstractions.Services;
using Startup_Project.Application.Dtos.Account;
using Startup_Project.Application.Models;
using Startup_Project.Application.Services.Interfaces.Account;
using Startup_Project.Application.Validators.Account;
using Startup_Project.Domain.Entities.Identity;
using Startup_Project.Domain.Exceptions;

namespace Startup_Project.Application.Services.Concretes.Account;

public class AccountService(UserManager<AppUser> userManager,
    IMapper mapper,
    IJwtTokenService jwtTokenService) : IAccountService
{
    public async Task<ResponseModel<string>> LoginAsync(UserLoginDto dto)
    {
        var user = await userManager.FindByEmailAsync(dto.Email);
        if (user == null)
            throw new UserNotFoundException("İstifadəçi tapılmadı");
        var isPasswordValid = await userManager.CheckPasswordAsync(user, dto.Password);
        if (!isPasswordValid)
            throw new Exception("Email və ya şifrə yanlışdır.");
        var roles = await userManager.GetRolesAsync(user);
        var token = jwtTokenService.GenerateTokenAsync(user, roles);
        return ResponseModel<string>.SuccessResponse(token);
    }

    public async Task<ResponseModel<bool>> RegisterAsync(UserRegisterDto dto)
    {
        var validator = new UserRegisterDtoValidator();
        var validationResult = await validator.ValidateAsync(dto);

        if (!validationResult.IsValid)
        {
            var errorMessages = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            return ResponseModel<bool>.FailureResponse(errorMessages);
        }
        var user = mapper.Map<AppUser>(dto);
        var result = await userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
        {
            var identityErrors = result.Errors.Select(e => e.Description).ToList();
            return ResponseModel<bool>.FailureResponse(identityErrors);
        }
        return ResponseModel<bool>.SuccessResponse(true);

    }
    
}
