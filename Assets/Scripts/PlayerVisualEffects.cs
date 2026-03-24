using UnityEngine;
using System.Collections;

public class PlayerVisualEffects : MonoBehaviour
{
    [SerializeField] private PlayerController player;

    [Header("Renderers")]
    [SerializeField] private Renderer[] renderers;

    [Header("Colors")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color damageColor = Color.red;
    [SerializeField] private Color healColor = Color.green;
    [SerializeField] private Color speedColor = Color.yellow;
    [SerializeField] private Color invincibleColor = Color.cyan;

    [Header("Settings")]
    [SerializeField] private float damageFlashDuration = 0.3f;
    [SerializeField] private int damageFlashCount = 3;

    private Coroutine activeEffectCoroutine;

    private void Start()
    {
        player.OnDamageTaken += PlayDamageEffect;
        player.OnBonusApplied += PlayBonusEffect;
    }

    private void OnDestroy()
    {
        if (player != null)
        {
            player.OnDamageTaken -= PlayDamageEffect;
            player.OnBonusApplied -= PlayBonusEffect;
        }
    }

    private void SetColor(Color color)
    {
        foreach (Renderer rend in renderers)
        {
            rend.material.color = color;
        }
    }

    private void PlayDamageEffect()
    {
        StartCoroutine(DamageFlashRoutine());
    }

    private IEnumerator DamageFlashRoutine()
    {
        for (int i = 0; i < damageFlashCount; i++)
        {
            SetColor(damageColor);
            yield return new WaitForSeconds(damageFlashDuration);
            SetColor(normalColor);
            yield return new WaitForSeconds(damageFlashDuration);
        }
    }

    private void PlayBonusEffect(BonusType type, float duration)
    {
        if (activeEffectCoroutine != null)
            StopCoroutine(activeEffectCoroutine);

        switch (type)
        {
            case BonusType.Heal:
                StartCoroutine(HealFlashRoutine());
                break;
            case BonusType.SpeedBoost:
                activeEffectCoroutine = StartCoroutine(
                    DurationEffectRoutine(speedColor, duration));
                break;
            case BonusType.Invincibility:
                activeEffectCoroutine = StartCoroutine(
                    DurationEffectRoutine(invincibleColor, duration));
                break;
        }
    }

    private IEnumerator HealFlashRoutine()
    {
        SetColor(healColor);
        yield return new WaitForSeconds(0.5f);
        SetColor(normalColor);
    }

    private IEnumerator DurationEffectRoutine(Color effectColor, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            float t = Mathf.PingPong(elapsed * 3f, 1f);
            SetColor(Color.Lerp(normalColor, effectColor, t));
            elapsed += Time.deltaTime;
            yield return null;
        }
        SetColor(normalColor);
    }
}