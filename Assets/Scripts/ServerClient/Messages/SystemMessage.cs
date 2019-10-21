using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnityChat.Messages
{
    [System.Serializable]
    public class SystemMessage : BaseMessage
    {
        [SerializeField]
        public SystemMessageAction systemAction;
        [SerializeField]
        public UserInformation userInformation;

        public SystemMessage(SystemMessageAction systemMessageAction, UserInformation userInformation, int messageID) : base(MessageType.System, messageID)
        {
            systemAction = systemMessageAction;
            this.userInformation = userInformation;
        }

        public override string ToString()
        {
            return string.Format("{0}\nSystemAction: {1}", base.ToString(), systemAction);
        }
    }
}
