//IAuth Service

using FleetPulse_BackEndDevelopment.Models;
using Google.Apis.Auth.OAuth2.Responses;
using System.Threading.Tasks;

public interface IAuthService
{
    // Authentication Methods
    User IsAuthenticated(string username, string password);
    Task<TokenResponse> Authenticate(User user);
    Task<TokenResponse> RefreshToken(string token);
    Task<bool> RevokeToken(string token);

    // User Existence Checks
    bool DoesUserExists(string username);
    bool DoesEmailExists(string email);

    // User Queries
    User GetById(int id);
    User[] GetAll();
    User? GetByUsername(string username);
    User GetByEmail(string email);
    Task<User> GetUserByEmailAsync(string email);
    Task<int> GetUserCountAsync();

    // User Registration and Management
    bool ResetPassword(string email, string newPassword);

    // Miscellaneous User Operations
    string GetUsernameByEmail(string email);
    Task<User?> GetUserByUsernameAsync(string username);
    Task<User?> AddUserAsync(User? user);
    Task<bool> UpdateUserAsync(User user);
    Task<bool> ResetPasswordAsync(string emailAddress, string newPassword);
    Task<bool> ResetDriverPasswordAsync(string emailAddress, string newPassword);
    Task<int?> GetUserIdByNICAsync(string nic);

    // Refresh Token Handling
    Task<bool> AddRefreshTokenAsync(RefreshToken refreshToken);
    Task<RefreshToken> GetRefreshTokenAsync(string token);
    Task<bool> RevokeRefreshTokenAsync(string token);
    Task<bool> IsRefreshTokenValidAsync(string token);

    // JWT Token Handling
    Task<string> GenerateJwtToken(string username, string jobTitle);
    Task<string> GenerateRefreshToken(int userId);
    Task<bool> ValidateRefreshToken(string token);
    Task<bool> AddNotificationAsync(FCMNotification notification);
   Task<bool> UpdateUserProfilePictureAsync(string username, string profilePicture);
}