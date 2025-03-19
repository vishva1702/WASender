using System.Threading.Tasks;

namespace WASender.Services
{
    public interface IForgotPasswordService
    {
        Task<bool> ResetPasswordAsync(string email, string token, string password);
        Task<bool> SendPasswordResetEmailAsync(string email);
        Task TestSmtpConnectionAsync();
    }
}
