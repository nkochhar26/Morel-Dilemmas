using UnityEngine;
using EzySlice;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine.Rendering;
public class EzyMeshSlicer : DragFoodInto
{
    Vector3 p1World, p2World;
    public float sliceTime = 1;
    public AnimationCurve sliceCurve;
    public float sliceMoveDistance = 1;
    public float sliceMoveTime = 1;
    public AnimationCurve sliceMoveCurve;
    public LineRenderer sliceLineRenderer;
    public LineRenderer effectLineRenderer;
    public LineRenderer outlineLineRenderer;
    private float sliceTimer;
    private bool allowCut;
    private List<Vector3> outlinePoints = new List<Vector3>();
    private float outlineDistance;
    private SliceOperation rootSliceOperation; // Root of the slice tree
    public int sliceCount = 0;
    
    //game stuff
    private Mushroom currentMushroom; //needs to accomodate poisonous mushrooms
    public List<InventoryItem> itemsOnBoard = new List<InventoryItem>();
    private Dictionary<InventoryItem, SliceOperation> itemSliceOperations = new Dictionary<InventoryItem, SliceOperation>();

    void Start()
    {
        GetComponent<Collider2D>().enabled = false;
        GetComponent<Collider2D>().enabled = true;
    }

    int GetSliceDepth(SliceOperation op)
    {
        if (op == null) return 0;
        int upperDepth = op.upperHullSlice != null ? GetSliceDepth(op.upperHullSlice) : 0;
        int lowerDepth = op.lowerHullSlice != null ? GetSliceDepth(op.lowerHullSlice) : 0;
        return 1 + Mathf.Max(upperDepth, lowerDepth);
    }

    public void InitializeFromItem(InventoryItem item) // when moving an already sliced item onto the board
    {
        if (item == null || item.foodItem == null || item.foodItem.sliceOperation == null)
        {
            rootSliceOperation = null;
            return;
        }

        rootSliceOperation = item.foodItem.sliceOperation;
    }

    public override void AddItem(InventoryItem item)
    {
        base.AddItem(item);
        itemsOnBoard.Add(item);
        itemSliceOperations[item] = item.foodItem?.sliceOperation ?? null;
    }

    Vector3 MouseWorldPos => Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 5));

    public void OnMouseDown()
    {
        if (sliceTimer > 0) return;
        sliceLineRenderer.enabled = true;
        p1World = MouseWorldPos;
        sliceLineRenderer.SetPosition(0, p1World);
    }

    public void OnMouseUp()
    {
        if (sliceTimer > 0) return;
        p2World = MouseWorldPos;
        sliceLineRenderer.SetPosition(1, p2World);
        sliceTimer = 1;
    }

    // Chopped ingredients go back to the inventory with the right-click lasso.
    // Dishes are only made at the plating station.

    public void ClearBoiler()
    {
        // return anything still on the board instead of deleting it
        foreach(InventoryItem item in itemsOnBoard)
        {
            if (item == null) continue;
            if (item.foodItem != null)
            {
                GameManager.Instance.inventoryManager.AddFoodObject(item.foodItem, item.foodItem.isIntermediate);
            }
            Destroy(item.gameObject);
        }
        itemsOnBoard.Clear();
        itemSliceOperations.Clear();
        sliceCount = 0;
    }


    void Update()
    {
        //Mouse outline logic
        
        if (Input.GetMouseButton(1))
        {
            Vector3 currentPoint = MouseWorldPos;
            
            if (outlinePoints.Count == 0)
            {
                outlinePoints.Add(currentPoint);
            }
            else
            {
                outlineDistance += Vector2.Distance(currentPoint, outlinePoints[outlinePoints.Count - 1]);
                if (outlineDistance >= 0.025f)
                {
                    outlinePoints.Add(currentPoint);
                    outlineDistance = 0f;
                }
            }
            
            outlineLineRenderer.positionCount = outlinePoints.Count;
            for (int i = 0; i < outlinePoints.Count; i++)
            {
                outlineLineRenderer.SetPosition(i, outlinePoints[i]);
            }
        }
        else
        {
            if(outlineLineRenderer.positionCount > 0)
            {
                var selectedMeshes = GetTransformsInsideOutline(
                    new List<Transform>(GetComponentsInChildren<MeshFilter>().Select(mf => mf.transform)),
                    outlinePoints
                );
                
                // The lasso can catch pieces from several items: handle each item separately
                foreach (var group in selectedMeshes.GroupBy(m => m.parent))
                {
                    LassoItem(group.Key, group.ToList());
                }


                outlinePoints.Clear();
                outlineLineRenderer.positionCount = 0;
            }
        }
        




        /// Slicing logic

        if (sliceTimer > 0f)
        {
            allowCut = true;

            sliceTimer -= Time.deltaTime / sliceTime;
            sliceTimer = Mathf.Max(sliceTimer, 0f);

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
            sliceLineRenderer.SetPosition(1, MouseWorldPos);
            if (allowCut)
            {
                allowCut = false;
                SliceAllChildren();
                DOTween.To(() => effectLineRenderer.widthMultiplier, x => effectLineRenderer.widthMultiplier = x, 0, 0.5f).OnComplete(() =>
                {
                    effectLineRenderer.enabled = false;
                });

                sliceLineRenderer.enabled = false;
                sliceCount += 1;
            }
        }
    }


    // Pull the lassoed pieces of one item back into the inventory; its other pieces are discarded
    void LassoItem(Transform parent, List<Transform> selected)
    {
        if (parent == null) return;
        InventoryItem parentItem = parent.GetComponent<InventoryItem>();
        if (parentItem == null) return; // not a food item (never destroy the board itself)

        // Take the item off the board right away so it can't be added twice
        FoodItemObject returned = null;
        if (itemsOnBoard.Contains(parentItem))
        {
            itemsOnBoard.Remove(parentItem);
            if (parentItem.foodItem != null)
            {
                returned = parentItem.foodItem.CloneSingle(); // keeps lookalike status, quality, tags
                if (itemSliceOperations.TryGetValue(parentItem, out SliceOperation op) && op != null)
                {
                    returned.sliceOperation = op;
                }
                if (!returned.tags.Contains(CookingStep.Chop))
                {
                    returned.tags.Add(CookingStep.Chop);
                }
            }
            itemSliceOperations.Remove(parentItem);
        }

        // unselected pieces fly away and are destroyed
        foreach (Transform child in parent)
        {
            if (!child.gameObject.activeSelf || selected.Contains(child) || child.GetComponent<MeshFilter>() == null) continue;
            if (child.GetComponent<Collider2D>()) child.GetComponent<Collider2D>().enabled = false;

            // mark branch as destroyed in the slice tree
            SliceInfo info = child.GetComponent<SliceInfo>();
            if (info?.sliceOperation != null)
            {
                if (info.isUpperHull) info.sliceOperation.upperDestroyed = true;
                else info.sliceOperation.lowerDestroyed = true;
            }

            child.DOKill();
            child.DOMove(child.position + (child.position - parent.position) * 3, .25f).OnComplete(() =>
            {
                child.DOMove(transform.position + transform.up, 0.5f);
            });
        }

        // selected pieces fly toward the player; once the last one lands, add the item to inventory
        int remaining = selected.Count;
        foreach (Transform mesh in selected)
        {
            if (mesh.GetComponent<Collider2D>()) mesh.GetComponent<Collider2D>().enabled = false;

            mesh.DOKill();
            mesh.DOMove(mesh.position + (mesh.position - parent.position) * 3, .25f).OnComplete(() =>
            {
                mesh.DOMove(transform.position - transform.up, 0.5f).OnComplete(() =>
                {
                    remaining--;
                    if (remaining > 0) return;

                    if (returned != null)
                    {
                        GameManager.Instance.inventoryManager.AddFoodObject(returned, true);
                    }
                    parent.DOKill(true);
                    foreach (Transform t in parent) t.DOKill();
                    Destroy(parent.gameObject);
                });
            });
        }
    }

    void SliceAllChildren()
    {
        //detect all meshes the line intersects
        var meshesToSlice = GetComponentsInChildren<MeshFilter>().Where(mf => mf.gameObject.activeInHierarchy && CutHitsMesh(mf.transform, p1World, p2World)).ToArray();

        Vector3 planeNormal = Vector3.Cross((p2World - p1World).normalized, Camera.main.transform.forward).normalized;

        foreach (var meshFilter in meshesToSlice)
        {
            Transform child = meshFilter.transform;
            MeshRenderer meshRenderer = child.GetComponent<MeshRenderer>();
            if (!meshRenderer) continue;

            SlicedHull hull = child.gameObject.Slice(p1World, planeNormal);
            if (hull == null) continue;

            Transform parent = child.parent;
            if (parent == null) continue;
            
            InventoryItem parentItem = parent.GetComponent<InventoryItem>();
            SliceOperation parentSliceOp = parentItem != null && itemSliceOperations.ContainsKey(parentItem) ? itemSliceOperations[parentItem] : null;

            // create a local slice operation
            SliceOperation currentSlice = new SliceOperation
            {
                centerPoint = child.InverseTransformPoint((p1World + p2World) * 0.5f),
                planeNormal = child.InverseTransformDirection(planeNormal)
            };

            // create upper and lower pieces
            GameObject upper = SetupHull(hull.CreateUpperHull(child.gameObject, meshRenderer.sharedMaterial), child, currentSlice, true);
            GameObject lower = SetupHull(hull.CreateLowerHull(child.gameObject, meshRenderer.sharedMaterial), child, currentSlice, false);

            // add the pieces to the tree
            SliceInfo childSliceInfo = child.GetComponent<SliceInfo>();
            if (childSliceInfo?.sliceOperation != null)
            {
                if (childSliceInfo.isUpperHull) childSliceInfo.sliceOperation.upperHullSlice = currentSlice;
                else childSliceInfo.sliceOperation.lowerHullSlice = currentSlice;
            }
            else if (parentSliceOp == null && parentItem != null)
            {
                itemSliceOperations[parentItem] = currentSlice;
            }

            StartCoroutine(MoveSlice(upper.transform, planeNormal, sliceMoveTime, sliceMoveDistance));
            StartCoroutine(MoveSlice(lower.transform, -planeNormal, sliceMoveTime, sliceMoveDistance));
            Destroy(child.gameObject);
        }
    }

    GameObject SetupHull(GameObject hull, Transform parent, SliceOperation sliceOp, bool isUpper)
    {
        hull.transform.SetParent(parent.parent, false);
        DragFoodInto.AddPolygonCollider2DFromMesh(hull);
        DragFoodInto.CloneRigidbody2D(hull, parent.gameObject);
        
        SliceInfo info = hull.AddComponent<SliceInfo>();
        info.sliceOperation = sliceOp;
        info.isUpperHull = isUpper;
        return hull;
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
            // the piece may have been sliced again or lassoed away mid-move
            if (sliceTransform == null) yield break;
            Vector3 originalposition = sliceTransform.position;
            Vector3 targetPosition = originalposition + point.normalized * distance;
            sliceTransform.position = Vector3.Lerp(originalposition, targetPosition, sliceMoveCurve.Evaluate(t/duration));
            yield return new WaitForFixedUpdate();
        }
    }

    List<Transform> GetTransformsInsideOutline(List<Transform> meshTransforms, List<Vector3> outlinePoints) // get all transforms inside the outline
    {
        if (outlinePoints.Count < 3) return new List<Transform>();
        
        // Convert outline points to screen space
        List<Vector2> screenPolygon = outlinePoints.Select(p => (Vector2)Camera.main.WorldToScreenPoint(p)).ToList();
        
        // Use mesh bounds center instead of transform position (pivot may be offset after slicing)
        return meshTransforms.Where(t => {
            var renderer = t.GetComponent<MeshRenderer>();
            Vector3 center = renderer ? renderer.bounds.center : t.position;
            return IsPointInPolygon(Camera.main.WorldToScreenPoint(center), screenPolygon);
        }).ToList();
    }

    bool IsPointInPolygon(Vector2 point, List<Vector2> polygon) // detect if point is in the outline, which is a list of points
    {
        int intersections = 0;
        for (int i = 0; i < polygon.Count; i++)
        {
            Vector2 a = polygon[i], b = polygon[(i + 1) % polygon.Count];
            if ((a.y > point.y) != (b.y > point.y))
            {
                float xIntersect = (b.x - a.x) * (point.y - a.y) / (b.y - a.y) + a.x;
                if (point.x < xIntersect) intersections++;
            }
        }
        return (intersections % 2) == 1;
    }
}
