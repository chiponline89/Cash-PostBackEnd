using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
namespace PayID.Core
{
    public class Helper
    {
        public static string Mail_From, Mail_User_Id, Mail_User_Password, Mail_Server;
        public static int Mail_Port;
        public static bool Mail_Is_SSL;
        public static void SendSMS(string to, string content)
        {
            string param = "http://payflow.vn/sms/api/smsmt?UserId={0}&Content={1}&CommandCode=6089&RequestId=";
            param = String.Format(param, to, content);
            WebRequest webRequest = WebRequest.Create(param);
            WebResponse webResp = webRequest.GetResponse();
        }
        public static void SendMail(string to, string subject, string content)
        {
                MailMessage mail = new MailMessage(Mail_From, to);
                mail.IsBodyHtml = true;
                SmtpClient client = new SmtpClient();
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.UseDefaultCredentials = false;
                client.Credentials = new NetworkCredential(Mail_User_Id, Mail_User_Password);
                client.Port = Mail_Port;
                client.EnableSsl = Mail_Is_SSL;
                client.Host = Mail_Server;
                mail.Subject = subject;
                mail.Body = content;
                client.Send(mail);
        }
    }
}
