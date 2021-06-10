using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerMovement : MonoBehaviour
{
    Rigidbody rBody;
    [SerializeField] float speed;
    [SerializeField] float mouseSensitivity;
    [SerializeField] float jumpForce;
    GameObject head;
    Vector2 mouseLook = Vector2.zero;
    [SerializeField] private bool inAir;
    // Start is called before the first frame update
    void Start()
    {
        rBody = gameObject.GetComponent<Rigidbody>();
        head = GameObject.FindGameObjectWithTag("MainCamera");
        LockMouse();
        inAir = true;
    }

    // Update is called once per frame
    void Update()
    {
        //Movement
        Vector3 move = new Vector3(Input.GetAxis("Horizontal") * Time.deltaTime * speed, 0,
            Input.GetAxis("Vertical") * Time.deltaTime * speed);
        //transform.Translate(move);
        Vector3 v = (transform.forward * Input.GetAxis("Vertical") * speed) + (transform.right * Input.GetAxis("Horizontal") * speed);
        rBody.velocity = new Vector3(0, rBody.velocity.y, 0);
        rBody.velocity += v;

        //Jumping
        if (Input.GetButtonDown("Jump") && !inAir)
        {
            inAir = true;
            rBody.AddForce(Vector3.up * jumpForce);
        }

        //Looking
        mouseLook += new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            transform.localRotation = Quaternion.AngleAxis(mouseLook.x * mouseSensitivity, Vector3.up);
            head.transform.localRotation = Quaternion.AngleAxis(-mouseLook.y * mouseSensitivity, Vector3.right);
        }
        
        //Mouse
        if (Input.GetKeyDown(KeyCode.J))
        {
            if (Cursor.visible)
                LockMouse();
            else
                UnlockMouse();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        inAir = false;
    }

    void LockMouse()
    {
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void UnlockMouse()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}

