using UnityEngine;
using TMPro;
using System.Collections;

public class StartScreenUI : MonoBehaviour
{
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI subtitleText;
    public CanvasGroup canvasGroup;

    public float fadeDuration = 1f;
    public float subtitleBlinkSpeed = 1.2f;

    private bool started = false;

    void Update()
    {
        if (started) return;

        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        if (h != 0f || v != 0f)
        {
            started = true;
            StopAllCoroutines();
            StartCoroutine(FadeOut());
        }
    }

    void Start()
    {
        StartCoroutine(BlinkSubtitle());
    }

    IEnumerator BlinkSubtitle()
    {
        while (!started)
        {
            float t = (Mathf.Sin(Time.time * subtitleBlinkSpeed * Mathf.PI) + 1f) * 0.5f;
            if (subtitleText != null)
                subtitleText.alpha = Mathf.Lerp(0.3f, 1f, t);
            yield return null;
        }
    }

    IEnumerator FadeOut()
    {
        float elapsed = 0f;
        float startAlpha = canvasGroup.alpha;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, elapsed / fadeDuration);
            yield return null;
        }
        canvasGroup.alpha = 0f;
        gameObject.SetActive(false);
    }
}
