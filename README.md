# НИС Unity HW2 Выполнил Кудрявцев Георгий Александрович БПИ 245
## Краткое описание игры

Игра представляет собой бесконечный `3D` раннер, расширенный по сравнению с HW1.
Игрок бесконечно бежит вперед по платформе, огороженной
стенами. На пути появляются препятствия двух типов (`стена` и `куб`), а также
бонусы трех типов: `лечение`, `ускорение` и `неуязвимость`.

Добавлен счетчик очков, которые начисляются за время в игре и за подбор бонусов.
Рекорд сохраняется между запусками игры через PlayerPrefs.

Игра теперь имеет главное меню, HUD во время игры и меню паузы.
При получении урона, подборе бонуса и во время действия эффектов
игрок меняет цвет (анимация через скрипт + Unity Animator).

## Что изменилось по сравнению с HW1

- Добавлена система бонусов (3 типа) с данными в ScriptableObject
- Добавлен спавнер бонусов по аналогии со спавнером препятствий
- Добавлен счетчик очков (за время + за подбор бонусов)
- Добавлено главное меню (название, автор, рекорд, кнопки "Играть" и "Выход")
- Добавлен HUD (здоровье, очки, рекорд, текст активного эффекта)
- Добавлено меню паузы (ESC - заморозка игры, кнопки "Продолжить" и "В меню")
- Добавлено сохранение рекорда между запусками (PlayerPrefs)
- Добавлены визуальные эффекты: мигание при уроне, вспышка при бонусе, пульсация при длительном эффекте
- Добавлен Animator Controller с графом состояний
- PlayerController значительно расширен: события, эффекты бонусов, неуязвимость, очки

## Точки входа и скрипты

Игра имеет 2 сцены. Главное меню (`MainMenu`, index 0) - стартовый экран.
Игровая сцена (`Game`, index 1) - сам раннер. Главный скрипт - `PlayerController.cs`.

**Новые скрипты**

- Данные бонусов. Отдельный скрипт для статических данных бонусов (ScriptableObject).
Хранит тип бонуса, количество лечения, множитель скорости и длительность эффекта.
```cs
using UnityEngine;

public enum BonusType
{
    Heal,
    SpeedBoost,
    Invincibility
}

[CreateAssetMenu(menuName = "Bonus Data")]
public class BonusData : ScriptableObject
{
    public BonusType bonusType;
    public int healAmount;
    public float speedMultiplier;
    public float effectDuration;
}
```

- Бонус. Обрабатывает подбор бонуса игроком через триггер. При входе игрока в триггер
вызывает ApplyBonus на PlayerController и удаляется со сцены. Также удаляется,
если игрок убежал далеко вперед.
```cs
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Bonus : MonoBehaviour
{
    [SerializeField] private BonusData data;
    private Transform player;

    public BonusData Data => data;

    public void Initialize(Transform playerTransform)
    {
        player = playerTransform;
    }

    private void Update()
    {
        if (player != null && transform.position.z < player.position.z - 15f)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController pc = other.GetComponent<PlayerController>();
            if (pc != null)
            {
                pc.ApplyBonus(data);
            }
            Destroy(gameObject);
        }
    }
}
```

- Спавнер бонусов. Работает по аналогии со спавнером препятствий. Является ребенком
игрока и спавнит бонусы на одну из линий впереди игрока.
```cs
using UnityEngine;

public class BonusSpawner : MonoBehaviour
{
    [SerializeField] private GameObject[] bonusPrefabs;
    [SerializeField] private float spawnInterval = 4f;
    [SerializeField] private float spawnDistance = 35f;
    [SerializeField] private float[] lanes;

    private float timer;
    private Transform player;

    private void Start()
    {
        player = GetComponentInParent<Transform>();
    }

    private void Update()
    {
        timer += Time.deltaTime;

        if (timer >= spawnInterval)
        {
            Spawn();
            timer = 0f;
        }
    }

    private void Spawn()
    {
        int lane = Random.Range(0, lanes.Length);
        int index = Random.Range(0, bonusPrefabs.Length);

        Vector3 pos = new Vector3(
            lanes[lane],
            1.5f,
            player.position.z + spawnDistance
        );

        GameObject obj = Instantiate(bonusPrefabs[index], pos, Quaternion.identity);
        obj.GetComponent<Bonus>().Initialize(player);
    }
}
```

- Главное меню. Отображает название игры, имя разработчика, рекорд из PlayerPrefs.
Две кнопки: начать игру и выйти.
```cs
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI developerText;
    [SerializeField] private TextMeshProUGUI bestScoreText;
    [SerializeField] private Button playButton;
    [SerializeField] private Button quitButton;

    private void Start()
    {
        int bestScore = PlayerPrefs.GetInt("BestScore", 0);
        bestScoreText.text = "Рекорд: " + bestScore;

        playButton.onClick.AddListener(StartGame);
        quitButton.onClick.AddListener(QuitGame);
    }

    private void StartGame()
    {
        SceneManager.LoadScene(1);
    }

    private void QuitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}
```

- HUD во время игры. Отображает текущее здоровье (слайдер + текст), очки,
рекорд и текст активного эффекта. Подписывается на события PlayerController.
```cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameHUD : MonoBehaviour
{
    [SerializeField] private PlayerController player;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private Slider healthBar;
    [SerializeField] private TextMeshProUGUI bestScoreText;
    [SerializeField] private TextMeshProUGUI effectText;

    private void Start()
    {
        player.OnScoreChanged += UpdateScore;
        player.OnHealthChanged += UpdateHealth;
        player.OnBonusApplied += ShowEffect;

        healthBar.maxValue = player.MaxHealth;
        healthBar.value = player.MaxHealth;
        scoreText.text = "Очки: 0";
        healthText.text = "HP: " + player.MaxHealth + "/" + player.MaxHealth;

        int bestScore = PlayerPrefs.GetInt("BestScore", 0);
        bestScoreText.text = "Рекорд: " + bestScore;
        effectText.text = "";
    }

    private void OnDestroy()
    {
        if (player != null)
        {
            player.OnScoreChanged -= UpdateScore;
            player.OnHealthChanged -= UpdateHealth;
            player.OnBonusApplied -= ShowEffect;
        }
    }

    private void UpdateScore(int newScore)
    {
        scoreText.text = "Очки: " + newScore;
    }

    private void UpdateHealth(int current, int max)
    {
        healthBar.value = current;
        healthText.text = "HP: " + current + "/" + max;
    }

    private void ShowEffect(BonusType type, float duration)
    {
        string effectName = "";
        if (type == BonusType.SpeedBoost) effectName = "УСКОРЕНИЕ";
        else if (type == BonusType.Invincibility) effectName = "НЕУЯЗВИМОСТЬ";
        else if (type == BonusType.Heal) effectName = "ЛЕЧЕНИЕ";

        float showDuration = (type == BonusType.Heal) ? 1f : duration;
        StartCoroutine(ShowTemporaryText(effectName, showDuration));
    }

    private System.Collections.IEnumerator ShowTemporaryText(string text, float duration)
    {
        effectText.text = text;
        yield return new WaitForSeconds(duration);
        effectText.text = "";
    }
}
```

- Меню паузы. По нажатию ESC игра замораживается (Time.timeScale = 0).
Две кнопки: продолжить и выйти в меню.
```cs
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button menuButton;

    private bool isPaused;

    private void Start()
    {
        pausePanel.SetActive(false);
        resumeButton.onClick.AddListener(Resume);
        menuButton.onClick.AddListener(GoToMenu);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused) Resume();
            else Pause();
        }
    }

    private void Pause()
    {
        isPaused = true;
        pausePanel.SetActive(true);
        Time.timeScale = 0f;
    }

    private void Resume()
    {
        isPaused = false;
        pausePanel.SetActive(false);
        Time.timeScale = 1f;
    }

    private void GoToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(0);
    }
}
```

- Визуальные эффекты игрока. Меняет цвет капсулы и сферы при уроне (мигание красным),
при лечении (вспышка зеленым), при ускорении (пульсация желтым)
и при неуязвимости (пульсация голубым). Принимает массив рендереров через инспектор,
что позволяет красить все части тела игрока одновременно.
```cs
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

        if (type == BonusType.Heal)
            StartCoroutine(HealFlashRoutine());
        else if (type == BonusType.SpeedBoost)
            activeEffectCoroutine = StartCoroutine(DurationEffectRoutine(speedColor, duration));
        else if (type == BonusType.Invincibility)
            activeEffectCoroutine = StartCoroutine(DurationEffectRoutine(invincibleColor, duration));
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
```

- Контроллер аниматора игрока. Управляет параметрами Animator Controller
на основе событий из PlayerController. Устанавливает триггер при уроне
и булевы параметры для длительных эффектов.
```cs
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerAnimatorController : MonoBehaviour
{
    [SerializeField] private PlayerController player;
    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        player.OnDamageTaken += () => animator.SetTrigger("TakeDamage");
    }

    private void Update()
    {
        animator.SetBool("SpeedBoost", player.CurrentSpeedMultiplier > 1f);
        animator.SetBool("Invincible", player.IsInvincible);
    }
}
```

**Измененные скрипты**

- PlayerController. Значительно расширен. Добавлены: счетчик очков (начисление за время
и за подбор бонусов), метод ApplyBonus для обработки бонусов, корутины для временных
эффектов (ускорение и неуязвимость), события (OnHealthChanged, OnScoreChanged,
OnBonusApplied, OnPlayerDied, OnDamageTaken) для связи с UI и визуальными эффектами,
сохранение рекорда в PlayerPrefs при смерти. Скорость движения теперь умножается
на currentSpeedMultiplier, который меняется при подборе бонуса ускорения.
При активной неуязвимости урон не наносится.

**Скрипты без изменений**

- ObstacleSpawner.cs - спавнер препятствий
- Obstacle.cs - логика препятствий
- ObstacleData.cs - ScriptableObject данных препятствий
- GroundMover.cs - перемещение платформы
- DestroyBehindPlayer.cs - удаление объектов позади игрока

## Выполнение требований

**Общие требования**
- Все характеристики доступны для настройки через Инспектор - Выполнено
- Статические данные повторяющихся объектов хранятся в ScriptableObject (ObstacleData, BonusData) - Выполнено
- Игровые объекты не аварийно прекращают работу, нет необработанных исключений - Выполнено

**Новый функционал (0.2)**
- Минимум два типа бонусов (лечение, ускорение, неуязвимость - три типа) - Выполнено
- Счетчик очков (за время в игре + за подбор бонусов) - Выполнено

**Главное меню и HUD (0.4)**
- Главное меню с названием раннера, именем разработчика, рекордом и двумя кнопками (играть, выход) - Выполнено
- HUD во время игры для отображения здоровья и очков - Выполнено
- Меню паузы по ESC с заморозкой времени и двумя кнопками (продолжить, в меню) - Выполнено

**Система сохранения (0.1)**
- Сохранение рекорда между запусками через PlayerPrefs - Выполнено

**Дополнительные требования (0.2)**
- Анимация при получении урона (мигание красным) - Выполнено
- Анимация при получении бонуса (вспышка соответствующим цветом) - Выполнено
- Отображение длительного эффекта бонуса (пульсация цветом на протяжении действия) - Выполнено
- Unity Animator Controller с графом состояний и переходами (Idle, Damaged, SpeedBoost, Invincible) - Выполнено

**Документация (0.1)**
- README с описанием - Выполнено

## Использованные ресурсы
Вспомогательных ресурсов использовано не было.
