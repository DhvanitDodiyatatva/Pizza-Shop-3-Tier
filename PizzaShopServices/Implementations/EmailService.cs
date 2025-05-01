using System.Net;
using System.Net.Mail;
using PizzaShopServices.Interfaces;
using System.Threading.Tasks;
using PizzaShopRepository.Models;
using PizzaShopRepository.Interfaces;

namespace PizzaShopServices.Implementations
{
    public class EmailService : IEmailService
    {
        private readonly IUserRepository _userRepository;

        private readonly string _logoPath = Path.Combine(Directory.GetCurrentDirectory(), "images", "logos", "pizzashop_logo.png");

        public EmailService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }
        public async Task SendResetPasswordEmailAsync(string email, string resetPasswordUrl)
        {



            var smtpClient = new SmtpClient("mail.etatvasoft.com")
            {
                Port = 587,
                Credentials = new NetworkCredential("test.dotnet@etatvasoft.com", "P}N^{z-]7Ilp"),
                EnableSsl = true
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress("test.dotnet@etatvasoft.com"),
                Subject = "Reset Password - PIZZASHOP",
                IsBodyHtml = true,
                Body = $@"
                    <html lang='en'>
                    <head>
                        <meta charset='UTF-8'>
                        <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                        <title>Reset Password - PIZZASHOP</title>
                        <style>
                            body {{
                                font-family: Arial, sans-serif;
                                background-color: #f4f4f4;
                                margin: 0;
                                padding: 0;
                                height: 100vh;
                            }}
                            .container {{
                                background-color: white;
                                width: 750px;
                                box-shadow: 0 4px 8px rgba(0, 0, 0, 0.1);
                                border-radius: 8px;
                                overflow: hidden;
                            }}
                            .header {{
                                background-color: #0d5e99;
                                color: white;
                                text-align: center;
                                padding: 15px;
                                font-size: 24px;
                                display: flex;
                                justify-content: center;
                                align-items: center;
                            }}
                            .content {{
                                padding: 20px;
                            }}
                            .content a {{
                                color: #0d5e99;
                                text-decoration: none;
                                font-weight: bold;
                            }}
                            .content a:hover {{
                                text-decoration: underline;
                            }}
                            .important-note {{
                                background-color: #fff5cc;
                                padding: 10px;
                                border-left: 5px solid #ffcc00;
                                font-size: 14px;
                                margin-top: 10px;
                            }}
                        </style>
                    </head>
                    <body>
                        <div class='container'>
                            <div class='header'>
                                <img src='/images/logos/pizzashop_logo.png' alt='logo' style='max-height: 120px; max-width: 120px;'>
                                <h1>PIZZASHOP</h1>
                            </div>
                            <div class='content'>
                                <p><strong>Pizza shop,</strong></p>
                                <p>Please <a href='{resetPasswordUrl}' style=""color: #1f73ae; text-decoration: underline;"" > click here</a> to reset your account password.</p>
                                <p>If you encounter any issues or have any questions, please do not hesitate to contact our support team.</p>
                                <p class='important-note'><strong>Important Note:</strong> For security reasons, the link will expire in 24 hours. If you did not request a password reset, please ignore this email or contact our support team immediately.</p>
                            </div>
                        </div>
                    </body>
                    </html>"
            };

            mailMessage.To.Add(email);
            await smtpClient.SendMailAsync(mailMessage);
        }


        public async Task SendEmailAsync(string toEmail, string username, string temporaryPassword)
        {
            var fromEmail = "test.dotnet@etatvasoft.com"; // Sender email
            var subject = "Your Temporary Login Details for PIZZASHOP";
            var emailBody = $@"<!DOCTYPE html>
                        <html lang=""en"">
                        <head>
                            <meta charset=""UTF-8"">
                            <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                            <title>PIZZASHOP Email</title>
                            <style>
                                body {{
                                    font-family: Arial, sans-serif;
                                    margin: 0;
                                    padding: 0;
                                    background-color: #f4f4f4;
                                }}
                                .container {{
                                    max-width: 600px;
                                    margin: 20px auto;
                                    background: #fff;
                                    padding: 20px;
                                    border-radius: 5px;
                                    box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);
                                }}
                                .header {{
                                    background: #005a9c;
                                    color: white;
                                    text-align: left;
                                    padding: 15px;
                                    font-size: 24px;
                                    font-weight: bold;
                                    display: flex;
                                    align-items: center;
                                    gap: 10px;
                                }}
                                .header img {{
                                    height: 30px;
                                }}
                                .content {{
                                    padding: 20px 0;
                                    text-align: left;
                                }}
                                a{{
                                    color: #005a9c;
                                    text-decoration: none;
                                }}
                                a:hover {{
                                    text-decoration: underline;
                                }}
                                .login-box {{
                                    border: 1px solid #000;
                                    padding: 15px;
                                    margin: 20px 0;
                                    background: #fff;
                                }}
                                .login-box b {{
                                    font-size: 16px;
                                }}
                                .footer {{
                                    text-align: left;
                                    font-size: 14px;
                                    color: #555;
                                    margin-top: 20px;
                                }}
                                @media (max-width: 600px) {{
                                    .container {{
                                        width: 90%;
                                    }}
                                    .header {{
                                        font-size: 20px;
                                    }}
                                }}
                            </style>
                        </head>
                        <body>
                            <div class=""container"">
                                <div class=""header"">
                                    <img src=""logo.png"" alt=""PIZZASHOP Logo"">
                                    PIZZASHOP
                                </div>
                                <div class=""content"">
                                    <p>Welcome to PIZZASHOP</p>
                                    <p>Please find the details below for login into your account.</p>
                                    <div class=""login-box"">
                                        <b>Login Details:</b><br><br>
                                        <b>Username:</b> {username}<br>
                                        <b>Temporary Password:</b> {temporaryPassword}
                                    </div>
                                    <p>If you encounter any issues or have any questions, please do not hesitate to contact our support team.</p>
                                </div>
                            </div>
                        </body>
                        </html>";

            using (var smtpClient = new SmtpClient("mail.etatvasoft.com")
            {
                Port = 587,
                Credentials = new NetworkCredential("test.dotnet@etatvasoft.com", "P}N^{z-]7Ilp"),
                EnableSsl = true
            })
            {
                var mailMessage = new MailMessage
                {
                    From = new MailAddress(fromEmail, "PIZZASHOP"),
                    Subject = subject,
                    Body = emailBody,
                    IsBodyHtml = true
                };

                mailMessage.To.Add(toEmail);
                await smtpClient.SendMailAsync(mailMessage);
            }
        }
    }
}