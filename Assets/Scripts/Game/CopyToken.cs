using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

public class CopyToken : MonoBehaviour
{
    public Text token;

    public void DoCopyToken()
    {
        GUIUtility.systemCopyBuffer = token.text;
    }
}
