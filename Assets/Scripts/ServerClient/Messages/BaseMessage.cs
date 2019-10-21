using UnityEngine;
using System.Collections.Generic;

namespace UnityChat.Messages
{
    [System.Serializable]
    public class BaseMessage
    {
        [SerializeField]
        public MessageType messageType;
        
        [SerializeField]
        public int MessageID;

        public BaseMessage(MessageType messageType, int messageID)
        {
            this.messageType = messageType;
            MessageID = messageID;
        }

        public override string ToString()
        {
            return string.Format("Message Type: {0}\nMessage ID: {1}", messageType.ToString(), MessageID);
        }
    }
}
