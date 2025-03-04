namespace PizzaShopServices.Interfaces
{
    public interface IEmailService
    {
        Task SendResetPasswordEmailAsync(string email, string resetPasswordUrl);

        Task SendEmailAsync(string toEmail, string username, string temporaryPassword);
    }
}