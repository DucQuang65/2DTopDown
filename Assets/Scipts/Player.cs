using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;// Speed of the player movement
    private Rigidbody2D rb; // Component for physics interactions
    private SpriteRenderer spriteRenderer; // For rendering the player sprite
    private Animator animator;
    private void Awake()
    {
        // Initialize the Rigidbody2D component
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>(); // GetComponent attach directly into player
        animator = GetComponent<Animator>(); // Get the Animator component for animations
    }
    void Start()
    {
        
    }

    void Update()
    {
        MoverPlayer(); // Call the method to move the player based on input
    }
    void MoverPlayer()
    {   // Get input from keyboard or controller
        Vector2 playerInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")); 
        rb.linearVelocity = playerInput.normalized * moveSpeed; // Set the velocity based on input and speed

        // Flip base on input direction
        if (playerInput.x < 0)
        {
            spriteRenderer.flipX = true;
        }
        else if (playerInput.x > 0)
        {
            spriteRenderer.flipX = false;
        }
        if (playerInput != Vector2.zero){
            animator.SetBool("IsRun", true); // Set the animation state to moving
        }
        else { 
            animator.SetBool("IsRun", false); // Set the animation state to idle
        }
    }
}