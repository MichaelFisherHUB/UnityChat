using UnityEngine;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Text;
using UnityChat.Messages;
using Extensions;

namespace ChatNetworking
{
    public class MessagesNetGateway
    {
        #region Server's fields

        private static TcpClient serverChanel;
        public static IPAddress Ip { get; private set; }
        public static int Port { get; private set; }

        public TcpConnectionState connectionState { get; private set; }

        private NetworkStream _serverNetworkStream;
        private NetworkStream ServerNetworkStream
        {
            get
            {
                return _serverNetworkStream ?? (_serverNetworkStream = serverChanel.GetStream());
            }
        }
        #endregion

        private Dictionary<int, System.Action<CallbackMessage>> waitingForCallback = new Dictionary<int, System.Action<CallbackMessage>>();

        private Thread serverListenerThread;

        public MessagesNetGateway(string ipAdress, int port)
        {
            Ip = IPAddress.Parse(ipAdress);
            Port = port;

            serverChanel = new TcpClient();

            connectionState = TcpConnectionState.ReadyToConnect;
        }

        public void ConnectToServer(System.Action<bool> onConnectCallback = null)
        {
            if (serverChanel != null)
            {
                string ipAndPort = string.Format("{0}:{1}", Ip.ToString(), Port);
                Debug.LogFormat("Try to establish connection to {0}", ipAndPort.ColorTag(ColorStringTag.Yellow));
                connectionState = TcpConnectionState.Connecting;
                serverChanel.BeginConnect(Ip, Port, (callback) =>
                {
                    if (serverChanel.Connected)
                    {
                        Debug.LogFormat("Connected to {0}", ipAndPort.ColorTag(ColorStringTag.Green));
                        connectionState = TcpConnectionState.Connected;

                        if (serverListenerThread == null)
                        {
                            serverListenerThread = new Thread(NetStreamListener);
                        }
                        serverListenerThread.Start();

                        ServerListener.Dispatcher.InvokeInMainThread(() =>
                        {
                            if (onConnectCallback != null)
                            {
                                onConnectCallback.Invoke(serverChanel.Connected);
                            }
                        });
                    }
                    else
                    {
                        Debug.LogWarningFormat("Can't connected to {0}", ipAndPort.ColorTag(ColorStringTag.Red));
                        connectionState = TcpConnectionState.Failed;
                    }
                }, serverChanel);
            }
            else
            {
                Debug.LogError("Can't find 'TCP client'. Construct me!!");
            }
        }

        public void Login(int autorizationMassageID, string userName, System.Action<UserInformation> userInformationCallback = null)
        {
            SendMessage(new SystemMessage(SystemMessageAction.Autorization, new UserInformation(0, userName), autorizationMassageID), callbackMessage =>
             {
                 if (callbackMessage.callbackType == CallbackType.Autorization)
                 {
                     UserInformation userInformation = JsonUtility.FromJson<UserInformation>(callbackMessage.callbackText);
                     if (userInformation != null)
                     {
                         Debug.LogFormat("Autorization is successfull\n{0}".ColorTag(ColorStringTag.Green), userInformation);
                         if (userInformationCallback != null)
                         {
                             userInformationCallback.Invoke(userInformation);
                         }
                     }
                     else
                     {
                         Debug.LogFormat("Autorization is failed\n{0}".ColorTag(ColorStringTag.Red), callbackMessage.ToString());
                     }
                 }
             });
        }

        private void NetStreamListener()
        {
            while (serverChanel != null)
            {
                if (serverChanel.Connected)
                {
                    Queue<byte> buffer = new Queue<byte>();
                    while (ServerNetworkStream.DataAvailable)
                    {
                        int readByte = ServerNetworkStream.ReadByte();
                        if (readByte != -1)
                        {
                            buffer.Enqueue((byte)readByte);
                        }
                    }
                    if (buffer.Count > 0)
                    {
                        AcceptRawMessage(buffer.ToArray());
                    }
                }
                else
                {
                    if (connectionState == TcpConnectionState.Connected)
                    {
                        connectionState = TcpConnectionState.Failed;
                    }
                }

                Thread.Sleep(333);
            }
        }

        private void AcceptRawMessage(byte[] bytestream)
        {
            string MessageJSON = Encoding.ASCII.GetString(bytestream);
            string debugString = "Accept massege:\n";
            BaseMessage baseMessage = JsonUtility.FromJson<BaseMessage>(MessageJSON);

            switch (baseMessage.messageType)
            {
                case (MessageType.System):
                    {
                        break;
                    }

                case MessageType.RoomOperation:
                    {
                        RoomOperationMessage roomMessage = JsonUtility.FromJson<RoomOperationMessage>(MessageJSON);
                        debugString += string.Format("{0}\nJSON:\n{1}", roomMessage.ToString(), MessageJSON);

                        switch (roomMessage.roomMassegeType)
                        {
                            case (RoomMassegeType.NewUserInRoom):
                                {
                                    ServerListener.Dispatcher.InvokeInMainThread(() =>
                                    {
                                        System.Array.ForEach(roomMessage.users, newUser =>
                                        {
                                            ServerListener.RoomsDataHolder.AddNewUserInRoom(roomMessage.roomTag, newUser);
                                        });
                                    });

                                    break;
                                }
                        }
                        break;
                    }

                case MessageType.RoomInner:
                    {
                        RoomMessage roomMessage = JsonUtility.FromJson<RoomMessage>(MessageJSON);
                        debugString += string.Format("{0}\nJSON:\n{1}", roomMessage.ToString(), MessageJSON);
                        //Redirect to main thread
                        ServerListener.Dispatcher.InvokeInMainThread(() =>
                        {
                            UIStateMachine.instance.chatUI.AcceptNewMessage(roomMessage.roomName, roomMessage);
                        });
                        break;
                    }

                case MessageType.Callback:
                    {
                        CallbackMessage callbackMessage = JsonUtility.FromJson<CallbackMessage>(MessageJSON);
                        debugString += string.Format("{0}\nJSON:\n{1}", callbackMessage.ToString(), MessageJSON);

                        if (waitingForCallback.ContainsKey(callbackMessage.MessageID) && waitingForCallback[callbackMessage.MessageID] != null)
                        {
                            System.Action<CallbackMessage> callback = waitingForCallback[callbackMessage.MessageID];
                            waitingForCallback.Remove(callbackMessage.MessageID);

                            //Redirect to main thread
                            ServerListener.Dispatcher.InvokeInMainThread(() =>
                            {
                                callback.Invoke(callbackMessage);
                            });
                        }
                        break;
                    }
                default:
                    {
                        Debug.LogWarning("'Message maneger' accept unknown massege type: " + baseMessage.messageType.ToString());
                        break;
                    }
            }

            Debug.Log(debugString);
        }

        public void SendMessage<T>(T message, System.Action<CallbackMessage> callback = null) where T : BaseMessage
        {
            if (serverChanel.Connected)
            {
                string JSONmessage = JsonUtility.ToJson(message);
                Debug.LogFormat("Try to send message:\n{0}\n\nJSON: {1}", message.ToString(), JSONmessage);
                byte[] buffer = Encoding.ASCII.GetBytes(JSONmessage.ToCharArray());
                ServerNetworkStream.Write(buffer, 0, buffer.Length);
            }

            if (callback != null)
            {
                waitingForCallback.Add(message.MessageID, callback);
            }
        }

        ~MessagesNetGateway()
        {
            if (serverChanel != null)
            {
                /*if (ServerListener.IsConnectToServer)
                {
                    SendMessage(new SystemMessage(SystemMessageAction.CloseConnection, ServerListener.thisUserInformation, 0));
                }*/
                ServerNetworkStream.Close();
                ServerNetworkStream.Dispose();
                serverChanel.Close();
            }
        }
    }
}