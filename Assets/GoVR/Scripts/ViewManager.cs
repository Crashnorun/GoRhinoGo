using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ViewManager : NetworkBehaviour {

    [SyncVar(hook = "OnChangeView")]
    public int currentView = 0;

    public PlayerControl activePlayer = null;

    private int lastView = 0;

    private Material defaultSky;

    public GameObject staticObjects = null;

    void Awake ()
    {
        defaultSky = RenderSettings.skybox;
        lastView = transform.childCount - 1;
        SetView(FindClientPlayer(),null);

        if (staticObjects == null)
        {
            staticObjects = GameObject.Find("Context");

        }

        if (staticObjects == null)
            Debug.Log("No context assigned or found. This is fine unless you have pre-rendered views, in which case you should group all static objects beneath an empty GameObject called Context.");

    }
	
	void Update ()
    {
        if (!isServer)
            return;

        if (activePlayer)
            if (activePlayer.transition)
                return;

        if (Input.GetKeyUp(KeyCode.LeftArrow))
            Previous();

        if (Input.GetKeyUp(KeyCode.RightArrow))
            Next();

        //use letters for hot keys
        if (Input.inputString != "")
        {
            int num = (int)Input.inputString[0] % 32;
            if (num > 0 && num <= transform.childCount)
            {
                currentView = num - 1;
                //update the mirrored view
                if (activePlayer)
                    StartCoroutine(activePlayer.UpdateView("Changing view..."));
            }
        }
    }

    void OnGUI()
    {
        GUI.backgroundColor = Color.black;

        if (activePlayer)
            GUI.Label(new Rect(10, 5, 100, 20), "Player " + activePlayer.playerID);

        GUI.skin.label.alignment = TextAnchor.UpperCenter;
        GUI.Label(new Rect(Screen.width / 2 - 250, 5, 500, 500), CurrentViewName());

        GUI.skin.label.alignment = TextAnchor.UpperRight;
        int i = 0;
        foreach (Transform child in transform) {
            GUI.Label(new Rect(Screen.width - 510, 5 + i * 20, 500, 500), child.gameObject.name + " - " + Number2String(i,true));
            i++;
        }
        
        GUI.skin.label.alignment = TextAnchor.UpperLeft;

    }

    PlayerControl FindClientPlayer()
    {
        foreach (GameObject cur in GameObject.FindGameObjectsWithTag("Player"))
        {
            if (cur.GetComponent<NetworkIdentity>().isLocalPlayer == true)
            {
                Debug.Log("Found client player");
                return cur.GetComponent<PlayerControl>();
            }
        }
        Debug.Log("Couldn't find client player");
        return null;
    }

    //runs on all clients when currentView is changed on any client
    void OnChangeView(int i)
    {
        //turn off current options
        PanoSet(false);

        currentView = i;
        StartCoroutine(FindClientPlayer().UpdateView("Changing view..."));
    }

    public void Gather(PlayerControl host)
    {
        RpcGather(host.playerID);
    }

    [ClientRpc]
    void RpcGather(int hostID)
    {
        Transform host = null;

        //find the host
        foreach (PlayerControl player in FindObjectsOfType<PlayerControl>())
        {
            if(player.playerID == hostID)
                host = player.transform;
        }

        if (!host)
            return;

        //update all clients
        foreach (PlayerControl player in FindObjectsOfType<PlayerControl>())
        {
            StartCoroutine(player.UpdateView("Joining player " + hostID + "...", host));
        }
    }
    
    //these functions are called from here or the player and run on the server
    public void Next()
    {
        PanoSet(false);

        if (currentView < lastView)
            currentView++;
        else
            currentView = 0;
        //update the mirrored view
        if (activePlayer)
            StartCoroutine(activePlayer.UpdateView("Next view..."));
    }

    //these functions are called from here or the player and run on the server
    public string NextViewName()
    {
        int i;
        if (currentView < lastView)
            i = currentView + 1;
        else
            i = 0;

        if (transform.childCount > i)
            return transform.GetChild(i).name;

        return "";
    }

    public void Previous()
    {
        //turn off current options
        PanoSet(false);

        if (currentView > 0)
            currentView--;
        else
            currentView = lastView;
        //update the mirrored view
        if (activePlayer)
            StartCoroutine(activePlayer.UpdateView("Previous view..."));
    }

    public void SetView(PlayerControl player, Transform teleport)
    {
        if (!player)
            return;
        
        player.SetLabel(CurrentViewName());
        if (!teleport)
            teleport = transform.GetChild(currentView).transform;

        OptionSet(true);
        PanoSet(true);

        Teleport(player, teleport);

    }

    string CurrentViewName()
    {
        return "View " + Number2String(currentView, true) + ": " + transform.GetChild(currentView).name;
    }

    string Number2String(int number, bool isCaps)
    {
        Char c = (Char)((isCaps ? 65 : 97) + number);
        return c.ToString();
    }

    void OptionSet(bool toggle)
    {
        ToggleStatic(true);
        //make sure it's on
        transform.gameObject.SetActive(true);
        Transform teleport = transform.GetChild(currentView).transform;
        if (teleport)
        {
            ViewOption option = teleport.GetComponent<ViewOption>();
            if (option)
            {
                //make sure it's active
                option.optionSet.SetActive(true);
                option.optionSet.GetComponent<OptionManager>().Activate(toggle);
            }
        }
    }

    void PanoSet(bool toggle)
    {
        //make sure it's on
        transform.gameObject.SetActive(true);
        Transform teleport = transform.GetChild(currentView).transform;
        if (teleport)
        {
            ViewPanos panos = teleport.GetComponent<ViewPanos>();
            if (panos)
            {
                ToggleStatic(false);
                //make sure it's active
                panos.Activate(toggle);
            }
        }
    }

    void ToggleStatic(bool toggle)
    {
        if(staticObjects)
            staticObjects.SetActive(toggle);
        if (toggle)
            RenderSettings.skybox = defaultSky;
    }

    public void Teleport(PlayerControl player, Transform location)
    {

        Vector3 offset = new Vector3(0, 0, 1);
        offset = Quaternion.Euler(0, 60 * player.playerID - 150, 0) * offset;
        offset = location.rotation * offset;
        player.transform.position = location.position + offset;

        player.trackingSpace.transform.rotation = location.rotation;
        player.trackingSpace.transform.RotateAround(player.transform.position, player.transform.up, 180f);

/*
#if UNITY_STANDALONE || UNITY_EDITOR
            player.transform.rotation = location.rotation;
            player.transform.RotateAround(player.transform.position, player.transform.up, 180f);
#elif UNITY_ANDROID
            player.trackingSpace.transform.rotation = location.rotation;
            player.trackingSpace.transform.RotateAround(player.transform.position, player.transform.up, 180f);
#endif
*/

    }

    public void RestoreModel()
    {
        RenderSettings.skybox = defaultSky;
    }

}
