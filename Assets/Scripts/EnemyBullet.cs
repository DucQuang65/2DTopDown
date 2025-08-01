using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    private Vector3 movementDrirection;
    void Start()
    {
        Destroy(gameObject, 5f); // Destroy the bullet after 5 seconds to prevent memory leaks
    }

    void Update()
    {
        if (movementDrirection == Vector3.zero) return;
        transform.position += movementDrirection * Time.deltaTime;
    }
    public void SetMovementDirection(Vector3 direction)
    {
        movementDrirection = direction;
    }
}
