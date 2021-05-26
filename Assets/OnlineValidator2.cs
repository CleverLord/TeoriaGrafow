using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Net;
using System.Text;
public class OnlineValidator2 : MonoBehaviour
{
    public string ret="";
    // Start is called before the first frame update
    void Start()
    {
        Main();
    }
    public void Main() {
        // Create a request for the URL. 		
        WebRequest request = WebRequest.Create ("http://cleverlord.ct8.pl/TeoriaGrafow/Validator");
        // If required by the server, set the credentials.
        request.Credentials = CredentialCache.DefaultCredentials;
        // Get the response.
        HttpWebResponse response = (HttpWebResponse)request.GetResponse ();
        // Display the status.
        Console.WriteLine(response.StatusDescription);
        // Get the stream containing content returned by the server.
        Stream dataStream = response.GetResponseStream ();
        // Open the stream using a StreamReader for easy access.
        StreamReader reader = new StreamReader (dataStream);
        // Read the content.
        string responseFromServer = reader.ReadToEnd ();
        // Display the content.
        Console.WriteLine(responseFromServer);
        ret = responseFromServer;
        // Cleanup the streams and the response.
        reader.Close();
        dataStream.Close();
        response.Close();
    }
// Update is called once per frame
void Update()
    {
        
    }
}
