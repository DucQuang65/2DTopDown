using UnityEngine;

public class Archer : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private GameObject arrowPrefab;
    [SerializeField] private float attackInterval = 1f;
    [SerializeField] private float arrowSpeed = 5f;
    [SerializeField] private float arrowDamage = 1f;
    [SerializeField] private Transform bow;
    [SerializeField] private float attackRange = 10f;
    private float attackTimer = 0f;
    private AudioSource audioSource;
    private static readonly int IsAttacking = Animator.StringToHash("IsAttacking");
    private Vector3 originalScale;

    public void Setup(int towerLevel)
    {
        originalScale = transform.localScale;

        // Reset rotation
        transform.rotation = Quaternion.Euler(0, 0, 0);
        if (bow != null)
            bow.rotation = Quaternion.Euler(0, 0, 0);
        int multiplier = towerLevel / 2; // Mỗi 2 cấp mới tăng 1 lần
        // Tính toán arrowDamage và arrowSpeed theo level
        arrowDamage = arrowDamage + multiplier * 0.5f;   // Ví dụ: tăng 0.5 dmg mỗi cấp
        arrowSpeed = arrowSpeed + multiplier * 0.2f;
    }

    private void Start()
    {
        Debug.Log("Attack method chạy");
        audioSource = GetComponent<AudioSource>(); // Get audio component.
        if (animator == null) Debug.LogError("Animator chưa gán cho Archer!");
        if (arrowPrefab == null) Debug.LogError("Arrow Prefab chưa gán!");
        if (bow == null) Debug.LogWarning("Bow chưa gán! Using archer position for arrows."); // Warning instead of error for fallback.
    }

    private void Update()
    {
        attackTimer += Time.deltaTime; // Increment timer using Time.deltaTime (UnityEngine.Time for frame-independent timing).
        if (attackTimer >= attackInterval)
        {
            Debug.Log("🟢 Attack method chạy");
            Attack(); // Attempt attack.
            attackTimer = 0f; // Reset timer.
        }
        else
        {
            animator.SetBool(IsAttacking, false); // Idle animation when not attacking.
        }
    }

    private void Attack()
    {
        Debug.Log("Attack method chạy");

        if (arrowPrefab == null)
        {
            Debug.LogError("arrowPrefab is null!");
            return;
        }
        GameObject nearestEnemy = FindNearestEnemy(); // Find target.
        if (nearestEnemy == null)
        {
            Debug.LogWarning("❗ Không tìm thấy enemy nào trong range!");
            return;
        }
        if (nearestEnemy != null)
        {
            animator.SetBool(IsAttacking, true); // Trigger attack animation.

            Vector3 spawnPosition = bow != null ? bow.position : transform.position; // Spawn at bow or fallback to archer.
            Vector2 direction = ((Vector2)nearestEnemy.transform.position - (Vector2)spawnPosition).normalized; // Normalize direction vector.
            if (direction.x < 0)
                transform.localScale = new Vector3(-originalScale.x, originalScale.y, originalScale.z); // Flip left.
            else
                transform.localScale = new Vector3(originalScale.x, originalScale.y, originalScale.z); // Flip right.
            if (bow != null)
                bow.right = direction; // Align bow to direction.
            if (arrowPrefab == null)
            {
                Debug.LogError("arrowPrefab is null!");
                return;
            }
            GameObject arrow = Instantiate(arrowPrefab, spawnPosition, Quaternion.identity); // Spawn arrow.
            if (arrow == null)
            {
                Debug.LogError("Instantiate trả về null!!!");
            }
            else
            {
                Debug.Log($"Arrow được tạo: name={arrow.name}, active={arrow.activeSelf}");
            }
            arrow.transform.right = direction; // Align arrow.

            Bullet arrowScript = arrow.GetComponent<Bullet>(); // Get Bullet script.
            if (arrowScript != null)
            {
                arrowScript.Setup(direction, arrowSpeed, arrowDamage); // Initialize bullet.
            }
            else
            {
                Debug.LogWarning("Không tìm thấy script Bullet trên arrow prefab!");
            }
            if (audioSource != null) audioSource.Play(); // Play sound.
        }
        else
        {
            animator.SetBool(IsAttacking, false); // No enemy, idle.
        }
    }

    private GameObject FindNearestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy"); // Find all enemies by tag (UnityEngine.GameObject for scene queries).
        GameObject nearest = null;
        float minDistance = float.MaxValue; // Use MaxValue for initial comparison.

        foreach (GameObject enemy in enemies)
        {
            float distance = Vector2.Distance(transform.position, enemy.transform.position);
            if (distance < minDistance && distance <= attackRange) // Enhanced: Only consider if within range.
            {
                minDistance = distance;
                nearest = enemy;
            }
        }
        return nearest; // Return nearest within range or null.
    }
}