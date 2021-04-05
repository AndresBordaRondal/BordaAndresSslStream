using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace ClientSSL
{
    class Program
    {
        private static int _hostPort = 2000;
        // Refiere a la IP del host a conectar
        private static string _hostName = "25.93.189.137";
        // Asignamos el nombre del certificado creado
        private static string ServerCertificateName = "MySslSocketCertificate";

        static void Main()
        {
            var clientCertificate = getServerCert();
            var clientCertificateCollection = new X509CertificateCollection(new X509Certificate[] { clientCertificate });

            using (var client = new TcpClient(_hostName, _hostPort))
            using (var sslStream = new SslStream(client.GetStream(), false, ValidateCertificate))
            {
                sslStream.AuthenticateAsClient(ServerCertificateName, clientCertificateCollection, SslProtocols.Tls12, false);

                Console.WriteLine("Conexion Establecida\n==================================");
                Console.WriteLine("Ingrese un comando...");
                string outputMessage = Console.ReadLine();
                byte[] outputBuffer = Encoding.UTF8.GetBytes(GetJsonMessage(outputMessage));
                sslStream.Write(outputBuffer);
                Console.WriteLine("\nSent:\n{0}", outputMessage);
                Console.WriteLine("=================================>\n");

                byte[] inputBuffer = new byte[4096];
                int inputBytes = 0;
                while (inputBytes == 0)
                {
                    inputBytes = sslStream.Read(inputBuffer, 0, inputBuffer.Length);
                }
                string inputMessage = Encoding.UTF8.GetString(inputBuffer, 0, inputBytes);
                Console.WriteLine("Raw Data:\n{0}\n", inputMessage);
                PrintInfo(outputMessage, inputMessage);
                Console.WriteLine("<=================================\n");

                Console.ReadKey();
            }
        }


        #region Certificate Methods


        static bool ValidateCertificate(Object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors == SslPolicyErrors.None)
            {
                return true;
            }
            // Ignoramos los Chain Errors debido a que trabajamos con un certificado autofirmado
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


        public static void PrintElseInfo(string message)
        {
            Console.WriteLine("Parse Data:\n");
            JsonMessage jsonMessage = ReadJsonMessage(message);
            Console.WriteLine("\tMessage: {0}\n", jsonMessage.Message);
            Console.WriteLine("\n");
        }


        public static void PrintGpuInfo(string message)
        {
            Console.WriteLine("Parse Data:\n");
            List<GPU> gpus = JsonConvert.DeserializeObject<List<GPU>>(message);
            foreach (GPU gpu in gpus)
            {
                Console.WriteLine("\tName: {0}\n", gpu.Name);
                Console.WriteLine("\tStatus: {0}\n", gpu.Status);
                Console.WriteLine("\tAdapter RAM: {0}\n", gpu.AdapterRAM);
                Console.WriteLine("\tAdapter DAC Type: {0}\n", gpu.AdapterDACType);
                Console.WriteLine("\tDriver Version: {0}\n", gpu.DriverVersion);
                Console.WriteLine("\n");
            }
        }


        public static void PrintStorageInfo(string message)
        {
            Console.WriteLine("Parse Data:\n");
            List<Storage> storages = JsonConvert.DeserializeObject<List<Storage>>(message);
            foreach (Storage storage in storages)
            {
                Console.WriteLine("\tRoot Directory: {0}\n", storage.RootDirectory);
                Console.WriteLine("\tTotal Size of Drive: {0}\n", storage.TotalSizeOfDrive);
                Console.WriteLine("\tTotal Available Space: {0}\n", storage.TotalAvailableSpace);
                Console.WriteLine("\n");
            }
        }


        public static void PrintAllInfo(string message)
        {
            Console.WriteLine("Parse Data:\n");
            All all = JsonConvert.DeserializeObject<All>(message);
            Console.WriteLine("Video Controllers:");
            foreach (GPU gpu in all.GPUs)
            {
                Console.WriteLine("\tName: {0}\n", gpu.Name);
                Console.WriteLine("\tStatus: {0}\n", gpu.Status);
                Console.WriteLine("\tAdapter RAM: {0}\n", gpu.AdapterRAM);
                Console.WriteLine("\tAdapter DAC Type: {0}\n", gpu.AdapterDACType);
                Console.WriteLine("\tDriver Version: {0}\n", gpu.DriverVersion);
                Console.WriteLine("\n");
            }

            Console.WriteLine("Drives:");
            foreach (Storage storage in all.Storages)
            {
                Console.WriteLine("\tRoot Directory: {0}\n", storage.RootDirectory);
                Console.WriteLine("\tTotal Size of Drive: {0}\n", storage.TotalSizeOfDrive);
                Console.WriteLine("\tTotal Available Space: {0}\n", storage.TotalAvailableSpace);
                Console.WriteLine("\n");
            }
        }


        public static void PrintInfo(string sent, string message)
        {
            if (sent == "getAll")
            {
                PrintAllInfo(message);
            }
            else if (sent == "getVideoController")
            {
                PrintGpuInfo(message);
            }
            else if (sent == "getStorage")
            {
                PrintStorageInfo(message);
            }
            else
            {
                PrintElseInfo(message);
            }
        }
        #endregion
    }
}
