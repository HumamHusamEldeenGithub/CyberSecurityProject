using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using ServerClient;
using MongoDB.Driver;
using MongoDB.Bson;
using AesEncryption;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Server;
using System.IO;

namespace MultiServer
{
    class Server
    {
        #region Parameters
        private static readonly Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private const int PORT = 100;
        private const int BUFFER_SIZE = 4096;
        private static readonly byte[] buffer = new byte[BUFFER_SIZE];

        public static readonly List<Socket> clientSockets = new List<Socket>();
        public static readonly List<ClientManager> clientsProfile = new List<ClientManager>();
        public static List<SocketMessage> MessageQueue = new List<SocketMessage>();

        private const string serverUUID = "www.iss-chat-app.com";

        public static RsaEncryption rsaEncryption = new RsaEncryption(); 

        public static MongoClient mongoDBClient;
        private const string mongoURI = "mongodb+srv://Humam:pwaIR4tEdVQIYcll@cyberproject.rfgzhgh.mongodb.net/?retryWrites=true&w=majority";

        #endregion

        #region Main
        static void Main()
        {
            Console.Title = "Server";

            var settings = MongoClientSettings.FromConnectionString(mongoURI);
            mongoDBClient = new MongoClient(settings);

            ServerSetup();

            Console.ReadLine(); // When we press enter close everything
            CloseAllSockets();
        }
        private static void AcceptCallback(IAsyncResult AR)
        {
            Socket socket;

            try
            {
                socket = serverSocket.EndAccept(AR);
            }
            catch (ObjectDisposedException)
            {
                return;
            }

            clientSockets.Add(socket);

            ClientManager newClient = new ClientManager(socket);
            clientsProfile.Add(newClient);

            Console.WriteLine("Client connected, waiting for request...");
            serverSocket.BeginAccept(AcceptCallback, null);
        }
        #endregion

        #region Server Setup 
        private static void ServerSetup()
        {
            Console.WriteLine("Setting up server...");

            SetupServerCertificate();

            serverSocket.Bind(new IPEndPoint(IPAddress.Any, PORT));
            serverSocket.Listen(0);
            serverSocket.BeginAccept(AcceptCallback, null);

            Console.WriteLine("Server setup complete");
        }

        private static void SetupServerCertificate()
        {
            Console.WriteLine("Getting server certificate ... ");

            X509Certificate2 cert = GetX509Certificate(serverUUID);

            string filename = serverUUID + "-private-key.txt";
            if (File.Exists(filename))
            {
                string privateKeyXML = File.ReadAllText(filename);
                rsaEncryption = new RsaEncryption(privateKeyXML);
            }
        }

        private static void CloseAllSockets()
        {
            foreach (Socket socket in clientSockets)
            {
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
            }

            serverSocket.Close();
        }
        #endregion

        #region Utils
        private static X509Certificate2 GetX509Certificate(string title)
        {
            // Open the X.509 "Current User" store in read only mode.
            X509Store store = new X509Store(StoreLocation.CurrentUser);

            store.Open(OpenFlags.ReadOnly);
            // Place all certificates in an X509Certificate2Collection object.
            X509Certificate2Collection certCollection = store.Certificates;

            // Loop through each certificate and find the certificate
            // with the appropriate name.
            foreach (X509Certificate2 c in certCollection)
            {
                string cn = "CN=" + title; 
                if (c.Subject == cn)
                {
                    return c; 
                }
            }

            store.Close();
            return CreateX509Certificate(title);
        }

        private static X509Certificate2 CreateX509Certificate(string uuid)
        {
            using (RSACryptoServiceProvider RSA = new RSACryptoServiceProvider(4096))
            {
                // Generate a self-signed certificate using the CertificateRequest class
                var request = new CertificateRequest("CN=" + uuid,
                RSA,
                HashAlgorithmName.SHA256,
                RSASignaturePadding.Pkcs1);

                // Set the certificate to be valid for 1 year
                var startDate = DateTime.Now;
                var endDate = startDate.AddYears(1);

                // Add an extended key usage extension to the certificate
                var extendedKeyUsage = new OidCollection();
                extendedKeyUsage.Add(new Oid("1.3.6.1.5.5.7.3.1")); // Server authentication
                request.CertificateExtensions.Add(new X509EnhancedKeyUsageExtension(extendedKeyUsage, true));

                // Create the self-signed certificate
                X509Certificate2 cert = request.CreateSelfSigned(startDate, endDate);

                // Open the X.509 "Current User" store in read only mode.
                X509Store store = new X509Store(StoreLocation.CurrentUser);

                store.Open(OpenFlags.ReadWrite);

                store.Add(cert);

                store.Close();

                //SavePrivateKeyToDB(uuid, RSA.ToXmlString(true));

                using (StreamWriter sw = File.AppendText(uuid + "-private-key.txt"))
                {
                    sw.WriteLine(RSA.ToXmlString(true));
                }

                Console.WriteLine("New certificate issued ! ");

                return cert;
            }
        }

        private static string GetPrivateKeyFromDB(string uuid)
        {
            IMongoDatabase db = Server.mongoDBClient.GetDatabase("private_keys");
            var profiles = db.GetCollection<BsonDocument>("keys");

            var filter = Builders<BsonDocument>.Filter.Eq("uuid", uuid);

            var doc = profiles.Find(filter).FirstOrDefault();

            if (doc == null)
                return null;

            string private_key = (string)doc.ToDictionary()["private_key"];

            Console.WriteLine(private_key);

            return private_key;
        }

        private static void SavePrivateKeyToDB(string uuid,string privateKey)
        {
            IMongoDatabase db = Server.mongoDBClient.GetDatabase("private_keys");
            var profiles = db.GetCollection<BsonDocument>("keys");

            var doc = new BsonDocument
            {
                {"uuid", uuid},
                {"private_key", privateKey},
            };

            profiles.InsertOne(doc);
        }
        #endregion
    }
}