namespace FleetPulse_BackEndDevelopment.Services.Interfaces;

public interface IEmailUserCredentialService
{
    Task SendUsernameAndPassword(string emailAddress, string userName, string password);
}