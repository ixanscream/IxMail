using RazorEngine;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net.Configuration;
using System.Net.Mail;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace IxMail
{
    [Serializable]
    public class IxWebMail
    {
        private SmtpSection section = (SmtpSection)ConfigurationManager.GetSection("system.net/mailSettings/smtp");
        private string folder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Views", "IxMail");
        private string demoFile = "Demo.html";

        #region Property
        private string Body { get; set; }
        private List<string> To { get; set; }
        private List<string> CC { get; set; }
        private List<string> Bcc { get; set; }
        private string Subject { get; set; }
        private List<Attachment> Attachment { get; set; }
        private MailPriority Priority { get; set; }

        #endregion

        #region Build Body
        public IxWebMail BuildBody(object yourObject, string yourTemplate)
        {
            string body = string.Empty;

            var currentHttp = HttpContext.Current;

            if (string.IsNullOrEmpty(yourTemplate)) yourTemplate = demoFile;

            string[] ext = yourTemplate.Split('.');

            if (ext.Last().ToLower().Equals("cshtml"))
            {
                body = ReadRazor(yourTemplate, yourObject, currentHttp);
            }
            else
            {
                body = ReadFileFrom(yourTemplate);

                Type myType = yourObject.GetType();
                IList<PropertyInfo> props = new List<PropertyInfo>(myType.GetProperties());

                if (string.IsNullOrWhiteSpace(body))
                {
                    body = "<html><head><title></title></head><body>";

                    foreach (PropertyInfo prop in props)
                    {
                        object propValue = prop.GetValue(yourObject, null);
                        object propName = prop.Name;

                        string propertyReplace = "{{" + propName.ToString() + "}}";

                        body += propName.ToString() + " : " + propertyReplace + "<hr/>";
                    }
                    body += "</body><html>";
                }

                foreach (PropertyInfo prop in props)
                {
                    object propValue = prop.GetValue(yourObject, null);
                    object propName = prop.Name;

                    string propertyReplace = "{{" + propName.ToString() + "}}";
                    string propertyValueReplace = propValue == null ? "Unbind Model" : propValue.ToString();

                    if (body.Contains(propertyReplace))
                    {
                        body = body.Replace(propertyReplace, propertyValueReplace);
                    }
                }
            }

            this.Body = body;

            return this;
        }
        private string ReadRazor(string template, object model, object currentHttp)
        {
            bool folderExists = Directory.Exists(folder);

            if (!folderExists) Directory.CreateDirectory(folder);

            dynamic dynamic = new ExpandoObject();
            IDictionary<string, object> dynamicNew = dynamic;

            Type myType = model.GetType();
            IList<PropertyInfo> props = new List<PropertyInfo>(myType.GetProperties());

            foreach (var item in props)
            {
                object propValue = item.GetValue(model, null);
                string propName = item.Name;

                dynamicNew.Add(propName, propValue);
            }

            dynamic.Url = currentHttp;

            string body = File.ReadAllText(folder + "/" + template);

            return Razor.Parse(body, dynamic);
        }
        private string ReadFileFrom(string templateName)
        {
            bool folderExists = Directory.Exists(folder);

            if (!folderExists) Directory.CreateDirectory(folder);

            string filePath = folder + "/" + templateName;

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
        #endregion

        #region To, CC, BCC, Subject
        public IxWebMail EmailTo(params string[] EmailTo)
        {
            this.To = EmailTo.ToList();
            return this;
        }

        public IxWebMail CCTo(params string[] CCTo)
        {
            this.CC = CCTo.ToList();
            return this;
        }

        public IxWebMail BCCTo(params string[] BCCTo)
        {
            this.Bcc = BCCTo.ToList();
            return this;
        }

        public IxWebMail EmailSubject(string Subject)
        {
            this.Subject = Subject;
            return this;
        }
        #endregion

        #region Attachment, Priority
        public IxWebMail Attacment(params Attachment[] attachment)
        {
            this.Attachment = attachment.ToList();
            return this;
        }

        public IxWebMail EmailPriority(MailPriority priority)
        {
            this.Priority = priority;
            return this;
        }
        #endregion

        public void SendBackground()
        {
            new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;

                MailMessage mail = new MailMessage();

                mail.Sender = new MailAddress(section.From);
                mail.From = new MailAddress(section.From);
                mail.Body = this.Body;
                mail.Subject = this.Subject;

                if (this.To != null)
                {
                    foreach (var item in this.To)
                    {
                        mail.To.Add(item);
                    }
                }

                if (this.CC != null)
                {
                    foreach (var item in this.CC)
                    {
                        mail.CC.Add(item);
                    }
                }

                if (this.Bcc != null)
                {
                    foreach (var item in this.Bcc)
                    {
                        mail.Bcc.Add(item);
                    }
                }

                if (this.Attachment != null)
                {
                    foreach (var item in this.Attachment)
                    {
                        mail.Attachments.Add(item);
                    }
                }

                mail.Priority = this.Priority;
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
                    string path = folder + "/" + "Error_" + DateTime.Now.ToString("dd_MMM_yyyy") + ".txt";
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


            }).Start();
        }

        public int Send()
        {
            MailMessage mail = new MailMessage();

            mail.Sender = new MailAddress(section.From);
            mail.From = new MailAddress(section.From);
            mail.Body = this.Body;
            mail.Subject = this.Subject;

            if (this.To != null)
            {
                foreach (var item in this.To)
                {
                    mail.To.Add(item);
                }
            }

            if (this.CC != null)
            {
                foreach (var item in this.CC)
                {
                    mail.CC.Add(item);
                }
            }

            if (this.Bcc != null)
            {
                foreach (var item in this.Bcc)
                {
                    mail.Bcc.Add(item);
                }
            }

            if (this.Attachment != null)
            {
                foreach (var item in this.Attachment)
                {
                    mail.Attachments.Add(item);
                }
            }

            mail.Priority = this.Priority;
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
                string path = folder + "/" + "Error_" + DateTime.Now.ToString("dd_MMM_yyyy") + ".txt";
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

                return 0;
            }

            return this.To.Count;
        }
    }
}
