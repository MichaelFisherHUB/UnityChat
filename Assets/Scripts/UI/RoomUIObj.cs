using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class RoomUIObj : MonoBehaviour
{
    [SerializeField][ReadOnly]
    public RoomData roomData;

    [SerializeField] private TextMeshProUGUI roomNameText;
    [SerializeField] private TextMeshProUGUI usersCountText;

    public void Init(RoomData thisRoomData)
    {
        roomData = thisRoomData;

        roomNameText.text = roomData.roomName;
        usersCountText.text = "users: " + thisRoomData.usersInRoom.Length.ToString();

        gameObject.GetComponent<Button>().onClick.AddListener(()=> 
        {
            UIStateMachine.instance.chatUI.OpenRoom(roomData);
            UIStateMachine.instance.SetUIState(UIState.Chat);
        });
    }
}
