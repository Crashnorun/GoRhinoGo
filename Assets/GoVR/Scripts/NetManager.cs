//This is a script that creates a Toggle that you enable to start the Server.
//Attach this script to an empty GameObject
//Create a Toggle GameObject by going to Create>UI>Toggle.
//Click on your empty GameObject.
//Click and drag the Toggle GameObject from the Hierarchy to the Toggle section in the Inspector window.

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

//This makes the GameObject a NetworkManager GameObject
public class NetManager : NetworkManager
{

    public string editorMode = "server";
    public string standaloneMode = "server";
    public string androidMode = "client";

    void Start()
    {
#if UNITY_EDITOR
        LaunchAction(editorMode);  
#elif UNITY_STANDALONE
        LaunchAction(standaloneMode);  
#elif UNITY_ANDROID
        LaunchAction(androidMode);  
#endif
    }
    
    void LaunchAction(string action)
    {
        switch (action)
        {
            case "server":
                StartServer();
                Debug.Log("Server Started!");
                break;
            case "host":
                StartHost();
                Debug.Log("Host Started!");
                break;
            case "client":
                StartClient();
                Debug.Log("Client Started!");
                break;
        }
    }

#if UNITY_ANDROID
    public override void OnClientDisconnect(NetworkConnection connection)
    {
        //Change the text to show the connection loss on the client side
        Application.Quit();
    }
#endif

}