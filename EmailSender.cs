using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net.Configuration;
using System.Net.Mail;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace IxMail
{
    public static class EmailSender
        {
        private static SmtpSection section = (SmtpSection)ConfigurationManager.GetSection("system.net/mailSettings/smtp");
        private static string folder = "~/EmailTemplate/";
        private static string demoFile = "Demo.html";
        private static Task<string> BuildBodyAsync(object mailBody, string tamplateMail, string pathFolder)
        {
            var task = new Task<string>(() =>
            {
                if (string.IsNullOrEmpty(tamplateMail)) tamplateMail = demoFile;
                string body = ReadFileFromAsync(pathFolder, tamplateMail);

                Type myType = mailBody.GetType();
                IList<PropertyInfo> props = new List<PropertyInfo>(myType.GetProperties());

                if (string.IsNullOrWhiteSpace(body))
                {
                    body = "<html><head><title></title></head><body>";

                    foreach (PropertyInfo prop in props)
                    {
                        object propValue = prop.GetValue(mailBody, null);
                        object propName = prop.Name;

                        string propertyReplace = "{{" + propName.ToString() + "}}";

                        body += propName.ToString() + " : " + propertyReplace + "<hr/>";
                    }
                    body += "</body><html>";
                }

                foreach (PropertyInfo prop in props)
                {
                    object propValue = prop.GetValue(mailBody, null);
                    object propName = prop.Name;

                    string propertyReplace = "{{" + propName.ToString() + "}}";
                    string propertyValueReplace = propValue == null ? "Unbind Model" : propValue.ToString();

                    if (body.Contains(propertyReplace))
                    {
                        body = body.Replace(propertyReplace, propertyValueReplace);
                    }
                }

                return body;
            });

            task.Start();

            return task;
        }
        private static string BuildBody(object mailBody, string tamplateMail)
        {
            if (string.IsNullOrEmpty(tamplateMail)) tamplateMail = demoFile;
            string body = ReadFileFrom(tamplateMail);

            Type myType = mailBody.GetType();
            IList<PropertyInfo> props = new List<PropertyInfo>(myType.GetProperties());

            if (string.IsNullOrWhiteSpace(body))
            {
                body = "<html><head><title></title></head><body>";

                foreach (PropertyInfo prop in props)
                {
                    object propValue = prop.GetValue(mailBody, null);
                    object propName = prop.Name;

                    string propertyReplace = "{{" + propName.ToString() + "}}";

                    body += propName.ToString() + " : " + propertyReplace + "<hr/>";
                }
                body += "</body><html>";
            }

            foreach (PropertyInfo prop in props)
            {
                object propValue = prop.GetValue(mailBody, null);
                object propName = prop.Name;

                string propertyReplace = "{{" + propName.ToString() + "}}";
                string propertyValueReplace = propValue == null ? "Unbind Model" : propValue.ToString();

                if (body.Contains(propertyReplace))
                {
                    body = body.Replace(propertyReplace, propertyValueReplace);
                }
            }

            return body;
        }
        private static string ReadFileFromAsync(string pathFolder, string tamplateMail)
        {
            bool folderExists = Directory.Exists(pathFolder);

            if (!folderExists) Directory.CreateDirectory(pathFolder);

            string filePath = pathFolder + tamplateMail;

            if (!File.Exists(filePath))
            {
                (new FileInfo(filePath)).Directory.Create();
                using (TextWriter tw = new StreamWriter(filePath))
                {
                    tw.WriteLine(string.Empty);
                    tw.Close();
                }
            }

            string body = File.ReadAllText(filePath);
            return body;
        }
        private static string ReadFileFrom(string templateName)
        {
            bool folderExists = Directory.Exists(HttpContext.Current.Server.MapPath(folder));

            if (!folderExists) Directory.CreateDirectory(HttpContext.Current.Server.MapPath(folder));

            string filePath = HttpContext.Current.Server.MapPath(folder + templateName);

            if (!File.Exists(filePath))
            {
                (new FileInfo(filePath)).Directory.Create();
                using (TextWriter tw = new StreamWriter(filePath))
                {
                    tw.WriteLine(string.Empty);
                    tw.Close();
                }
            }

            string body = File.ReadAllText(filePath);
            return body;
        }
        public static void SendMailAsync(IxMailMessage mail)
        {
            string pathFolder = HttpContext.Current.Server.MapPath(folder);
            new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;
                var body = BuildBodyAsync(mail.mailBody, mail.tamplateMail, pathFolder);

                mail.Sender = new MailAddress(section.From);
                mail.From = new MailAddress(section.From);
                mail.Body = body.Result;
                mail.IsBodyHtml = true;

                if (body.IsCompleted)
                {
                    try
                    {
                        using (var smtpClient = new SmtpClient())
                        {
                            smtpClient.Send(mail);
                        }
                    }
                    catch (Exception e)
                    {
                        string path = pathFolder + "Error_" + DateTime.Now.ToString("dd_MMM_yyyy") + ".txt";
                        if (!File.Exists(path))
                        {
                            (new FileInfo(path)).Directory.Create();
                            using (TextWriter tw = new StreamWriter(path))
                            {
                                tw.WriteLine(mail.Subject + "|" + mail.Body + "|" + e.ToString() + Environment.NewLine);
                                tw.Close();
                            }
                        }
                        else
                        {
                            using (StreamWriter tw = File.AppendText(path))
                            {
                                tw.WriteLine(mail.Subject + "|" + e.ToString() + Environment.NewLine);
                                tw.Close();
                            }
                        }
                    }
                }


            }).Start();
        }
        public static void SendMail(IxMailMessage mail)
        {
            string pathFolder = HttpContext.Current.Server.MapPath(folder);
            mail.Sender = new MailAddress(section.From);
            mail.From = new MailAddress(section.From);
            mail.Body = BuildBody(mail.mailBody, mail.tamplateMail);
            mail.IsBodyHtml = true;

            try
            {
                using (var smtpClient = new SmtpClient())
                {
                    smtpClient.Send(mail);
                }
            }
            catch (Exception e)
            {
                string path = pathFolder + "Error_" + DateTime.Now.ToString("dd_MMM_yyyy") + ".txt";
                if (!File.Exists(path))
                {
                    (new FileInfo(path)).Directory.Create();
                    using (TextWriter tw = new StreamWriter(path))
                    {
                        tw.WriteLine(mail.Subject + "|" + mail.Body + "|" + e.ToString() + Environment.NewLine);
                        tw.Close();
                    }
                }
                else
                {
                    using (StreamWriter tw = File.AppendText(path))
                    {
                        tw.WriteLine(mail.Subject + "|" + e.ToString() + Environment.NewLine);
                        tw.Close();
                    }
                }
            }
        }
    }

    public class IxMailMessage : MailMessage
    {
        public object mailBody { get; set; }
        public string tamplateMail { get; set; }
    }
}
