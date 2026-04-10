using UnityEngine;

public class SharkPatrol : MonoBehaviour
{
    public float speed = 3f;
    public float patrolDistance = 5f; // distance left and right from start

    private bool movingRight = true;
    private float leftBound;
    private float rightBound;
    private SpriteRenderer sr;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();

        // Set bounds relative to wherever he's placed
        leftBound = transform.position.x - patrolDistance;
        rightBound = transform.position.x + patrolDistance;
    }

    void Update()
    {
        float direction = movingRight ? 1f : -1f;
        transform.Translate(Vector2.right * speed * direction * Time.deltaTime);

        if (transform.position.x >= rightBound && movingRight)
        {
            movingRight = false;
            sr.flipX = false;
        }
        else if (transform.position.x <= leftBound && !movingRight)
        {
            movingRight = true;
            sr.flipX = true;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Handle player collision (e.g., damage, game over)
            Debug.Log("Shark hit the player!");
        }
    }
}