using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class UserInformation
{
    [SerializeField]
    public int id;
    [SerializeField]
    public string Name;

    public UserInformation(int id, string name = "Unnamed")
    {
        this.id = id;
        this.Name = name;
    }

    public override string ToString()
    {
        return string.Format("Name: {0} | ID: {1}", Name, id);
    }
}
