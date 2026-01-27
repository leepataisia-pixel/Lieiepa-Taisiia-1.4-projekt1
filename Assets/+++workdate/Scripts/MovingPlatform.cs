using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class MovingPlatform2D : MonoBehaviour
{
    [Header("Path")]
    [SerializeField] private Transform pointA;
    [SerializeField] private Transform pointB;

    [Header("Motion")]
    [SerializeField] private float speed = 2f;
    [SerializeField] private float waitTimeAtEnds = 0.2f;

    private Rigidbody2D rb;
    private Vector2 target;
    private float waitTimer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
    }

    private void Start()
    {
        if (!pointA || !pointB)
        {
            Debug.LogError("Assign pointA and pointB in Inspector.");
            enabled = false;
            return;
        }

        rb.position = pointA.position;
        target = pointB.position;
    }

    private void FixedUpdate()
    {
        if (waitTimer > 0f)
        {
            waitTimer -= Time.fixedDeltaTime;
            return;
        }

        Vector2 newPos = Vector2.MoveTowards(rb.position, target, speed * Time.fixedDeltaTime);
        rb.MovePosition(newPos);

        if (Vector2.Distance(rb.position, target) < 0.02f)
        {
            target = (target == (Vector2)pointA.position) ? (Vector2)pointB.position : (Vector2)pointA.position;
            waitTimer = waitTimeAtEnds;
        }
    }
}