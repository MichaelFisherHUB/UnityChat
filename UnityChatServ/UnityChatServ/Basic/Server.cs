using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Linq;
using System.IO;
using System.Threading;
using UnityChat.Messages;
using UnityChat;
using Newtonsoft.Json;

namespace UnityChatServ
{
    public class Server
    {
        public TcpListener Listener;
        private readonly TextWriter outWriter;

        private List<NetworkClientInfo> clients = new List<NetworkClientInfo>();

        //newClients - is connected, but not autorized users
        private List<NetworkClientInfo> newClients = new List<NetworkClientInfo>();

        private Rooms allRooms = new Rooms();

        private int maxID = 1;

        public Server(IPAddress ip, int port, TextWriter outWriter)
        {
            this.outWriter = outWriter;
            Listener = new TcpListener(ip, port);
            Listener.Start();
        }

        public void Work()
        {
            Thread clientListener = new Thread(() =>
            {
                while (true)
                {
                    TcpClient newClient = Listener.AcceptTcpClient();
                    lock (newClients)
                    {
                        newClients.Add(new NetworkClientInfo(newClient));
                    }
                }
            });
            clientListener.Start();

            while (true)
            {
                #region Autorization Handle

                //newClients - is connected, but not autorized users. New user can send System messages only.
                newClients.FindAll(x => x.isConnected && x.isAcceptSomeMessage).ForEach(newClient =>
                {
                    newClient.ReadMessageFromThis((BaseMessage message) =>
                    {
                        if (message.GetType() == typeof(SystemMessage))
                        {
                            SystemMessage sysMessage = (SystemMessage)message;
                            if (sysMessage.systemAction == SystemMessageAction.Autorization)
                            {
                                HandleSystemMessage(newClient);
                            }
                        }
                    });
                });
                #endregion

                clients.FindAll(x => x.isConnected && x.isAcceptSomeMessage).ForEach(client =>
                {
                    client.ReadMessageFromThis((BaseMessage message) =>
                    {
                        switch (message.messageType)
                        {
                            case MessageType.System:
                                {
                                    HandleSystemMessage(client);
                                    break;
                                }
                            case MessageType.RoomOperation:
                                {
                                    HandleRoomOperationMessage(client);
                                    break;
                                }

                            case MessageType.Callback:
                                {
                                    break;
                                }

                            case MessageType.RoomInner:
                                {
                                    HandleRoomInnerMessage(client);
                                    break;
                                }
                            default:
                                {
                                    break;
                                }
                        }
                    });
                });

                clients.RemoveAll((NetworkClientInfo x) =>
                {
                    if (!x.isConnected)
                    {
                        outWriter.WriteLine("Client disconnected");
                        return true;
                    }
                    return false;
                });
                if (newClients.Count > 0)
                {
                    List<NetworkClientInfo> newLoggedInClients = newClients.FindAll(x => x.isLogedIn);
                    if (newLoggedInClients.Count > 0)
                    {
                        clients.AddRange(newLoggedInClients);
                    }
                    newClients.RemoveAll(x => x.isLogedIn);
                }
                Thread.Sleep(333);
            }
        }

        private void ResendToAll(NetworkClientInfo massegeSender, BaseMessage massegeToSend)
        {
            clients.ForEach(accepter =>
            {
                if (accepter != massegeSender)
                {
                    accepter.SendMassegeToThis(massegeToSend);
                }
            });
        }

        private void HandleSystemMessage(NetworkClientInfo client)
        {
            if (client.Message.GetType() != typeof(SystemMessage))
            {
                outWriter.WriteLine("[HandleSystemMessage] Wrong message type: " + client.Message.messageType.ToString());
                return;
            }

            SystemMessage systemMessage = (SystemMessage)client.Message;

            switch (systemMessage.systemAction)
            {
                case SystemMessageAction.Autorization:
                    {
                        string autorizationProcess = "\n<Autorization request>";
                        if (!client.isLogedIn)
                        {
                            client.isLogedIn = true;

                            //Remove artifact
                            if (systemMessage.userInformation.Name.EndsWith("?"))
                            {
                                systemMessage.userInformation.Name = systemMessage.userInformation.Name.Remove(systemMessage.userInformation.Name.Length - 1);
                            }

                            client.userInformation = new UserInformation(maxID++, systemMessage.userInformation.Name);
                            client.SendMassegeToThis(
                                new CallbackMessage(CallbackType.Autorization, systemMessage.MessageID, JsonConvert.SerializeObject(client.userInformation)));

                            AddUserToRoom(client, "all");

                            autorizationProcess += string.Format("\nNew Client: <{0}>\n", client.userInformation.ToString());

                            newClients.Remove(client);
                            clients.Add(client);
                        }
                        else
                        {
                            autorizationProcess += string.Format("\nTrying to autorize already autorized user: <{0}>", client.userInformation.ToString());
                            //TODO Add "False Autorization Request"
                        }
                        outWriter.WriteLine(autorizationProcess);
                        break;
                    }
                case SystemMessageAction.CloseConnection:
                    {
                        outWriter.WriteLine("User close connection");
                        client.isConnected = false;
                        break;
                    }
                case SystemMessageAction.RenameUser:
                    {
                        break;
                    }
                default:
                    {
                        outWriter.WriteLine("\nUnknown message action: " + systemMessage.systemAction.ToString());
                        break;
                    }
            }
        }

        private void HandleRoomOperationMessage(NetworkClientInfo client)
        {
            if (client.Message.GetType() != typeof(RoomOperationMessage))
            {
                outWriter.WriteLine("[HandleRoomMessage] Wrong message type: " + client.Message.messageType.ToString());
                return;
            }

            RoomOperationMessage roomMessage = (RoomOperationMessage)client.Message;

            switch (roomMessage.roomMassegeType)
            {
                case RoomMassegeType.GetAllRoomsData:
                    {
                        CallbackMessage retMessage = new CallbackMessage(
                            CallbackType.RoomData,
                            roomMessage.MessageID,
                            JsonConvert.SerializeObject(allRooms.GetRoomsRelatedToUser(client.userInformation)));

                        client.SendMassegeToThis(retMessage);
                        break;
                    }
                case RoomMassegeType.GetRoomData:
                    {
                        CallbackMessage retMessage = new CallbackMessage(
                            CallbackType.RoomData,
                            roomMessage.MessageID,
                            JsonConvert.SerializeObject(allRooms.GetRoom(roomMessage.roomTag)));

                        client.SendMassegeToThis(retMessage);
                        break;
                    }
                case RoomMassegeType.NewUserInRoom:
                    {
                        break;
                    }
                default:
                    {
                        outWriter.WriteLine("[HandleRoomMessage] Unknown roomMessage type: " + roomMessage.roomMassegeType);
                        break;
                    }
            }
        }

        private void HandleRoomInnerMessage(NetworkClientInfo sender)
        {
            if (sender.Message.GetType() != typeof(RoomMessage))
            {
                outWriter.WriteLine("[HandleRoomInnerMessage] Wrong message type: " + sender.Message.messageType.ToString());
                return;
            }

            RoomMessage roomMessage = (RoomMessage)sender.Message;

            if (!string.IsNullOrEmpty(roomMessage.roomName))
            {
                outWriter.WriteLine(string.Format("Client <{0}> wrote message to room <{1}>\nText of message: {2}\n", sender.userInformation, roomMessage.roomName, roomMessage.textMessage));

                RoomData room = allRooms.GetRoom(roomMessage.roomName);
                if (room != null)
                {
                    //Resend to all in this room
                    System.Array.ForEach(room.usersInRoom, roomedUser =>
                    {
                        List<NetworkClientInfo> usersToResend = new List<NetworkClientInfo>();
                        usersToResend = clients.FindAll(x => (x.userInformation.id == roomedUser.id) && (x.userInformation.id != sender.userInformation.id));
                        usersToResend.ForEach(usr =>
                        {
                            usr.SendMassegeToThis(roomMessage);
                        });
                    });
                }
            }
        }

        private void AddUserToRoom(NetworkClientInfo userToAdd, string roomName)
        {
            allRooms.AddUserToPublicRoom(roomName, userToAdd.userInformation);

            RoomData room = allRooms.GetRoom(roomName);

            // Form a message
            RoomOperationMessage newUserNotification = 
                new RoomOperationMessage(
                    RoomMassegeType.NewUserInRoom,
                    new UserInformation[1] { userToAdd.userInformation },
                    0, roomName);

            // Get users information in room
            UserInformation[] clientsToNotify = System.Array.FindAll(room.usersInRoom, accepter => { return accepter.id != userToAdd.userInformation.id; });

            if (clientsToNotify != null && clientsToNotify.Length > 0)
            {
                // Get users network gateways
                List<NetworkClientInfo> clientsToNotifyGateWays = clients.FindAll(clientGateway => { return clientsToNotify.Contains(clientGateway.userInformation); });

                if(clientsToNotifyGateWays != null && clientsToNotifyGateWays.Count > 0)
                {
                    clientsToNotifyGateWays.ForEach( clientGateway => 
                    {
                        clientGateway.SendMassegeToThis(newUserNotification);
                    });
                }
            }
        }

        ~Server()
        {
            clients.ForEach(x => x.SendMassegeToThis(new SystemMessage(SystemMessageAction.CloseConnection, x.userInformation, 0)));
            if (Listener != null)
            {
                Listener.Stop();
            }
            clients.ForEach(x => { x.tcpClient.Close(); });

            newClients.ForEach(x => { x.tcpClient.Close(); });
        }
    }
}
