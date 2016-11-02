using GetThins.Infrastructure;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WinSCP;

namespace GetThings.Notifier
{
    public class EmailNotifier : INotifier
    {
        class RemoteDirInfo
        {
            public string RemoteDir { get; set; }

            public long SizeZip { get; set; }
        }

        private static object lockFlag = new object();

        public void Notify(bool flag, BaseInfo info)
        {
            if (!flag)
                return;

            RemoteDirInfo remoteDir = null;
            lock (lockFlag)
                remoteDir = SendCourse(info);

            string idResource = GetId(info.Resource);
            long sizeDirectory = new DirectoryInfo($"info.Password\\{idResource}").EnumerateFiles("*", SearchOption.AllDirectories).Sum(f => f.Length);
            string smtpHost = ConfigurationSettings.AppSettings["SMTP-HOST"];
            int smtpPort = int.Parse(ConfigurationSettings.AppSettings["SMTP-PORT"]);
            var fromAddress = new MailAddress(ConfigurationSettings.AppSettings["FROM-EMAIL"], ConfigurationSettings.AppSettings["FROM-NAME"]);
            var toAddress = new MailAddress(ConfigurationSettings.AppSettings["TO-EMAIL"], ConfigurationSettings.AppSettings["TO-NAME"]);
            string fromPassword = ConfigurationSettings.AppSettings["FROM-PASSWORD"];
            string subject = "Curso de Pluralsight descargado";
            string body = $"Ya te mandé el curso {idResource}. Se lleva {sizeDirectory}. Te lo mandé en un zip que se lleva {remoteDir.SizeZip}. Eso está en {remoteDir.RemoteDir}.";

            using (var smtp = new SmtpClient()
            {
                Host = smtpHost,
                Port = smtpPort,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
            })
            {
                using (var message = new MailMessage(fromAddress, toAddress) { Subject = subject, Body = body })
                    smtp.Send(message);
            }
        }

        private RemoteDirInfo SendCourse(BaseInfo info)
        {
            var remoteDir = ConfigurationSettings.AppSettings["REMOTE-DIR"];
            var result = new RemoteDirInfo()
            {
                RemoteDir = $"{remoteDir}\\{GetId(info.Resource)}"
            };

            var destinationFileName = $"{info.PathTempDirectory}\\{GetId(info.Resource)}.zip";
            ZipFile.CreateFromDirectory($"{info.PathDirectory}\\{GetId(info.Resource)}", destinationFileName);
            result.SizeZip = new FileInfo(destinationFileName).Length;

            int retry = int.Parse(ConfigurationSettings.AppSettings["NUMBER-RETRY"]);
            for (int i = 0; i < retry; i++)
            {
                try
                {
                    // Setup session options
                    var sessionOptions = new SessionOptions
                    {
                        Protocol = Protocol.Sftp,
                        HostName = ConfigurationSettings.AppSettings["HOST-SFTP"],
                        UserName = ConfigurationSettings.AppSettings["USER-SFTP"],
                        Password = ConfigurationSettings.AppSettings["PASSWORD-SFTP"],
                        SshHostKeyFingerprint = ConfigurationSettings.AppSettings["KEY-SFTP"]
                    };

                    using (Session session = new Session())
                    {
                        // Connect
                        session.Open(sessionOptions);

                        // Upload files
                        var transferOptions = new TransferOptions();
                        transferOptions.TransferMode = TransferMode.Binary;

                        TransferOperationResult transferResult = session.PutFiles(destinationFileName, remoteDir, false, transferOptions);

                        // Throw on any error
                        transferResult.Check();
                    }

                    Task.Run(() => File.Delete(destinationFileName));
                    break;
                }
                catch (Exception e)
                {

                }
            }

            return result;
        }

        private string GetId(string resource)
        {
            var segments = resource.Split('/');
            return segments[segments.Length - 1];
        }
    }
}