using UnityEngine;
using TMPro;

[RequireComponent(typeof(Animator), typeof(SoldierStats), typeof(Collider2D))]
public class SoldierController : MonoBehaviour
{
    [Header("Movement & Combat")]
    public float speed = 2f;
    public float chaseRadius = 3f;
    public float attackRange = 0.5f;
    public float attackDamage = 20f;
    public float attackCooldown = 1f;

    [Header("Patrol Settings")]
    public float patrolRadius = 2f;

    [Header("UI Display")]
    public TextMeshProUGUI statText; // <- Gán trong Inspector

    Animator animator;
    public SoldierStats stats; // public để dễ gọi từ bên ngoài nếu cần
    Vector3 homePosition;
    Transform target;
    float lastAttackTime;
    private Vector3 patrolTarget;

    void Awake()
    {
        animator = GetComponent<Animator>();
        stats = GetComponent<SoldierStats>();
        homePosition = transform.position;
        ChooseNewPatrolTarget();

        stats.OnDie += OnSoldierDie;
    }

    void Update()
    {
        if (stats.CurrentHealth <= 0f)
        {
            animator.SetFloat("Speed", 0f);
            return;
        }

        FindTarget();

        if (target != null)
            ChaseAndAttack();
        else
            Patrol();

        float sp = (target != null && Vector2.Distance(transform.position, target.position) > attackRange)
                   ? speed
                   : 0f;
        animator.SetFloat("Speed", sp);

        UpdateStatUI(); // ← Thêm dòng này để cập nhật text mỗi frame
    }

    void OnDestroy()
    {
        if (stats != null)
            stats.OnDie -= OnSoldierDie;
    }

    private void OnSoldierDie()
    {
        Debug.Log($"{gameObject.name} died.");
        animator.SetTrigger("Die");

        var collider = GetComponent<Collider2D>();
        if (collider != null)
            collider.enabled = false;
    }

    void FindTarget()
    {
        target = null;
        float bestDist = chaseRadius;
        var hits = Physics2D.OverlapCircleAll(transform.position, chaseRadius);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Enemy"))
            {
                float d = Vector2.Distance(transform.position, hit.transform.position);
                if (d < bestDist)
                {
                    bestDist = d;
                    target = hit.transform;
                }
            }
        }
    }

    void ChaseAndAttack()
    {
        float distance = Vector2.Distance(transform.position, target.position);
        if (distance > attackRange)
        {
            Vector3 dir = (target.position - transform.position).normalized;
            transform.position += dir * speed * Time.deltaTime;
        }
        else if (Time.time >= lastAttackTime + attackCooldown)
        {
            lastAttackTime = Time.time;
            animator.SetTrigger("Attack");

            var enemy = target.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(attackDamage);
            }
        }
    }

    void Patrol()
    {
        if (Vector2.Distance(transform.position, patrolTarget) < 0.2f)
            ChooseNewPatrolTarget();

        transform.position = Vector3.MoveTowards(
            transform.position,
            patrolTarget,
            speed * Time.deltaTime
        );
    }

    void ChooseNewPatrolTarget()
    {
        Vector2 rnd = Random.insideUnitCircle * patrolRadius;
        patrolTarget = homePosition + new Vector3(rnd.x, rnd.y, 0f);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(homePosition, patrolRadius);
    }

    void UpdateStatUI()
    {
        if (statText == null) return;

        statText.text =
            $"<b>Stat</b>\n" +
            $"Speed: {speed}\n" +
            $"Damage: {attackDamage}\n" +
            $"Chase Range: {chaseRadius}\n" +
            $"Attack Range: {attackRange}";
    }
}
