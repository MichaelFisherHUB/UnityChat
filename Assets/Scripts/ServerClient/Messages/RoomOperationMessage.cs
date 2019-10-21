using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityChat.Messages
{
    public class RoomOperationMessage : BaseMessage
    {
        [SerializeField]
        public UserInformation[] users;
        [SerializeField]
        public RoomMassegeType roomMassegeType;
        [SerializeField]
        public string roomTag = "all";

        public RoomOperationMessage(RoomMassegeType type, UserInformation[] users, int messageID, string roomTag) : base(MessageType.RoomOperation, messageID)
        {
            this.roomMassegeType = type;
            this.users = users;
            this.roomTag = roomTag;
        }

        public override string ToString()
        {
            return string.Format("{0}\nRoomMassegeType: {1}\nRoomTag: {2}", base.ToString(), roomMassegeType, roomTag);
        }
    }
}
