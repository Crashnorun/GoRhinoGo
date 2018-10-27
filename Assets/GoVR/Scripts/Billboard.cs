using UnityEngine;
using System.Collections;

public class Billboard : MonoBehaviour
{

    void Update()
    {
        if (Camera.main) this.transform.LookAt(Camera.main.transform);
    }

}