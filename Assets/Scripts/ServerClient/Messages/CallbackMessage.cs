using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityChat.Messages
{
    public class CallbackMessage : BaseMessage
    {
        [SerializeField]
        public CallbackType callbackType;
        [SerializeField]
        public string callbackText;

        public CallbackMessage(CallbackType type, int messageID, string callbackText = null) : base(MessageType.Callback, messageID)
        {
            this.callbackText = callbackText;
            callbackType = type;
        }

        public override string ToString()
        {
            return string.Format("{0}\nCallbackType: {1}\nCallbackText: {2}", base.ToString(), callbackType, callbackText);
        }
    }
}
