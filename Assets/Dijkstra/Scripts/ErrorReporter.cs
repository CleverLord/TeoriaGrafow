using System.Collections;
using System.Collections.Generic;
using System.Web;
using UnityEngine;
using UnityEngine.Networking;
using System.Linq;
using System.Net;
using System;
using System.Globalization;
using System.IO;

public class ErrorReporter : MonoBehaviour
{
    string deviceID="";
    public static ErrorReporter singleton;
    public static bool working=false;
    public static string toSend="";
    public void Awake() {
        singleton = this;
        deviceID = SystemInfo.deviceUniqueIdentifier;
    }
    public void SubmitReport(string s) {
        if(working) return;
        toSend = s;
        new System.Threading.Thread(uploadFile).Start();

        /*FtpWebRequest fwr;
        string webReq="http://cleverlord.ct8.pl/TeoriaGrafow/ErrorReporter.php?D=";

        string sUrlFriendly=HttpUtility.UrlEncode(s);
        webReq += sUrlFriendly;
        Debug.Log(webReq);
        UnityWebRequest www = UnityWebRequest.Get(webReq);
        yield return www.SendWebRequest();
        Debug.Log(www.downloadHandler.text);*/
    }
    public void uploadFile() {
        working = true;
        try {
            string fileName=deviceID+" "+DateTime.Now.ToString("G",CultureInfo.CreateSpecificCulture("de-DE"))+".txt";
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(new Uri(string.Format("{0}/{1}", "ftp://s1.ct8.pl", fileName)));
            request.Method = WebRequestMethods.Ftp.UploadFile;
            request.Credentials = new NetworkCredential("f16547_TeoriaGrafow", "Abc123");
            Stream ftpStream = request.GetRequestStream();
            Stream fs = GenerateStreamFromString(toSend);
            byte[] buffer = new byte[1024];
            int byteRead = 0;
            do {
                byteRead = fs.Read(buffer, 0, 1024);
                ftpStream.Write(buffer, 0, byteRead);
            }
            while(byteRead != 0);
            fs.Close();
            ftpStream.Close();
        }
        catch(Exception e) {
            Debug.Log(e);
            Debug.Log("Error with uploading");
        }
        working = false;
        Debug.Log("Error reported succesfuly");
    }
    public Stream GenerateStreamFromString(string s) {
        MemoryStream stream = new MemoryStream();
        StreamWriter writer = new StreamWriter(stream);
        writer.Write(s);
        writer.Flush();
        stream.Position = 0;
        return stream;
    }
}
