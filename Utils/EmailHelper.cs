using System;
using System.Net;
using System.Net.Mail;
using dotenv.net;

namespace Utils
{
    /// <summary>
    /// Handles sending email alerts via SMTP with credentials loaded from environment variables.
    /// </summary>
    public static class EmailHelper
    {
        /// <summary>
        /// Sends an email using credentials and settings in the .env file.
        /// </summary>
        /// <param name="subject">Email subject line</param>
        /// <param name="body">Email body content</param>
        public static async Task SendEmailAsync(string subject, string body)
        {
            string envPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".env");
            DotEnvOptions options = new DotEnvOptions(
                envFilePaths: new[] { envPath },
                probeForEnv: false,
                overwriteExistingVars: true
            );
            DotEnv.Load(options);

            DotEnv.Load(options);
            string? smtpHost = Environment.GetEnvironmentVariable("SMTP_HOST");
            string? smtpPort = Environment.GetEnvironmentVariable("SMTP_PORT");
            string? smtpUser = Environment.GetEnvironmentVariable("SMTP_USER");
            string? smtpPass = Environment.GetEnvironmentVariable("SMTP_PASS");
            string? toEmail = Environment.GetEnvironmentVariable("ALERT_RECIPIENT");

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