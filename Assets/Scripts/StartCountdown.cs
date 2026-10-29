using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Created by GameManager so every game entry gets the same countdown.
public class StartCountdown : MonoBehaviour
{
    public string CurrentText { get; private set; } = "";
    GameObject overlay;
    CanvasGroup group;
    TextMeshProUGUI label;

    public IEnumerator Play()
    {
        overlay = new GameObject("Cuenta regresiva", typeof(Canvas), typeof(CanvasScaler), typeof(CanvasGroup));
        overlay.transform.SetParent(transform, false);
        var canvas = overlay.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 50;
        var scaler = overlay.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = .5f;
        group = overlay.GetComponent<CanvasGroup>();
        group.blocksRaycasts = false;
        group.interactable = false;
        var textObject = new GameObject("Numero", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(overlay.transform, false);
        label = textObject.GetComponent<TextMeshProUGUI>();
        label.rectTransform.anchorMin = label.rectTransform.anchorMax = new Vector2(.5f, .5f);
        label.rectTransform.anchoredPosition = Vector2.zero;
        label.rectTransform.sizeDelta = new Vector2(700, 240);
        label.alignment = TextAlignmentOptions.Center;
        label.fontSize = 150;
        label.fontStyle = FontStyles.Bold;
        label.color = new Color(1f, .83f, .3f);
        label.outlineWidth = .2f;
        label.outlineColor = new Color32(35, 20, 45, 255);
        label.raycastTarget = false;
        for (int number = 3; number > 0; number--)
            yield return Show(number.ToString(), 1f);
        yield return Show("¡YA!", .45f);
        CurrentText = "";
        Destroy(overlay);
        Destroy(this);
    }

    IEnumerator Show(string text, float duration)
    {
        CurrentText = label.text = text;
        float elapsed = 0;
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            label.rectTransform.localScale = Vector3.one * Mathf.Lerp(1.2f, 1f, Mathf.SmoothStep(0, 1, Mathf.Clamp01(t * 4)));
            // Pause hides the overlay so it does not cover the pause menu.
            group.alpha = Time.timeScale == 0 ? 0 : 1f - Mathf.SmoothStep(0, 1, Mathf.InverseLerp(.75f, 1f, t));
            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    void OnDestroy()
    {
        if (overlay != null) Destroy(overlay);
    }
}
