using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityChat.Messages;

[System.Serializable]
public class RoomData
{
    [SerializeField]
    public string roomName;
    [SerializeField]
    public RoomType roomType;
    [SerializeField]
    public UserInformation[] usersInRoom;
    [SerializeField]
    public List<RoomMessage> roomMessages = new List<RoomMessage>();

    public RoomData (string roomName, RoomType roomType, UserInformation[] usersInRoom)
    {
        this.roomName = roomName;
        this.roomType = roomType;
        if(roomType == RoomType.Private)
        {
            if(usersInRoom.Length >= 2)
            {
                this.usersInRoom = new UserInformation[2] { usersInRoom[0], usersInRoom[1] };
            }
        }
        else
        {
            this.usersInRoom = usersInRoom;
        }
    }

    public void AddUser(UserInformation newUserInfo)
    {
        UserInformation[] newUsersInRoom = new UserInformation[usersInRoom.Length + 1];
        for (int i = 0; i < usersInRoom.Length; i++)
        {
            newUsersInRoom[i] = usersInRoom[i];
        }
        newUsersInRoom[newUsersInRoom.Length - 1] = newUserInfo;
        usersInRoom = newUsersInRoom;
    }
}