﻿using ListGenius.Api.Entities.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ListGenius.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UserController(UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    RoleManager<IdentityRole> roleManager,
    IConfiguration configuration) : ControllerBase
{
    [HttpPost("Register")]
    public async Task<IActionResult> CreateUser([FromBody] UserRegister model)
    {
        var user = new ApplicationUser { UserName = model.Email, Email = model.Email, FullName = model.FullName };

        var result = await userManager.CreateAsync(user, model.Password);

        if (result.Succeeded)
        {
            const string defaultRole = "User";

            if (!await roleManager.RoleExistsAsync(defaultRole))
            {
                await roleManager.CreateAsync(new IdentityRole(defaultRole));
            }

            await userManager.AddToRoleAsync(user, defaultRole);
            return Ok(model);
        }
        else
        {
            return BadRequest("Usuário ou senha inválidos");
        }
    }

    [HttpPost("Login")]
    public async Task<ActionResult<UserToken>> Login([FromBody] User userInfo)
    {
        var result = await signInManager.PasswordSignInAsync(userInfo.Email,
                         userInfo.Password, isPersistent: false, lockoutOnFailure: false);

        if (result.Succeeded)
        {
            return BuildToken(userInfo);
        }
        else
        {
            ModelState.AddModelError(string.Empty, "login inválido.");
            return BadRequest(ModelState);
        }
    }

    private UserToken BuildToken(User userInfo)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.UniqueName, userInfo.Email),
            new Claim("joaodiasdev", "http://joaodiasdev.com"),
            new Claim(JwtRegisteredClaimNames.Aud, configuration["Jwt:Audience"] ?? string.Empty),
            new Claim(JwtRegisteredClaimNames.Iss, configuration["Jwt:Issuer"] ?? string.Empty),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT:key"] ?? string.Empty));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var expiration = DateTime.UtcNow.AddDays(7);

        JwtSecurityToken token = new(
           issuer: null,
           audience: null,
           claims: claims,
           expires: expiration,
           signingCredentials: creds);

        return new UserToken()
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            Expiration = expiration
        };
    }
}