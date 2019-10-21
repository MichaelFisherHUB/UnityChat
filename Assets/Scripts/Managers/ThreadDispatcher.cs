using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityChat.Messages;
using Extensions;

[System.Serializable]
public class ThreadDispatcher : MonoBehaviour
{
#if UNITY_EDITOR
    [SerializeField]
    [ReadOnly]
    private int actionsToMainThread = 0;
#endif

    private Queue<System.Action> actionsToInvoke = new Queue<System.Action>();

    private void Update()
    {
        if (actionsToInvoke.Count > 0)
        {
            lock (actionsToInvoke)
            {
                actionsToInvoke.Dequeue().Invoke();
                #if UNITY_EDITOR
                actionsToMainThread--;
                #endif
            }
        }
    }

    public void InvokeInMainThread(System.Action actionToInvoke)
    {
        lock (actionsToInvoke)
        {
            actionsToInvoke.Enqueue(actionToInvoke);

            #if UNITY_EDITOR
            actionsToMainThread++;
            #endif
        }
    }

    ~ThreadDispatcher()
    {
        if (actionsToInvoke != null && actionsToInvoke.Count > 0)
        {
            Debug.LogErrorFormat("[{0}] There are some({1}) actions, waiting to be inoked", "Thread dispatcher".ColorTag(ColorStringTag.Red), actionsToInvoke.Count);
        }
    }
}
