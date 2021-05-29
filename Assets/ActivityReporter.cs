using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using System;
using System.Globalization;

public class ActivityReporter : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(ReportActivity());
    }

    IEnumerator ReportActivity() {
        string fileName=SystemInfo.deviceUniqueIdentifier+" "+DateTime.Now.ToString("G",CultureInfo.CreateSpecificCulture("de-DE"))+".txt";
        UnityWebRequest www = UnityWebRequest.Get("http://cleverlord.ct8.pl/TeoriaGrafow/ActivityReporter.php?D="+fileName);
        yield return www.SendWebRequest();

    }
}
