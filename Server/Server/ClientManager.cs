using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using MultiServer;
using MongoDB.Driver;
using MongoDB.Bson;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using Server;

namespace ServerClient
{
    class ClientManager
    {
        #region Parameters
        public string[] commands = { "/login -usr=456132 -pass=abcd1234" , "/logout" , "/chat" , "/msg" , "/h" }; 
        public string phone_number;
        public string password;
        public string currentChatID;
        public string currentReceiver;

        public  Socket socket;
        private const int BUFFER_SIZE = 2048;
        private static readonly byte[] buffer = new byte[BUFFER_SIZE];
        private Dictionary<string,object> profile; 
        private string aes_key , aes_iv ;
        #endregion

        public ClientManager(Socket socket)
        {
            this.socket = socket;

            SendRSAPublicKey();

            //GenerateNewAESCreadentials();

            //SendSocketMessage("Welcome , Here's list of available commands : \n" + string.Join(" , ", commands));
            this.socket.BeginReceive(buffer, 0, BUFFER_SIZE, SocketFlags.None, ReceiveCallback, this.socket);
        }

        #region Events Triggers

        private void ReceiveCallback(IAsyncResult AR)
        {
            Socket current = (Socket)AR.AsyncState;

            string jsonStr = GetMessageFromSocket(AR);
            Console.WriteLine("Received Text: " + jsonStr);

            SignedSocketMessage signedSocketMessage =
                JsonSerializer.Deserialize<SignedSocketMessage>(jsonStr);

            SocketMessage socketMessage =
                JsonSerializer.Deserialize<SocketMessage>(signedSocketMessage.Data);

            switch (socketMessage.Flag)
            {
                case ("login"):
                    TriggerLoginEvent(socketMessage.Message);
                    break;
                case ("logout"):
                    TriggerLogoutEvent();
                    break;
                case ("chat"):
                    TriggerChatEvent(socketMessage.Message);
                    break;
                case ("msg"):
                    TriggerMessageEvent(socketMessage.Message);
                    break;
                case ("AES"):
                    SaveAESCredentials(socketMessage.Message);
                    break;
                default:
                    SendSocketMessage("ERR", "Invalid command");
                    break;
            }

            current.BeginReceive(buffer, 0, BUFFER_SIZE, SocketFlags.None, ReceiveCallback, current);
        }

        private void TriggerLoginEvent(string input)
        {
            if (phone_number != null)
                SendSocketMessage("ERR", "User already logged in ...");

            string pattern = @"-usr=([0-9]+) -pass=([A-Za-z0-9]+)";
            var matches = Regex.Match(input, pattern);

            this.phone_number = matches.Groups[1].Value;
            this.password = matches.Groups[2].Value;

            if (this.phone_number == "" || this.password== "")
            {
                SendSocketMessage("ERR", "Invalid Username and Password");
            }

            if (!CheckUserCredentails())
            {
                Console.WriteLine("Password incorrect ! ");

                SendSocketMessage("ERR","Password incorrect ! ");

                ResetClient();

                return;
            }
            Console.WriteLine("User with phone number " + this.phone_number + " has logged in .");

            SendSocketMessage("ACK","Logged in successfully !");
        }

        private void TriggerLogoutEvent()
        {
            ResetClient();
            SendSocketMessage("ERR","Logged out successfully");
        }

        private void TriggerChatEvent(string receiver)
        {
            if (this.profile == null)
            {
                SendSocketMessage("ERR","Plesae log in first ...");
                return;
            }

            this.currentChatID = GetChatID(receiver);
            if (this.currentChatID == null)
                SendSocketMessage("ERR","User doesn't have an account ...");

            this.currentReceiver = receiver;
            SendSocketMessage("INF",GetChatMessages());
        }

        private void TriggerMessageEvent(string msg)
        {
            if (this.profile == null)
            {
                SendSocketMessage("ERR","Plesae log in first ...");
                return;
            }

            if (this.currentChatID == null || this.currentChatID == "")
            {
                SendSocketMessage("ERR","User doesn't have an account ...");
            }
            SendMessageToPhonenumber(this.currentReceiver, msg);

            SendSocketMessage("RCV","");
        }

        #endregion

        #region MongoDB Function
        private static string GetMessageFromSocket(IAsyncResult AR)
        {
            Socket current = (Socket)AR.AsyncState;
            int received;

            try
            {
                received = current.EndReceive(AR);
                byte[] recBuf = new byte[received];
                Array.Copy(buffer, recBuf, received);

                byte[]  decryptedMessage = RsaEncryption.RSADecrypt(recBuf, Program.rsaEncryption.privateKey, false); 

                string input = Encoding.ASCII.GetString(decryptedMessage);
                return input;
            }
            catch (SocketException)
            {
                Console.WriteLine("Client forcefully disconnected");
                // Don't shutdown because the socket may be disposed and its disconnected anyway.
                current.Close();
                Program.clientSockets.Remove(current);
                return "";
            }
        }

        private void SendMessageToPhonenumber(string receiver, string msg)
        {
            SaveMessageToDB(this.currentChatID, msg);

            msg = receiver + " >> " + msg; 
            
            foreach (ClientManager client in Program.clientsProfile)
            {
                if (client.phone_number == receiver)
                {
                    client.SendSocketMessage("MSG",msg);
                    return;
                }
            }
        }



        private BsonDocument GetUserProfile(string phonenumber)
        {
            IMongoDatabase db = Program.mongoDBClient.GetDatabase("users");
            var profiles = db.GetCollection<BsonDocument>("profiles");

            var filter = Builders<BsonDocument>.Filter.Eq("phonenumber", this.phone_number);

            var doc = profiles.Find(filter).FirstOrDefault();

            return doc;
        }

        private void CreateNewProfile()
        {
            IMongoDatabase db = Program.mongoDBClient.GetDatabase("users");
            var profiles = db.GetCollection<BsonDocument>("profiles");
            string uuid = System.Guid.NewGuid().ToString();

            var doc = new BsonDocument
            {
                {"uuid", uuid},
                {"phonenumber", this.phone_number},
                {"password", this.password},
                {"chats", new BsonArray{ } }
            };

            profiles.InsertOne(doc);
            this.profile = doc.ToDictionary();
        }

        private void SaveMessageToDB(string chatID , string msg)
        {
            IMongoDatabase db = Program.mongoDBClient.GetDatabase("users");
            var chats = db.GetCollection<BsonDocument>("chats");

            var doc = new BsonDocument
            {
                {"chat_id", chatID} ,
                {"sender", this.phone_number},
                {"message", msg }
            };

            chats.InsertOne(doc);
        }

        private string GetChatID(string receiver)
        {
            var chats =(Object[])this.profile["chats"];
            foreach (Dictionary<string,Object> chat in chats)
            {
                var chatDic = chat;
                if (string.Compare(chatDic["receiver"].ToString() , receiver) == 0)
                {
                    return chatDic["chat_id"].ToString();
                }
            }

            var receiverProfile = GetUserProfile(receiver);
            if (receiverProfile == null)
                return null; 

            string newChatID = System.Guid.NewGuid().ToString();

            AddNewChatToProfile(newChatID, receiver);

            return newChatID; 
        }


        private string GetChatMessages()
        {
            IMongoDatabase db = Program.mongoDBClient.GetDatabase("users");
            var chats = db.GetCollection<BsonDocument>("chats");

            var filter = Builders<BsonDocument>.Filter.Eq("chat_id", this.currentChatID);

            var docs = chats.Find(filter).ToList();

            string messages = "Messages:\n"; 

            docs.ForEach(doc =>
            {
                var tempDic = doc.ToDictionary();
                string msg = (string)tempDic["message"];
                string sender = (string)tempDic["sender"];

                messages += sender; 
                if (sender == this.phone_number)
                    messages += " >> ";
                else
                    messages += " << ";
                messages += msg + "\n";
            });
            return messages; 
        }


        // TODO : Update using async 
        public void AddNewChatToProfile(string chatID , string receiver)
        {
            var filter1 = Builders<BsonDocument>.Filter.Eq("phonenumber",this.phone_number);
            var update1 = Builders<BsonDocument>.Update.Push("chats", new BsonDocument { { "chat_id", chatID }, { "receiver", receiver } });

            var filter2 = Builders<BsonDocument>.Filter.Eq("phonenumber", receiver);
            var update2 = Builders<BsonDocument>.Update.Push("chats", new BsonDocument { { "chat_id", chatID }, { "receiver", this.phone_number} });

            IMongoDatabase db = Program.mongoDBClient.GetDatabase("users");
            var profiles = db.GetCollection<BsonDocument>("profiles");

            profiles.UpdateOne(filter1, update1);
            profiles.UpdateOne(filter2, update2);
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
        private void SendSocketMessage(string flag,string msg)
        {
            SocketMessage socketMessage = new SocketMessage { 
                Flag=flag,
                Message=msg,
            };

            string jsonString = JsonSerializer.Serialize(socketMessage);

            byte[] signatureBuff = RsaEncryption.CreateSignature(Encoding.ASCII.GetBytes(jsonString), Program.rsaEncryption.privateKey);

            SignedSocketMessage signedSocketMessage = new SignedSocketMessage
            {
                Data = jsonString,
                Signature = Convert.ToBase64String(signatureBuff)
            }; 

            Console.WriteLine(RsaEncryption.VerifySignature(Encoding.ASCII.GetBytes(jsonString),Program.rsaEncryption.publicKey ,Convert.FromBase64String(signedSocketMessage.Signature)));

            string response = JsonSerializer.Serialize(signedSocketMessage);

            if (this.aes_key != null)
            {
                response = AesEncryption.Encryptor.EncryptDataWithAes(response, aes_key, aes_iv);
            }

            this.socket.Send(Encoding.ASCII.GetBytes(response));
        }

        private void  GenerateNewAESCreadentials()
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

            SendSocketMessage("AES", aes_keyBase64 + " " + aes_ivBase64);

            this.aes_key = aes_keyBase64;
            this.aes_iv = aes_ivBase64;
        }

        private void SaveAESCredentials(string data)
        {
            // TODO : VALIDATE AES KEY
            string[] temp = data.Split(' ');
            this.aes_key = temp[0];
            this.aes_iv = temp[1];
        }

        private void SendRSAPublicKey()
        {
            SocketMessage socketMessage = new SocketMessage
            {
                Flag = "PUB_KEY",
                Message = Program.rsaEncryption.publicKeyStr,
            };

            Console.WriteLine(socketMessage.Message);

            string jsonString = JsonSerializer.Serialize(socketMessage);

            SignedSocketMessage signedSocketMessage = new SignedSocketMessage
            {
                Data = jsonString,
                Signature = ""
            };

            string response = JsonSerializer.Serialize(signedSocketMessage);

            Console.WriteLine(response);

            this.socket.Send(Encoding.ASCII.GetBytes(response));
        }
        #endregion
    }
}
