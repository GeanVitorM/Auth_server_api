using SendGrid.Helpers.Mail;
using SendGrid;
using Microsoft.Extensions.Logging;

namespace Authetication.Server.Api.Services;

public class EmailService : IEmailService
{
    private readonly string _sendGridApiKey;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _sendGridApiKey = configuration["SendGridApiKey"];
        _logger = logger;
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        try
        {
            var client = new SendGridClient(_sendGridApiKey);
            var from = new EmailAddress("lads@iesgo.edu.br", "LADS");
            var toEmail = new EmailAddress(to);

            // Extraindo a URL de recuperação de senha do corpo do e-mail
            var recoveryUrl = body.Substring(body.IndexOf("http"), body.Length - body.IndexOf("http")).Trim();

            // HTML estilizado do corpo do e-mail com botão de recuperação de senha
            var htmlContent = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #ddd; border-radius: 10px;'>
                    <h1 style='font-size: 24px; color: #333; text-align: center;'>Laboratório de Desenvolvimento de Software</h1>
                    <h2 style='font-size: 20px; color: #333; text-align: center;'>Recuperação de Senha</h2>
                    <p style='font-size: 16px; color: #555;'>Olá,</p>
                    <p style='font-size: 16px; color: #555;'>Recebemos uma solicitação para redefinir a senha da sua conta. Se você não fez essa solicitação, pode ignorar este e-mail com segurança.</p>
                    <p style='font-size: 16px; color: #555;'>Para redefinir sua senha, clique no botão abaixo:</p>
                    <div style='text-align: center; margin: 20px 0;'>
                        <a href='{recoveryUrl}' 
                           style='display: inline-block; padding: 12px 24px; font-size: 16px; color: white; background-color: #007bff; text-decoration: none; border-radius: 5px;'>
                            Redefinir Senha
                        </a>
                    </div>
                    <p style='font-size: 16px; color: #555;'>O link para redefinir sua senha é válido por 5 minutos. Após esse período, será necessário solicitar um novo link.</p>
                    <p style='font-size: 16px; color: #555;'>Se você tiver qualquer dúvida, entre em contato com o nosso suporte.</p>
                    <p style='font-size: 14px; color: #aaa; text-align: center; margin-top: 30px;'>Este é um e-mail automático, por favor, não responda.</p>
                </div>";

            var msg = MailHelper.CreateSingleEmail(from, toEmail, subject, null, htmlContent);

            var response = await client.SendEmailAsync(msg);

            if (!response.IsSuccessStatusCode)
            {
                var errorMessage = await response.Body.ReadAsStringAsync();
                throw new Exception($"Failed to send email: {response.StatusCode} - {errorMessage}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while sending email.");
            throw;
        }
    }
}
