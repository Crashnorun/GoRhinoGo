using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;


public class PlayerControl : NetworkBehaviour
{
    public GameObject trackingSpace;
    public GameObject locomotion;
    public GameObject viewCam;
    public GameObject viewLabel;
    private ViewManager viewmgr = null;
    private OptionManager[] optmgr = new OptionManager[0];
    private PlayManager playmgr = null;

    public float eyeHeightStanding = 5.5f;
    public float eyeHeightSeated = 3.5f;
    private bool standing = true;

    public bool transition = false;
    public bool teleported = false;

    public Color clr = Color.cyan;

    //use to store new location if necessary
    private Transform teleport = null;

    // Used to buffer trigger
    [SyncVar(hook = "OnSetTrigger")]
    public bool lastTriggerState = false;

    // Used to buffer touch
    [SyncVar(hook = "OnSetTouch")]
    public bool lastTouchState = false;

    // Used to buffer touch
    [SyncVar(hook = "OnTouchPress")]
    public bool lastTouchPress = false;

    [SyncVar(hook = "OnSetID")]
    public int playerID = -1;

    void Start()
    {

        //turn off the default main camera
        if (GameObject.Find("Main Camera"))
            GameObject.Find("Main Camera").SetActive(false);

        viewLabel.SetActive(false);

        if (GameObject.Find("PlayManager"))
        {
            playmgr = GameObject.Find("PlayManager").GetComponent<PlayManager>();
            playmgr.AddPlayer();
        }

        if (GameObject.Find("Views"))
        {
            viewmgr = GameObject.Find("Views").GetComponent<ViewManager>();
            viewmgr.activePlayer = this;
            Debug.Log("Teleport points found");

            optmgr = GameObject.FindObjectsOfType<OptionManager>();
            if (optmgr.Length > 0)
                foreach (OptionManager opt in optmgr)
                    opt.transform.gameObject.SetActive(true);
        }

        SetHeight();

        if (isLocalPlayer)
        {
#if !UNITY_EDITOR
            //hide headset model
            transform.Find("Head/OculusGo").gameObject.SetActive(false);
#endif
            CmdSetID();

        }
        else
        {
            //hide headset light
            transform.Find("Head/Point Light").gameObject.SetActive(false);
            //disable tracking space
            trackingSpace.SetActive(false);
            //turn off locomotion
            locomotion.SetActive(false);
        }

#if UNITY_STANDALONE || UNITY_EDITOR
            //turn off locomotion to prevent controller access warnings
            locomotion.SetActive(false);
            //turn off ovr cameras
            trackingSpace.transform.Find("LeftEyeAnchor").gameObject.SetActive(false);
            trackingSpace.transform.Find("RightEyeAnchor").gameObject.SetActive(false);
            trackingSpace.transform.Find("CenterEyeAnchor").gameObject.SetActive(false);
            trackingSpace.transform.Find("LeftHandAnchor").gameObject.SetActive(false);
            trackingSpace.transform.Find("RightHandAnchor").gameObject.SetActive(false);
#endif

    }

    int PlayerCount()
    {
        if (playmgr)
            return playmgr.playerCount;
        else
            return 0;
    }

    void Update()
    {

        if (clr.Equals(Color.cyan))
            SetColor();

        if (!isLocalPlayer)
            return;

        //player move controls for testing inside editor
#if UNITY_EDITOR
        //var x = Input.GetAxis("Horizontal") * Time.deltaTime * 3f;
        //if(x > 0)
         //   teleported = true;
        var z = Input.GetAxis("Vertical") * Time.deltaTime * 3f;
        if(z > 0)
            teleported = true;
        transform.Translate(0, 0, z);
#else
        viewCam.SetActive(false);
#endif


        //no action while transitioning views
        if (!transition)
        {
            // Track trigger state
            bool currentTriggerState = OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger) || Input.GetKey(KeyCode.LeftShift);
            // || Input.GetMouseButton(2)

            if (lastTriggerState != currentTriggerState)
                CmdSetTrigger(currentTriggerState);

            lastTriggerState = currentTriggerState;


            // Track touch state
            bool currentTouchState = OVRInput.Get(OVRInput.Touch.PrimaryTouchpad) || Input.GetKey(KeyCode.BackQuote);
            // || Input.GetMouseButton(0)

            if (lastTouchState != currentTouchState)
                CmdSetTouch(currentTouchState);

            lastTouchState = currentTouchState;


            // Track touch press
            bool currentTouchPress = OVRInput.Get(OVRInput.Button.PrimaryTouchpad) || Input.GetKey(KeyCode.Return);
            //  || Input.GetMouseButton(0)

            if (lastTouchPress != currentTouchPress)
                CmdTouchPress(currentTouchPress, teleported);

            lastTouchPress = currentTouchPress;

            //these messages only appear on the client
            //touchpad is touched and trigger is pulled
            if (lastTouchState && lastTriggerState)
            {
                viewLabel.SetActive(true);
                if (teleported && PlayerCount() > 1)
                    SetLabel("Press to gather others");
                else
                    SetLabel("Press for next view: " + viewmgr.NextViewName());
            }
            else
                viewLabel.SetActive(false);

            // No need to detect short press of back button
            // || Input.GetMouseButtonUp(1)
            if (OVRInput.Get(OVRInput.Button.Back) || Input.GetKeyUp(KeyCode.Tab))
            {
                //trigger is pulled
                if (lastTriggerState)
                    RestoreView();
                else
                    CycleOptions();
            }

        }
    }

    [Command]
    void CmdSetID()
    {
        if (playmgr)
            playerID = playmgr.playerCount;
        else
            playerID = transform.GetComponent<NetworkIdentity>().observers.Count;
        Debug.Log("Set player ID to " + playerID);
    }

    void OnSetID(int i)
    {
        playerID = i;
        SetView();
    }

    void SetColor()
    {

        switch (playerID % 6)
        {
            case 1:
                clr = Color.red;
                break;
            case 2:
                clr = Color.yellow;
                break;
            case 3:
                clr = Color.green;
                break;
            case 4:
                clr = Color.blue;
                break;
            case 5:
                clr = Color.cyan;
                break;
            case 0:
                clr = Color.magenta;
                break;
        }

        //set text properties
        TextMeshPro text = transform.Find("Label/Text").GetComponent<TextMeshPro>();
        text.faceColor = clr;
        text.SetText(playerID.ToString());

    }

    [Command]
    void CmdSetTrigger(bool currentTriggerState)
    {
        lastTriggerState = currentTriggerState;

        //display pointing on the big screen
        BringToFront();
    }

    void OnSetTrigger(bool currentTriggerState)
    {
        lastTriggerState = currentTriggerState;
    }

    [Command]
    void CmdSetTouch(bool currentTouchState)
    {
        lastTouchState = currentTouchState;
        //don't bring to front - no need to show all movement
    }

    void OnSetTouch(bool currentTouchState)
    {
        lastTouchState = currentTouchState;
    }

    [Command]
    void CmdTouchPress(bool currentTouchPress, bool teleState)
    {
        //track teleported variable on server
        teleported = teleState;
        //trigger is pressed
        if (lastTriggerState)
        {
            //button was released this frame
            if (lastTouchPress && !currentTouchPress)
            {
                if (teleported && PlayerCount() > 1)
                    viewmgr.Gather(this);
                else
                    viewmgr.Next();
            }
            BringToFront();
        }

        lastTouchPress = currentTouchPress;
        //don't bring to front - no need to show all movement
    }

    void OnTouchPress(bool currentTouchPress)
    {
        lastTouchPress = currentTouchPress;
    }

    [Command]
    void CmdMakeActive()
    {
        BringToFront();
    }

    void CycleOptions()
    {
        if (!viewmgr)
            return;

        if (viewmgr.transform.GetChild(viewmgr.currentView).GetComponent<ViewOption>())
            viewmgr.transform.GetChild(viewmgr.currentView).GetComponent<ViewOption>().optionSet.GetComponent<OptionManager>().Next();
        else if (viewmgr.transform.GetChild(viewmgr.currentView).GetComponent<ViewPanos>())
            viewmgr.transform.GetChild(viewmgr.currentView).GetComponent<ViewPanos>().Next();

        CmdCycleOptions();
        BringToFront();
    }

    [Command]
    void CmdCycleOptions()
    {
        if (viewmgr.transform.GetChild(viewmgr.currentView).GetComponent<ViewOption>())
            viewmgr.transform.GetChild(viewmgr.currentView).GetComponent<ViewOption>().optionSet.GetComponent<OptionManager>().Next();
        else if (viewmgr.transform.GetChild(viewmgr.currentView).GetComponent<ViewPanos>())
            viewmgr.transform.GetChild(viewmgr.currentView).GetComponent<ViewPanos>().Next();

    }

    void RestoreView()
    {
        if (teleported)
        {
            teleported = false;
            //Restore on client
            //StartCoroutine(UpdateView("Restoring view..."));
            //This only affects the local user, so no need for fade
            SetView();
            //Restore on server
            CmdRestoreView();
        }
        else
        {
            CmdPreviousView();
        }
    }

    [Command]
    void CmdRestoreView()
    {
        //StartCoroutine(UpdateView("Restoring view..."));
        //This only affects the local user, so no need for fade
        SetView();
        BringToFront();
    }

    [Command]
    void CmdNextView()
    {
        if (viewmgr)
            viewmgr.Next();
        BringToFront();
    }

    [Command]
    void CmdPreviousView()
    {
        if (viewmgr)
            viewmgr.Previous();
        BringToFront();
    }

    //called from command, so runs on server
    //loops through all players and turns off other player cameras
    //sets this player as active view
    void BringToFront()
    {
        foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Player"))
        {
            GameObject cam = obj.transform.Find("Head/Camera").gameObject;
            cam.SetActive(false);
        }

        viewCam.SetActive(true);

        if (viewmgr)
            viewmgr.activePlayer = this;
    }

    public void SetLabel(string strText)
    {
        viewLabel.GetComponent<TextMeshPro>().SetText(strText);
    }

    public void ShowLabel(bool toggle)
    {
        viewLabel.SetActive(toggle);
    }

    public IEnumerator UpdateView(string msg, Transform location)
    {
        //SetView will now go to the new teleport location rather than the currentView
        teleport = location;
        StartCoroutine(UpdateView(msg));
        yield return null;
    }

    public IEnumerator UpdateView(string msg)
    {
        //don't allow another action while this is running
        transition = true;

        //reset teleported
        teleported = false;

        SetLabel(msg);

        GameObject textObj = viewLabel;
        textObj.SetActive(true);

        //set text properties
        TextMeshPro text = textObj.GetComponent<TextMeshPro>();
        Color32 clr = text.faceColor;
        Color32 otln = text.outlineColor;
        Color32 underlay = text.fontSharedMaterial.GetColor("_UnderlayColor");

        OVRScreenFade Fader = null;
        if (isLocalPlayer && trackingSpace.transform.Find("CenterEyeAnchor").gameObject.activeSelf)
            Fader = trackingSpace.transform.Find("CenterEyeAnchor").gameObject.GetComponent<OVRScreenFade>();
        
        if (Fader)
        {
            //fade out
            Fader.fadeTime = 1f;
            Fader.FadeOut();
        }

        //invoke action and short pause
        Invoke("SetView", 1.0f);

        //wait before text fade out
        yield return new WaitForSeconds(1f);

        if (Fader)
        {
            //fade in
            Fader.fadeTime = 2f;
            Fader.FadeIn();
        }

        //wait before text fade out
        yield return new WaitForSeconds(1f);

        //fade text out
        float aTime = 0.5f;
        for (float t = 0.0f; t < 1.0f; t += Time.deltaTime / aTime)
        {
            clr.a = (byte)Mathf.Lerp(255, 0, t);
            text.faceColor = clr;
            otln.a = (byte)Mathf.Lerp(255, 0, t);
            text.outlineColor = otln;
            underlay.a = (byte)Mathf.Lerp(255, 0, t);
            text.fontSharedMaterial.SetColor("_UnderlayColor", underlay);
            yield return null;
        }

        textObj.SetActive(false);

        //reset text color
        clr.a = 255;
        text.faceColor = clr;
        otln.a = 255;
        text.outlineColor = otln;
        underlay.a = 255;
        text.fontSharedMaterial.SetColor("_UnderlayColor", underlay);

        //allow another action
        transition = false;
    }

    void SetView()
    {
        if (optmgr.Length > 0)
            foreach (OptionManager opt in optmgr)
                opt.Activate(false);
        viewmgr.SetView(this, teleport);
        teleport = null;
        if (PlayerCount() == 1)
            transform.Find("Label").gameObject.SetActive(false);
    }

    void SetHeight()
    {
        Vector3 pos = trackingSpace.transform.localPosition;
        if (standing)
            pos.y = eyeHeightStanding * 0.3048f;
        else
            pos.y = eyeHeightSeated * 0.3048f;
        trackingSpace.transform.localPosition = pos;
    }


}