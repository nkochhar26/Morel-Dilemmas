using System.Collections.Generic;
using UnityEngine;
public class AssetScattering : MonoBehaviour
{
    //borrowing from Alex's Parallax layer script

    public float parallaxMultiplier = 0.5f;
    Transform cam;
    public GameObject mushroomMan;
    float distanceBetweenObjects;
    Transform[] tiles;
    public float renderDistance = 20f;
    float tileWidth;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        cam = Camera.main.transform;
        //interval = UnityEngine.Random.Range(1f,1.5f);
        //set length of array to the amount of children
        tiles = new Transform[transform.childCount];
        for (int i = 0; i < tiles.Length; i++){
            tiles[i] = transform.GetChild(i);
        }
        tileWidth = tiles[0].GetComponent<Renderer>().bounds.size.x;

    }


    // Update is called once per frame
    void Update(){
                if (renderDistance < 20f){
            renderDistance = 20f;
        }
    }
    void LateUpdate()
    {
        //x coord of camera (center)
        //take renderDistance
        //calculate new position for thing
            //determine if thing needs to be repositioned on the left or right side.
                //determine if thing is closer to the min or the max.
                //will result in either a negative or positive change in position
            //add/sub renderDistance with camX
                //float on float operations so no worries
            //new Vector!
        //apply position.
        float camX = cam.position.x ;

        for (int i = 0; i < tiles.Length; i++)
        {
            
            Vector3 tileClosestPoint = tiles[i].GetComponent<Renderer>().bounds.ClosestPoint(mushroomMan.transform.position);

            distanceBetweenObjects = Mathf.Abs(tileClosestPoint.x - mushroomMan.transform.position.x);
            
            
            if (distanceBetweenObjects > renderDistance){ 
                Vector3 tileMax =tiles[i].GetComponent<Renderer>().bounds.max;
                Vector3 tileMin = tiles[i].GetComponent<Renderer>().bounds.min;
            //if distance between our point and the right edge is greater 
            // than the distance between our point and the left edge
            // then we know where to change the position of our placeable
                bool compare = Mathf.Abs(tileClosestPoint.x - tileMax.x)
                    > Mathf.Abs(tileClosestPoint.x - tileMin.x);

                float adjustment = compare ? -renderDistance: renderDistance;
                //count exists to keep each objects original positions relative to each other
                //update the position of the thing
                //preserve z
                //changing dependent on the cameras position
            tiles[i].position = 
                new Vector3(camX + adjustment, tiles[i].position.y, tiles[i].position.z); 
            }
        }

    }
}
