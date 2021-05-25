using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using TMPro;
public enum licence { ok,err,none}
public class OnlineValidator : MonoBehaviour {
    
    public GameObject activateOnError;
    public GameObject activateOnOK;
    public TextMeshProUGUI errorText;
    public licence licence=licence.none;

    void Start() {
        StartCoroutine(GetText());
        StartCoroutine(Timeouter());
    }
    IEnumerator Timeouter() {
        yield return new WaitForSeconds(4);
        if(licence==licence.none) {
            activateOnError.SetActive(true);
            errorText.text="Sorry, but you need Internet Connection to use this app";
        }
    }
    IEnumerator GetText() {
        UnityWebRequest www = UnityWebRequest.Get("http://cleverlord.ct8.pl/TeoriaGrafow/Validator");
        yield return www.SendWebRequest();

        if(www.isHttpError) {
            Debug.Log(www.error);
            licence = licence.err;
            activateOnError.SetActive(true);
            errorText.text="Sorry, but you need internet connection to use this app";
        } 
        else {
            // Show results as text
            if(!www.downloadHandler.text.Contains("OK")) {
                licence = licence.err;
                activateOnError.SetActive(true);
                errorText.text="Sorry, but you are not allowed to use this software ";
            }
            else {
                licence = licence.ok;
                activateOnOK.SetActive(true);
            }
        }
    }
}
