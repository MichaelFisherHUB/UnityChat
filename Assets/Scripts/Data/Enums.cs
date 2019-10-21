using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnityChat.Messages
{
    public enum MessageType
    {
        System,
        Callback,
        RoomOperation,
        RoomInner
    }

    public enum SystemMessageAction
    {
        Autorization,
        CloseConnection,
        RenameUser,
    }

    public enum CallbackType
    {
        Autorization,
        ServerAcceptMessage,
        RoomData
    }

    public enum RoomMassegeType
    {
        // Client -> Server
        GetAllRoomsData = 0,

        // Client -> Server
        GetRoomData = 1,

        // Server -> Client  
        NewUserInRoom = 2
    }

    public enum RoomType
    {
        Public,
        Private
    }
}


public enum TcpConnectionState
{
    ReadyToConnect = 0,
    Connecting = 1,
    Connected = 2,
    Failed = 3
}

public enum UIState
{
    Loading,
    LogIn,
    Rooms,
    Chat,
    Members
}

namespace Extensions
{
    public enum ColorStringTag
    {
        Red, Green, Blue, Yellow
    }
}

