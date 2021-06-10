using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class pictureManager : MonoBehaviour
{
    public Shader rayMarcher;
    public Material raymarchMaterial;
    public raymarchObject[] objs;
    public Vector3 pos;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        Vector4[] positions = new Vector4[objs.Length];
        Vector4[] colors = new Vector4[objs.Length];
        int[] types = new int[objs.Length];
        for(int i = 0; i < objs.Length; i++)
        {
            positions[i] = objs[i].transform.position;
            positions[i].w = objs[i].objType;
            colors[i] = objs[i].color;
        }
        raymarchMaterial.SetVectorArray("_objPos", positions);
        raymarchMaterial.SetVectorArray("_objColor", colors);

    }

}
