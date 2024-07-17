using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using FleetPulse_BackEndDevelopment.Data;
using FleetPulse_BackEndDevelopment.Models;
using FleetPulse_BackEndDevelopment.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace FleetPulse_BackEndDevelopment.Services;

public class JwtService : IJwtService
{
    private readonly IConfiguration _configuration;
    private readonly FleetPulseDbContext dataContext;

    public JwtService(IConfiguration configuration, FleetPulseDbContext dataContext)
    {
        _configuration = configuration;
        this.dataContext = dataContext;
    }

    public string GenerateAccessToken(string userName, string[] roles)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userName)
        };

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddMinutes(60), // Short-lived access token
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false,
            ValidateIssuer = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"])),
            ValidateLifetime = false
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);
        if (securityToken is not JwtSecurityToken jwtSecurityToken || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            throw new SecurityTokenException("Invalid token");

        return principal;
    }
    
    public async Task<bool> AddRefreshTokenAsync(RefreshToken refreshToken)
    {
        try {
            await dataContext.RefreshTokens.AddAsync(refreshToken);
            await dataContext.SaveChangesAsync();
            return true;
        } catch (Exception e) {
            Console.WriteLine(e);
            return false;
        }
    }
    
    public async Task<bool> DeleteRefreshTokenAsync(string token)
    {
        var refreshTokenEntity = await dataContext.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == token);

        if (refreshTokenEntity == null)
            return false;

        dataContext.RefreshTokens.Remove(refreshTokenEntity);
        await dataContext.SaveChangesAsync();

        return true;
    }
    
    public async Task<RefreshToken?> GetRefreshTokenData(string token)
    {
        return await dataContext.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == token);
    }
}