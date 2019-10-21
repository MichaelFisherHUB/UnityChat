using System.Net.Sockets;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using System.Text;
using UnityChat.Messages;
using Newtonsoft.Json;

public class NetworkClientInfo
{
    public readonly TcpClient tcpClient;
    public bool isConnected;
    public bool isLogedIn;
    public int ID;

    private NetworkStream _netStream;
    public NetworkStream NetStream
    {
        get
        {
            return _netStream ?? (_netStream = tcpClient.GetStream());
        }
    }

    public Queue<byte> massegeBuffer = new Queue<byte>();
    
    public string MessageJSON { get; private set; }

    public BaseMessage Message { get; private set; }

    public UserInformation userInformation;

    public bool isAcceptSomeMessage
    {
        get
        {
            return NetStream.DataAvailable;
        }
    }

    public void ReadMessageFromThis(System.Action<BaseMessage> MessageCallback, bool inMainThread = true)
    {
        try
        {
            if (inMainThread)
            {
                MessageAcceptHandler();

                MessageCallback?.Invoke(Message);
            }
            else
            {
                // TODO Make Threads or Tasks
            }
        }
        catch (AggregateException ae)
        {
            Console.WriteLine("One or more exceptions occurred: ");
            foreach (var ex in ae.Flatten().InnerExceptions)
            {
                Console.WriteLine("   {0}", ex.Message);
            }
        }
    }

    public bool SendMassegeToThis<T>(T message) where T : BaseMessage
    {
        return SendMassegeToThis(JsonConvert.SerializeObject(message));
    }

    private bool SendMassegeToThis(string JSONmassege)
    {
        return SendMessegeToThis(Encoding.ASCII.GetBytes(JSONmassege.ToCharArray()));
    }

    public bool SendMessegeToThis(byte[] message)
    {
        try
        {
            NetStream.Write(message, 0, message.Length);
            return true;
        }
        catch (System.Exception e)
        {
            isConnected = false;
            tcpClient.Close();
            return false;
        }
    }

    private void MessageAcceptHandler()
    {
        massegeBuffer.Clear();
        while (NetStream.DataAvailable)
        {
            int readByte = NetStream.ReadByte();
            if (readByte != -1)
            {
                massegeBuffer.Enqueue((byte)readByte);
            }
        }
        
        MessageJSON = Encoding.ASCII.GetString(massegeBuffer.ToArray());
        Message = null;
        Message = JsonConvert.DeserializeObject<BaseMessage>(MessageJSON);
        if (Message != null)
        {
            switch (Message.messageType)
            {
                case (MessageType.System):
                    {
                        Message = JsonConvert.DeserializeObject<SystemMessage>(MessageJSON);
                        break;
                    }
                case MessageType.RoomOperation:
                    {
                        Message = JsonConvert.DeserializeObject<RoomOperationMessage>(MessageJSON);
                        break;
                    }

                case MessageType.Callback:
                    {
                        Message = JsonConvert.DeserializeObject<CallbackMessage>(MessageJSON);
                        break;
                    }

                case MessageType.RoomInner:
                    {
                        Message = JsonConvert.DeserializeObject<RoomMessage>(MessageJSON);
                        break;
                    }
            }
        }
    }

    public NetworkClientInfo(TcpClient client)
    {
        this.tcpClient = client;
        isConnected = true;
    }
}