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
using Server;

namespace MultiServer
{
    class Program
    {
        #region Parameters
        private static readonly Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        public static readonly List<Socket> clientSockets = new List<Socket>();
        public static readonly List<ClientManager> clientsProfile = new List<ClientManager>();
        private const int PORT = 100;

        private const int BUFFER_SIZE = 2048;
        private static readonly byte[] buffer = new byte[BUFFER_SIZE];

        public static RsaEncryption rsaEncryption = new RsaEncryption(); 

        public static MongoClient mongoDBClient;
        private const string mongoURI = "mongodb+srv://Humam:pwaIR4tEdVQIYcll@cyberproject.rfgzhgh.mongodb.net/?retryWrites=true&w=majority";
        #endregion

        static void Main()
        {
            Console.Title = "Server";

            var settings = MongoClientSettings.FromConnectionString(mongoURI);
            mongoDBClient = new MongoClient(settings);

            SetupServer();
            Console.ReadLine(); // When we press enter close everything
            CloseAllSockets();
        }
        #region Setup Server
        private static void SetupServer()
        {
            Console.WriteLine("Setting up server...");
            serverSocket.Bind(new IPEndPoint(IPAddress.Any, PORT));
            serverSocket.Listen(0);
            serverSocket.BeginAccept(AcceptCallback, null);
            Console.WriteLine("Server setup complete");
        }

        /// <summary>
        /// Close all connected client (we do not need to shutdown the server socket as its connections
        /// are already closed with the clients).
        /// </summary>
        private static void CloseAllSockets()
        {
            foreach (Socket socket in clientSockets)
            {
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
            }

            serverSocket.Close();
        }

        private static void AcceptCallback(IAsyncResult AR)
        {
            Socket socket;

            try
            {
                socket = serverSocket.EndAccept(AR);
            }
            catch (ObjectDisposedException) // I cannot seem to avoid this (on exit when properly closing sockets)
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
    }
}