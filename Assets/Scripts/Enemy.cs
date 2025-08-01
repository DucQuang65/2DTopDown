using UnityEngine;
using UnityEngine.UI;

public abstract class Enemy : MonoBehaviour
{
    [SerializeField] protected float moveSpeed = 2f; // Movement speed towards target.
    protected Transform playerTarget; // Reference to player for chasing.
    [SerializeField] public MainTower tower; // Reference to tower (primary target).
    public float currentHp; // Current health (public for external access if needed).
    [SerializeField] public float maxHp = 50f; // Max health.
    [SerializeField] private Image hpBar; // UI HP bar.
    [SerializeField] private float playerChaseRange = 10f; // Range to prioritize player over tower.
    [SerializeField] public float enterDamage = 10f; // Damage on initial contact/attack.
    [SerializeField] public float stayDamage = 5f; // Ongoing damage (for ranged/subclasses).
    [SerializeField] public float attackRange = 1f; // Range to trigger attack animation/damage.
    protected float stayDamageTimer = 0f; // Timer to apply damage periodically.
    [SerializeField] protected float stayDamageInterval = 0.5f; // Interval between damage ticks.

    public delegate void EnemyDestroyedHandler(Enemy enemy); // Delegate for destruction event.
    public event EnemyDestroyedHandler OnEnemyDestroyed; // Event to notify listeners (e.g., score system).
    protected Animator animator; // Animator for run/attack states.

    [SerializeField] protected GameObject coinPrefab; // Prefab for coins on death.
    [SerializeField] protected int minCoins = 10; // Min coins to drop.
    [SerializeField] protected int maxCoins = 30; // Max coins.

    // Hashed parameter IDs for efficiency and to avoid string typos.
    protected static readonly int IsRunHash = Animator.StringToHash("IsRun"); // Bool for run state.
    protected static readonly int AttackHash = Animator.StringToHash("Attack"); // Trigger for attack.

    [SerializeField] protected int minExp = 10;
    [SerializeField] protected int maxExp = 20;
    [SerializeField] public bool isBoss = false;

    // *** Thêm biến để lưu loại enemy, phục vụ lưu/load ***
    public int enemyTypeIndex = 0;

    protected enum EnemyState
    {
        Idle,
        ChasingTower,
        ChasingPlayer
    }
    protected EnemyState currentState = EnemyState.ChasingTower;

    protected virtual void Awake()
    {
        // Find player by tag
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTarget = playerObj.transform;
        }
        else
        {
            Debug.LogError("GameObject with tag 'Player' not found in scene!", this);
        }

        // Find tower if not assigned
        if (tower == null)
        {
            tower = FindAnyObjectByType<MainTower>();
            if (tower == null)
            {
                Debug.LogError("MainTower not found in scene!", this);
            }
        }

        // Find Animator
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogError("Animator not found on Enemy!", this);
        }
    }

    protected virtual void Start()
    {
        currentHp = maxHp;
        UpdateHpBar();
    }

    protected virtual void Update()
    {
        if (GamePauseManager.IsPaused) return;
        if (tower == null)
        {
            Debug.LogWarning("Cannot move: Tower is missing.", this);
            return;
        }

        GameObject foundPlayer = GameObject.FindGameObjectWithTag("Player");
        bool isPlayerAlive = false;

        if (foundPlayer != null)
        {
            PlayerController playerComp = foundPlayer.GetComponent<PlayerController>();
            if (playerComp != null)
            {
                isPlayerAlive = !playerComp.IsDead && playerComp.currentHp > 0;
            }
            else
            {
                isPlayerAlive = true; // Nếu không tìm thấy PlayerController thì coi player alive
            }
        }

        if (foundPlayer != null && isPlayerAlive)
        {
            playerTarget = foundPlayer.transform;
            float distanceToPlayer = Vector2.Distance(transform.position, playerTarget.position);
            currentState = (distanceToPlayer <= playerChaseRange) ? EnemyState.ChasingPlayer : EnemyState.ChasingTower;
        }
        else
        {
            playerTarget = null;
            currentState = EnemyState.ChasingTower;
        }

        switch (currentState)
        {
            case EnemyState.ChasingPlayer:
                MoveToPlayer();
                break;
            case EnemyState.ChasingTower:
                MoveToTower();
                break;
        }

        float distToPlayer = playerTarget != null ? Vector2.Distance(transform.position, playerTarget.position) : Mathf.Infinity;
        HandleAnimation(distToPlayer);
    }

    protected virtual void MoveToPlayer()
    {
        if (playerTarget == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, playerTarget.position);
        if (distanceToPlayer <= playerChaseRange)
        {
            Vector2 newPosition = Vector2.MoveTowards(transform.position, playerTarget.position, moveSpeed * Time.deltaTime);
            transform.position = new Vector3(newPosition.x, newPosition.y, transform.position.z);
            FlipToTarget(playerTarget);
            Debug.Log("Enemy moving to player.");
        }
    }

    protected virtual void MoveToTower()
    {
        if (tower != null)
        {
            Vector2 newPosition = Vector2.MoveTowards(transform.position, tower.transform.position, moveSpeed * Time.deltaTime);
            transform.position = new Vector3(newPosition.x, newPosition.y, transform.position.z);
            FlipToTarget(tower.transform);
            Debug.Log("Enemy moving to tower.");
        }
    }

    protected void FlipToTarget(Transform target)
    {
        if (target == null) return;

        Vector3 scale = transform.localScale;
        scale.x = target.position.x < transform.position.x ? -Mathf.Abs(scale.x) : Mathf.Abs(scale.x);
        transform.localScale = scale;
    }

    public virtual void TakeDamage(float damage)
    {
        Debug.Log($"Enemy took {damage} damage!");
        currentHp -= damage;
        currentHp = Mathf.Max(currentHp, 0);
        UpdateHpBar();
        if (currentHp <= 0)
        {
            Die();
        }
    }

    protected virtual void Die()
    {
        GrantExpToPlayer();
        DropMoney();
        OnEnemyDestroyed?.Invoke(this);
        Destroy(gameObject);
    }

    protected void UpdateHpBar()
    {
        if (hpBar != null)
        {
            hpBar.fillAmount = currentHp / maxHp;
        }
        else
        {
            Debug.LogWarning("HP Bar Image not assigned in Enemy!");
        }
    }

    protected virtual void DamageToPlayer()
    {
        if (playerTarget != null)
        {
            PlayerController playerScript = playerTarget.GetComponent<PlayerController>();
            if (playerScript != null)
            {
                playerScript.TakeDamage(enterDamage);
            }
            else
            {
                Debug.LogWarning("Player script not found on target tagged 'Player'.");
            }
        }
    }

    protected virtual void StayDamageToPlayer()
    {
        if (playerTarget == null) return;

        stayDamageTimer += Time.deltaTime;
        if (stayDamageTimer >= stayDamageInterval)
        {
            PlayerController playerComp = playerTarget.GetComponent<PlayerController>();
            if (playerComp != null)
            {
                playerComp.TakeDamage(stayDamage);
                Debug.Log("Enemy dealt stayDamage to player.");
            }
            stayDamageTimer = 0f;
        }
    }

    protected virtual void DamageToTower()
    {
        if (tower != null)
        {
            tower.TakeDamage(enterDamage);
            Debug.Log("Enemy damaging Tower on attack!");
        }
    }

    protected virtual void HandleAnimation(float distanceToPlayer)
    {
        float distanceToTower = Vector2.Distance(transform.position, tower.transform.position);

        if (animator != null)
        {
            bool isRunning = (currentState == EnemyState.ChasingPlayer && distanceToPlayer > attackRange) ||
                             (currentState == EnemyState.ChasingTower && distanceToTower > attackRange);

            if (HasBoolParameter("IsRun"))
            {
                animator.SetBool(IsRunHash, isRunning);
            }
            else
            {
                Debug.LogError($"Parameter 'IsRun' does not exist in Animator Controller for {gameObject.name}. Existing parameters: {GetAllParametersList()}");
            }

            if (currentState == EnemyState.ChasingPlayer && distanceToPlayer <= attackRange)
            {
                animator.SetTrigger(AttackHash);
                DamageToPlayer();
                StayDamageToPlayer();
            }
            else if (currentState == EnemyState.ChasingTower && distanceToTower <= attackRange)
            {
                animator.SetTrigger(AttackHash);
                DamageToTower();
            }
            else
            {
                stayDamageTimer = 0f;
            }
        }
    }

    protected bool HasBoolParameter(string name)
    {
        foreach (var param in animator.parameters)
        {
            if (param.name == name && param.type == AnimatorControllerParameterType.Bool)
            {
                return true;
            }
        }
        return false;
    }

    protected string GetAllParametersList()
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        foreach (var param in animator.parameters)
        {
            sb.Append($"{param.name} ({param.type}), ");
        }
        return sb.ToString().TrimEnd(',', ' ');
    }

    protected virtual void DropMoney()
    {
        if (coinPrefab != null)
        {
            int coinCount = Random.Range(minCoins, maxCoins + 1);
            for (int i = 0; i < coinCount; i++)
            {
                Vector3 dropPosition = transform.position + new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f), 0);
                Instantiate(coinPrefab, dropPosition, Quaternion.identity);
            }
        }
        else
        {
            Debug.LogWarning("Coin prefab not assigned! No money dropped.");
        }
    }

    protected virtual void GrantExpToPlayer()
    {
        if (playerTarget != null)
        {
            var playerScript = playerTarget.GetComponent<PlayerController>();
            if (playerScript != null)
            {
                int baseExp = Random.Range(minExp, maxExp + 1);
                int finalExp = isBoss ? Mathf.RoundToInt(baseExp * 1.5f) : baseExp;
                playerScript.GainExp(finalExp);
                Debug.Log($"Enemy granted {finalExp} EXP to player. IsBoss: {isBoss}");
            }
        }
    }

    // Hàm bổ sung để set đúng HP khi load lại enemy
    public void SetEnemyDataOnLoad(float hp)
    {
        currentHp = hp;
        UpdateHpBar();
    }
}
