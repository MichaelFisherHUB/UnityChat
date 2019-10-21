using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityChat.Messages;
using TMPro;

public class Chat : MonoBehaviour
{
    [ReadOnly]
    private RoomData currentOpenedRoomData;

    [SerializeField]
    private GameObject messagesContainer;

    [SerializeField]
    private UIMessage messageGameobject;

    [SerializeField]
    private TMP_InputField inputChatText;

    private List<GameObject> messangesGOs = new List<GameObject>();

    private List<RoomData> openedRooms = new List<RoomData>();
    
    // For Button
    public void SendMassegeToRoom()
    {
        string textMessage = inputChatText.text;
        if (!string.IsNullOrEmpty(textMessage) && currentOpenedRoomData != null)
        {
            RoomMessage message = new RoomMessage(currentOpenedRoomData.roomName, ServerListener.currentUserInformation, MessageType.RoomInner, LocalStorageManager.GetRandomUnrepitingInt(), textMessage);

            AcceptNewMessage(currentOpenedRoomData ,message);

            ServerListener.SendMessage(message, callback =>
            {
                if (!string.IsNullOrEmpty(callback.callbackText))
                {
                    currentOpenedRoomData.roomMessages.Add(message);
                }
            });

            inputChatText.text = default(string);
        }
    }

    public void AcceptNewMessage(string roomToAdd, RoomMessage newMessage)
    {
        RoomData matchNameRoom = openedRooms.Find(x=> x.roomName == roomToAdd);

        if (matchNameRoom != null)
        {
            AcceptNewMessage(matchNameRoom, newMessage);
        }
    }

    public void AcceptNewMessage(RoomData roomToAdd, RoomMessage newMessage)
    {
        if (roomToAdd != null)
        {
            if (!openedRooms.Contains(roomToAdd))
            {
                openedRooms.Add(roomToAdd);
            }
            UIMessage newUIMessage = Instantiate(messageGameobject, messagesContainer.transform).GetComponent<UIMessage>();
            messangesGOs.Add(newUIMessage.gameObject);
            newUIMessage.SetText(newMessage.sourceUser, newMessage.textMessage);
            roomToAdd.roomMessages.Add(newMessage);
        }
    }

    public void OpenRoom(RoomData roomData)
    {
        currentOpenedRoomData = roomData;
        if(openedRooms.Contains(roomData))
        {
            for (int i = 0; i < roomData.roomMessages.Count; i++)
            {
                UIMessage newUIMessage = Instantiate(messageGameobject, messagesContainer.transform).GetComponent<UIMessage>();
                messangesGOs.Add(newUIMessage.gameObject);
                newUIMessage.SetText(roomData.roomMessages[i].sourceUser, roomData.roomMessages[i].textMessage);
            }
        }
        else
        {
            openedRooms.Add(roomData);
        }
    }

    public void CloseRoom()
    {
        currentOpenedRoomData = null;
        messangesGOs.ForEach(x => { Destroy(x); });
        messangesGOs.Clear();

        UIStateMachine.instance.SetUIState(UIState.Rooms);
    }
}
