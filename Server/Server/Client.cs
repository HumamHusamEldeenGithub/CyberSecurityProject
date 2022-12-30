using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using MultiServer;
using MongoDB.Driver;
using MongoDB.Bson;

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
        public string phone_number;
        public string password;
        public Socket socket;
        public Status currentStatus = Status.NewUser;

        private const int BUFFER_SIZE = 2048;
        private static readonly byte[] buffer = new byte[BUFFER_SIZE];
        private Dictionary<string,object> profile; 

        public Client(Socket socket)
        {
            this.socket = socket;
            ReceiveUsernameAndPhonenumber(null);
        }
        private void ReceiveUsernameAndPhonenumber(IAsyncResult AR)
        {
            if (this.currentStatus == Status.NewUser)
            {
                byte[] data = Encoding.ASCII.GetBytes("Welcome , Enter your phone number: ");
                socket.Send(data);

                this.currentStatus = Status.Logging;

                this.socket.BeginReceive(buffer, 0, BUFFER_SIZE, SocketFlags.None, this.ReceiveUsernameAndPhonenumber, this.socket);
                return;
            }
            else if (this.currentStatus == Status.Logging)
            {
                Socket current = (Socket)AR.AsyncState;
                string input = GetMessageFromSocket(AR);

                if (this.phone_number == null)
                {
                    this.phone_number = input;
                    Console.WriteLine("Recieved phone number : " + this.phone_number);

                    byte[] data = Encoding.ASCII.GetBytes("Enter password : ");
                    current.Send(data);

                    this.socket.BeginReceive(buffer, 0, BUFFER_SIZE, SocketFlags.None, this.ReceiveUsernameAndPhonenumber, current);
                }
                else if (this.password == null)
                {
                    this.password = input;
                    Console.WriteLine("Recieved password : " + this.password);

                    if (!CheckPhonenumberAndPassword())
                    {
                        byte[] error = Encoding.ASCII.GetBytes("Password incorrect ! ");
                        Console.WriteLine("Password incorrect ! ");

                        current.Send(error);

                        ResetClient();

                        this.socket.BeginReceive(buffer, 0, BUFFER_SIZE, SocketFlags.None, this.ReceiveUsernameAndPhonenumber, current);
                    }
                    else
                    {
                        byte[] data = Encoding.ASCII.GetBytes("Done!\nTo send messgae enter the number then the message in one request \nExample : '0999558844 Hi'");
                        current.Send(data);

                        currentStatus = Status.Logged;
                        this.socket.BeginReceive(buffer, 0, BUFFER_SIZE, SocketFlags.None, this.ReceiveCallback, current);
                    }
                }
            }
        }
        private void ReceiveCallback(IAsyncResult AR)
        {
            Socket current = (Socket)AR.AsyncState;

            string text = GetMessageFromSocket(AR);
            Console.WriteLine("Received Text: " + text);

            string[] input = text.Split(' ');
            string requested_phonenumber = input[0];
            input[0] = "";
            string msg = string.Join(" ", input);

            SendMessageToPhonenumber(requested_phonenumber, msg);

            current.BeginReceive(buffer, 0, BUFFER_SIZE, SocketFlags.None, ReceiveCallback, current);
            /*if (text.ToLower() == "get time") // Client requested time
            {
                Console.WriteLine("Text is a get time request");
                byte[] data = Encoding.ASCII.GetBytes(DateTime.Now.ToLongTimeString());
                current.Send(data);
                Console.WriteLine("Time sent to client");
            }
            else if (text.ToLower() == "exit") // Client wants to exit gracefully
            {
                // Always Shutdown before closing
                current.Shutdown(SocketShutdown.Both);
                current.Close();
                //clientSockets.Remove(current);
                Console.WriteLine("Client disconnected");
                return;
            }
            else
            {
                Console.WriteLine("Text is an invalid request");
                byte[] data = Encoding.ASCII.GetBytes("Invalid request");
                current.Send(data);
                Console.WriteLine("Warning Sent");
            }*/
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

        private void SendMessageToPhonenumber(string phonenumber, string msg)
        {
            
            foreach (Client client in Program.clientsProfile)
            {
                if (client.phone_number == phonenumber)
                {
                    client.socket.Send(Encoding.ASCII.GetBytes(msg));
                    return;
                }
            }
        }

        private bool CheckPhonenumberAndPassword()
        {
            var bsonDoc = GetUserProfile(this.phone_number);
            if (profile == null)
            {
                CreateNewProfile();
                return true;
            }
            this.profile = bsonDoc.ToDictionary();
            string pass = (string)profile["password"];

            // TODO : Hash and Compare the hash 
            Console.WriteLine(string.Compare(pass, this.password));
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
            var doc = new BsonDocument
            {
                {"uuid", uuid},
                {"phonenumber", this.phone_number},
                {"password", this.password},
                {"chats", new BsonArray{ } }
            };

            profiles.InsertOne(doc);
        }

        private void SaveMessageToDB(string receiver , string msg)
        {
            //TODO Get Chat ID 

            IMongoDatabase db = Program.mongoDBClient.GetDatabase("users");
            var profiles = db.GetCollection<BsonDocument>("chats");

            var doc = new BsonDocument
            {
                {"from", this.phone_number },
                {"to", receiver} ,
                {"message", msg }
            };

            profiles.InsertOne(doc);
        }

        private string GetChatID(string receiver)
        {
            var chats = (BsonArray)profile["chats"];
            foreach (BsonDocument chat in chats)
            {
                var chatDic = chat.ToDictionary();
                if (string.Compare(chatDic["receiver"].ToString() , receiver) == 0)
                {
                    return chatDic["uuid"].ToString();
                }
            }
            IMongoDatabase db = Program.mongoDBClient.GetDatabase("users");
            var profiles = db.GetCollection<BsonDocument>("profiles");
            string uuid = System.Guid.NewGuid().ToString();
            /*profiles.Update(Query.EQ("phonenumber", "Aurora"), Update.Push("loves", "sugar"));*/
            return ""; 
        }

        private void ResetClient()
        {
            this.phone_number = null;
            this.password = null;
            this.currentStatus = Status.NewUser;
            this.profile = null;
        }
    }
}
