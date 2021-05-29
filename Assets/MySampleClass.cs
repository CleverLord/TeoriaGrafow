using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using System;
using System.Globalization;

public class MySampleClass : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(ReportActivity());
    }

    IEnumerator ReportActivity() {
        Uri u= new Uri("http://cleverlord.ct8.pl/TeoriaGrafow/ActivityReporter.php?D="+ SystemInfo.deviceUniqueIdentifier);
        UnityWebRequest www = UnityWebRequest.Get(u);
        yield return www.SendWebRequest();
    }
}
