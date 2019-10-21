using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LoginUI : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI inputName;
	
    public void OnLoginButtonPressed()
    {
        string inputText = inputName.text;
        if (!string.IsNullOrEmpty(inputText))
        {
            ServerListener.Login(inputName.text, (userInfo) =>
            {
                UIStateMachine.instance.SetUIState(UIState.Rooms);
            });
        }
    }
}
