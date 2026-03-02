using UnityEngine;

public class FootParticles : MonoBehaviour
{
    public ParticleSystem ps;
    public AlexTopDownMovement movement;
    public float maxEmission = 10;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        var main = ps.main;
        var emission = ps.emission;
        var shape = ps.shape;

        emission.rateOverTime = movement.dir.magnitude * maxEmission;
        shape.rotation = new Vector3(0, 0, Mathf.Atan2(-movement.currDirection.x, movement.currDirection.y) * Mathf.Rad2Deg - 90);
    }
}
