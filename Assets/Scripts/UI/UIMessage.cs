using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIMessage : MonoBehaviour
{
    [SerializeField]
    private Image avatar;
    [SerializeField]
    private PlayerColorCompare[] backGroundColors;
    [SerializeField]
    private TextMeshProUGUI userName;
    [SerializeField]
    private TextMeshProUGUI messageText;
    [SerializeField]
    public UserInformation userInformation { get; private set; }
    
    public bool IsPlayer
    {
        get
        {
            if(userInformation == null || ServerListener.currentUserInformation == null)
            {
                return false;
            }

            return userInformation.id == ServerListener.currentUserInformation.id;
        }
    }

    private void Start()
    {
        GetComponent<Image>().color = Array.Find(backGroundColors, x => x.isPlayerColor == IsPlayer).BackGroundCollor;
    }

    public void SetText(UserInformation userInformation, string text)
    {
        this.userInformation = userInformation;
        userName.text = userInformation.Name;
        messageText.text = text;
    }

    [System.Serializable]
    private struct PlayerColorCompare
    {
        public bool isPlayerColor;
        public Color BackGroundCollor;
    }
}
