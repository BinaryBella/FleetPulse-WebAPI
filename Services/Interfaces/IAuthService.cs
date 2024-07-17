//IAuth Service

using FleetPulse_BackEndDevelopment.Models;
using Google.Apis.Auth.OAuth2.Responses;
using System.Threading.Tasks;

public interface IAuthService
{
    // Authentication Methods
    User? IsAuthenticated(string username, string password);
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
   Task<bool> AddNotificationAsync(FCMNotification notification);
   Task<bool> UpdateUserProfilePictureAsync(string username, string profilePicture);
}