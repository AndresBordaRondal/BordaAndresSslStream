using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Management;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace ServerSSL
{
    class Program
    {
        // El número del puerto
        private static int _port = 2000;
        // Asignamos la IP del host
        private static string _ip = "25.93.189.137";

        static void Main()
        {
            var serverCertificate = getServerCert();

            var listener = new TcpListener(IPAddress.Parse(_ip), _port);
            listener.Start();

            while (true)
            {
                using (var client = listener.AcceptTcpClient())
                using (var sslStream = new SslStream(client.GetStream(), false, ValidateCertificate))
                {
                    sslStream.AuthenticateAsServer(serverCertificate, true, SslProtocols.Tls12, false);

                    Console.WriteLine("Esperando Mensaje...\n==================================");

                    byte[] inputBuffer = new byte[4096];
                    int inputBytes = 0;
                    while (inputBytes == 0)
                    {
                        inputBytes = sslStream.Read(inputBuffer, 0, inputBuffer.Length);
                    }

                    string inputMessage = Encoding.UTF8.GetString(inputBuffer, 0, inputBytes);
                    Console.WriteLine("Raw Data:\n{0}\n", inputMessage);
                    JsonMessage jsonMessage = ReadJsonMessage(inputMessage);
                    Console.WriteLine("Parse Data:\n{0}", jsonMessage.Message);
                    Console.WriteLine("<=================================");

                    string outputMessage = "";

                    if (jsonMessage.Message == "getVideoController")
                    {
                        Console.WriteLine("Text is a get video controller request");
                        outputMessage = GetGpuInfo();
                    }
                    else if (jsonMessage.Message == "getStorage")
                    {
                        Console.WriteLine("Text is a get storage request");
                        outputMessage = GetStorageInfo();
                    }
                    else if (jsonMessage.Message == "getAll")
                    {
                        Console.WriteLine("Text is a get all request");
                        outputMessage = GetAllInfo();
                    }
                    else
                    {
                        Console.WriteLine("Invaled Petition");
                        outputMessage = GetJsonMessage("Peticion invalida");
                    }

                    byte[] outputBuffer = Encoding.UTF8.GetBytes(outputMessage);
                    sslStream.Write(outputBuffer);
                    Console.WriteLine("Sent:\n{0}", outputMessage);
                    Console.WriteLine("=================================>\n");

                }
            }
        }


        #region Certificate Methods


        static bool ValidateCertificate(Object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            // Retornamos true debido a que el certificado SSL
            // es uno autofirmado, por lo tanto confiamos en él
            // en el mundo real seria una mala practica
            return true;

            // En caso de ser un certificado no autofirmado, se debe
            // realizar los siguientes controles
            if (sslPolicyErrors == SslPolicyErrors.None)
            {
                return true;
            }
            if (sslPolicyErrors == SslPolicyErrors.RemoteCertificateChainErrors)
            {
                return true;
            }
            return false;
        }

        private static X509Certificate getServerCert()
        {
            X509Store store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
            store.Open(OpenFlags.ReadOnly);

            X509Certificate2 foundCertificate = null;
            foreach (X509Certificate2 currentCertificate in store.Certificates)
            {
                if (currentCertificate.IssuerName.Name != null && currentCertificate.IssuerName.Name.Equals("CN=MySslSocketCertificate"))
                {
                    foundCertificate = currentCertificate;
                    break;
                }
            }

            return foundCertificate;
        }
        #endregion


        #region Info Methods


        public static string GetJsonMessage(string message)
        {
            JsonMessage jsonMessage = new JsonMessage(message);
            return JsonConvert.SerializeObject(jsonMessage);
        }


        public static JsonMessage ReadJsonMessage(string message)
        {
            JsonMessage jsonMessage = JsonConvert.DeserializeObject<JsonMessage>(message);
            return jsonMessage;
        }


        public static string GetGpuInfo()
        {
            List<GPU> videoControllers = new List<GPU>();
            ManagementObjectSearcher myVideoObject = new ManagementObjectSearcher("select * from Win32_VideoController");
            foreach (ManagementObject obj in myVideoObject.Get())
            {
                GPU gpu = new GPU(obj["Name"].ToString(), obj["Status"].ToString(), obj["AdapterRAM"].ToString(),
                    obj["AdapterDACType"].ToString(), obj["DriverVersion"].ToString());
                videoControllers.Add(gpu);
            }
            return JsonConvert.SerializeObject(videoControllers);
        }


        public static string GetStorageInfo()
        {
            List<Storage> storages = new List<Storage>();
            DriveInfo[] allDrives = DriveInfo.GetDrives();
            foreach (DriveInfo d in allDrives)
            {
                if (d.IsReady == true)
                {
                    Storage storage = new Storage(d.TotalFreeSpace, d.TotalSize, d.RootDirectory.Name);
                    storages.Add(storage);
                }
            }
            return JsonConvert.SerializeObject(storages);
        }


        public static string GetAllInfo()
        {
            List<GPU> videoControllers = new List<GPU>();
            ManagementObjectSearcher myVideoObject = new ManagementObjectSearcher("select * from Win32_VideoController");
            foreach (ManagementObject obj in myVideoObject.Get())
            {
                GPU gpu = new GPU(obj["Name"].ToString(), obj["Status"].ToString(), obj["AdapterRAM"].ToString(),
                    obj["AdapterDACType"].ToString(), obj["DriverVersion"].ToString());
                videoControllers.Add(gpu);
            }
            List<Storage> storages = new List<Storage>();
            DriveInfo[] allDrives = DriveInfo.GetDrives();
            foreach (DriveInfo d in allDrives)
            {
                if (d.IsReady == true)
                {
                    Storage storage = new Storage(d.TotalFreeSpace, d.TotalSize, d.RootDirectory.Name);
                    storages.Add(storage);
                }
            }
            All all = new All(videoControllers, storages);
            return JsonConvert.SerializeObject(all);
        }
        #endregion
    }
}
