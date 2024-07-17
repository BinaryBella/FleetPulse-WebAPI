using System.Security.Claims;
using FleetPulse_BackEndDevelopment.Models;

namespace FleetPulse_BackEndDevelopment.Services.Interfaces;

public interface IJwtService
{
    string GenerateAccessToken(string userName, string[] roles);
    string GenerateRefreshToken();
    Task<bool> AddRefreshTokenAsync(RefreshToken refreshToken);
    Task<bool> DeleteRefreshTokenAsync(string token);
    Task<RefreshToken?> GetRefreshTokenData(string token);
    ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
    
}