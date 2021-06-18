using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class sceneSwitch : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            nextScene();
        }
    }

    void nextScene()
    {
        Debug.Log("switch to next scene");
    }
}
