using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class SocketMessage
{
    public string Flag { get; set; }
    public string MessageUUID { get; set; }
    public string Message { get; set; }
    public string Sender { get; set; }
    public string Receiver { get; set; }
    public string Signature { get; set; }

}
class SignedSocketMessage
{
    public string Data { get; set; }
    public string Signature { get; set; }

}
class ChatMessage
{
    public string UUID;
    public string Message;
    public string Sender;
    public bool Received;
}
