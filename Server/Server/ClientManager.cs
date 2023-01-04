using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using MultiServer;
using MongoDB.Driver;
using MongoDB.Bson;
using System.Text.RegularExpressions;
using System.Text.Json;
using Server;

namespace ServerClient
{
    class ClientManager
    {
        #region Parameters

        public string phone_number;
        private string password;
        private Dictionary<string, object> profile;

        public  Socket socket;
        private const int BUFFER_SIZE = 4096;
        private static readonly byte[] buffer = new byte[BUFFER_SIZE];

        private string aes_key , aes_iv ;

        #endregion

        #region Main
        public ClientManager(Socket socket)
        {
            this.socket = socket;

            this.socket.BeginReceive(buffer, 0, BUFFER_SIZE, SocketFlags.None, MainLoop, this.socket);
        }

        private void MainLoop(IAsyncResult AR)
        {
            Socket current = (Socket)AR.AsyncState;

            SocketMessage socketMessage = GetMessageFromSocket(AR);

            switch (socketMessage.Flag)
            {
                case ("login"):
                    TriggerLoginEvent(socketMessage.Message);
                    break;
                case ("logout"):
                    TriggerLogoutEvent();
                    break;
                case ("msg"):
                    TriggerMessageEvent(socketMessage);
                    break;
                case ("AES"):
                    SaveAESCredentials(socketMessage.Message);
                    break;
                default:
                    SendSocketMessage(new SocketMessage
                    {
                        Flag = "ERR" , 
                        Message = "Invalid command"
                    });
                    break;
            }
            current.BeginReceive(buffer, 0, BUFFER_SIZE, SocketFlags.None, MainLoop, current);
        }

        #endregion

        #region Events
        private void TriggerLoginEvent(string input)
        {
            if (this.phone_number != null)
                SendSocketMessage(new SocketMessage
                {
                    Flag = "ERR",
                    Message = "User already logged in ..."
                });

            string pattern = @"-usr=([0-9]+) -pass=([A-Za-z0-9]+)";
            var matches = Regex.Match(input, pattern);

            this.phone_number = matches.Groups[1].Value;
            this.password = matches.Groups[2].Value;

            if (this.phone_number == "" || this.password== "")
            {
                SendSocketMessage(new SocketMessage
                {
                    Flag = "ERR",
                    Message = "Invalid Username and Password"
                });
            }

            if (!CheckUserCredentails())
            {
                Console.WriteLine("Password incorrect ! ");

                SendSocketMessage(new SocketMessage
                {
                    Flag = "ERR",
                    Message = "Password incorrect ! "
                });

                ResetClient();

                return;
            }

            Console.WriteLine("User with phone number " + this.phone_number + " has logged in .");

            SendSocketMessage(new SocketMessage
            {
                Flag = "login_successful",
                Message = "Logged in successfully!"
            });
            CheckForIncomingMessages();
        }

        private void TriggerLogoutEvent()
        {
            ResetClient();
            SendSocketMessage(new SocketMessage
            {
                Flag = "logout_successful",
                Message = "Logged out successfully"
            });
        }

        private void TriggerMessageEvent(SocketMessage msg)
        {
            if (this.profile == null)
            {
                SendSocketMessage(new SocketMessage
                {
                    Flag = "ERR",
                    Message = "Plesae log in first ..."
                });
                return;
            }
            msg.Sender = this.phone_number; 
            SendChatMessage(msg);
        }

        private void CheckForIncomingMessages()
        {
            foreach (SocketMessage message in MultiServer.Server.MessageQueue.ToList())
            {
                if (message.Receiver == this.phone_number)
                {
                    SendSocketMessage(message);
                    MultiServer.Server.MessageQueue.Remove(message);
                    System.Threading.Thread.Sleep(500);
                }
            }
        }

        #endregion

        #region Send - Receive messages

        private SocketMessage GetMessageFromSocket(IAsyncResult AR)
        {
            Socket current = (Socket)AR.AsyncState;
            int received;

            try
            {
                received = current.EndReceive(AR);
                byte[] recBuf = new byte[received];
                Array.Copy(buffer, recBuf, received);
                string input = Encoding.ASCII.GetString(recBuf); ;

                if (aes_key != null)
                {
                    input = AesEncryption.Encryptor.DecryptDataWithAes(Encoding.ASCII.GetString(recBuf), aes_key, aes_iv);
                }
                else
                {
                    byte[] decryptedMessage = RsaEncryption.RSADecrypt(recBuf, MultiServer.Server.rsaEncryption.privateKey, false);

                    input = Encoding.ASCII.GetString(decryptedMessage);
                }

                SignedSocketMessage signedSocketMessage =
                    JsonSerializer.Deserialize<SignedSocketMessage>(input);

                SocketMessage socketMessage =
                    JsonSerializer.Deserialize<SocketMessage>(signedSocketMessage.Data);

                return socketMessage;
            }
            catch (SocketException)
            {
                Console.WriteLine("Client forcefully disconnected");
                // Don't shutdown because the socket may be disposed and its disconnected anyway.
                current.Close();
                MultiServer.Server.clientSockets.Remove(current);
                return null;
            }
        }

        private void SendChatMessage(SocketMessage msg)
        {

            foreach (ClientManager client in MultiServer.Server.clientsProfile)
            {
                if (client.phone_number == msg.Receiver)
                {
                    client.SendSocketMessage(msg);
                    return;
                }
            }
            MultiServer.Server.MessageQueue.Add(msg);
        }

        private void SendSocketMessage(SocketMessage socketMessage)
        {
            string jsonString = JsonSerializer.Serialize(socketMessage);

            byte[] signatureBuff = RsaEncryption.CreateSignature(Encoding.ASCII.GetBytes(jsonString), MultiServer.Server.rsaEncryption.privateKey);

            SignedSocketMessage signedSocketMessage = new SignedSocketMessage
            {
                Data = jsonString,
                Signature = Convert.ToBase64String(signatureBuff)
            };

            string response = JsonSerializer.Serialize(signedSocketMessage);

            if (this.aes_key != null)
            {
                response = AesEncryption.Encryptor.EncryptDataWithAes(response, aes_key, aes_iv);
            }

            this.socket.Send(Encoding.ASCII.GetBytes(response));
        }

        #endregion

        #region MongoDB Function

        private BsonDocument GetUserProfile(string phonenumber)
        {
            IMongoDatabase db = MultiServer.Server.mongoDBClient.GetDatabase("users");
            var profiles = db.GetCollection<BsonDocument>("profiles");

            var filter = Builders<BsonDocument>.Filter.Eq("phonenumber", this.phone_number);

            var doc = profiles.Find(filter).FirstOrDefault();

            return doc;
        }

        private void CreateNewProfile()
        {
            IMongoDatabase db = MultiServer.Server.mongoDBClient.GetDatabase("users");
            var profiles = db.GetCollection<BsonDocument>("profiles");
            string uuid = System.Guid.NewGuid().ToString();

            var doc = new BsonDocument
            {
                {"uuid", uuid},
                {"phonenumber", this.phone_number},
                {"password", this.password}
            };

            profiles.InsertOne(doc);
            this.profile = doc.ToDictionary();
        }

        #endregion

        #region Utils
        private bool CheckUserCredentails()
        {
            var bsonDoc = GetUserProfile(this.phone_number);
            if (bsonDoc == null)
            {
                CreateNewProfile();
                return true;
            }
            this.profile = bsonDoc.ToDictionary();
            string pass = (string)profile["password"];

            // TODO : Hash and Compare the hash 
            if (string.Compare(pass, this.password) == 0)
                return true;
            return false;
        }
        private void ResetClient()
        {
            this.phone_number = null;
            this.password = null;
            this.profile = null;
        }
        private void SaveAESCredentials(string data)
        {
            string[] temp = data.Split(' ');
            this.aes_key = temp[0];
            this.aes_iv = temp[1];
        }
        #endregion
    }
}
