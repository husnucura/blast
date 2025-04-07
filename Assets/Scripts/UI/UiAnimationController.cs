using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class UiAnimationController : MonoBehaviour
{
    public static UiAnimationController Instance { get; private set; }

    public GameObject star;
    public GameObject particlePrefab;
    public float animationDuration = 0.5f;
    public int particleCount = 14;
    public float spreadRadius = 3f;
    public float particleLifetime = 2f;

    private Vector3 originalScale;
    public GameObject lossPanel;         // Assign in inspector
    public float fadeDuration = 1f;      // Duration of the fade-in

    public void PlayLossAnimation()
    {
        if (lossPanel != null)
        {
            StartCoroutine(FadeInLossPanel());
        }
    }

    private IEnumerator FadeInLossPanel()
    {
        CanvasGroup cg = lossPanel.GetComponent<CanvasGroup>();
        if (cg == null)
        {
            cg = lossPanel.AddComponent<CanvasGroup>();
        }

        cg.alpha = 0f;
        lossPanel.SetActive(true);

        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            cg.alpha = Mathf.Clamp01(elapsed / fadeDuration);
            yield return null;
        }

        cg.alpha = 1f;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        if (star != null)
        {
            originalScale = star.transform.localScale;
            star.transform.localScale = Vector3.zero;
        }
    }

    public void PlayWinAnimation()
    {
        StartCoroutine(AnimateWin());
    }

    private IEnumerator AnimateWin()
    {
        Vector3 spawnCenter = new Vector3(0f, 0f, -30f);

        // Spawn and animate particles
        for (int i = 0; i < particleCount; i++)
        {
            GameObject particle = Instantiate(particlePrefab, spawnCenter, Quaternion.identity);
            Vector2 randomDir = Random.insideUnitCircle * spreadRadius;
            StartCoroutine(MoveAndFadeParticle(particle.transform, randomDir, particleLifetime));
        }

        // Animate star scale
        float elapsed = 0f;
        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float scale = Mathf.SmoothStep(0f, originalScale.x, elapsed / animationDuration);
            star.transform.localScale = new Vector3(scale, scale, scale);
            yield return null;
        }

        star.transform.localScale = originalScale;
        LevelManager.IncrementLevel();
        SceneManager.LoadScene(0);
    }

    private IEnumerator MoveAndFadeParticle(Transform particle, Vector2 direction, float duration)
    {
        float elapsed = 0f;
        SpriteRenderer sr = particle.GetComponent<SpriteRenderer>();
        if (sr == null)
        {
            Destroy(particle.gameObject);
            yield break;
        }

        Color startColor = sr.color;
        Vector2 velocity = direction;
        float gravity = -9.8f;
        Vector3 position = particle.position;

        while (elapsed < duration)
        {
            float deltaTime = Time.deltaTime;

            velocity.y += gravity * deltaTime;
            position += (Vector3)(velocity * deltaTime);
            particle.position = position;

            sr.color = Color.Lerp(startColor, new Color(startColor.r, startColor.g, startColor.b, 0f), elapsed / duration);

            elapsed += deltaTime;
            yield return null;
        }

        Destroy(particle.gameObject);
    }
}
