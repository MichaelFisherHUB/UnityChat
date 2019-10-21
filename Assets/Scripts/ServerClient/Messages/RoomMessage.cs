using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityChat.Messages
{
    [System.Serializable]
    public class RoomMessage : BaseMessage
    {
        [SerializeField]
        public string roomName;

        [SerializeField]
        public UserInformation sourceUser;

        [SerializeField]
        public string textMessage;

        public RoomMessage(string roomName, UserInformation sourceUser, MessageType messageType, int messageID, string textMessage) : base(messageType, messageID)
        {
            this.textMessage = textMessage;
            this.roomName = roomName;
            this.sourceUser = sourceUser;
        }

        public override string ToString()
        {
            return string.Format("{0}\nSender: {1}\nTo room: {2}\nText: {3}", base.ToString(), sourceUser.ToString(), roomName, textMessage);
        }
    }
}
