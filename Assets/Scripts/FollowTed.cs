using UnityEngine;

public class FollowTed : MonoBehaviour
{
    [Header("Drag Settings")]
    [SerializeField] private KeyCode dragKey = KeyCode.LeftShift; // Changed from Space since Space is used for interaction
    [SerializeField] private float dragDistance = 1.5f; // Distance body stays behind player
    [SerializeField] private float dragSpeed = 0.8f; // Multiplier for player speed while dragging
    [SerializeField] private float pickupRadius = 2f; // How close player needs to be to pick up a body

    [Header("References")]
    [SerializeField] private Transform player; // Reference to Ted

    private GameObject currentFollow;
    private bool isDragging = false;
    private Vector3 bodyTargetPosition;
    private Rigidbody2D playerRb;
    private float originalPlayerSpeed;
    private TopDownMovement topDownMovement; // Your existing movement script

    public bool isGuiding;

    void Start()
    {
        if (player == null)
        {
            player = transform; // Default to this GameObject if not assigned
        }

        playerRb = player.GetComponent<Rigidbody2D>();
        topDownMovement = player.GetComponent<TopDownMovement>();
        if (topDownMovement != null)
        {
            originalPlayerSpeed = topDownMovement.moveSpeed;
        }

        isGuiding = false;
    }

    void Update()
    {
        HandleDragInput();

        // if (isDragging && currentFollow != null)
        // {
        //     UpdateFollowPosition();
        // }

        // if (isDragging == false && isGuidingCustomer)
        // {
            
        // }

        if (currentFollow != null)
        {
            UpdateFollowPosition();
        }

    }

    void HandleDragInput()
    {
        // Start dragging when key is first pressed
        if (Input.GetKeyDown(dragKey) && !isDragging && CanStartDragging())
        {
            StartDragging();
        }

        // Stop dragging when key is released
        if (Input.GetKeyUp(dragKey) && isDragging)
        {
            StopDragging();
        }

        // Also stop if key is not being held
        if (!Input.GetKey(dragKey) && isDragging)
        {
            StopDragging();
        }
    }

    bool CanStartDragging()
    {
        if (isGuiding)
        {
            return false;
        }
        // Check if there's a body nearby to drag
        Collider2D[] nearbyBodies = Physics2D.OverlapCircleAll(player.position, pickupRadius);
        foreach (Collider2D col in nearbyBodies)
        {
            if (col.CompareTag("Body"))
            {
                currentFollow = col.gameObject;
                return true;
            }
        }

        // No bodies found nearby
        return false;
    }

    public void SetIsGuiding(GameObject customer)
    {
        // you are holding a dead body, do not guide a customer rn
        if (isDragging == true)
        {
            return;
        }
        isGuiding = true;
        currentFollow = customer;
        customer.GetComponent<BoxCollider2D>().enabled = false;  // set back to true once guided
    }

    void StartDragging()
    {
        isDragging = true;

        // Slow down player while dragging
        if (topDownMovement != null)
        {
            topDownMovement.moveSpeed = originalPlayerSpeed * dragSpeed;
        }

        // Notify vision cone system that player is now suspicious
        MusicManager.instance.SceneMusic(3);  //hardcoded oops
        VisionConeManager.Instance?.SetPlayerDragging(true);
    }

    void StopDragging()
    {
        isDragging = false;

        // Restore player speed
        if (topDownMovement != null)
        {
            topDownMovement.moveSpeed = originalPlayerSpeed;
        }

        // Notify vision cone system
        MusicManager.instance.SceneMusic(2);  //hardcoded oops
        VisionConeManager.Instance?.SetPlayerDragging(false);

        currentFollow = null;
    }

    void UpdateFollowPosition()
    {
        if (currentFollow == null)
        {
            Debug.LogWarning("Nobody following rn");
            return;
        }

        // Get movement direction from TopDownMovement component
        Vector3 movementDir = Vector3.zero;

        if (topDownMovement != null)
        {
            movementDir = topDownMovement.currDirection.normalized;
        }

        // Fallback: use velocity if TopDownMovement direction isn't available
        if (movementDir.magnitude < 0.1f && playerRb != null)
        {
            movementDir = playerRb.linearVelocity.normalized;
        }

        // Calculate target position behind player
        if (movementDir.magnitude > 0.1f)
        {
            // Body follows behind the player's movement direction
            bodyTargetPosition = player.position - movementDir * dragDistance;
        }
        else
        {
            // If not moving, position body slightly behind player's last known direction
            bodyTargetPosition = player.position - topDownMovement.currDirection.normalized * dragDistance;
        }

        // Move body to target position (faster lerp for more responsive feel)
        currentFollow.transform.position = Vector3.Lerp(
            currentFollow.transform.position,
            bodyTargetPosition,
            Time.deltaTime * 15f
        );
    }

    public void StopFollow()
    {
        Debug.Log("You are now stopping follow");
        currentFollow.GetComponent<BoxCollider2D>().enabled = true; 
        currentFollow = null;
        isGuiding=false;
    }

    // void UpdateBodyPosition()
    // {
    //     if (currentBody == null)
    //     {
    //         Debug.LogWarning("UpdateBodyPosition called but currentBody is null!");
    //         return;
    //     }

    //     // Get movement direction from TopDownMovement component
    //     Vector3 movementDir = Vector3.zero;

    //     if (topDownMovement != null)
    //     {
    //         movementDir = topDownMovement.currDirection.normalized;
    //     }

    //     // Fallback: use velocity if TopDownMovement direction isn't available
    //     if (movementDir.magnitude < 0.1f && playerRb != null)
    //     {
    //         movementDir = playerRb.linearVelocity.normalized;
    //     }

    //     // Calculate target position behind player
    //     if (movementDir.magnitude > 0.1f)
    //     {
    //         // Body follows behind the player's movement direction
    //         bodyTargetPosition = player.position - movementDir * dragDistance;
    //     }
    //     else
    //     {
    //         // If not moving, position body slightly behind player's last known direction
    //         bodyTargetPosition = player.position - topDownMovement.currDirection.normalized * dragDistance;
    //     }

    //     // Move body to target position (faster lerp for more responsive feel)
    //     currentBody.transform.position = Vector3.Lerp(
    //         currentBody.transform.position,
    //         bodyTargetPosition,
    //         Time.deltaTime * 15f
    //     );
    // }

    public bool IsDragging()
    {
        return isDragging;
    }

    public bool GetIsGuiding()
    {
        return isGuiding;
    }

    public GameObject GetCurrentFollow()
    {
        return currentFollow;
    }

    void OnDrawGizmosSelected()
    {
        if (player != null)
        {
            // Visualize drag detection range
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(player.position, pickupRadius);
        }
    }
}