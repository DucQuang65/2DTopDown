using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    // Health & Money
    [SerializeField] protected float maxHp = 100f; // Maximum health points
    protected float currentHp; // Current health
    [SerializeField] private Image hpBar; // UI ImageHP bar 
    public int money = 0; // Player's money

    // Movement & Control
    public Rigidbody2D rb; // Rigidbody2D for physics movement.
    public float moveSpeed = 5f;
    public Vector2 moveDirection;
    public float xRange = 10f;// Range limit movement (x-axis).
    public float yRange = 17f;// Range limit movement (y-axis).

    // Animation & Effects
    private Animator animator; // Animator for control animations (run, attack, etc.).
    private SpriteRenderer spriteRenderer; // flip sprite
    public ParticleSystem fxSword; // Effect skill 1.
    public ParticleSystem fxSpecialSkill; // Effect skill 2.

    //  HitBox 7 Damage
    public HitBoxPlayer hitBox; // Reference to HitBoxPlayer component
    public float damage; // Damage to enemy.

    //  Skills
    public int countDownSkill1; // Cooldown skill 1.
    public int countDownSkill2; // Cooldown skill 2.
    private bool isPlaySkill1; // Flag skill 1.
    private bool isPlaySkill2; // Flag skill 2.
    [SerializeField] private Button skill1; // Button UI skill 1.
    [SerializeField] private Button skill2; // Button skill 2.
    [SerializeField] private TextMeshProUGUI skill1CountdownText; // Text countdown skill 1.
    [SerializeField] private TextMeshProUGUI skill2CountdownText; // For skill 2.
    [SerializeField] private GameObject mask1; // Mask UI blind button when in cooldown skill 1.
    [SerializeField] private GameObject mask2; // For skill 2.

    private void Awake()
    {
        // Get components
        rb = GetComponent<Rigidbody2D>(); // move physics.
        spriteRenderer = GetComponent<SpriteRenderer>(); //  flip sprite.
        animator = GetComponent<Animator>(); // control animation.

        // Null check
        if (rb == null) Debug.LogError("Rigidbody2D not found on Player!");
        if (spriteRenderer == null) Debug.LogError("SpriteRenderer not found on Player!");
        if (animator == null) Debug.LogError("Animator not found on Player!");
    }

    private void Start()
    {
        // Init Player.cs: Health.
        currentHp = maxHp;
        UpdateHpBar();

        // Init PlayerController: Hitbox inactive, set damage.
        if (hitBox != null) // Null check.
        {
            hitBox.gameObject.SetActive(false);
            hitBox.damage = damage;
        }

        // Button listeners for skills.
        if (skill1 != null) skill1.onClick.AddListener(OnSkill1);
        if (skill2 != null) skill2.onClick.AddListener(OnSkill2);

        // Init UI countdown
        if (skill1CountdownText != null) skill1CountdownText.gameObject.SetActive(false);
        if (skill2CountdownText != null) skill2CountdownText.gameObject.SetActive(false);
        if (mask1 != null) mask1.SetActive(false);
        if (mask2 != null) mask2.SetActive(false);
    }

    private void OnDestroy()
    {
        // Remove listeners avoid memory leak
        if (skill1 != null) skill1.onClick.RemoveListener(OnSkill1);
        if (skill2 != null) skill2.onClick.RemoveListener(OnSkill2);
    }

    void Update()
    {
        // input PlayerController
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            return; // Not move if clicked UI.
        }

        var keyboard = Keyboard.current;
        moveDirection = Vector2.zero;

        if (keyboard != null)
        {
            if (keyboard.wKey.isPressed) moveDirection.y += 1; // Up.
            if (keyboard.sKey.isPressed) moveDirection.y -= 1; // Down.
            if (keyboard.aKey.isPressed) moveDirection.x -= 1; // left.
            if (keyboard.dKey.isPressed) moveDirection.x += 1; // right.
        }

        // Flip sprite
        if (moveDirection.x != 0)
        {
            Vector3 scale = transform.localScale;
            scale.x = moveDirection.x > 0 ? -1 : 1; // Flip.
            transform.localScale = scale;
        }

        // Set animation parameters
        animator.SetFloat("Horizontal", moveDirection.x); // Horizontal movement.
        animator.SetFloat("Vertical", moveDirection.y); // Vertical movement.
        animator.SetFloat("Speed", moveDirection.sqrMagnitude); // Speed (>0 = run).
        animator.SetBool("IsRun", moveDirection != Vector2.zero); // Bool run Player.cs.

        // Attack triggers from mouse
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            animator.SetTrigger("Attack"); // Trigger attack animation.
        }
        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            animator.SetTrigger("Casting"); // Trigger casting.
        }
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            animator.SetTrigger("Die"); // Trigger die (test).
        }

        // Boundary check
        if (transform.position.x < -xRange) transform.position = new Vector3(-xRange, transform.position.y, transform.position.z);
        if (transform.position.x > xRange) transform.position = new Vector3(xRange, transform.position.y, transform.position.z);
        if (transform.position.y < -yRange) transform.position = new Vector3(transform.position.x, -yRange, transform.position.z);
        if (transform.position.y > yRange) transform.position = new Vector3(transform.position.x, yRange, transform.position.z);
    }

    private void FixedUpdate()
    {
        // Move physics
        rb.MovePosition(rb.position + moveDirection.normalized * moveSpeed * Time.fixedDeltaTime);
    }

    // Skills PlayerController (Coroutine delay).
    private void OnSkill1()
    {
        StartCoroutine(IESkill()); //coroutine skill 1.
    }

    private void OnSkill2()
    {
        StartCoroutine(IESpecialSkill()); // Skill 2.
    }

    private IEnumerator IESkill()
    {
        if (isPlaySkill1) yield break; //Prevented spam skill.

        isPlaySkill1 = true;
        animator.SetTrigger("Attack"); // Trigger animation.

        yield return new WaitForSeconds(0.5f); // Delay 0.5s active hitbox.

        if (hitBox != null) hitBox.gameObject.SetActive(true); // Active hitbox for damage.

        yield return new WaitForSeconds(0.1f); // Hitbox active 0.1s.

        if (hitBox != null)
        {
            hitBox.gameObject.SetActive(false);
            hitBox.isDeal = false; // Reset flag.
        }

        if (fxSword != null) fxSword.Play(); // Play effect.

        // Countdown UI.
        int countDown = countDownSkill1;
        if (mask1 != null) mask1.SetActive(true);
        if (skill1CountdownText != null)
        {
            skill1CountdownText.gameObject.SetActive(true);
            while (countDown > 0)
            {
                countDown--;
                skill1CountdownText.text = countDown.ToString(); // Update text.
                yield return new WaitForSeconds(1f);
            }
            skill1CountdownText.gameObject.SetActive(false);
        }
        if (mask1 != null) mask1.SetActive(false);

        isPlaySkill1 = false; // Reset flag cooldown.
    }

    private IEnumerator IESpecialSkill()
    {
        if (isPlaySkill2) yield break;

        isPlaySkill2 = true;
        animator.SetTrigger("Attack");

        yield return new WaitForSeconds(0.5f);

        if (hitBox != null) hitBox.gameObject.SetActive(true);

        yield return new WaitForSeconds(0.1f);

        if (hitBox != null)
        {
            hitBox.gameObject.SetActive(false);
            hitBox.isDeal = false;
        }

        if (fxSpecialSkill != null) fxSpecialSkill.Play();

        int countDown = countDownSkill2;
        if (mask2 != null) mask2.SetActive(true);
        if (skill2CountdownText != null)
        {
            skill2CountdownText.gameObject.SetActive(true);
            while (countDown > 0)
            {
                countDown--;
                skill2CountdownText.text = countDown.ToString();
                yield return new WaitForSeconds(1f);
            }
            skill2CountdownText.gameObject.SetActive(false);
        }
        if (mask2 != null) mask2.SetActive(false);

        isPlaySkill2 = false;
    }

    public void TakeDamage(float damage)
    {
        Debug.Log($"Player took {damage} damage!");
        currentHp -= damage;
        currentHp = Mathf.Max(currentHp, 0);
        UpdateHpBar();
        if (currentHp <= 0)
        {
            Die();
        }
    }

    public void Heal(float healAmount)
    {
        if (currentHp < maxHp)
        {
            currentHp += healAmount;
            currentHp = Mathf.Min(currentHp, maxHp);
            UpdateHpBar();
            Debug.Log($"Player healed for {healAmount} points!");
        }
        else
        {
            Debug.Log("Player is already at full health!");
        }
    }

    private void Die()
    {
        Debug.Log("Player has died!");
        Destroy(gameObject);
    }

    protected void UpdateHpBar()
    {
        if (hpBar != null)
        {
            hpBar.fillAmount = currentHp / maxHp; // Update bar.
        }
        else
        {
            Debug.LogWarning("HP Bar Image not assigned!");
        }
    }

    public void AddMoney(int amount)
    {
        money += amount;
        Debug.Log($"Added {amount} money! Total: {money}");
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("EnemyProjectile"))
        {
            TakeDamage(20); // Damage from bullet.
        }
        else if (collision.CompareTag("Coin"))
        {
            int randomMoney = UnityEngine.Random.Range(10, 31);
            AddMoney(randomMoney);
            Destroy(collision.gameObject);
        }
    }
}