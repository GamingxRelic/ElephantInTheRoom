using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WalkBehindWallLogic : MonoBehaviour
{
    private SpriteRenderer sprite;
    private Coroutine fadeCoroutine;
    [SerializeField] private float fade_time = 0.25f;

    private void Start()
    {
        sprite = GetComponent<SpriteRenderer>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }
        fadeCoroutine = StartCoroutine(FadeTo(0f, fade_time));
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }
        fadeCoroutine = StartCoroutine(FadeTo(1f, fade_time));
    }

    private IEnumerator FadeTo(float targetAlpha, float duration)
    {
        float startAlpha = sprite.color.a;
        float time = 0;

        while (time < duration)
        {
            time += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, targetAlpha, time / duration);
            Color color = sprite.color;
            color.a = alpha;
            sprite.color = color;
            yield return null;
        }

        Color finalColor = sprite.color;
        finalColor.a = targetAlpha;
        sprite.color = finalColor;
    }
}
