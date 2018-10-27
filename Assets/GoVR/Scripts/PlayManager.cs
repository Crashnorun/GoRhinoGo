using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PlayManager : NetworkBehaviour {

    [SyncVar(hook = "OnChangePlayerCount")]
    public int playerCount = 0;
    
    void OnChangePlayerCount(int count)
    {
        playerCount = count;
    }

    public void AddPlayer()
    {
        playerCount++;
    }
    
    //this is called from a command on PlayerControl_Client
    //runs on server and should trigger hook on clients...
    [Command]
    void CmdAddPlayer()
    {
        playerCount++;
    }

}
