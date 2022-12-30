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

namespace ServerClient
{
    class Client
    {
        public enum Status
        {
            NewUser , 
            Logging,
            Logged
        }

        public string[] commands = { "/login -usr=456132 -pass=abcd1234" , "/logout" , "/chat" , "/msg" , "/h" }; 
        public string phone_number;
        public string password;
        public string currentChatID;
        public string currentReceiver;
        public Socket socket;
        public Status currentStatus = Status.NewUser;

        private const int BUFFER_SIZE = 2048;
        private static readonly byte[] buffer = new byte[BUFFER_SIZE];
        private Dictionary<string,object> profile; 
        private string aes_key , aes_iv ; 

        public Client(Socket socket)
        {
            this.socket = socket;
            SendSocketMessage("Welcome , Here's list of available commands : \n" + string.Join(" , ", commands));
            this.socket.BeginReceive(buffer, 0, BUFFER_SIZE, SocketFlags.None, ReceiveCallback, this.socket);
        }

        private void ReceiveCallback(IAsyncResult AR)
        {
            Socket current = (Socket)AR.AsyncState;

            string text = GetMessageFromSocket(AR);
            Console.WriteLine("Received Text: " + text);

            if (aes_key != null)
            {
                text = AesEncryption.Encryptor.DecryptDataWithAes(text, aes_key, aes_iv);
                Console.WriteLine("Decrypt Text : " + text);
            }

            string pattern = @"/([a-zA-Z]+)(.*)";
            var matches = Regex.Match(text, pattern);

            string command = matches.Groups[1].Value;
            string msg = matches.Groups[2].Value.Trim();

            switch (command)
            {
                case ("login"):
                    TriggerLoginEvent(msg);
                    break;
                case ("logout"):
                    TriggerLogoutEvent();
                    break;
                case ("chat"):
                    TriggerChatEvent(msg);
                    break;
                case ("msg"):
                    TriggerMessageEvent(msg);
                    break;
                case ("h"):
                    break;
                default:
                    SendSocketMessage("Invalid command");
                    break;
            }

            current.BeginReceive(buffer, 0, BUFFER_SIZE, SocketFlags.None, ReceiveCallback, current);
/*
            else if (text.ToLower() == "exit") // Client wants to exit gracefully
            {
                // Always Shutdown before closing
                current.Shutdown(SocketShutdown.Both);
                current.Close();
                //clientSockets.Remove(current);
                Console.WriteLine("Client disconnected");
                return;
            }*/
        }

        private void TriggerLoginEvent(string input)
        {
            if (phone_number != null)
                SendSocketMessage("User already logged in ...");

            string pattern = @"-usr=([0-9]+) -pass=([A-Za-z0-9]+)";
            var matches = Regex.Match(input, pattern);

            this.phone_number = matches.Groups[1].Value;
            this.password = matches.Groups[2].Value;

            if (this.phone_number == "" || this.password== "")
            {
                SendSocketMessage("Invalid Username and Password");
            }

            if (!CheckPhonenumberAndPassword())
            {
                Console.WriteLine("Password incorrect ! ");

                SendSocketMessage("Password incorrect ! ");

                ResetClient();
            }
            Console.WriteLine("User with phone number " + this.phone_number + " has logged in .");

            string aes_key = (string)this.profile["aes_key"];
            string aes_iv = (string)this.profile["aes_iv"];

            SendSocketMessage("<|AES|> " + aes_key + " " + aes_iv);

            this.aes_key = aes_key;
            this.aes_iv = aes_iv;

        }

        private void TriggerLogoutEvent()
        {
            ResetClient();
            SendSocketMessage("Logged out successfully");
        }

        private void TriggerChatEvent(string receiver)
        {
            if (this.profile == null)
            {
                SendSocketMessage("Plesae log in first ...");
                return;
            }

            this.currentChatID = GetChatID(receiver);
            if (this.currentChatID == null)
                this.socket.Send(Encoding.ASCII.GetBytes("User doesn't have an account ..."));
            this.currentReceiver = receiver;
            SendSocketMessage(GetChatMessages());
        }

        private void TriggerMessageEvent(string msg)
        {
            if (this.profile == null)
            {
                SendSocketMessage("Plesae log in first ...");
                return;
            }

            if (this.currentChatID == null || this.currentChatID == "")
            {
                SendSocketMessage("User doesn't have an account ...");
            }
            SendMessageToPhonenumber(this.currentReceiver, msg);
        }



        private static string GetMessageFromSocket(IAsyncResult AR)
        {
            Socket current = (Socket)AR.AsyncState;
            int received;

            try
            {
                received = current.EndReceive(AR);
                byte[] recBuf = new byte[received];
                Array.Copy(buffer, recBuf, received);
                string input = Encoding.ASCII.GetString(recBuf);
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

        private string SendMessageToPhonenumber(string receiver, string msg)
        {
            SaveMessageToDB(this.currentChatID, msg);

            msg = receiver + " >> " + msg; 
            
            foreach (Client client in Program.clientsProfile)
            {
                if (client.phone_number == receiver)
                {
                    client.socket.Send(Encoding.ASCII.GetBytes(msg));
                    return "Sent and Received";
                }
            }
            return "Sent but not received";
        }

        private bool CheckPhonenumberAndPassword()
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
            if (string.Compare(pass , this.password) == 0)
                return true;
            return false;
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
            string aes_keyBase64, aes_ivBase64;

            using (Aes aesAlgorithm = Aes.Create())
            {
                aesAlgorithm.KeySize = 256;
                aesAlgorithm.GenerateKey();
                aesAlgorithm.GenerateIV();
                aes_keyBase64 = Convert.ToBase64String(aesAlgorithm.Key);
                aes_ivBase64 = Convert.ToBase64String(aesAlgorithm.IV);
            }

            var doc = new BsonDocument
            {
                {"uuid", uuid},
                {"phonenumber", this.phone_number},
                {"password", this.password},
                {"aes_key" , aes_keyBase64},
                {"aes_iv" , aes_ivBase64},
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

        private void ResetClient()
        {
            this.phone_number = null;
            this.password = null;
            this.currentStatus = Status.NewUser;
            this.profile = null;
        }
        private void SendSocketMessage(string msg)
        {
            if (this.aes_key != null)
            {
                msg = AesEncryption.Encryptor.EncryptDataWithAes(msg, aes_key, aes_iv);
            }

            this.socket.Send(Encoding.ASCII.GetBytes(msg));
        }










       /*private void ReceiveUsernameAndPhonenumber(IAsyncResult AR)
        {
            if (this.currentStatus == Status.NewUser)
            {
                SendSocketMessage("Enter phone number: ");

                this.currentStatus = Status.Logging;

                this.socket.BeginReceive(buffer, 0, BUFFER_SIZE, SocketFlags.None, this.ReceiveUsernameAndPhonenumber, this.socket);
            }
            else if (this.currentStatus == Status.Logging)
            {
                Socket current = (Socket)AR.AsyncState;
                string input = GetMessageFromSocket(AR);

                if (this.phone_number == null)
                {
                    this.phone_number = input;
                    Console.WriteLine("Received phone number : " + this.phone_number);

                    SendSocketMessage("Enter password : ");

                    this.socket.BeginReceive(buffer, 0, BUFFER_SIZE, SocketFlags.None, this.ReceiveUsernameAndPhonenumber, this.socket);
                }
                else if (this.password == null)
                {
                    this.password = input;
                    Console.WriteLine("Recieved password : " + this.password);

                    if (!CheckPhonenumberAndPassword())
                    {
                        Console.WriteLine("Password incorrect ! ");

                        SendSocketMessage("Password incorrect ! ");

                        ResetClient();
                    }
                }
            }
        }*/






    }
}
