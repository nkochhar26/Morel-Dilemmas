using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;

public class VisionConeManager : MonoBehaviour
{
    public static VisionConeManager Instance { get; private set; }

    [Header("Detection Settings")]
    [SerializeField] private float detectionGracePeriod = 0.5f; // Time before detection triggers penalty
    [SerializeField] private bool showDetectionWarning = true;

    [Header("Star Penalty")]
    [SerializeField] private float starPenaltyAmount = 1f; // How many stars to lose when caught
    [SerializeField] private StarManager starManager; // Reference to your StarManager

    [Header("Events")]
    public UnityEvent OnPlayerCaught; // Fired when player is caught
    public UnityEvent OnDetectionWarning; // Fired when player first enters vision

    private List<VisionCone> activeVisionCones = new List<VisionCone>();
    private bool playerIsDragging = false;
    private bool detectionTimerActive = false;
    private float detectionTimer = 0f;
    private VisionCone currentDetectingCone = null;

    [Header("Invulnerability")]
    [SerializeField] private float postCatchInvulnerability = 2f; // Seconds of invulnerability after being caught
    private float invulnerabilityTimer = 0f;

    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Find StarManager if not assigned
        if (starManager == null)
        {
            starManager = FindFirstObjectByType<StarManager>();
            if (starManager == null)
            {
                Debug.LogWarning("VisionConeManager: No StarManager found in scene!");
            }
        }
    }

    void Update()
    {
        // Handle detection timer
        if (detectionTimerActive)
        {
            detectionTimer += Time.deltaTime;

            if (detectionTimer >= detectionGracePeriod)
            {
                Debug.Log($"[DETECTION] Grace period ended ({detectionTimer:F2}s >= {detectionGracePeriod}s). Calling PlayerCaught()");
                PlayerCaught();
            }

            // Check if player escaped vision
            bool stillDetected = false;
            foreach (VisionCone cone in activeVisionCones)
            {
                if (cone.IsPlayerDetected())
                {
                    stillDetected = true;
                    break;
                }
            }

            if (!stillDetected)
            {
                // Player escaped!
                detectionTimerActive = false;
                detectionTimer = 0f;
                currentDetectingCone = null;
                Debug.Log("[DETECTION] Player escaped detection!");
            }
        }
    }

    public void RegisterVisionCone(VisionCone cone)
    {
        if (!activeVisionCones.Contains(cone))
        {
            activeVisionCones.Add(cone);
        }
    }

    public void UnregisterVisionCone(VisionCone cone)
    {
        activeVisionCones.Remove(cone);
    }

    public void SetPlayerDragging(bool dragging)
    {
        playerIsDragging = dragging;

        if (!dragging)
        {
            // Reset detection if player stops dragging
            detectionTimerActive = false;
            detectionTimer = 0f;
            currentDetectingCone = null;
        }
    }

    public bool IsPlayerDragging()
    {
        return playerIsDragging;
    }

    public void OnPlayerSpotted(VisionCone cone)
    {
        // Don't start new detection during invulnerability period
        if (Time.time < invulnerabilityTimer)
        {
            Debug.Log($"[DETECTION BLOCKED] Still invulnerable! {invulnerabilityTimer - Time.time:F1}s remaining");
            return;
        }

        if (!detectionTimerActive)
        {
            // First time being spotted
            detectionTimerActive = true;
            detectionTimer = 0f;
            currentDetectingCone = cone;

            Debug.Log($"[DETECTION STARTED] Player spotted by {cone.gameObject.name}! Grace period: {detectionGracePeriod}s");

            if (showDetectionWarning)
            {
                OnDetectionWarning?.Invoke();
                Debug.Log("WARNING: Player has been spotted! Escape quickly!");
            }
        }
        else
        {
            Debug.Log($"[DETECTION] Already detecting (timer: {detectionTimer:F2}s)");
        }
    }

    void PlayerCaught()
    {
        detectionTimerActive = false;

        // Decrease star rating
        if (starManager != null)
        {
            float beforeValue = starManager.GetStarValue();
            starManager.DecreaseStarValue(starPenaltyAmount);
            float afterValue = starManager.GetStarValue();
            Debug.Log($"CAUGHT! Star value: {beforeValue} → {afterValue} (lost {beforeValue - afterValue})");
        }
        else
        {
            Debug.LogWarning("Player caught but no StarManager assigned!");
        }

        // Set invulnerability period
        invulnerabilityTimer = Time.time + postCatchInvulnerability;
        Debug.Log($"Invulnerable for {postCatchInvulnerability} seconds");

        OnPlayerCaught?.Invoke();

        // can add more here like spawn angry customer reaction
    }

    public float GetDetectionProgress()
    {
        return detectionTimerActive ? (detectionTimer / detectionGracePeriod) : 0f;
    }
}