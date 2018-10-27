using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class OptionManager : NetworkBehaviour {


    [SyncVar(hook = "OnChangeOption")]
    public int currentOption = 0;

    public int lastOption = 0;

    [SyncVar(hook="OnChangeActive")]
    public bool active = false;
    
    // Use this for initialization
    void Awake ()
    {
        lastOption = transform.childCount - 1;
        SetOption();
    }
	
	// Update is called once per frame
	void Update () {

        if (!active)
            return;

        if (!isServer)
            return;

        if (Input.GetKeyUp(KeyCode.UpArrow) || Input.GetKeyUp(KeyCode.Space))
            Next();

        if (Input.GetKeyUp(KeyCode.DownArrow))
            Previous();

        //use numbers for hot keys
        int num = 0;
        if (Int32.TryParse(Input.inputString, out num))
        {
            if (num > 0 && num <= transform.childCount)
            {
                currentOption = -1;
                SetOption(Convert.ToInt32(Input.inputString) - 1);
            }
        }
    }

    //runs on all clients when currentOption is changed on any client
    void OnChangeOption(int i)
    {
        SetOption(i);
    }

    //these functions are called from here or the player and run on the server
    public void Next()
    {
        if (currentOption < lastOption)
            currentOption++;
        else
            currentOption = 0;
        SetOption();
    }

    public void Previous()
    {
        if (currentOption > 0)
            currentOption--;
        else
            currentOption = lastOption;
        SetOption();
    }
    
    public void Activate(bool toggle)
    {
        active = toggle;
        SetOption();
    }

    void OnChangeActive(bool toggle)
    {
        active = toggle;
        SetOption();
    }

    public void SetOption(int i)
    {
        currentOption = i;
        SetOption();
    }

    void SetOption()
    {
        ToggleVisibility(transform, false);

        if (!active)
            return;

        if (currentOption >= 0 && currentOption < transform.childCount)
            ToggleVisibility(transform.GetChild(currentOption), true);
    }

    void ToggleVisibility(Transform target, bool toggle)
    {
        //make sure it's on
        target.gameObject.SetActive(true);

        Renderer[] renderers = target.GetComponentsInChildren<Renderer>();
        MeshCollider[] colliders = target.GetComponentsInChildren<MeshCollider>();
        foreach (Renderer r in renderers)
        {
            r.enabled = toggle;
        }
        foreach (MeshCollider c in colliders)
        {
            c.enabled = toggle;
        }

    }

}
