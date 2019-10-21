using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ChatNetworking;
using UnityChat;
using UnityEngine.Events;
using Extensions;
using UnityChat.Messages;

[RequireComponent(typeof(ThreadDispatcher))]
public class ServerListener : MonoBehaviour
{
    [SerializeField]
    private string ipAdress;
    private const int PORT = 90;

    public static bool IsConnectToServer { get; private set; }

    public static ThreadDispatcher Dispatcher { get; private set; }

    [Range(0, 1)]
    [SerializeField]
    private float serverCheckConnectionTime = 1f;

    public static UserInformation currentUserInformation { get; private set; }

    public static MessagesNetGateway server;

    public static UnityEvent onConnectToServer = new UnityEvent();

    public static RoomsManager RoomsDataHolder = new RoomsManager();

    private void Awake()
    {
        if (Dispatcher == null)
        {
            Dispatcher = gameObject.GetComponent<ThreadDispatcher>();
        }
    }

    private void Start()
    {
        server = new MessagesNetGateway(ipAdress, PORT);

        UIStateMachine.instance.SetUIState(UIState.Loading);

        StartCoroutine(ConnectToServer());
    }

    private IEnumerator ConnectToServer()
    {
        while (true)
        {
            if (server.connectionState == TcpConnectionState.ReadyToConnect || server.connectionState == TcpConnectionState.Failed)
            {
                //Connect to server
                server.ConnectToServer(isConnected =>
                {
                    IsConnectToServer = isConnected;
                    UIStateMachine.instance.SetUIState(UIState.LogIn);
                    if (onConnectToServer != null)
                    {
                        onConnectToServer.Invoke();
                    }
                });
            }
            yield return serverCheckConnectionTime > 0 ? new WaitForSeconds(serverCheckConnectionTime) : null;
        }
    }

    public static void Login(string name, System.Action<UserInformation> onLoginCallback = null)
    {
        server.Login(LocalStorageManager.GetRandomUnrepitingInt(), name, userInformationCallback =>
        {
            if (userInformationCallback != null)
            {
                if (onLoginCallback != null)
                {
                    onLoginCallback.Invoke(userInformationCallback);
                }
                currentUserInformation = userInformationCallback;

                GetAllRoomsOfThisUsers(allrooms =>
                {
                    RoomsDataHolder.SetNewRooms(allrooms);
                });
            }
        });
    }

    public static void SendMessage<T>(T messageToSend, System.Action<CallbackMessage> callback = null) where T : BaseMessage
    {
        if (server.connectionState == TcpConnectionState.Connected)
        {
            server.SendMessage(messageToSend, callback);
        }
        else
        {
            Debug.LogErrorFormat("Can't send message\nIs game connected to server: {0}", IsConnectToServer.ToString());
        }
    }

    public static void GetAllRoomsOfThisUsers(System.Action<RoomData[]> roomsCallback)
    {
        if (currentUserInformation != null && IsConnectToServer)
        {
            RoomOperationMessage roomMessage = new RoomOperationMessage(RoomMassegeType.GetAllRoomsData, new UserInformation[1] { currentUserInformation }, LocalStorageManager.GetRandomUnrepitingInt(), "");
            SendMessage(roomMessage, callback => 
            {
                if(callback.callbackType == CallbackType.RoomData && roomsCallback != null)
                {
                    RoomData[] parsedData = new RoomData[0];
                    parsedData = parsedData.FromJson(callback.callbackText);
                    roomsCallback.Invoke(parsedData);
                }
            });
        }
        else
        {
            Debug.LogErrorFormat("Can't send message\nIs game connected to server: {0}\nUser information: {1}", IsConnectToServer.ToString(), currentUserInformation.ToString());
        }
    }
}
