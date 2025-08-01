using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMove : MonoBehaviour
{
    public Rigidbody2D rb;
    public float moveSpeed = 5f;
    public Vector2 moveDirection;
    public Animator animator;

    public float xRange = 10f;
    public float yRange = 17f;



    void Update()
    {
        var keyboard = Keyboard.current;
        moveDirection = Vector2.zero;

        if (keyboard != null)
        {
            if (keyboard.wKey.isPressed) moveDirection.y += 1;
            if (keyboard.sKey.isPressed) moveDirection.y -= 1;
            if (keyboard.aKey.isPressed) moveDirection.x -= 1;
            if (keyboard.dKey.isPressed) moveDirection.x += 1;
        }

        if (moveDirection.x != 0)
        {
            Vector3 scale = transform.localScale;
            scale.x = moveDirection.x > 0 ? -0.3f : 0.3f; 
            transform.localScale = scale;
        }

        animator.SetFloat("Horizontal", moveDirection.x);
        animator.SetFloat("Vertical", moveDirection.y);
        animator.SetFloat("Speed", moveDirection.sqrMagnitude);

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {   
            animator.SetTrigger("Attack");
           
        }

        if (Mouse.current.rightButton.wasPressedThisFrame)
        {   
            animator.SetTrigger("Casting");
        }

        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            animator.SetTrigger("Die");
        }

        if (transform.position.x < -xRange || transform.position.x > xRange)
        {
            transform.position = new Vector3(-xRange, transform.position.y, transform.position.z);
        }
        if (transform.position.x > xRange || transform.position.x > xRange)
        {
            transform.position = new Vector3(xRange, transform.position.y, transform.position.z);
        }
        if (transform.position.y < -yRange || transform.position.y > yRange)
        {
            transform.position = new Vector3(transform.position.x, -yRange, transform.position.z);
        }
        if (transform.position.y > yRange || transform.position.y > yRange)
        {
            transform.position = new Vector3(transform.position.x, -yRange, transform.position.z);
        }



    }

    

    private void FixedUpdate()
    {
        rb.MovePosition(rb.position + moveDirection.normalized * moveSpeed * Time.fixedDeltaTime);
    }
}
