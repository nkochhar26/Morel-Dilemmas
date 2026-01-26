using UnityEngine;
using EzySlice;
using System.Collections;
using System.Collections.Generic;
public class EzyMeshSlicer : MonoBehaviour
{
    Vector3 p1World, p2World;
    public GameObject sliceParticle;
    public float sliceTime = 1;
    public AnimationCurve sliceCurve;
    public float sliceMoveDistance = 1;
    public float sliceMoveTime = 1;
    public AnimationCurve sliceMoveCurve;
    public LineRenderer sliceLineRenderer;
    public LineRenderer effectLineRenderer;
    private float sliceTimer;
    private GameObject sliceParticleInstance;
    private bool allowCut;

    void Start()
    {
        GetComponent<Collider>().enabled = false;
        GetComponent<Collider>().enabled = true;
    }

    public void OnMouseDown()
    {
        if(sliceTimer > 0) return;
        sliceLineRenderer.enabled = true;
        p1World = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 5));
        sliceLineRenderer.SetPosition(0, p1World);
    }

    public void OnMouseUp()
    {
        if(sliceTimer > 0) return;

        p2World = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 5));
        sliceLineRenderer.SetPosition(1, p2World);
        sliceTimer = 1;
    }

    void Update()
    {
        if (sliceTimer > 0f)
        {
            allowCut = true;

            sliceTimer -= Time.deltaTime / sliceTime;
            sliceTimer = Mathf.Max(sliceTimer, 0f); // IMPORTANT

            effectLineRenderer.enabled = true;
            effectLineRenderer.widthMultiplier = 1f;

            int num = 10;
            if (effectLineRenderer.positionCount != num)
                effectLineRenderer.positionCount = num;

            effectLineRenderer.SetPosition(0, p1World);

            for (int i = 1; i < num; i++)
            {
                float percent = i / (num - 1f);
                float curveT = sliceCurve.Evaluate(1f - sliceTimer);
                effectLineRenderer.SetPosition(
                    i,
                    Vector3.Lerp(p1World, p2World, percent * curveT)
                );
            }
        }
        else
        {
            sliceLineRenderer.SetPosition(1, Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 5)));
            if (allowCut)
            {
                allowCut = false;
                SliceAllChildren();
                StartCoroutine(ShrinkSliceEffect(0.5f));

                sliceLineRenderer.enabled = false;
            }
        }
    }


    IEnumerator ShrinkSliceEffect(float duration)
    {
        for(float t=0; t<duration; t+=Time.fixedDeltaTime)
        {
            effectLineRenderer.widthMultiplier = Mathf.Lerp(1, 0, t/duration);
            yield return new WaitForFixedUpdate();
        }
        effectLineRenderer.enabled = false;
    }

    void SliceAllChildren()
    {
        List<Transform> children = new List<Transform>();
        foreach (Transform child in transform) 
        {
            if(CutHitsMesh(child, p1World, p2World)) children.Add(child);
        }

        Transform[] childrenArray = children.ToArray();
        
        //go throgh every child and slice
        foreach (Transform child in childrenArray)
        {
            MeshFilter meshFilter = child.GetComponent<MeshFilter>();
            MeshRenderer meshRenderer = child.GetComponent<MeshRenderer>();
            if (!meshFilter || !meshRenderer) continue;

            //get direction and normal to make the slicing plane
            Vector3 swipeDir = (p2World - p1World).normalized;
            Vector3 planeNormal = Vector3.Cross(swipeDir, Camera.main.transform.forward).normalized;

            Vector3 planePoint = p1World;

            //create the hull, which is just the sliced mesh data from the plane, used from the ezyslice library
            SlicedHull hull = child.gameObject.Slice(
                planePoint,
                planeNormal
            );

            if (hull == null) continue;

            //make the two sliced mesh portions
            GameObject upper = hull.CreateUpperHull(child.gameObject, meshRenderer.sharedMaterial);
            GameObject lower = hull.CreateLowerHull(child.gameObject, meshRenderer.sharedMaterial);

            upper.transform.SetParent(transform, false);
            lower.transform.SetParent(transform, false);

            //do the moving animation
            StartCoroutine(MoveSlice(upper.transform, planeNormal, sliceMoveTime, sliceMoveDistance));
            StartCoroutine(MoveSlice(lower.transform, -planeNormal, sliceMoveTime, sliceMoveDistance));

            Destroy(child.gameObject);
        }
    }

    bool CutHitsMesh(Transform meshTf, Vector3 mousePoint1, Vector3 mousePoint2)
    {
        MeshFilter mf = meshTf.GetComponent<MeshFilter>();
        if (!mf) return false;

        //get mesh triangle data
        Mesh mesh = mf.mesh;
        Vector3[] verts = mesh.vertices;
        int[] tris = mesh.triangles;

        Vector2 m1 = mousePoint1;
        Vector2 m2 = mousePoint2;

        for (int i = 0; i < tris.Length; i += 3)
        {
            //get triangle points in world space
            Vector2 a = meshTf.TransformPoint(verts[tris[i]]);
            Vector2 b = meshTf.TransformPoint(verts[tris[i + 1]]);
            Vector2 c = meshTf.TransformPoint(verts[tris[i + 2]]);

            //first detect if line intersects triangle of mesh
            if (LineIntersectsTriangle(m1, m2, a, b, c))
            {
                //then check if both points are outside of triangle to make the player have to actually slice through
                if (IsPointInTriangle(m1, a, b, c) ||
                    IsPointInTriangle(m2, a, b, c))
                    return false;

                return true;
            }
        }
        return false;
    }


    bool LineIntersectsTriangle(Vector2 p1, Vector2 p2, Vector2 a, Vector2 b, Vector2 c) //returns true if the line from p1 to p2 intersects with any line on the triangle
    {
        bool Seg(Vector2 p, Vector2 p2, Vector2 q, Vector2 q2)
        {
            Vector2 r = p2 - p, s = q2 - q;
            float d = r.x * s.y - r.y * s.x;
            if (Mathf.Abs(d) < 1e-6f) return false;
            float u = ((q.x - p.x) * r.y - (q.y - p.y) * r.x) / d;
            float t = ((q.x - p.x) * s.y - (q.y - p.y) * s.x) / d;
            return t >= 0 && t <= 1 && u >= 0 && u <= 1;
        }

        return Seg(p1, p2, a, b) ||
            Seg(p1, p2, b, c) ||
            Seg(p1, p2, c, a);
    }


    public bool IsPointInTriangle(Vector2 p, Vector2 a, Vector2 b, Vector2 c) // returns true if our point p is inside the triangle
    {
        float s1 = Mathf.Sign((b.x - a.x) * (p.y - a.y) - (b.y - a.y) * (p.x - a.x));
        float s2 = Mathf.Sign((c.x - b.x) * (p.y - b.y) - (c.y - b.y) * (p.x - b.x));
        float s3 = Mathf.Sign((a.x - c.x) * (p.y - c.y) - (a.y - c.y) * (p.x - c.x));

        return (s1 == s2 && s2 == s3);
    }

    IEnumerator MoveSlice(Transform sliceTransform, Vector3 point, float duration, float distance)
    {
        for(float t=0; t<duration; t+=Time.fixedDeltaTime)
        {
            Vector3 originalposition = sliceTransform.position;
            Vector3 targetPosition = originalposition + point.normalized * distance;
            sliceTransform.position = Vector3.Lerp(originalposition, targetPosition, sliceMoveCurve.Evaluate(t/duration));
            yield return new WaitForFixedUpdate();
        }
    }
}
