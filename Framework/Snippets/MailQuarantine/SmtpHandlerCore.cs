using System.Net.Mail;

namespace Snippets.MailQuarantine
{
    public abstract class SmtpHandlerCore
    {
        public abstract void Send(MailMessage mail);
    }
}