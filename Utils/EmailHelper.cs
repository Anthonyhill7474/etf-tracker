using System;
using System.Net;
using System.Net.Mail;
using dotenv.net;

namespace Utils
{
    public static class EmailHelper
    {
        public static async Task SendEmailAsync(string subject, string body)
        {
            DotEnvOptions options = new DotEnvOptions(
                envFilePaths: new[] { "../../../../.env" },
                probeForEnv: false,
                overwriteExistingVars: true
            );

            DotEnv.Load(options);
            string? smtpHost = Environment.GetEnvironmentVariable("SMTP_HOST");
            string? smtpPort = Environment.GetEnvironmentVariable("SMTP_PORT");
            string? smtpUser = Environment.GetEnvironmentVariable("SMTP_USER");
            string? smtpPass = Environment.GetEnvironmentVariable("SMTP_PASS");
            string? toEmail  = Environment.GetEnvironmentVariable("ALERT_RECIPIENT");     

            if (string.IsNullOrEmpty(smtpHost) || string.IsNullOrEmpty(smtpPort) ||
                string.IsNullOrEmpty(smtpUser) || string.IsNullOrEmpty(smtpPass) ||
                string.IsNullOrEmpty(toEmail))
            {
                Console.WriteLine("❌ Missing email configuration in environment variables.");
                return;
            }

            try
            {
                var client = new SmtpClient(smtpHost, int.Parse(smtpPort))
                {
                    Credentials = new NetworkCredential(smtpUser, smtpPass),
                    EnableSsl = true
                };

                var mail = new MailMessage(smtpUser, toEmail, subject, body);
                await client.SendMailAsync(mail);
                Console.WriteLine("✅ Email sent.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❗ Error sending email: {ex.Message}");
            }
        }
    }
}