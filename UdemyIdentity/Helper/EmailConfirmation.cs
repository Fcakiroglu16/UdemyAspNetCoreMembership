using System.Net.Mail;

namespace UdemyIdentity.Helper
{
    public static class EmailConfirmation
    {
        public static void SendEmail(string link, string email)

        {
            MailMessage mail = new MailMessage();

            SmtpClient smtpClient = new SmtpClient("mail.teknohub.net");

            mail.From = new MailAddress("admin@teknohub.net");
            mail.To.Add(email);

            mail.Subject = $"www.bıdıbı.com::Email doğrulama";
            mail.Body = "<h2>Email adresinizi doğrulamak için lütfen aşağıdaki linke tıklayınız.</h2><hr/>";
            mail.Body += $"<a href='{link}'>email doğrulama linki</a>";
            mail.IsBodyHtml = true;
            smtpClient.Port = 587;
            smtpClient.Credentials = new System.Net.NetworkCredential("admin@teknohub.net", "Fatih1234");

            smtpClient.Send(mail);
        }
    }
}