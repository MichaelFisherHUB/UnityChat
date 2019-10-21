using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityChat.Messages;
using UnityEngine.Events;

public class RoomsManager
{
    public List<RoomData> rooms = new List<RoomData>();
    public UnityEvent onRoomsChanged = new UnityEvent();

    public void SetNewRooms(RoomData[] newRooms)
    {
        rooms.Clear();
        rooms.AddRange(newRooms);
        if (onRoomsChanged != null)
        {
            onRoomsChanged.Invoke();
        }
    }

    public void AddNewUserInRoom(string roomName, UserInformation newUser)
    {
        RoomData searchRoom = rooms.Find(room => { return room.roomName == roomName; });
        if (searchRoom != null)
        {
            searchRoom.AddUser(newUser);
            if (onRoomsChanged != null)
            {
                onRoomsChanged.Invoke();
            }
        }
    }

    public void AddNewUserInRoom(RoomData room, UserInformation newUser)
    {
        AddNewUserInRoom(room.roomName, newUser);
    }
}
