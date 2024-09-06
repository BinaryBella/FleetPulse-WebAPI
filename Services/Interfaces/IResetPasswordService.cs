// IResetPasswordService.cs
using System.Threading.Tasks;

namespace FleetPulse_BackEndDevelopment.Services.Interfaces
{
    public interface IResetPasswordService
    {
        Task SendResetPasswordEmailAsync(string toEmail, string userName, string userEmail);
    }
}