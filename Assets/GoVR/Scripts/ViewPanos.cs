using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ViewPanos : NetworkBehaviour {

    public Material[] StereoCubemaps;
    
    [SyncVar(hook = "OnChangePano")]
    public int currentPano = 0;

    private int lastPano = 0;

    [SyncVar(hook = "OnChangeActive")]
    public bool active = false;

    // Use this for initialization
    void Start ()
    {
        lastPano = StereoCubemaps.Length - 1;
    }

    // Update is called once per frame
    void Update()
    {

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
                currentPano = -1;
                currentPano = Convert.ToInt32(Input.inputString) - 1;
                SetPano();
            }
        }
    }

    //runs on all clients when currentPano is changed on any client
    void OnChangePano(int i)
    {
        currentPano = i;
        SetPano();
    }

    //these functions are called from here or the player and run on the server
    public void Next()
    {
        if (currentPano < lastPano)
            currentPano++;
        else
            currentPano = 0;
        SetPano();
    }

    public void Previous()
    {
        if (currentPano > 0)
            currentPano--;
        else
            currentPano = lastPano;
        SetPano();
    }

    public void Activate(bool toggle)
    {
        active = toggle;
        SetPano();
    }

    void OnChangeActive(bool toggle)
    {
        active = toggle;
        SetPano();
    }

    public void SetPano()
    {

        RenderSettings.skybox = StereoCubemaps[currentPano];

    }

}
