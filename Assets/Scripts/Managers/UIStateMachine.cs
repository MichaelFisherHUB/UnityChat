using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIStateMachine : MonoBehaviour
{
    [SerializeField]
    private List<UIStateAndValue> uiStateMachine = new List<UIStateAndValue>();
    
    public Chat chatUI;

    public static UIStateMachine instance;

    private void Awake()
    {
        if(instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
        }
        uiStateMachine.ForEach(x => { x.UI.SetActive(false); });
    }

    public void SetUIState(UIState newUIState)
    {
        uiStateMachine.ForEach(x => { x.UI.SetActive(x.correspondingState == newUIState); });
    }

    [System.Serializable]
    private struct UIStateAndValue
    {
        public UIState correspondingState;
        public GameObject UI;
    }
}
