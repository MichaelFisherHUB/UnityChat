using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class Rooms
{
    private List<RoomData> roomsData = new List<RoomData>();

    public void CreateNewRoom(RoomData newRoom)
    {
        if (!roomsData.Contains(newRoom))
        {
            roomsData.Add(newRoom);
        }
    }

    public bool IsRoomExist(string roomName)
    {
        foreach (RoomData room in roomsData)
        {
            if (room.roomName == roomName) { return true; }
        }
        return false;
    }

    public RoomData GetRoom(string roomName)
    {
        if (IsRoomExist(roomName))
        {
            return roomsData.Find(x => x.roomName == roomName);
        }
        return null;
    }

    public void AddUserToPublicRoom(string roomName, UserInformation userToAdd)
    {
        if (IsRoomExist(roomName))
        {
            RoomData searchedRoom = GetRoom(roomName);
            if (searchedRoom.roomType == UnityChat.Messages.RoomType.Public)
            {
                UserInformation[] newUsersArray = new UserInformation[searchedRoom.usersInRoom.Length + 1];
                for (int i = 0; i < searchedRoom.usersInRoom.Length; i++)
                {
                    newUsersArray[i] = searchedRoom.usersInRoom[i];
                }
                newUsersArray[newUsersArray.Length - 1] = userToAdd;
                searchedRoom.usersInRoom = newUsersArray;
            }
        }
        else
        {
            CreateNewRoom(new RoomData(roomName, UnityChat.Messages.RoomType.Public, new UserInformation[1] { userToAdd }));
        }
    }

    public RoomData[] GetRoomsRelatedToUser(UserInformation searchingUserInfo)
    {
        List<RoomData> retList = new List<RoomData>();
        foreach (RoomData room in roomsData)
        {
            foreach (UserInformation userInfo in room.usersInRoom)
            {
                if (searchingUserInfo.id == userInfo.id)
                {
                    retList.Add(room);
                    break;
                }
            }
        }
        return retList.ToArray();
    }
}
