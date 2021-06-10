using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class boundaryObject : MonoBehaviour
{
    public cameraManager camManager;
    public int index;
    public float time;

    private void OnCollisionExit(Collision collision)
    {
        Destroy(gameObject);
    }

    private void Start()
    {
        Destroy(gameObject, time);
    }

    private void OnDestroy()
    {
        camManager.boundaryObjects[index] = false;
    }

    private void Awake()
    {
        camManager = GameObject.Find("Main Camera").GetComponent<cameraManager>();
    }
}
