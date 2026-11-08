using System.Net;
using System.Net.Mail;

namespace Laragon
{
    /// <summary>Sends a basic SMTP message to verify the supplied mail settings.</summary>
    internal static class MailTest
    {
        internal static void Send(string address, string password)
        {
            using var message = new MailMessage(address, address)
            {
                Subject = "XLaragon mail configuration test",
                Body = "This message confirms that the configured SMTP account can send mail."
            };

            using var client = new SmtpClient("smtp.gmail.com", 587)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(address, password),
                Timeout = 120000
            };

            client.Send(message);
        }
    }
}
