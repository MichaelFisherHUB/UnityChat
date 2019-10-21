using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Events;
using UnityChat.Messages;

public class RoomsUI : MonoBehaviour
{
    [SerializeField]
    [ReadOnly]
    private bool isSubscribed;
    [SerializeField]
    private GameObject roomsListParrent;
    [SerializeField]
    private RoomUIObj roomUIObject;
    [SerializeField] private TextMeshProUGUI userNameText;

    private List<RoomUIObj> uisElements = new List<RoomUIObj>();

    private void OnEnable()
    {
        SubscribeOnRoomsChanging();
    }

    private void OnDisable()
    {
        UnsubscribeOnRoomsChanging();
    }

    void Start ()
    {
        userNameText.text = ServerListener.currentUserInformation.Name;
        SubscribeOnRoomsChanging();
        RoomsChanged();
    }

    private void SubscribeOnRoomsChanging()
    {
        if(!isSubscribed)
        {
            ServerListener.RoomsDataHolder.onRoomsChanged.AddListener(RoomsChanged);
            isSubscribed = true;
        }
    }

    private void UnsubscribeOnRoomsChanging()
    {
        if (isSubscribed)
        {
            ServerListener.RoomsDataHolder.onRoomsChanged.RemoveListener(RoomsChanged);
            isSubscribed = true;
        }
    }

    private void RoomsChanged()
    {
        uisElements.ForEach(x => 
        {
            Destroy(x.gameObject);
        });
        uisElements.Clear();

        foreach (RoomData room in ServerListener.RoomsDataHolder.rooms)
        {
            RoomUIObj newRoom = Instantiate(roomUIObject, roomsListParrent.transform);
            uisElements.Add(newRoom);
            newRoom.Init(room);
        }
    }
}
