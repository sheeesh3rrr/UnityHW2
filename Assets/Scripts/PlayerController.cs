using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System;
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float forwardSpeed = 8f;
    [SerializeField] private float sideSpeed = 6f;
    [SerializeField] private float jumpForce = 7f;
    [Header("Ground Check")]
    [SerializeField] private float groundCheckDistance = 1f;
    [SerializeField] private LayerMask groundLayer;
    [Header("Health")]
    [SerializeField] private int maxHealth = 100;

    private int currentHealth;
    private Rigidbody rb;
    private bool isGrounded;
    private bool isDead;

    private int score;
    private float scoreTimer;
    [SerializeField] private float scoreInterval = 1f;

    private bool isInvincible;
    private float currentSpeedMultiplier = 1f;
    private Coroutine speedBoostCoroutine;
    private Coroutine invincibilityCoroutine;
    public event Action<int, int> OnHealthChanged;
    public event Action<int> OnScoreChanged;
    public event Action<BonusType, float> OnBonusApplied;
    public event Action OnPlayerDied;
    public event Action OnDamageTaken;
    public int Score => score;
    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public bool IsDead => isDead;
    public bool IsInvincible => isInvincible;
    public float CurrentSpeedMultiplier => currentSpeedMultiplier;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        currentHealth = maxHealth;
        score = 0;
    }

    private void Update()
    {
        if (isDead) return;
        CheckGround();
        HandleJump();
        UpdateScore();
    }

    private void FixedUpdate()
    {
        if (isDead) return;
        rb.linearVelocity = new Vector3(
        Input.GetAxis("Horizontal") * sideSpeed,
        rb.linearVelocity.y,
        forwardSpeed * currentSpeedMultiplier
        );
    }

    private void UpdateScore()
    {
        scoreTimer += Time.deltaTime;
        if (scoreTimer >= scoreInterval)
        {
            score++;
            scoreTimer = 0f;
            OnScoreChanged?.Invoke(score);
        }
    }

    private void HandleJump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            Jump();
        }
    }

    private void Jump()
    {
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f,
       rb.linearVelocity.z);
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    private void CheckGround()
    {
        isGrounded = Physics.Raycast(
        transform.position,
        Vector3.down,
        groundCheckDistance,
        groundLayer
        );
    }

    public void ApplyBonus(BonusData data)
    {
        switch (data.bonusType)
        {
            case BonusType.Heal:
                Heal(data.healAmount);
                break;
            case BonusType.SpeedBoost:
                if (speedBoostCoroutine != null)
                    StopCoroutine(speedBoostCoroutine);
                speedBoostCoroutine = StartCoroutine(
                SpeedBoostRoutine(data.speedMultiplier, data.effectDuration));
                break;
            case BonusType.Invincibility:
                if (invincibilityCoroutine != null)
                    StopCoroutine(invincibilityCoroutine);
                invincibilityCoroutine = StartCoroutine(
                InvincibilityRoutine(data.effectDuration));
                break;
        }
        OnBonusApplied?.Invoke(data.bonusType, data.effectDuration);
        score += 5;
        OnScoreChanged?.Invoke(score);
    }

    private void Heal(int amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    private IEnumerator SpeedBoostRoutine(float multiplier, float duration)
    {
        currentSpeedMultiplier = multiplier;
        yield return new WaitForSeconds(duration);
        currentSpeedMultiplier = 1f;
    }

    private IEnumerator InvincibilityRoutine(float duration)
    {
        isInvincible = true;
        yield return new WaitForSeconds(duration);
        isInvincible = false;
    }

    public void TakeDamage(int damage)
    {
        if (isDead || isInvincible) return;
        currentHealth -= damage;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        OnDamageTaken?.Invoke();
        Debug.Log($"Здоровье: {currentHealth}");
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        isDead = true;
        Debug.Log("Игрок погиб.");

        int bestScore = PlayerPrefs.GetInt("BestScore", 0);
        if (score > bestScore)
        {
            PlayerPrefs.SetInt("BestScore", score);
            PlayerPrefs.Save();
        }
        OnPlayerDied?.Invoke();
        StartCoroutine(ReturnToMainMenu());
    }

    private IEnumerator ReturnToMainMenu()
    {
        yield return new WaitForSeconds(3f);
        SceneManager.LoadScene(0);
    }
}
