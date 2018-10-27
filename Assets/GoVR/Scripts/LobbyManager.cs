﻿//This is a script that creates a Toggle that you enable to start the Server.
//Attach this script to an empty GameObject
//Create a Toggle GameObject by going to Create>UI>Toggle.
//Click on your empty GameObject.
//Click and drag the Toggle GameObject from the Hierarchy to the Toggle section in the Inspector window.

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

//This makes the GameObject a NetworkManager GameObject
public class LobbyManager : NetworkLobbyManager
{

    void Start()
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        StartServer();
        //Output that the Client has started
        Debug.Log("Server Started!");
#elif UNITY_ANDROID
        StartClient();
        //Output that the Server has started
        Debug.Log("Client Started!");
#endif
    }

    void Update()
    {
        
    }

    public override void OnClientDisconnect(NetworkConnection connection)
    {
        //Change the text to show the connection loss on the client side
        Application.Quit();
    }
    
}