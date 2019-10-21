using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LoadingBar : MonoBehaviour {

    [SerializeField]
    private Animation progressAnimaton;
    [SerializeField]
    private TextMeshProUGUI loadingText;
    
    private void OnEnable()
    {
        progressAnimaton.Play();
    }

    private void OnDisable()
    {
        progressAnimaton.Stop();
    }

    public void SetText(string text)
    {
        loadingText.text = text;
    }
}
