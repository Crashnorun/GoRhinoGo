using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Synchronize : NetworkBehaviour {

    public GameObject leadingObject;
    public GameObject followingObject;

    // Update is called once per frame
    void Update() {
        if (isLocalPlayer)
        {
            followingObject.transform.position = leadingObject.transform.position;
            followingObject.transform.rotation = leadingObject.transform.rotation;
        }
    }
}
