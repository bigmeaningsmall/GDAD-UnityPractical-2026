using UnityEngine;
using UnityEngine.Splines;

[RequireComponent(typeof(SplineContainer))]
public class SplineFollower : MonoBehaviour
{
    [Header("Closed Spline Behavior")]
    [Tooltip("When enabled, target will be directly followed inside closed splines without tracking the spline")]
    public bool directFollowInsideClosedSpline = false;

    [Tooltip("When enabled, follower will not track the target outside of closed splines")]
    public bool onlyTrackInsideClosedSpline = false;

    [Header("Tracking Settings")]
    [Tooltip("Maximum distance at which the follower will track along the spline")]
    [Range(1f, 50f)]
    public float proximityThreshold = 10f;

    [Tooltip("How quickly the follower object moves along the spline")]
    [Range(0.1f, 10f)]
    public float movementSpeed = 5f;

    [Tooltip("Smoothing factor for follower movement (higher = smoother but less responsive)")]
    [Range(0f, 0.95f)]
    public float smoothing = 0.7f;

    [Header("Branch Stickiness")]
    [Tooltip("When two parts of the spline are about equally close to the target (a U-bend, a figure-8), the follower " +
             "stays on the part it's already on until the other is closer by more than this fraction. Prevents flip-flopping.")]
    [Range(0.01f, 0.5f)]
    public float equidistantThreshold = 0.05f;

    [Header("Inside a Closed Spline")]
    [Tooltip("Where the follower sits while the target is inside a closed spline (ignored when Direct Follow is on)")]
    public ClosedSplinePositionMode insidePositionMode = ClosedSplinePositionMode.RelativeToTarget;

    [Tooltip("Smoothing factor while inside a closed spline (higher = smoother but less responsive)")]
    [Range(0f, 0.95f)]
    public float transitionSmoothing = 0.7f;

    [Tooltip("Time (seconds) to move from the spline into the closed area when the target enters it")]
    [Range(0.1f, 3.0f)]
    public float transitionTime = 0.5f;

    [Header("Rejoining the Spline (after leaving a closed spline)")]
    [Tooltip("Maximum speed (m/s) the follower closes the gap back onto the spline when the target leaves the inside of " +
             "a closed spline. Lower = gentler, avoiding a sudden loudness jump as the sound moves.")]
    [Range(0.5f, 20f)]
    public float rejoinMaxSpeed = 3f;

    [Tooltip("Roughly how long (seconds) the rejoin eases in and out. Higher = softer start and finish.")]
    [Range(0.05f, 2f)]
    public float rejoinSmoothTime = 0.5f;

    [Header("Position Offset")]
    [Tooltip("Offset from the spline position (local to spline direction)")]
    public Vector3 positionOffset = Vector3.zero;

    [Tooltip("How quickly the offset transitions when changed")]
    [Range(0.1f, 10f)]
    public float offsetTransitionSpeed = 3.0f;

    [Header("Performance")]
    [Tooltip("Target updates per second (0 = every frame)")]
    [Range(0, 60)]
    public float targetUpdatesPerSecond = 0f;

    [Header("References")]
    [Tooltip("The child GameObject that will follow the spline (optional, will be created if not set)")]
    public GameObject followObject;

    [Tooltip("Target transform to follow (optional, will use AudioListener or Camera if not set)")]
    public Transform targetTransform;

    [Header("Sleep Manager")]
    public SplineSleepManager sleepManager = new SplineSleepManager();

    [Header("Debug")]
    public bool showDebugVisuals = true;
    public bool showSamplePoints = false;
    public bool showProximityThreshold = true;
    public bool showSleepBounds = false;
    public bool showClosedSplineArea = true;
    public Color debugColor = Color.green;

    [Tooltip("Number of sample points used for distance checks and shown along the spline")]
    [Range(10, 100)]
    public int splineSampleCount = 20;

    public enum ClosedSplinePositionMode
    {
        RelativeToTarget, // near the target, plus the offset
        FixedOffset       // at the spline's origin, plus the offset
    }

    // Core state
    public enum FollowState
    {
        Normal,           // Following spline normally
        InsideClosed,     // Inside a closed spline
        OutOfRange        // Too far from spline
    }

    private SplineContainer splineContainer;
    private Transform trackingTarget;
    private bool isInitialized = false;

    private FollowState currentState = FollowState.Normal;
    private float currentSplinePosition = 0f;
    private bool hasTrackedPosition = false; // first placement snaps, later moves are smoothed
    private float currentDirection = 1f;     // last movement direction along the spline - debug arrow only
    private float distanceToSpline = float.MaxValue;

    // Entering the inside of a closed spline: a timed ease from where the follower was to the inside position
    private bool entering = false;
    private float enterProgress = 0f;
    private Vector3 enterStartPosition;

    // Leaving the inside of a closed spline: the follower eases from where it was (near the target) onto
    // the spline at the exit point. The remaining gap is held as an offset from the (moving) spline point
    // and smoothed to zero, so the closing speed is capped no matter how fast the target walks on.
    private bool rejoining = false;
    private bool rejoinStarting = false;
    private Vector3 rejoinOffset;
    private Vector3 rejoinVelocity;
    private Vector3 currentOffset = Vector3.zero;

    // Update timing
    private float accumulatedTime = 0f;
    private float updateInterval = 0f;

    private int ClosestPointSamples => Mathf.Max(50, splineSampleCount * 2);

    #region Initialization

    private void Awake()
    {
        splineContainer = GetComponent<SplineContainer>();
        InitializeFollowObject();
        InitializeTarget();
        sleepManager.Initialize(splineContainer);
        UpdateTimingSettings();
    }

    private void Start()
    {
        if (!isInitialized)
        {
            FindDefaultTarget();
        }
    }

    private void InitializeFollowObject()
    {
        if (followObject == null)
        {
            // Try to find existing audio source first
            AudioSource[] childAudioSources = GetComponentsInChildren<AudioSource>();
            if (childAudioSources.Length > 0)
            {
                followObject = childAudioSources[0].gameObject;
            }
            else
            {
                // Create new follower object
                followObject = new GameObject("Spline Follower Object");
                followObject.transform.parent = transform;
                followObject.transform.localPosition = Vector3.zero;
            }
        }
    }

    private void InitializeTarget()
    {
        currentOffset = positionOffset;

        if (targetTransform != null)
        {
            trackingTarget = targetTransform;
            isInitialized = true;
        }
        else
        {
            isInitialized = false;
        }
    }

    private void FindDefaultTarget()
    {
        if (targetTransform != null)
        {
            trackingTarget = targetTransform;
            isInitialized = true;
            return;
        }

        // Try AudioListener first
        AudioListener listener = FindAnyObjectByType<AudioListener>();
        if (listener != null)
        {
            trackingTarget = listener.transform;
            isInitialized = true;
            return;
        }

        // Try main camera
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            trackingTarget = mainCamera.transform;
            isInitialized = true;
            return;
        }

        // Try any camera
#if UNITY_6000_4_OR_NEWER
        Camera[] cameras = FindObjectsByType<Camera>();
#else
        Camera[] cameras = FindObjectsByType<Camera>(FindObjectsSortMode.None);
#endif
        if (cameras.Length > 0)
        {
            trackingTarget = cameras[0].transform;
            isInitialized = true;
            return;
        }

        Debug.LogWarning("SplineFollower: No suitable target found. Please set targetTransform manually or call Initialize().");
    }

    #endregion

    #region Public API

    /// <summary>
    /// Manually initialize the SplineFollower with a specific target
    /// </summary>
    public void Initialize(Transform target)
    {
        if (target == null)
        {
            Debug.LogWarning("SplineFollower: Cannot initialize with null target");
            return;
        }

        trackingTarget = target;
        targetTransform = target;
        isInitialized = true;

        RestartFollowing();
    }

    /// <summary>
    /// Set a new target for the follower to track
    /// </summary>
    public void SetTarget(Transform newTarget)
    {
        if (newTarget == null)
        {
            Debug.LogWarning("SplineFollower: Cannot set null target");
            return;
        }

        trackingTarget = newTarget;
        targetTransform = newTarget;

        if (!isInitialized)
        {
            isInitialized = true;
            RestartFollowing();
        }
    }

    /// <summary>
    /// Snap the follower to the closest point on the spline. Useful after teleporting the target.
    /// </summary>
    public void RestartFollowing()
    {
        if (!isInitialized || trackingTarget == null) return;

        // Find closest point and position follower there immediately
        float closestT = SplineUtilities.FindClosestPoint(splineContainer, trackingTarget.position, ClosestPointSamples);
        currentSplinePosition = closestT;
        hasTrackedPosition = true;
        rejoining = false;
        entering = false;

        Vector3 splinePosition = splineContainer.EvaluatePosition(currentSplinePosition);
        Vector3 finalPosition = SplineUtilities.ApplyOffsetToSplinePosition(
            splineContainer, splinePosition, currentSplinePosition, currentOffset);

        followObject.transform.position = finalPosition;
    }

    public void UpdateTimingSettings()
    {
        updateInterval = (targetUpdatesPerSecond > 0) ? 1f / targetUpdatesPerSecond : 0f;
        accumulatedTime = 0f;
    }

    #endregion

    #region Main Update Loop

    private void LateUpdate()
    {
        if (!isInitialized || trackingTarget == null) return;

        // Handle update timing
        if (targetUpdatesPerSecond > 0)
        {
            accumulatedTime += Time.deltaTime;
            if (accumulatedTime < updateInterval) return;

            // Use multiple of interval to avoid gradual drift
            accumulatedTime -= updateInterval;

            // If frame rate dropped dramatically, prevent multiple catch-up updates
            if (accumulatedTime >= updateInterval)
                accumulatedTime = 0f;
        }

        // Check sleep mode first
        if (sleepManager.HandleSleepMode(trackingTarget)) return;

        // Update state and position
        UpdateFollowState();
        UpdatePosition();
    }

    private void UpdateFollowState()
    {
        FollowState previousState = currentState;
        bool isSplineClosed = splineContainer.Spline != null && splineContainer.Spline.Closed;
        bool isInside = isSplineClosed &&
                        SplineUtilities.IsPointInsideClosedSpline(splineContainer, trackingTarget.position, ClosestPointSamples);

        if (isInside)
        {
            currentState = FollowState.InsideClosed;
        }
        else if (isSplineClosed && onlyTrackInsideClosedSpline)
        {
            currentState = FollowState.OutOfRange;
        }
        else
        {
            distanceToSpline = SplineUtilities.GetDistanceToSpline(splineContainer, trackingTarget.position, splineSampleCount);
            currentState = distanceToSpline <= proximityThreshold ? FollowState.Normal : FollowState.OutOfRange;
        }

        if (currentState == FollowState.InsideClosed && previousState != FollowState.InsideClosed)
            OnEnteredClosedArea();
        else if (previousState == FollowState.InsideClosed && currentState != FollowState.InsideClosed)
            OnExitedClosedArea();
    }

    /// <summary>
    /// The target just entered the inside of a closed spline. Ease from the follower's current position to the
    /// inside position over transitionTime rather than jumping there.
    /// </summary>
    private void OnEnteredClosedArea()
    {
        rejoining = false;
        entering = true;
        enterProgress = 0f;
        enterStartPosition = followObject.transform.position;
    }

    /// <summary>
    /// The target just left the inside of a closed spline. While inside, the follower tracked the target in
    /// world space and its spline position went stale (it still held the spot where the target entered).
    /// Re-seed it at the exit point and glide onto the spline from the follower's current position -
    /// otherwise it snaps back to the entry point and travels round the loop to the target.
    /// </summary>
    private void OnExitedClosedArea()
    {
        currentSplinePosition = SplineUtilities.FindClosestPointNear(splineContainer, trackingTarget.position,
            currentSplinePosition, ClosestPointSamples, equidistantThreshold);
        hasTrackedPosition = true;
        entering = false;

        rejoining = true;
        rejoinStarting = true;   // the gap is measured on the next position update
        rejoinVelocity = Vector3.zero;
    }

    private void UpdatePosition()
    {
        // Update offset
        UpdateOffset();

        switch (currentState)
        {
            case FollowState.Normal:
                UpdateNormalPosition();
                break;

            case FollowState.InsideClosed:
                UpdateInsideClosedPosition();
                break;

            case FollowState.OutOfRange:
                // Follower stays where it is
                break;
        }
    }

    private void UpdateNormalPosition()
    {
        // Find closest point on spline (sticking to the current part when another is about equally close)
        float targetT = SplineUtilities.FindClosestPointNear(splineContainer, trackingTarget.position,
            currentSplinePosition, ClosestPointSamples, equidistantThreshold);

        if (!hasTrackedPosition)
        {
            // First placement: snap to the closest point rather than gliding in from the spline's start
            currentSplinePosition = targetT;
            hasTrackedPosition = true;
        }
        else
        {
            // Smooth movement along spline - always the shortest way round. A plain Lerp on t went the long
            // way whenever the target crossed a closed spline's first knot (where t wraps from 1 back to 0).
            bool closed = splineContainer.Spline.Closed;
            float delta = SplineUtilities.ShortestDelta(currentSplinePosition, targetT, closed);
            if (Mathf.Abs(delta) > 0.0001f) currentDirection = Mathf.Sign(delta);
            currentSplinePosition = SplineUtilities.StepTowards(currentSplinePosition, targetT,
                Time.deltaTime * movementSpeed * (1 - smoothing), closed);
        }

        // Position the follower
        Vector3 splinePosition = splineContainer.EvaluatePosition(currentSplinePosition);
        Vector3 finalPosition = SplineUtilities.ApplyOffsetToSplinePosition(
            splineContainer, splinePosition, currentSplinePosition, currentOffset);

        // Just left the inside of a closed spline - ease onto the spline at a capped speed rather than snapping
        if (rejoining)
        {
            if (rejoinStarting)
            {
                rejoinOffset = followObject.transform.position - finalPosition;
                rejoinStarting = false;
            }

            rejoinOffset = Vector3.SmoothDamp(rejoinOffset, Vector3.zero, ref rejoinVelocity,
                rejoinSmoothTime, rejoinMaxSpeed, Time.deltaTime);

            if (rejoinOffset.sqrMagnitude < 0.0004f) // within 2 cm - hand over to normal following
                rejoining = false;
            else
                finalPosition += rejoinOffset;
        }

        followObject.transform.position = finalPosition;
    }

    private void UpdateInsideClosedPosition()
    {
        // Direct follow tracks the target; otherwise use the configured inside position mode
        Vector3 targetPosition = directFollowInsideClosedSpline
            ? trackingTarget.position + currentOffset
            : GetInsideClosedPosition();

        // Just entered - timed ease toward the inside position (it may follow the target)
        if (entering)
        {
            enterProgress += Time.deltaTime / transitionTime;

            if (enterProgress >= 1f)
            {
                entering = false;
                followObject.transform.position = targetPosition;
            }
            else
            {
                // Ease out quad for a smoother finish
                float t = 1f - Mathf.Pow(1f - enterProgress, 2f);
                followObject.transform.position = Vector3.Lerp(enterStartPosition, targetPosition, t);
            }
            return;
        }

        float followSmoothing = directFollowInsideClosedSpline ? smoothing : transitionSmoothing;
        followObject.transform.position = Vector3.Lerp(
            followObject.transform.position,
            targetPosition,
            Time.deltaTime * movementSpeed * (1 - followSmoothing));
    }

    private Vector3 GetInsideClosedPosition()
    {
        if (insidePositionMode == ClosedSplinePositionMode.RelativeToTarget)
            return trackingTarget.position + currentOffset;

        // Fixed offset from the spline's origin
        return splineContainer.transform.position + currentOffset;
    }

    private void UpdateOffset()
    {
        // Smoothly transition offset changes
        currentOffset = Vector3.Lerp(currentOffset, positionOffset,
            Time.deltaTime * offsetTransitionSpeed);
    }

    #endregion

    #region Debug Visuals

    private void OnDrawGizmosSelected()
    {
        if (!showDebugVisuals) return;

        // Awake hasn't run in edit mode
        if (splineContainer == null) splineContainer = GetComponent<SplineContainer>();
        if (splineContainer == null || splineContainer.Spline == null) return;

        DrawSplineBase();

        if (Application.isPlaying && isInitialized && trackingTarget != null && followObject != null)
        {
            DrawRuntimeDebugVisuals();
        }
        else
        {
            DrawEditorDebugVisuals();
        }
    }

    private void DrawSplineBase()
    {
        // Draw the spline itself
        Gizmos.color = new Color(debugColor.r * 0.8f, debugColor.g * 0.8f, debugColor.b * 0.8f, 0.5f);

        // Show sample points along the spline if enabled
        if (showSamplePoints)
        {
            for (int i = 0; i <= splineSampleCount; i++)
            {
                float t = (float)i / splineSampleCount;
                Vector3 point = splineContainer.EvaluatePosition(t);
                Gizmos.DrawSphere(point, 0.15f);
            }
        }
    }

    private void DrawRuntimeDebugVisuals()
    {
        // Draw proximity threshold first (so it's behind other elements)
        if (showProximityThreshold)
        {
            DrawProximityThreshold();
        }

        // Set color based on current state
        switch (currentState)
        {
            case FollowState.Normal:
                Gizmos.color = Color.green;
                break;
            case FollowState.InsideClosed:
                Gizmos.color = Color.cyan;
                DrawInsideClosedIndicators();
                break;
            case FollowState.OutOfRange:
                Gizmos.color = Color.yellow;
                break;
        }

        // Draw connection between target and follower
        Gizmos.DrawLine(trackingTarget.position, followObject.transform.position);

        // Draw target position
        Gizmos.DrawWireSphere(trackingTarget.position, 0.5f);

        // Draw current position on spline
        Vector3 currentPos = splineContainer.EvaluatePosition(currentSplinePosition);
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(currentPos, 0.3f);

        // Draw follower position
        Gizmos.color = debugColor;
        Gizmos.DrawSphere(followObject.transform.position, 0.25f);

        // Draw line from spline position to follower (showing offset)
        if (Vector3.Distance(currentPos, followObject.transform.position) > 0.1f)
        {
            Gizmos.color = new Color(debugColor.r, debugColor.g, debugColor.b, 0.7f);
            Gizmos.DrawLine(currentPos, followObject.transform.position);
        }

        // Draw tracking direction on closed splines
        if (splineContainer.Spline.Closed && currentState == FollowState.Normal)
        {
            DrawDirectionArrow(currentPos);
        }

        // Draw sleep indicator
        if (sleepManager.IsSleeping)
        {
            DrawSleepIndicator();
        }

        // Draw sleep bounds if enabled
        if (showSleepBounds)
        {
            DrawSleepBounds();
        }

        // Draw proximity information
        DrawProximityIndicators();
    }

    private void DrawEditorDebugVisuals()
    {
        // Show proximity threshold in editor
        if (showProximityThreshold)
        {
            DrawProximityThreshold();
        }

        // Show closed spline area if applicable
        if (showClosedSplineArea && splineContainer.Spline.Closed)
        {
            DrawClosedSplineArea();
        }

        // Show sleep bounds if enabled
        if (showSleepBounds)
        {
            DrawSleepBounds();
        }
    }

    private void DrawInsideClosedIndicators()
    {
        // Draw special indicators when inside closed spline
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(trackingTarget.position, 1.0f);

        // Draw "inside" icon
        Vector3 iconPos = trackingTarget.position + Vector3.up * 2f;
        Gizmos.DrawIcon(iconPos, "d_greenLight", true);

        // Draw the entering transition and its progress
        if (entering)
        {
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.8f);
            Gizmos.DrawLine(enterStartPosition, followObject.transform.position);
            Gizmos.DrawSphere(followObject.transform.position, 0.3f);
        }

        // Draw direct connection line
        Gizmos.color = new Color(0f, 1f, 1f, 0.6f);
        Gizmos.DrawLine(trackingTarget.position, followObject.transform.position);
    }

    private void DrawDirectionArrow(Vector3 currentPos)
    {
        // Draw an arrow indicating the current tracking direction
        float arrowT = Mathf.Repeat(currentSplinePosition + (currentDirection * 0.05f), 1f);
        Vector3 arrowPos = splineContainer.EvaluatePosition(arrowT);

        Gizmos.color = new Color(1f, 0.3f, 0.3f); // Reddish
        Gizmos.DrawLine(currentPos, arrowPos);

        // Draw a small sphere at the arrow tip
        Gizmos.DrawSphere(arrowPos, 0.25f);
    }

    private void DrawSleepIndicator()
    {
        Vector3 pos = transform.position + Vector3.up * 3f;

        // Draw sleep icon
        Gizmos.color = new Color(0.5f, 0.5f, 0.5f, 0.8f);
        Gizmos.DrawIcon(pos, "console.infoicon.sml", true);

        // Draw "ZZZ" effect with spheres
        for (int i = 0; i < 3; i++)
        {
            Vector3 zzz = pos + Vector3.up * (i * 0.5f) + Vector3.right * (i * 0.2f);
            Gizmos.DrawSphere(zzz, 0.1f);
        }
    }

    private void DrawSleepBounds()
    {
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.2f); // Orange, semi-transparent
        Bounds splineBounds = SplineUtilities.GetApproximateSplineBounds(splineContainer);
        splineBounds.Expand(sleepManager.sleepThreshold);
        Gizmos.DrawWireCube(splineBounds.center, splineBounds.size);

        // Add label
        Vector3 labelPos = splineBounds.center + Vector3.up * (splineBounds.size.y * 0.5f + 1f);
        Gizmos.DrawIcon(labelPos, "console.warnicon.sml", true);
    }

    private void DrawProximityThreshold()
    {
        // Make the proximity threshold more visible
        Gizmos.color = new Color(debugColor.r, debugColor.g, debugColor.b, 0.25f);

        // Draw proximity threshold tubes along the spline
        int tubeSamples = 15; // Good balance between detail and performance
        for (int i = 0; i <= tubeSamples; i++)
        {
            float t = (float)i / tubeSamples;
            Vector3 point = splineContainer.EvaluatePosition(t);

            // Draw wire sphere for proximity threshold
            Gizmos.DrawWireSphere(point, proximityThreshold);
        }

        // Draw connecting lines between proximity spheres to create a "tube" effect
        for (int i = 0; i < tubeSamples; i++)
        {
            float t1 = (float)i / tubeSamples;
            float t2 = (float)(i + 1) / tubeSamples;

            Vector3 point1 = splineContainer.EvaluatePosition(t1);
            Vector3 point2 = splineContainer.EvaluatePosition(t2);

            // Draw lines at proximity threshold distance
            Vector3 direction1 = splineContainer.EvaluateTangent(t1);
            Vector3 direction2 = splineContainer.EvaluateTangent(t2);

            // Get perpendicular vectors for "tube" outline
            Vector3 right1 = Vector3.Cross(Vector3.up, direction1).normalized * proximityThreshold;
            Vector3 right2 = Vector3.Cross(Vector3.up, direction2).normalized * proximityThreshold;

            // Draw tube outline
            Gizmos.color = new Color(debugColor.r, debugColor.g, debugColor.b, 0.3f);
            Gizmos.DrawLine(point1 + right1, point2 + right2);
            Gizmos.DrawLine(point1 - right1, point2 - right2);
        }
    }

    private void DrawClosedSplineArea()
    {
        Gizmos.color = new Color(0f, 1f, 1f, 0.1f); // Cyan, very transparent

        // Draw triangular segments to approximate the filled area
        Vector3 center = splineContainer.transform.position;
        int segments = 20;

        for (int i = 0; i < segments; i++)
        {
            float t1 = (float)i / segments;
            float t2 = (float)(i + 1) / segments;

            Vector3 p1 = splineContainer.EvaluatePosition(t1);
            Vector3 p2 = splineContainer.EvaluatePosition(t2);

            // Draw lines to create triangular fill effect
            Gizmos.DrawLine(center, p1);
            Gizmos.DrawLine(center, p2);
            Gizmos.DrawLine(p1, p2);
        }

        // Draw border more prominently
        Gizmos.color = new Color(0f, 1f, 1f, 0.4f);
        for (int i = 0; i < segments; i++)
        {
            float t1 = (float)i / segments;
            float t2 = (float)(i + 1) / segments;

            Vector3 p1 = splineContainer.EvaluatePosition(t1);
            Vector3 p2 = splineContainer.EvaluatePosition(t2);

            Gizmos.DrawLine(p1, p2);
        }
    }

    private void DrawProximityIndicators()
    {
        // Color code based on proximity (distance as of the last update)
        if (distanceToSpline <= proximityThreshold)
        {
            Gizmos.color = Color.green;
        }
        else if (distanceToSpline <= sleepManager.sleepThreshold)
        {
            Gizmos.color = Color.yellow;
        }
        else
        {
            Gizmos.color = Color.red;
        }

        // Draw distance line from target to current point on spline
        Vector3 closestPoint = splineContainer.EvaluatePosition(currentSplinePosition);
        Gizmos.DrawLine(trackingTarget.position, closestPoint);

        // Draw small indicator at closest point
        Gizmos.DrawSphere(closestPoint, 0.2f);
    }

    // Additional utility for showing state information in editor
    private void OnDrawGizmos()
    {
        if (!showDebugVisuals) return;

        // Show basic state even when not selected (less intrusive)
        if (Application.isPlaying && isInitialized && trackingTarget != null && followObject != null)
        {
            // Just show a small connection line
            Gizmos.color = new Color(debugColor.r, debugColor.g, debugColor.b, 0.05f);
            Gizmos.DrawLine(trackingTarget.position, followObject.transform.position);
        }
    }

    #endregion
}
