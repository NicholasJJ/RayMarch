using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grab : MonoBehaviour
{
    public cameraManager camManager;
    public float reach;
    public float minHoldDist;
    public float holdDist;
    public float pullForce;
    bool grabbedHadGravity;
    [SerializeField] private GameObject grabbed;
    [SerializeField] float maxV;
    // Start is called before the first frame update
    void Start()
    {
        maxV = float.NegativeInfinity;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (!grabbed)
            {
                Vector3 ori = camManager.inFoldedWorldSpace(transform.position);
                Vector3 forw = camManager.inFoldedWorldSpace(transform.position + transform.forward) - ori;
                Ray ray = new Ray(ori, forw);
                Debug.DrawRay(ray.origin, ray.direction,Color.red,10);
                Debug.Log(ray.origin + "     " + ray.direction);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, reach))
                {
                    if (hit.collider.gameObject.tag == "Moveable")
                    {
                        StartGrab(hit.collider.gameObject);
                    }
                }
            } else
            {
                EndGrab();
            }
        }
        //move hold distance back and forth
        holdDist = Mathf.Clamp(holdDist + (Input.mouseScrollDelta.y * .2f), minHoldDist, reach);

        //move object to grab point
        if (grabbed)
        {
            //smooth grab, doesn't work with noncontinuous space e.g. portals :(
            //Vector3 v = camManager.inFoldedWorldSpace(transform.position + (transform.forward * holdDist)) - camManager.inFoldedWorldSpace(grabbed.transform.position);
            //maxV = Mathf.Max(maxV, v.magnitude);
            //v *= pullForce;
            //grabbed.GetComponent<Rigidbody>().velocity = v;

            //direct grab
            grabbed.transform.position = camManager.inFoldedWorldSpace(transform.position + (transform.forward * holdDist));
        }
    }

    void StartGrab(GameObject o)
    {
        grabbed = o;
        holdDist = Mathf.Clamp(Vector3.Distance(transform.position, grabbed.transform.position), minHoldDist, reach);
        grabbedHadGravity = grabbed.GetComponent<Rigidbody>().useGravity;
        grabbed.GetComponent<Rigidbody>().useGravity = false;
    }

    void EndGrab()
    {
        grabbed.GetComponent<Rigidbody>().useGravity = grabbedHadGravity;
        grabbed.GetComponent<Rigidbody>().velocity = Vector3.zero;
        grabbed = null;
    }
}
