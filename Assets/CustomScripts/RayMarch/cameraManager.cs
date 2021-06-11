using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cameraManager : MonoBehaviour
{
    public ComputeShader RaymarchShader;
    private RenderTexture _target;
    private Camera _camera;
    public raymarchObject[] objs;
    public raymarchObject[] vampireObjs; //no reflection
    public bool useMirrors;
    public int mirrorDepth;
    public GameObject[] mirrorObjects;
    public GameObject rlight;
    public int colResolution;
    public Vector3[] pointsOnSphere;
    private int totalSize;
    private int totalBoundarySize;
    private int totalMirrorSize;
    public GameObject boundaryObject;
    public bool[] boundaryObjects;
    

    public struct obj
    {
        public Vector3 position;
        public int type;
        public Vector4 color;
        public Vector4 dimensions;
        public int combType;
    }

    private obj[] objects;
    private obj[] vObjects;

    public struct pointCollider
    {
        public Vector3 position;
        public int hit;
        public Vector3 norm;
    }

    private pointCollider[] boundaryPoints;

    public struct mirror
    {
        public Vector3 position;
        public Vector3 normal;
    }

    private mirror[] mirrors;

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        SetShaderParameters();
        Render(destination);
    }

    private void Render(RenderTexture destination)
    {
        InitRenderTexture();

        RaymarchShader.SetTexture(0, "Result", _target);
        int threadGroupsX = Mathf.CeilToInt(Screen.width / 8.0f);
        int threadGroupsY = Mathf.CeilToInt(Screen.height / 8.0f);

        objects = new obj[objs.Length];
        for (int i = 0; i < objs.Length; i++)
        {
            obj o = new obj();
            raymarchObject obji = objs[i];
            o.position = obji.transform.position;
            o.type = obji.objType;
            o.color = obji.color;
            o.dimensions = obji.transform.localScale * 0.5f;
            o.combType = obji.combType;
            objects[i] = o;
        }

        vObjects = new obj[vampireObjs.Length];
        for (int i = 0; i < vampireObjs.Length; i++)
        {
            obj o = new obj();
            raymarchObject obji = vampireObjs[i];
            o.position = obji.transform.position;
            o.type = obji.objType;
            o.color = obji.color;
            o.dimensions = obji.transform.localScale * 0.5f;
            o.combType = obji.combType;
            vObjects[i] = o;
        }

        boundaryPoints = new pointCollider[colResolution];
        for (int i = 0; i < colResolution; i++)
        {
            pointCollider point = new pointCollider();
            point.position = transform.position + (pointsOnSphere[i]*.5f);
            point.hit = 0;
            point.norm = Vector3.zero;
            boundaryPoints[i] = point;
        }

        mirrors = new mirror[mirrorObjects.Length];
        for (int i = 0; i < mirrorObjects.Length; i++)
        {
            mirror m = new mirror();
            m.position = mirrorObjects[i].transform.position;
            m.normal = mirrorObjects[i].transform.up;
            mirrors[i] = m;
        }
         
        ComputeBuffer objectBuffer = new ComputeBuffer(objects.Length, totalSize);
        objectBuffer.SetData(objects);
        RaymarchShader.SetBuffer(0, "objs", objectBuffer);
        RaymarchShader.SetBuffer(1, "objs", objectBuffer);

        ComputeBuffer vObjectBuffer = new ComputeBuffer(vObjects.Length, totalSize);
        vObjectBuffer.SetData(vObjects);
        RaymarchShader.SetBuffer(0, "vampireObjs", vObjectBuffer);
        RaymarchShader.SetBuffer(1, "vampireObjs", vObjectBuffer);

        ComputeBuffer boundaryBuffer = new ComputeBuffer(colResolution, totalBoundarySize);
        boundaryBuffer.SetData(boundaryPoints);
        RaymarchShader.SetBuffer(1, "bounds", boundaryBuffer);

        ComputeBuffer mirrorBuffer = new ComputeBuffer(mirrors.Length, totalMirrorSize);
        mirrorBuffer.SetData(mirrors);
        RaymarchShader.SetBuffer(0, "mirrors", mirrorBuffer);
        RaymarchShader.SetBuffer(1, "mirrors", mirrorBuffer);

        RaymarchShader.SetVector("_lightDir", -rlight.transform.forward);
        RaymarchShader.SetBool("_useMirrors", useMirrors);
        RaymarchShader.SetInt("_mirrorDepth", mirrorDepth);

        RaymarchShader.Dispatch(0, threadGroupsX, threadGroupsY, 1);

        Graphics.Blit(_target, destination);

        RaymarchShader.Dispatch(1, colResolution/10, 1, 1);

        boundaryBuffer.GetData(boundaryPoints);

        for (int i = 0; i < boundaryPoints.Length; i++)
        {
            if ((boundaryPoints[i].hit == 1 && !boundaryObjects[i])) 
            {
                //Debug.Log("Hit at point" + i + "norm is" + boundaryPoints[i].norm);
                //GameObject b = Instantiate(boundaryObject, boundaryPoints[i].position + (pointsOnSphere[i] * .5f), Quaternion.identity);
                GameObject b = Instantiate(boundaryObject, boundaryPoints[i].position + (-boundaryPoints[i].norm * .5f), Quaternion.identity);
                b.GetComponent<boundaryObject>().index = i;
                b.GetComponent<boundaryObject>().camManager = this;
                boundaryObjects[i] = true;
            }
        }

        objectBuffer.Dispose();
        vObjectBuffer.Dispose();
        boundaryBuffer.Dispose();
        mirrorBuffer.Dispose();
    }

    private void InitRenderTexture()
    {
        if (_target == null || _target.width != Screen.width || _target.height != Screen.height)
        {
            if (_target != null)
            {
                _target.Release();
            }

            _target = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.ARGBFloat,
                RenderTextureReadWrite.Linear);
            _target.enableRandomWrite = true;
            _target.Create();
        }
    }

    private void Awake()
    {
        _camera = GetComponent<Camera>();

        int posSize = sizeof(float) * 3;
        int typeSize = sizeof(int);
        int ColorSize = sizeof(float) * 4;
        int boolSize = sizeof(int);
        int DimSize = sizeof(float) * 4;

        totalSize = posSize + typeSize + ColorSize + DimSize + typeSize;

        totalBoundarySize = posSize + boolSize + posSize;

        totalMirrorSize = posSize + posSize;

        pointsOnSphere = PointsOnSphere(colResolution);
        boundaryObjects = new bool[colResolution];
    }

    private void SetShaderParameters()
    {
        RaymarchShader.SetMatrix("_CameraToWorld", _camera.cameraToWorldMatrix);
        RaymarchShader.SetMatrix("_CameraInverseProjection", _camera.projectionMatrix.inverse);
    }

    Vector3[] PointsOnSphere(int n)
    {
        List<Vector3> upts = new List<Vector3>();
        float inc = Mathf.PI * (3 - Mathf.Sqrt(5));
        float off = 2.0f / n;
        float x = 0;
        float y = 0;
        float z = 0;
        float r = 0;
        float phi = 0;

        for (var k = 0; k < n; k++)
        {
            y = k * off - 1 + (off / 2);
            r = Mathf.Sqrt(1 - y * y);
            phi = k * inc;
            x = Mathf.Cos(phi) * r;
            z = Mathf.Sin(phi) * r;

            upts.Add(new Vector3(x, y, z));
        }
        Vector3[] pts = upts.ToArray();
        pts[0] = new Vector3(0, -1, 0);
        return pts;
    }

    public Vector3 inFoldedWorldSpace(Vector3 p)
    {
        if (!useMirrors) return p;
        int depth = 0;
        while (depth < mirrorDepth)
        {
            Vector3 np = new Vector3(p.x, p.y, p.z);
            for (int i = 0; i < mirrors.Length; i++)
            {
                np = planeFold(np, mirrors[i].normal, mirrors[i].position);
            }
            if (np == p) break;
            p = np;
            //depth++;
        }
        
        return p;
    }

    Vector3 planeFold(Vector3 p, Vector3 plNorm, Vector3 plPos)
    {
        p -= plPos;
        p -= (2 * Mathf.Min(0,Vector3.Dot(p, plNorm)) * plNorm);
        return p + plPos;
    }
}
