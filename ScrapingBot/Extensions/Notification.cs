using System;
using System.Net;
using System.Net.Mail;

namespace ScrapingBot.Extensions;

public static class Notification {
    public static void SendEmail(this string alertMessage) {
        var smtpClient = new SmtpClient() {
            Port = 587,
            Credentials = new NetworkCredential("jk427202@gmail.com", Environment.GetEnvironmentVariable("EmailPassword")),
            EnableSsl = true,
            Host = "smtp.gmail.com"
        };
        smtpClient.Send("jk427202@gmail.com", "luki12302@gmail.com", $"Scraping bot error", alertMessage);
    }
}
