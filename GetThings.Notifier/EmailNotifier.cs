namespace GetThings.Notifier
{
    using Infrastructure;
    using System;
    using System.Configuration;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Net;
    using System.Net.Mail;
    using System.Threading.Tasks;
    using WinSCP;

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
            long sizeDirectory = new DirectoryInfo($"{info.PathDirectory}{idResource}").EnumerateFiles("*", SearchOption.AllDirectories).Sum(f => f.Length);
            string smtpHost = ConfigurationSettings.AppSettings["SMTP-HOST"];
            int smtpPort = int.Parse(ConfigurationSettings.AppSettings["SMTP-PORT"]);
            var fromAddress = new MailAddress(ConfigurationSettings.AppSettings["FROM-EMAIL"], ConfigurationSettings.AppSettings["FROM-NAME"]);
            var toAddress = new MailAddress(ConfigurationSettings.AppSettings["TO-EMAIL"], ConfigurationSettings.AppSettings["TO-NAME"]);
            string fromPassword = ConfigurationSettings.AppSettings["FROM-PASSWORD"];
            string subject = "Curso de Pluralsight descargado";
            string body = $"Ya te mandé el curso {idResource}. Se lleva {sizeDirectory} bytes. Te lo mandé en un zip que se lleva {remoteDir.SizeZip} bytes. Eso está en {remoteDir.RemoteDir}. El fichero es un ZIP.";

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
            var idResource = GetId(info.Resource);
            var result = new RemoteDirInfo()
            {
                RemoteDir = $"{remoteDir}\\{GetId(info.Resource)}"
            };

            var destinationFileName = $"{info.PathTempDirectory}{idResource}.zip";
            if (File.Exists(destinationFileName))
                File.Delete(destinationFileName);
            ZipFile.CreateFromDirectory($"{info.PathDirectory}{idResource}", destinationFileName, CompressionLevel.Optimal, false);
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
                        PortNumber = int.Parse(ConfigurationSettings.AppSettings["PORT-SFTP"]),
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

                    Task.Run(() => { try { File.Delete(destinationFileName); } catch (Exception) { } });
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