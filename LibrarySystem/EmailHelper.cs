using System;
using System.Net;
using System.Net.Mail;

namespace LibrarySystem
{
    public static class EmailHelper
    {
        // Tries to send an email using SMTP settings from environment variables.
        // Required env vars: SMTP_HOST, SMTP_PORT, SMTP_USER, SMTP_PASS, SMTP_FROM
        // Returns true if sent, false otherwise.
        public static bool TrySendEmail(string to, string subject, string body, out string error)
        {
            error = string.Empty;
            try
            {
                var host = Environment.GetEnvironmentVariable("SMTP_HOST");
                var portStr = Environment.GetEnvironmentVariable("SMTP_PORT");
                var user = Environment.GetEnvironmentVariable("SMTP_USER");
                var pass = Environment.GetEnvironmentVariable("SMTP_PASS");
                var from = Environment.GetEnvironmentVariable("SMTP_FROM");

                if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(portStr) || string.IsNullOrWhiteSpace(from))
                {
                    error = "SMTP settings are not configured (SMTP_HOST/SMTP_PORT/SMTP_FROM).";
                    return false;
                }

                if (!int.TryParse(portStr, out var port)) port = 25;

                var mail = new MailMessage();
                mail.From = new MailAddress(from);
                mail.To.Add(to);
                mail.Subject = subject;
                mail.Body = body;
                mail.IsBodyHtml = false;

                using (var smtp = new SmtpClient(host, port))
                {
                    smtp.EnableSsl = true;
                    if (!string.IsNullOrWhiteSpace(user))
                    {
                        smtp.Credentials = new NetworkCredential(user, pass);
                    }
                    smtp.Send(mail);
                }

                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }
    }
}
