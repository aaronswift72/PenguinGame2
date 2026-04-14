using TMPro;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.InputSystem;

public class PenguinController : MonoBehaviour
{
    public float speed = 5f;
    public float smoothTime = 1f;

    public float diveForceMultiplier = 2.5f;
    public float diveAcceleration = 20f;
    public float swimForce = 5f;
    public float swimSpeed = 3f;

    public Rigidbody2D rb;
    private float xVelocityRef = 0;
    private float airTime;
    private float fishCount = 0;
    public TextMeshProUGUI fishCounter;
    public TextMeshProUGUI timer;
    public GameObject panel;

    public enum PenguinState {Sliding, Swimming, Gliding}
    public PenguinState currentState = PenguinState.Sliding;
    private Animator animator;
    public AudioSource splash;
    public AudioSource swim;
    public AudioSource exitWater;
    public AudioSource iceSkating;
    private bool gameStarted = false;
    private bool hasFinished = false;
    public GameObject youWinScreen;
    void Start()
    {
        airTime = Time.time;
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    void FixedUpdate()
    {
        if (currentState == PenguinState.Sliding && Time.time - airTime > 0.5f)
        {
            currentState = PenguinState.Gliding;
            animator.SetBool("isGliding", true);
            iceSkating.Stop();
        }

        if (currentState == PenguinState.Sliding)
        {
            HandleSliding();
        }
        else if (currentState == PenguinState.Swimming)
        {
            HandleSwimming();
        }
        else
        {
            HandleGliding();
        }
    }
    
    //Called when in land
    void HandleSliding()
    {
        float smoothedX = Mathf.SmoothDamp(rb.linearVelocity.x, speed, ref xVelocityRef, smoothTime);
        rb.linearVelocity = new Vector2(smoothedX, rb.linearVelocity.y);
        //Vector2 currentVelocity = rb.linearVelocity;
        //Vector2 targetVelocity = new Vector2(speed, currentVelocity.y);

        if (Keyboard.current.spaceKey.isPressed)
        {
            float downwardBoost = Mathf.Abs(rb.linearVelocity.x) * diveForceMultiplier;
            rb.AddForce(Vector2.down * downwardBoost * diveAcceleration * Time.fixedDeltaTime, ForceMode2D.Force);
        }
    }

    //Called when swimming
    void HandleSwimming()
    {
        //Makes penguin float
        rb.AddForce(Vector2.up * Physics2D.gravity.magnitude * rb.mass, ForceMode2D.Force);

        //Slows horizontal movement
        rb.linearVelocity = new Vector2(Mathf.Lerp(rb.linearVelocity.x, swimSpeed, Time.fixedDeltaTime * 3f), rb.linearVelocity.y);

        //Space to swim up
        if(Keyboard.current.spaceKey.isPressed)
        {
            rb.AddForce(Vector2.up * swimForce, ForceMode2D.Force);
        }
    }

    // Called when gliding
    void HandleGliding()
    {
        //Space to descend
        if(Keyboard.current.spaceKey.isPressed)
        {
            rb.AddForce(Vector2.down * swimForce, ForceMode2D.Force);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Water"))
        {
            currentState = PenguinState.Swimming;
            animator.SetBool("isSwimming", true);
            animator.SetBool("isGliding", false);
            iceSkating.Stop();
            splash.Play();
            swim.Play();
        }
        else if (other.CompareTag("Finish"))
        {
            EndGame();
        }
        else if (other.CompareTag("Fish"))
        {
            other.GetComponent<Animator>().SetTrigger("fishCollect");
            fishCount++;
            fishCounter.SetText("Fish: " + fishCount);
        }
    }
    
    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Water"))
        {
            currentState = PenguinState.Sliding;
            animator.SetBool("isSwimming", false);
            splash.Stop();
            swim.Stop();
            exitWater.Play();
        }
    }
    void OnCollisionEnter2D(Collision2D collision)
    {
        // if penguin lands on ice
        if (collision.gameObject.CompareTag("Ice") && currentState == PenguinState.Gliding)
        {
            // calculate landing angle
            Vector2 velocity = GetComponent<Rigidbody2D>().linearVelocity;
            Vector2 normal = collision.GetContact(0).normal;
            float impactAngle = Vector2.Angle(velocity, -normal);
            
            // for debug
            print("Impact angle: " + impactAngle);

            // play different animations based on success of landing
            animator.SetBool("isGliding", false);
            currentState = PenguinState.Sliding;
        }
        if(collision.gameObject.CompareTag("Ice") && hasFinished)
        {
            rb.bodyType = RigidbodyType2D.Static;
            animator.SetTrigger("hasWon");
        }
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ice"))
        {
            airTime = Time.time;

            if(currentState == PenguinState.Sliding && !iceSkating.isPlaying && gameStarted)
            {
                iceSkating.Play();
            }
        }
    }

    public void StartGame()
    {
        gameStarted = true;
    }
    public void EndGame()
    {
        //Stops horizontal movement, audio, and shows win screen (when created and hooked up))
        hasFinished = true;
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y); //Leaves vertical movment free so penguin hits ground
        iceSkating.Stop();
        swim.Stop();
        GameObject stats = youWinScreen.transform.Find("Stats text").gameObject;
        stats.GetComponent<TextMeshProUGUI>().SetText("You caught " + fishCount + " fish\nin " + timer.gameObject.GetComponent<TextMeshProUGUI>().text);
        timer.GetComponent<TimerBehavior>().stopped = true;
        fishCounter.gameObject.SetActive(false);
        timer.gameObject.SetActive(false);
        panel.gameObject.SetActive(false);
        youWinScreen.SetActive(true);
    }
}
