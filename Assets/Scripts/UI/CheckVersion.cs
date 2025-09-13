using TMPro;
using UnityEngine;
using System;

public class CheckVersion : MonoBehaviour
{
    [SerializeField] private TMP_Text versionText;
    [SerializeField] private string prefix = "v";

    void Start()
    {
        if (versionText == null)
        {
            versionText = GetComponent<TMP_Text>();
        }

        if (versionText != null)
        {
            versionText.text = GetVersionString();
        }
    }

    private string GetVersionString()
    {
        string version = Application.version;

        return $"{prefix}{version}";
    }
}