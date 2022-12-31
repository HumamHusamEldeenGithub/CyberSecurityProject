using System;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using System.Text.RegularExpressions;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Security.Cryptography;
using Client; 

namespace MultiClient
{
    class Client
    {

        public enum Flags
        {
            PUB_KEY,
            ERR,
            MSG,
            INF,
            AES
        };

        private static readonly Socket ClientSocket = new Socket
            (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private static string aes_key;
        private static string aes_iv;

        public static RsaEncryption rsaEncryption ;

        private static readonly string[] flags = { "PUB_KEY", "ERR" , "MSG" , "INF" , "AES" };

        private const int PORT = 100;

        static void Main()
        {
            Console.Title = "Client";
            ConnectToServer();
            Thread t1 = new Thread(new ThreadStart(SendLoop));
            Thread t2 = new Thread(new ThreadStart(RequestLoop));
            t2.Start();
            t1.Start();
            t2.Join();
            Exit();
        }

        private static void ConnectToServer()
        {
            int attempts = 0;

            while (!ClientSocket.Connected)
            {
                try
                {
                    attempts++;
                    Console.WriteLine("Connection attempt " + attempts);
                    // Change IPAddress.Loopback to a remote IP to connect to a remote host.
                    ClientSocket.Connect(IPAddress.Loopback, PORT);
                }
                catch (SocketException)
                {
                    Console.Clear();
                }
            }

            Console.Clear();
            Console.WriteLine("Connected");
        }

        private static void SendLoop()
        {
            
            while (true)
            {
                SendRequest();
            }
        }
        private static void RequestLoop()
        {

            while (true)
            {
                ReceiveResponse();
            }
        }

        /// <summary>
        /// Close socket and exit program.
        /// </summary>
        private static void Exit()
        {
            SendString("exit"); // Tell the server we are exiting
            ClientSocket.Shutdown(SocketShutdown.Both);
            ClientSocket.Close();
            Environment.Exit(0);
        }

        private static void SendRequest()
        {
            Console.WriteLine("FLAG = "); 
            string requestFlag = Console.ReadLine();
            Console.WriteLine("message = ");
            string requestMsg = Console.ReadLine();

            SocketMessage socketMessage = new SocketMessage
            {
                Flag = requestFlag,
                Message = requestMsg
            };

            string jsonString = JsonSerializer.Serialize(socketMessage);

            SendString(jsonString);

            if (requestFlag.ToLower() == "exit")
            {
                Exit();
            }
        }

        /// <summary>
        /// Sends a string to the server with ASCII encoding.
        /// </summary>
        private static void SendString(string text)
        {
            byte[] buffer = Encoding.ASCII.GetBytes(text);
            if (rsaEncryption != null)
            {
                buffer = RsaEncryption.RSAEncrypt(buffer, rsaEncryption.publicKey, false);
            }
            
            ClientSocket.Send(buffer, 0, buffer.Length, SocketFlags.None);
        }

        private static void ReceiveResponse()
        {
            var buffer = new byte[2048];
            int received = ClientSocket.Receive(buffer, SocketFlags.None);
            if (received == 0) return;

            var data = new byte[received];
            Array.Copy(buffer, data, received);
            string jsonStr = Encoding.ASCII.GetString(data);

            if (aes_key != null)
            {
                jsonStr = AesEncryption.Encryptor.DecryptDataWithAes(jsonStr, aes_key, aes_iv);
            }

            SocketMessage socketMessage =
                JsonSerializer.Deserialize<SocketMessage>(jsonStr);

            HandelIncomingData(socketMessage.Flag ,socketMessage.Message);
        }

        private static void HandelIncomingData(string flag, string message)
        {
            switch (flag)
            {
                case ("ACK"):
                    break;
                case ("PUB_KEY"):
                    SaveRSAPublicKey(message);
                    GenerateNewAESCreadentials();
                    SendAESKey();
                    break;
                case ("AES"):
                    SaveAESCredentials(message);
                    break;
                case ("RCV"):
                    TriggerMessageReceivedEvent();
                    break;
                case ("ERR"):
                    TriggerErrorEvent();
                        break; 
                default:
                    Console.WriteLine(message);
                    break;
            }
        }

        private static void GenerateNewAESCreadentials()
        {
            string aes_keyBase64, aes_ivBase64;
            using (Aes aesAlgorithm = Aes.Create())
            {
                aesAlgorithm.KeySize = 256;
                aesAlgorithm.GenerateKey();
                aesAlgorithm.GenerateIV();
                aes_keyBase64 = Convert.ToBase64String(aesAlgorithm.Key);
                aes_ivBase64 = Convert.ToBase64String(aesAlgorithm.IV);
            }
            Console.WriteLine("New AES Key " + aes_keyBase64);
            Console.WriteLine("New AES IV " + aes_ivBase64);

            aes_key = aes_keyBase64;
            aes_iv = aes_ivBase64;
        }

        private static void SaveAESCredentials(string data)
        {
            string[] temp = data.Split(' ');
            aes_key = temp[0];
            aes_iv = temp[1];
        }

        private static void SendAESKey()
        {
            SocketMessage socketMessage = new SocketMessage
            {
                Flag = "AES",
                Message = aes_key+" "+aes_iv
            };

            string jsonString = JsonSerializer.Serialize(socketMessage);

            SendString(jsonString);
        }

        private static void SaveRSAPublicKey(string key)
        {
            rsaEncryption = new RsaEncryption(key);
        }

        private static void TriggerMessageReceivedEvent()
        {
            Console.WriteLine("Message Sent and Received successfully !");
        }

        private static void TriggerErrorEvent()
        {
            Console.WriteLine("Error occured !");
        }
    }
}