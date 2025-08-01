using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    [Header("UI - Status Text")]
    [SerializeField] private GameObject statusPanel;
    [SerializeField] private TextMeshProUGUI dieText;
    [SerializeField] private TextMeshProUGUI spawnInText;

    private Vector2 velocity = Vector2.zero;
    [SerializeField] private float smoothTime = 0.1f;

    [Header("VFX")]
    public ParticleSystem fxSword;
    public ParticleSystem fxSpecialSkill;
    public Animator animator;

    [Header("Attack")]
    public HitBoxPlayer hitBox;
    public float damage; // Không dùng nữa, giữ lại nếu cần
    private bool isNormalAttacking = false;

    [Header("Cooldown & Skill")]
    public int countDownSkill1;
    public int countDownSkill2;
    private bool isPlaySkill1;
    private bool isPlaySkill2;
    [SerializeField] private float normalAttackCooldown = 1.5f;

    [SerializeField] private Button skill1;
    [SerializeField] private Button skill2;
    [SerializeField] private TextMeshProUGUI skill1CountdownText;
    [SerializeField] private TextMeshProUGUI skill2CountdownText;
    [SerializeField] private GameObject mask1;
    [SerializeField] private GameObject mask2;

    [Header("Movement")]
    public Rigidbody2D rb;
    public float moveSpeed = 5f;
    public Vector2 moveDirection;
    public float xRange = 20f;
    public float yRange = 20f;

    [Header("HP")]
    public float currentHp;
    public Image hpBarImage;

    [Header("Stats & Level")]
    public int level = 1;
    public float baseAtk = 10f;
    public float currentAtk;

    public float baseAspd = 1.5f;
    public float currentAspd;

    public float baseMaxHp = 100f;
    public float currentMaxHp;

    public float exp = 0f;
    public float expToNextLevel = 100f;
    public Image expBarImage;

    [Header("UI - Player Stats")]
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI atkStatText;
    [SerializeField] private TextMeshProUGUI aspdStatText;
    [SerializeField] private TextMeshProUGUI hpStatText;
    [SerializeField] private TextMeshProUGUI expText;

    private Renderer[] renderers;
    private Collider2D collider2d;
    private bool isDead = false;
    private bool isSkill2Unlocked = false;
    public bool IsDead => isDead; // <-- Property công khai để kiểm tra trạng thái chết

    private void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>();
        collider2d = GetComponent<Collider2D>();
    }

    private void Start()
    {
        // Không gọi InitStats() để tránh reset dữ liệu đã load
        // Thay vào đó, đảm bảo các biến có giá trị hợp lệ nếu chưa load
        skill2.interactable = false;
        mask2.SetActive(true);
        skill2CountdownText.gameObject.SetActive(false);
        if (currentMaxHp <= 0)
            currentMaxHp = baseMaxHp;

        if (currentHp <= 0)
            currentHp = currentMaxHp;

        if (currentAtk <= 0)
            currentAtk = baseAtk;

        if (currentAspd <= 0)
            currentAspd = baseAspd;

        if (expToNextLevel <= 0)
            expToNextLevel = 100f;

        UpdateHpBar();
        UpdateExpBar();         // <== Bổ sung cập nhật thanh exp lúc start
        UpdateAllStatText();

        hitBox.gameObject.SetActive(false);
        hitBox.damage = currentAtk;

        skill1.onClick.AddListener(OnSkill1);
        skill2.onClick.AddListener(OnSkill2);

        skill1CountdownText.gameObject.SetActive(false);
        skill2CountdownText.gameObject.SetActive(false);
        mask1.SetActive(false);
        mask2.SetActive(false);
    }

    public void NewGameInit()
    {
        exp = 0f;
        expToNextLevel = 100f;
        level = 1;
        currentAtk = baseAtk;
        currentAspd = baseAspd;
        currentMaxHp = baseMaxHp;
        currentHp = currentMaxHp;

        UpdateHpBar();
        UpdateExpBar();
        UpdateAllStatText();
    }

    private void OnDestroy()
    {
        skill1.onClick.RemoveListener(OnSkill1);
        skill2.onClick.RemoveListener(OnSkill2);
    }

    private void Update()
    {
        if (GamePauseManager.IsPaused) return;
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

        if (isDead) return;
        if (Keyboard.current.digit2Key.wasPressedThisFrame && isSkill2Unlocked && !isPlaySkill2)
{
    OnSkill2();
}
        var keyboard = Keyboard.current;
        moveDirection = Vector2.zero;

        if (keyboard != null)
        {
            if (keyboard.wKey.isPressed) moveDirection.y += 0.5f;
            if (keyboard.sKey.isPressed) moveDirection.y -= 0.5f;
            if (keyboard.aKey.isPressed) moveDirection.x -= 0.5f;
            if (keyboard.dKey.isPressed) moveDirection.x += 0.5f;
        }

        if (moveDirection.x != 0)
        {
            Vector3 scale = transform.localScale;
            scale.x = moveDirection.x > 0 ? -0.5f : 0.5f;
            transform.localScale = scale;
        }

        animator.SetFloat("Horizontal", moveDirection.x);
        animator.SetFloat("Vertical", moveDirection.y);
        float speed = moveDirection.sqrMagnitude;
        animator.SetFloat("Speed", speed, 0.1f, Time.deltaTime);
        if (Keyboard.current.digit2Key.wasPressedThisFrame && isSkill2Unlocked && !isPlaySkill2)
        {
            OnSkill2();
        }
        if (Mouse.current.leftButton.wasPressedThisFrame && !isNormalAttacking)
        {
            animator.SetTrigger("Attack");
            StartCoroutine(NormalAttackCooldown());
        }
        if (Keyboard.current.digit1Key.wasPressedThisFrame && !isPlaySkill1)
        {
            OnSkill1();
        }

        if (Keyboard.current.digit2Key.wasPressedThisFrame && !isPlaySkill2)
        {
            OnSkill2();
        }

        float clampedX = Mathf.Clamp(transform.position.x, -xRange, xRange);
        float clampedY = Mathf.Clamp(transform.position.y, -yRange, yRange);
        transform.position = new Vector3(clampedX, clampedY, transform.position.z);
    }

    private IEnumerator NormalAttackCooldown()
    {
        isNormalAttacking = true;
        yield return new WaitForSeconds(0.15f);
        hitBox.damage = currentAtk;
        hitBox.gameObject.SetActive(true);
        fxSword.Play();
        yield return new WaitForSeconds(0.1f);
        hitBox.gameObject.SetActive(false);
        hitBox.isDeal = false;
        yield return new WaitForSeconds(currentAspd);
        isNormalAttacking = false;
    }

    private void OnSkill1() => StartCoroutine(IESkill());
    private void OnSkill2()
    {
        if (!isSkill2Unlocked) return;      // <-- Kiểm tra đã mở khóa chưa
        if (isPlaySkill2) return;           // <-- Đang sử dụng thì bỏ qua
        StartCoroutine(IESpecialSkill());
    }

    private IEnumerator IESkill()
    {
        if (isPlaySkill1) yield break;
        isPlaySkill1 = true;

        animator.SetTrigger("Attack");
        yield return new WaitForSeconds(0.5f);
        hitBox.damage = currentAtk;
        hitBox.gameObject.SetActive(true);

        yield return new WaitForSeconds(0.1f);
        hitBox.gameObject.SetActive(false);
        hitBox.isDeal = false;

        fxSword.Play();

        int countDown = countDownSkill1;
        mask1.SetActive(true);
        skill1CountdownText.gameObject.SetActive(true);
        while (countDown > 0)
        {
            countDown--;
            skill1CountdownText.text = countDown.ToString();
            yield return new WaitForSeconds(1f);
        }

        mask1.SetActive(false);
        skill1CountdownText.gameObject.SetActive(false);
        isPlaySkill1 = false;
    }

    private IEnumerator IESpecialSkill()
    {
        if (isPlaySkill2) yield break;
        isPlaySkill2 = true;

        animator.SetTrigger("Attack");
        yield return new WaitForSeconds(0.5f);
        hitBox.damage = currentAtk;
        hitBox.gameObject.SetActive(true);

        yield return new WaitForSeconds(0.1f);
        hitBox.gameObject.SetActive(false);
        hitBox.isDeal = false;

        fxSpecialSkill.Play();

        int countDown = countDownSkill2;
        mask2.SetActive(true);
        skill2CountdownText.gameObject.SetActive(true);
        while (countDown > 0)
        {
            countDown--;
            skill2CountdownText.text = countDown.ToString();
            yield return new WaitForSeconds(1f);
        }

        mask2.SetActive(false);
        skill2CountdownText.gameObject.SetActive(false);
        isPlaySkill2 = false;
    }

    private void FixedUpdate()
    {
        if (!isDead)
        {
            // Di chuyển mượt mà với SmoothDamp
            Vector2 targetPosition = rb.position + moveDirection.normalized * moveSpeed * Time.fixedDeltaTime;
            Vector2 smoothPosition = Vector2.SmoothDamp(rb.position, targetPosition, ref velocity, smoothTime);

            rb.MovePosition(smoothPosition);
        }
    }

    public void TakeDamage(float damageAmount)
    {
        Debug.Log($"Player TakeDamage called with amount {damageAmount}, current HP before: {currentHp}");
        if (isDead) return;

        currentHp -= damageAmount;
        currentHp = Mathf.Clamp(currentHp, 0, currentMaxHp);
        UpdateHpBar();

        if (currentHp <= 0)
        {
            StartCoroutine(DieRoutine());
        }

        UpdateAllStatText();
    }

    public void UpdateHpBar()
    {
        if (hpBarImage != null)
        {
            hpBarImage.fillAmount = currentHp / currentMaxHp;
        }
    }

    public void UpdateExpBar()
    {
        if (expBarImage != null)
        {
            if (expToNextLevel <= 0f) expToNextLevel = 100f; // Phòng trường hợp expToNextLevel = 0
            float fillAmount = Mathf.Clamp01(exp / expToNextLevel);
            expBarImage.fillAmount = fillAmount;
        }
    }

    private IEnumerator DieRoutine()
    {
        if (isDead) yield break;
        isDead = true;
        if (statusPanel != null) statusPanel.SetActive(true);
        if (dieText != null)
        {
            dieText.gameObject.SetActive(true);
            dieText.text = "You Died";
        }
        if (spawnInText != null) spawnInText.gameObject.SetActive(false);
        animator.SetTrigger("Die");

        float dieAnimLength = 1.0f;
        yield return new WaitForSeconds(dieAnimLength);

        foreach (var rend in renderers)
        {
            rend.enabled = false;
        }

        if (collider2d != null)
            collider2d.enabled = false;

        this.enabled = false;
        // Giữ "You Died" trong 3 giây
        yield return new WaitForSeconds(3f);

        // Ẩn "You Died", hiện "Spawn In..."
        if (dieText != null)
            dieText.gameObject.SetActive(false);

        if (spawnInText != null)
        {
            spawnInText.gameObject.SetActive(true);
            int countdown = 15;
            while (countdown > 0)
            {
                spawnInText.text = $"Spawn In... {countdown}";
                yield return new WaitForSeconds(1f);
                countdown--;
            }
        }

        // Đợi thêm 12 giây nữa (tổng thời gian là 15 giây)
        yield return new WaitForSeconds(15f);

        Respawn();
    }

    private void Respawn()
    {
        if (statusPanel != null) statusPanel.SetActive(true);
        if (dieText != null) dieText.gameObject.SetActive(false);
        if (spawnInText != null) spawnInText.gameObject.SetActive(false);
        Invoke(nameof(HideStatusPanel), 3f);
        foreach (var rend in renderers)
        {
            rend.enabled = true;
        }

        if (collider2d != null)
            collider2d.enabled = true;

        currentHp = currentMaxHp;
        UpdateHpBar();
        UpdateAllStatText();

        transform.position = new Vector3(0f, -1f, transform.position.z);

        animator.Rebind();
        animator.Update(0f);
        animator.Play("Idle", 0, 0);
        isDead = false;
        this.enabled = true;
    }
    private void HideStatusPanel()
    {
        if (statusPanel != null)
            statusPanel.SetActive(false);
    }

    public void GainExp(float amount)
    {
        exp += amount;
        if (exp >= expToNextLevel)
        {
            LevelUp();
        }
        UpdateExpBar();
        UpdateAllStatText();
    }

    private void LevelUp()
    {
        exp -= expToNextLevel;
        level++;
        expToNextLevel = Mathf.Max(1f, expToNextLevel * 1.25f);

        currentAtk *= 1.035f;
        currentAspd *= 1 / 1.035f;
        currentMaxHp *= 1.035f;
        currentHp = currentMaxHp;

        UpdateHpBar();
        UpdateAllStatText();

        if (exp >= expToNextLevel)
        {
            LevelUp();
        }
        if (!isSkill2Unlocked && level >= 10)
        {
            UnlockSkill2();
        }
    }
    private void UnlockSkill2()
    {
        isSkill2Unlocked = true;
        skill2.interactable = true;
        mask2.SetActive(false);
        Debug.Log("Skill 2 unlocked!");
    }

    public void UpdateAllStatText()
    {
        if (levelText != null) levelText.text = $"Level: {level}";
        if (atkStatText != null) atkStatText.text = $"ATK: {currentAtk:F1}";
        if (aspdStatText != null) aspdStatText.text = $"ASPD: {currentAspd:F2}";
        if (hpStatText != null) hpStatText.text = $"HP: {currentMaxHp:F0}";
        if (expText != null) expText.text = $"EXP: {exp:F0} / {expToNextLevel:F0}";
    }
}
