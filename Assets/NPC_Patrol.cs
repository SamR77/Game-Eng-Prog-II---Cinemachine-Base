using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class NPCPatrol : MonoBehaviour
{
    [Header("Patrol Settings")]
    public Transform[] patrolWaypoints;
    [Tooltip("Move speed of the NPC in m/s")]
    public float moveSpeed = 2.0f;
    [Tooltip("Rotation speed when turning towards target")] 
    public float rotationSpeed = 5f;
    [Tooltip("Distance to waypoint considered 'reached'")]
    public float reachThreshold = 0.2f;

    [Header("Animation")]
    [SerializeField] private Animator _animator;

    // Animator parameter hashes
    private int _animIDSpeed;
    private int _animIDMotionSpeed;

    private CharacterController _controller;
    private int _currentWaypointIndex = 0;
    private Transform _currentTarget;

    private void Start()
    {
        _controller = GetComponent<CharacterController>();

        // Check if waypoints are assigned
        if (patrolWaypoints == null || patrolWaypoints.Length == 0)
        {
            Debug.LogWarning("No patrol waypoints assigned to NPCPatrol.");
            return;
        }

        _currentTarget = patrolWaypoints[_currentWaypointIndex];

        // Cache Animator parameter hashes for efficiency
        if (_animator != null)
        {
            _animIDSpeed = Animator.StringToHash("Speed");
            _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
        }
    }

    private void Update()
    {
        if (patrolWaypoints == null || patrolWaypoints.Length == 0) return;

        MoveTowardsTarget();
        CheckWaypointDistance();
    }

    /// <summary>
    /// Moves the NPC towards the current target waypoint and rotates smoothly.
    /// </summary>
    private void MoveTowardsTarget()
    {
        // Calculate horizontal direction to target
        Vector3 direction = (_currentTarget.position - transform.position);
        direction.y = 0f;

        // Rotate smoothly towards target
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        // Move forward using CharacterController
        _controller.Move(transform.forward * moveSpeed * Time.deltaTime);

        // Update animator if assigned
        if (_animator != null)
        {
            _animator.SetFloat(_animIDSpeed, moveSpeed);
            _animator.SetFloat(_animIDMotionSpeed, 1f);
        }
    }

    /// <summary>
    /// Check if NPC reached the current waypoint and switch to the next.
    /// </summary>
    private void CheckWaypointDistance()
    {
        if (Vector3.Distance(transform.position, _currentTarget.position) < reachThreshold)
        {
            _currentWaypointIndex = (_currentWaypointIndex + 1) % patrolWaypoints.Length;
            _currentTarget = patrolWaypoints[_currentWaypointIndex];
        }
    }

    /// <summary>
    /// Draw waypoints and connecting lines in the editor for visualization.
    /// </summary>
    private void OnDrawGizmos()
    {
        if (patrolWaypoints == null || patrolWaypoints.Length == 0) return;

        Gizmos.color = Color.green;
        foreach (Transform wp in patrolWaypoints)
        {
            if (wp != null)
                Gizmos.DrawSphere(wp.position, 0.2f);
        }

        Gizmos.color = Color.blue;
        for (int i = 0; i < patrolWaypoints.Length; i++)
        {
            Transform next = patrolWaypoints[(i + 1) % patrolWaypoints.Length];
            if (patrolWaypoints[i] != null && next != null)
                Gizmos.DrawLine(patrolWaypoints[i].position, next.position);
        }
    }
}
