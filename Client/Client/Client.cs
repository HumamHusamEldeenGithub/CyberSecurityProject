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
using System.Security.Cryptography.X509Certificates;
using System.IO;
using System.Windows.Forms;
using System.Diagnostics;
using System.Linq;

namespace MultiClient
{
    class Client
    {
        #region Parameters
        private const string serverUUID = "www.chatapp.com";

        private static readonly Socket ClientSocket = new Socket
            (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private const int PORT = 100;

        private static string aes_key;
        private static string aes_iv;
        public static RsaEncryption serverRSA;
        public static RsaEncryption userRSA;
        public static RsaEncryption receiverRSA;

        public static ChatApplicationFrom form;

        private static string phone_number;
        #endregion

        #region Main

        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            form = new ChatApplicationFrom();

            Thread thread1 = new Thread(new ThreadStart(StartApp));
            thread1.Start();

            Application.Run(form);
        }

        public static void StartApp()
        {
            ConnectToServer();
            Thread thread2 = new Thread(new ThreadStart(RequestLoop));
            thread2.Start();
            thread2.Join();
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
                    form.AddTextToMainChatBox("Connection attempt " + attempts);
                    ClientSocket.Connect(IPAddress.Loopback, PORT);
                }
                catch (SocketException){}
            }
            TriggerHandShakeEvent();

            form.AddTextToMainChatBox("Connetcted");
        }

        private static void RequestLoop()
        {

            while (true)
            {
                ReceiveResponse();
            }
        }

        private static void Exit()
        {
            //SendString("exit"); // Tell the server we are exiting
            ClientSocket.Shutdown(SocketShutdown.Both);
            ClientSocket.Close();
            Environment.Exit(0);
        }

        private static void HandelIncomingData(SocketMessage socketMessage)
        {
            string flag = socketMessage.Flag;
            string message = socketMessage.Message;

            switch (flag)
            {
                case ("login_successful"):
                    TriggerLoginSuccessfulEvent();
                    GetUserX509Certificate();
                    break;
                case ("login_failed"):
                    TriggerLoginFailedEvent();
                    break;
                case ("ACK"):
                    break;
                case ("msg"):
                    TriggerNewMessageEvent(socketMessage);
                    break;
                case ("RCV"):
                    TriggerMessageReceivedEvent();
                    break;
                case ("ERR"):
                    TriggerErrorEvent(message);
                    break;
                default:
                    form.AddTextToMainChatBox(message);
                    break;
            }
        }

        #endregion

        #region Events

        private static void TriggerHandShakeEvent()
        {
            X509Certificate2 serverCertificate = GetX509Certificate(serverUUID);
            serverRSA = new RsaEncryption(serverCertificate.PublicKey.Key.ToXmlString(false),false);
            GenerateNewAESCreadentials();
            SendAESKey();
        }

        public static void TriggerLoginEvent(string req_phone_number , string req_password) 
        {
            form.AddTextToMainChatBox("Logging ...");

            SocketMessage socketMessage = new SocketMessage
            {
                Flag = "login",
                Message = "-usr=" + req_phone_number + " -pass=" + req_password,
            };
            phone_number = req_phone_number;
            SendSocketMessage(socketMessage);
        }

        private static void TriggerLoginSuccessfulEvent()
        {
            form.AddTextToMainChatBox("Logged in successfully");
        }

        private static void TriggerLoginFailedEvent()
        {
            form.AddTextToMainChatBox("Logged in failed");
            phone_number = "";
        }

        private static void TriggerNewMessageEvent(SocketMessage message)
        {
            // Get the sender certificate 
            X509Certificate2 cert = GetX509Certificate(serverUUID + "/" + message.Sender);
            if (cert == null)
            {
                form.AddTextToMainChatBox("User doesn't have public key");
                return;
            }
            receiverRSA = new RsaEncryption(cert.PublicKey.Key.ToXmlString(false), false);

            if (!RsaEncryption.VerifySignature(Convert.FromBase64String(message.Message),
                receiverRSA.publicKey, Convert.FromBase64String(message.Signature)))
            {
                Debug.WriteLine("invalid signature");
                return;
            }
            string messageStr = Encoding.ASCII.GetString(RsaEncryption.RSADecrypt(Convert.FromBase64String(message.Message), userRSA.privateKey, false));

            // Save and display the message 
            Debug.WriteLine(messageStr);
            form.AddTextToMainChatBox(message.Sender +  " >> " + messageStr);
            AppendMessageToChat(phone_number,message.Sender , message.Sender + " >> " + messageStr);
        }

        private static void TriggerMessageReceivedEvent()
        {
            form.AddTextToMainChatBox("Message Sent and Received successfully !");
        }

        private static void TriggerErrorEvent(string message)
        {
            form.AddTextToMainChatBox("Error occured : " + message);
        }


        #endregion

        #region Send - Receive messages
        private static void ReceiveResponse()
        {
            var buffer = new byte[4096];
            int received = ClientSocket.Receive(buffer, SocketFlags.None);
            if (received == 0) return;

            var data = new byte[received];
            Array.Copy(buffer, data, received);
            string jsonStr = Encoding.ASCII.GetString(data);

            if (aes_key != null)
            {
                jsonStr = AesEncryption.AesEncryptor.DecryptDataWithAes(jsonStr, aes_key, aes_iv);
            }

            SignedSocketMessage signedSocketMessage  =
                JsonSerializer.Deserialize<SignedSocketMessage>(jsonStr);

            SocketMessage socketMessage =
                JsonSerializer.Deserialize<SocketMessage>(signedSocketMessage.Data);

            if (signedSocketMessage.Signature != "")
            {
                if (!RsaEncryption.VerifySignature(Encoding.ASCII.GetBytes(signedSocketMessage.Data) ,
                    serverRSA.publicKey, Convert.FromBase64String(signedSocketMessage.Signature)))
                {
                    form.AddTextToMainChatBox("Signature invalid !!");
                    return; 
                }
            }

            HandelIncomingData(socketMessage);
        }

        public static void SendChatMessage(string receiver, string msg)
        {
            // Save locally
            AppendMessageToChat(phone_number, receiver, phone_number + " >> " + msg);
            form.AddTextToMainChatBox(phone_number + " >> " + msg);

            // Get receiver public key 
            X509Certificate2 cert = GetX509Certificate(serverUUID + "/" + receiver);
            if (cert == null)
            {
                form.AddTextToMainChatBox("User doesn't have public key");
                return;
            }
            receiverRSA = new RsaEncryption(cert.PublicKey.Key.ToXmlString(false), false);

            string encryptedMsg = Convert.ToBase64String(RsaEncryption.RSAEncrypt(Encoding.ASCII.GetBytes(msg), receiverRSA.publicKey, false));
            string signature = Convert.ToBase64String(RsaEncryption.CreateSignature(Convert.FromBase64String(encryptedMsg), userRSA.privateKey));

            SendSocketMessage( new SocketMessage
            {
                Flag = "msg",
                Message = encryptedMsg,
                Receiver = receiver,
                Signature = signature,

            });
        }

        private static void SendAESKey()
        {
            SocketMessage socketMessage = new SocketMessage { Flag = "AES", Message = aes_key + " " + aes_iv };

            string jsonString = JsonSerializer.Serialize(socketMessage);

            SignedSocketMessage signedSocketMessage = new SignedSocketMessage
            {
                Data = jsonString,
            };

            string response = JsonSerializer.Serialize(signedSocketMessage);

            byte[] buffer = Encoding.ASCII.GetBytes(response);

            buffer = RsaEncryption.RSAEncrypt(buffer, serverRSA.publicKey, false);

            ClientSocket.Send(buffer, 0, buffer.Length, SocketFlags.None);
        }

        private static void SendSocketMessage(SocketMessage socketMessage)
        {
            string jsonString = JsonSerializer.Serialize(socketMessage);

            SignedSocketMessage signedSocketMessage = new SignedSocketMessage
            {
                Data = jsonString,
            };

            string response = JsonSerializer.Serialize(signedSocketMessage);

            response = AesEncryption.AesEncryptor.EncryptDataWithAes(response, aes_key, aes_iv);

            byte[] buffer = Encoding.ASCII.GetBytes(response);

            ClientSocket.Send(buffer, 0, buffer.Length, SocketFlags.None);
        }

        #endregion

        #region Helpers

        public static void OpenChat(string receiver)
        {
            form.ClearTextFromMainChatBox();
            string filename = phone_number + "-" + receiver + ".txt";
            if (File.Exists(filename))
            {
                string str = File.ReadAllText(filename);
                form.AddTextToMainChatBox(str);
            }
        }

        private static void AppendMessageToChat(string phone1 ,string phone2, string msg)
        {
            // Creating a file
            string myfile = phone1 + "-"+ phone2 + ".txt";

            // Appending the given texts
            using (StreamWriter sw = File.AppendText(myfile))
            {
                sw.WriteLine(msg);
            }
        }

        #endregion

        #region Utils
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
            form.AddTextToMainChatBox("New AES Key " + aes_keyBase64);
            form.AddTextToMainChatBox("New AES IV " + aes_ivBase64);

            aes_key = aes_keyBase64;
            aes_iv = aes_ivBase64;
        }
        private static void GetUserX509Certificate()
        {
            X509Certificate2 cert = GetX509Certificate(serverUUID + "/" + phone_number);
            if (cert == null)
            {
                CreateX509Certificate();
            }
            string filename = phone_number + "-private-key.txt";
            if (File.Exists(filename))
            {
                form.AddTextToMainChatBox("User PrivateKey Found");
                string str = File.ReadAllText(filename);
                userRSA = new RsaEncryption(str, true);
            }

        }
        private static X509Certificate2 GetX509Certificate(string title)
        {
            X509Certificate2 cert = null;
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
                    cert = c;
                    break;
                }
            }

            store.Close();

            return cert;
        }
        private static X509Certificate2 CreateX509Certificate()
        {
            using (RSACryptoServiceProvider RSA = new RSACryptoServiceProvider(4096))
            {
                // Generate a self-signed certificate using the CertificateRequest class
                var request = new CertificateRequest("CN=" + serverUUID+"/"+phone_number,
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

                /*                // Save the certificate to a file
                                cert.Export(X509ContentType.Pfx, "selfsigned.pfx");*/

                // Open the X.509 "Current User" store in read only mode.
                X509Store store = new X509Store(StoreLocation.CurrentUser);

                store.Open(OpenFlags.ReadWrite);

                store.Add(cert);

                store.Close();

                userRSA = new RsaEncryption(RSA.ToXmlString(true),true);

                using (StreamWriter sw = File.AppendText(phone_number+"-private-key.txt"))
                {
                    sw.WriteLine(RSA.ToXmlString(true));
                }

                form.AddTextToMainChatBox("new certificate");

                return cert;
            }
        }
        #endregion
    }
}