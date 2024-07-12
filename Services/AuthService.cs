//Auth Service

using FleetPulse_BackEndDevelopment.Models;
using FleetPulse_BackEndDevelopment.Data;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Google.Apis.Auth.OAuth2.Responses;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace FleetPulse_BackEndDevelopment.Services
{
    public class AuthService : IAuthService
    {
        private readonly FleetPulseDbContext dataContext;
        private readonly IConfiguration configuration;

        public AuthService(FleetPulseDbContext dataContext, IConfiguration configuration)
        {
            this.dataContext = dataContext;
            this.configuration = configuration;
        }

        public User IsAuthenticated(string username, string password)
        {
            var user = this.GetByUsername(username);
            return this.DoesUserExists(username) && BCrypt.Net.BCrypt.Verify(password, user.HashedPassword) ? user : null;
        }

        public async Task<int> GetUserCountAsync()
        {
            return await dataContext.Users.CountAsync();
        }
        
        public bool DoesUserExists(string username)
        {
            var user = dataContext.Users.FirstOrDefault(x => x.UserName == username);
            return user != null;
        }

        public bool DoesEmailExists(string email)
        {
            var user = dataContext.Users.FirstOrDefault(x => x.EmailAddress == email);
            return user != null;
        }

        public string GetUsernameByEmail(string email)
        {
            var user = dataContext.Users.FirstOrDefault(x => x.EmailAddress == email);
            return user != null ? user.UserName : null;
        }

        public User GetById(int id)
        {
            return dataContext.Users.FirstOrDefault(c => c.UserId == id);
        }

        public User[] GetAll()
        {
            return this.dataContext.Users.ToArray();
        }

        public User GetByUsername(string username)
        {
            return dataContext.Users.FirstOrDefault(c => c.UserName == username);
        }

        public User RegisterUser(User model)
        {
            model.HashedPassword = BCrypt.Net.BCrypt.HashPassword(model.HashedPassword);

            var userEntity = dataContext.Users.Add(model);
            dataContext.SaveChanges();

            return userEntity.Entity;
        }

        public string DecodeEmailFromToken(string token)
        {
            var decodedToken = new JwtSecurityTokenHandler();
            var indexOfTokenValue = 7;

            var t = decodedToken.ReadJwtToken(token.Substring(indexOfTokenValue));

            return t.Payload.FirstOrDefault(x => x.Key == "username").Value.ToString();
        }

        public User ChangeRole(string username, string jobTitle)
        {
            var user = this.GetByUsername(username);
            user.JobTitle = jobTitle;
            this.dataContext.SaveChanges();

            return user;
        }

        public User GetByEmail(string email)
        {
            return dataContext.Users.FirstOrDefault(c => c.EmailAddress == email);
        }

        public bool ResetPassword(string email, string newPassword)
        {
            var user = GetByEmail(email);
            if (user != null)
            {
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(newPassword);

                user.HashedPassword = hashedPassword;
                dataContext.SaveChanges();

                return true;
            }
            return false;
        }

        public async Task<User> GetUserByUsernameAsync(string username)
        {
            return await dataContext.Users.FirstOrDefaultAsync(u => u.UserName == username);
        }

        public async Task<User> AddUserAsync(User user)
        {
            dataContext.Users.Add(user);
            await dataContext.SaveChangesAsync();
            return user;
        }

        public async Task<bool> UpdateUserAsync(User user)
        {
            dataContext.Entry(user).State = EntityState.Modified;
            await dataContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeactivateUserAsync(User user)
        {
            user.Status = false;
            await dataContext.SaveChangesAsync();
            return true;
        }
        public async Task<User> GetUserByEmailAsync(string email)
        {
            return await dataContext.Users.FirstOrDefaultAsync(u => u.EmailAddress == email);
        }

        public async Task<bool> ResetPasswordAsync(string email, string newPassword)
        {
            var user = await GetUserByEmailAsync(email);
            if (user != null)
            {
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(newPassword);
                user.HashedPassword = hashedPassword;
                await dataContext.SaveChangesAsync();
                return true;
            }
            return false;
        }
        
        public async Task<bool> ResetDriverPasswordAsync(string emailAddress, string newPassword)
        {
            var user = await dataContext.Users.SingleOrDefaultAsync(u => u.EmailAddress == emailAddress);
            if (user == null)
                return false;

            user.HashedPassword = BCrypt.Net.BCrypt.HashPassword(newPassword);
            await dataContext.SaveChangesAsync();
            
            return true;
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
            }
        }

        public async Task<bool> AddNotificationAsync(FCMNotification notification)
        {
            try
            {
                await dataContext.FCMNotifications.AddAsync(notification);
                await dataContext.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<int?> GetUserIdByNICAsync(string nic)
        {
            var user = await dataContext.Users.FirstOrDefaultAsync(c => c.NIC == nic);
            return user?.UserId;
        }

        public async Task<TokenResponse> Authenticate(User user)
        {
            var existingUser = await dataContext.Users
                .SingleOrDefaultAsync(u => u.UserName == user.UserName);

            if (existingUser == null || !BCrypt.Net.BCrypt.Verify(user.HashedPassword, existingUser.HashedPassword))
                throw new UnauthorizedAccessException();

            var token = await GenerateJwtToken(existingUser.UserName, existingUser.JobTitle);
            var refreshToken = await GenerateRefreshToken(existingUser.UserId);
            
            return new TokenResponse { AccessToken = token, RefreshToken = refreshToken };
        }

        public async Task<string> GenerateJwtToken(string username, string jobTitle)
        {
            var issuer = configuration["Jwt:Issuer"];
            var audience = configuration["Jwt:Audience"];
            var key = Encoding.ASCII.GetBytes(configuration["Jwt:Key"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim("Id", Guid.NewGuid().ToString()),
                    new Claim(JwtRegisteredClaimNames.Sub, username),
                    new Claim(JwtRegisteredClaimNames.Email, username),
                    new Claim(ClaimTypes.Role, jobTitle),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                }),
                Expires = DateTime.UtcNow.AddMinutes(30),
                Issuer = issuer,
                Audience = audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha512Signature)
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return await Task.FromResult(tokenHandler.WriteToken(token));
        }

        public async Task<string> GenerateRefreshToken(int userId)
        {
            var randomBytes = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }

            var refreshToken = new RefreshToken
            {
                Token = Convert.ToBase64String(randomBytes),
                Expires = DateTime.UtcNow.AddDays(7),
                IsRevoked = false,
                UserId = userId
            };

            await dataContext.RefreshTokens.AddAsync(refreshToken);
            await dataContext.SaveChangesAsync();

            return refreshToken.Token;
        }

        public async Task<TokenResponse> RefreshToken(string token)
        {
            var refreshTokenEntity = await dataContext.RefreshTokens
                .SingleOrDefaultAsync(rt => rt.Token == token && rt.Expires > DateTime.UtcNow && !rt.IsRevoked);

            if (refreshTokenEntity == null)
                throw new UnauthorizedAccessException();

            var userId = refreshTokenEntity.UserId;
            var user = await dataContext.Users.FindAsync(userId);
            if (user == null)
                throw new UnauthorizedAccessException();

            var newJwtToken = await GenerateJwtToken(user.UserName, user.JobTitle);
            var newRefreshToken = await GenerateRefreshToken(userId);

            refreshTokenEntity.IsRevoked = true;

            await dataContext.SaveChangesAsync();

            return new TokenResponse { AccessToken = newJwtToken, RefreshToken = newRefreshToken };
        }

        public async Task<bool> IsRefreshTokenValidAsync(string token)
        {
            var refreshToken = await dataContext.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == token);
            return refreshToken != null && refreshToken.Expires > DateTime.UtcNow && !refreshToken.IsRevoked;
        }

        public async Task<bool> RevokeToken(string token)
        {
            var refreshTokenEntity = await dataContext.RefreshTokens
                .SingleOrDefaultAsync(rt => rt.Token == token);

            if (refreshTokenEntity == null)
                return false;

            refreshTokenEntity.IsRevoked = true;
            await dataContext.SaveChangesAsync();

            return true;
        }

        public async Task<RefreshToken> GetRefreshTokenAsync(string token)
        {
            return await dataContext.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == token);
        }

        public async Task<bool> RevokeRefreshTokenAsync(string token)
        {
            var refreshTokenEntity = await dataContext.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == token);

            if (refreshTokenEntity == null)
                return false;

            refreshTokenEntity.IsRevoked = true;
            await dataContext.SaveChangesAsync();

            return true;
        }

        public async Task<bool> AddRefreshTokenAsync(RefreshToken refreshToken)
        {
            await dataContext.RefreshTokens.AddAsync(refreshToken);
            await dataContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ValidateRefreshToken(string token)
        {
            var refreshToken = await dataContext.RefreshTokens
                .SingleOrDefaultAsync(rt => rt.Token == token && rt.Expires > DateTime.UtcNow && !rt.IsRevoked);

            return refreshToken != null;
        }
        
        public async Task<bool> UpdateUserProfilePictureAsync(string username, string profilePicture)
        {
            try
            {
                var user = await GetUserByUsernameAsync(username);

                if (user == null)
                {
                    return false; 
                }

                if (string.IsNullOrEmpty(profilePicture))
                {
                    user.ProfilePicture = null;
                }
                else
                {
                    user.ProfilePicture = Convert.FromBase64String(profilePicture);
                }

                var result = await UpdateUserAsync(user);

                return result;
            }
            catch (Exception ex)
            {
                // Optionally handle or log the exception
                throw; // Propagate the exception
            }
        }
    }
}