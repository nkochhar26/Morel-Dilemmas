using UnityEngine;

public class VisionCone : MonoBehaviour
{
    [Header("Vision Cone Settings")]
    [SerializeField] private float visionRange = 5f;
    [SerializeField] private float visionAngle = 60f; // Total angle of cone
    [SerializeField] private int rayCount = 10; // Number of rays to cast for cone
    [SerializeField] private LayerMask obstacleLayers; // Walls, tables, etc.
    [SerializeField] private LayerMask playerLayer;

    [Header("Visual Settings")]
    [SerializeField] private Color normalColor = new Color(1f, 1f, 0f, 0.3f); // Yellow, transparent
    [SerializeField] private Color alertColor = new Color(1f, 0f, 0f, 0.5f); // Red when player detected
    [SerializeField] private Material visionConeMaterial;

    [Header("Rotation")]
    [SerializeField] private bool rotates = false;
    [SerializeField] private float rotationSpeed = 30f; // Degrees per second
    [SerializeField] private float rotationRange = 90f; // How far to rotate left/right

    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private Mesh visionConeMesh;
    private float currentRotation = 0f;
    private float rotationDirection = 1f;
    private bool playerDetected = false;
    private bool playerDetectedLastFrame = false; // Track previous frame to stabilize detection

    void Start()
    {
        // Create mesh components for vision cone visualization
        meshFilter = gameObject.AddComponent<MeshFilter>();
        meshRenderer = gameObject.AddComponent<MeshRenderer>();

        visionConeMesh = new Mesh();
        meshFilter.mesh = visionConeMesh;

        // Set material
        if (visionConeMaterial != null)
        {
            meshRenderer.material = visionConeMaterial;
        }
        else
        {
            // Create default material if none assigned
            meshRenderer.material = new Material(Shader.Find("Sprites/Default"));
            meshRenderer.material.color = normalColor;
        }

        // Make sure it renders correctly
        meshRenderer.sortingLayerName = "Default";
        meshRenderer.sortingOrder = 5; // Above floor, below UI

        // Reset local scale to 1 to ignore parent scaling
        transform.localScale = Vector3.one;

        // Register with manager
        if (VisionConeManager.Instance != null)
        {
            VisionConeManager.Instance.RegisterVisionCone(this);
        }
        else
        {
            Debug.LogWarning($"VisionCone on {gameObject.name}: No VisionConeManager found in scene! Please create a GameObject with VisionConeManager component.");
        }
    }

    void Update()
    {
        if (rotates)
        {
            RotateVisionCone();
        }

        // Counteract parent scaling to keep vision cone size consistent
        if (transform.parent != null)
        {
            transform.localScale = new Vector3(
                1.0f / transform.parent.lossyScale.x,
                1.0f / transform.parent.lossyScale.y,
                1.0f / transform.parent.lossyScale.z
            );
        }

        DetectPlayer();

        // Only draw vision cone when player is dragging
        if (VisionConeManager.Instance != null && VisionConeManager.Instance.IsPlayerDragging())
        {
            DrawVisionCone();
            meshRenderer.enabled = true;
        }
        else
        {
            meshRenderer.enabled = false;
        }
    }

    void RotateVisionCone()
    {
        currentRotation += rotationSpeed * rotationDirection * Time.deltaTime;

        if (Mathf.Abs(currentRotation) > rotationRange / 2f)
        {
            rotationDirection *= -1;
            currentRotation = Mathf.Clamp(currentRotation, -rotationRange / 2f, rotationRange / 2f);
        }
    }

    void DetectPlayer()
    {
        playerDetected = false;

        // Only detect if player is dragging a body
        if (VisionConeManager.Instance == null || !VisionConeManager.Instance.IsPlayerDragging())
        {
            UpdateVisionConeColor(false);
            playerDetectedLastFrame = false;
            return;
        }

        // Cast rays in cone pattern
        float angleStep = visionAngle / rayCount;
        float startAngle = -visionAngle / 2f;

        for (int i = 0; i <= rayCount; i++)
        {
            // Calculate angle in world space
            float localAngle = startAngle + (angleStep * i) + currentRotation;
            float worldAngle = localAngle + transform.eulerAngles.z;

            Vector2 direction = new Vector2(
                Mathf.Cos(worldAngle * Mathf.Deg2Rad),
                Mathf.Sin(worldAngle * Mathf.Deg2Rad)
            );

            RaycastHit2D hit = Physics2D.Raycast(
                transform.position,
                direction,
                visionRange,
                playerLayer | obstacleLayers
            );

            if (hit.collider != null)
            {
                // Check if we hit the player layer
                if (((1 << hit.collider.gameObject.layer) & playerLayer) != 0)
                {
                    // Player detected!
                    playerDetected = true;

                    // Only call OnPlayerDetected when first spotted (not every frame)
                    if (!playerDetectedLastFrame)
                    {
                        Debug.Log($"{gameObject.name} DETECTED PLAYER at {hit.point}!");
                        OnPlayerDetected();
                    }
                    break;
                }
            }
        }

        UpdateVisionConeColor(playerDetected);
        playerDetectedLastFrame = playerDetected;
    }

    void OnPlayerDetected()
    {
        if (VisionConeManager.Instance != null)
        {
            VisionConeManager.Instance.OnPlayerSpotted(this);
        }
        Debug.Log($"Player spotted by {gameObject.name}!");
    }

    void UpdateVisionConeColor(bool detected)
    {
        Color targetColor = detected ? alertColor : normalColor;
        // Instant color change instead of lerp to prevent blinking
        meshRenderer.material.color = targetColor;
    }

    void DrawVisionCone()
    {
        int vertexCount = rayCount + 2;
        Vector3[] vertices = new Vector3[vertexCount];
        int[] triangles = new int[rayCount * 3];

        vertices[0] = Vector3.zero; // Origin point

        float angleStep = visionAngle / rayCount;
        float startAngle = -visionAngle / 2f;

        // Get world rotation to match raycasts
        float worldRotation = transform.eulerAngles.z;

        for (int i = 0; i <= rayCount; i++)
        {
            // Calculate angle in local space (mesh is in local coordinates)
            float localAngle = startAngle + (angleStep * i) + currentRotation;

            // For mesh vertices, use local angle (don't add worldRotation)
            // The mesh rotates with the transform automatically
            Vector2 direction = new Vector2(
                Mathf.Cos(localAngle * Mathf.Deg2Rad),
                Mathf.Sin(localAngle * Mathf.Deg2Rad)
            );

            // For raycasts, we need world angle
            float worldAngle = localAngle + worldRotation;
            Vector2 worldDirection = new Vector2(
                Mathf.Cos(worldAngle * Mathf.Deg2Rad),
                Mathf.Sin(worldAngle * Mathf.Deg2Rad)
            );

            // Check for obstacles using world direction
            RaycastHit2D hit = Physics2D.Raycast(
                transform.position,
                worldDirection,
                visionRange,
                obstacleLayers
            );

            float distance = hit.collider != null ? hit.distance : visionRange;
            vertices[i + 1] = direction * distance;

            if (i < rayCount)
            {
                triangles[i * 3] = 0;
                triangles[i * 3 + 1] = i + 1;
                triangles[i * 3 + 2] = i + 2;
            }
        }

        visionConeMesh.Clear();
        visionConeMesh.vertices = vertices;
        visionConeMesh.triangles = triangles;
    }

    void OnDestroy()
    {
        VisionConeManager.Instance?.UnregisterVisionCone(this);
    }

    // Called when inspector values change in editor
    void OnValidate()
    {
        // Ensure values stay in reasonable ranges
        visionRange = Mathf.Max(0.1f, visionRange);
        visionAngle = Mathf.Clamp(visionAngle, 1f, 360f);
        rayCount = Mathf.Max(3, rayCount);

        // Update the mesh if it exists
        if (Application.isPlaying && visionConeMesh != null)
        {
            DrawVisionCone();
        }
    }

    // Public getter for detection state
    public bool IsPlayerDetected()
    {
        return playerDetected;
    }
}