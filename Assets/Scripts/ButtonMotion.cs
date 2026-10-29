using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[DisallowMultipleComponent]
[RequireComponent(typeof(Button), typeof(CanvasGroup))]
public class ButtonMotion : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler,
    IPointerDownHandler, IPointerUpHandler, ISelectHandler, IDeselectHandler, ISubmitHandler
{
    [Range(1f, 1.15f)] public float hoverScale = 1.06f;
    [Range(0.85f, 1f)] public float pressedScale = 0.94f;
    [Range(0.03f, 0.3f)] public float smoothTime = 0.085f;
    public float entryDuration = 0.2f;

    Button button;
    CanvasGroup group;
    Graphic graphic;
    Vector3 originalScale;
    Color originalColor;
    float originalAlpha;
    float scale = 1f, velocity, enteredAt, submittedUntil, releasePulse;
    bool hovered, held, selected, initialized;

    void Awake()
    {
        button = GetComponent<Button>();
        group = GetComponent<CanvasGroup>();
        graphic = button.targetGraphic;
        originalScale = transform.localScale;
        originalAlpha = group.alpha;
        originalColor = graphic != null ? graphic.color : Color.white;
        initialized = true;
    }

    void OnEnable()
    {
        if (!initialized) return;
        hovered = held = false;
        selected = EventSystem.current != null && EventSystem.current.currentSelectedGameObject == gameObject;
        enteredAt = Time.unscaledTime;
        submittedUntil = 0;
        velocity = releasePulse = 0;
        scale = 0.94f;
        ApplyScale();
        group.alpha = originalAlpha * 0.35f;
    }

    void Update()
    {
        bool interactable = button.IsInteractable();
        bool pressed = interactable && (held || Time.unscaledTime < submittedUntil);
        if (!interactable) { held = false; submittedUntil = 0; releasePulse = 0; }
        float targetScale = !interactable ? 1f : pressed ? pressedScale : hovered ? hoverScale : selected ? 1.025f : 1f;
        releasePulse = Mathf.MoveTowards(releasePulse, 0, Time.unscaledDeltaTime * 0.25f);
        scale = Mathf.SmoothDamp(scale, targetScale + releasePulse, ref velocity,
            pressed ? smoothTime * 0.65f : smoothTime, Mathf.Infinity, Time.unscaledDeltaTime);
        ApplyScale();

        float entry = Mathf.Clamp01((Time.unscaledTime - enteredAt) / Mathf.Max(0.01f, entryDuration));
        entry = 1f - Mathf.Pow(1f - entry, 3f);
        float targetAlpha = originalAlpha * (interactable ? 1f : 0.5f) * Mathf.Lerp(0.35f, 1f, entry);
        float blend = 1f - Mathf.Exp(-18f * Time.unscaledDeltaTime);
        group.alpha = Mathf.Lerp(group.alpha, targetAlpha, blend);
        if (graphic != null)
        {
            float brightness = !interactable ? 0.75f : pressed ? 0.88f : hovered ? 1.08f : selected ? 1.04f : 1f;
            Color color = new Color(originalColor.r * brightness, originalColor.g * brightness,
                originalColor.b * brightness, originalColor.a);
            graphic.color = Color.Lerp(graphic.color, color, blend);
        }
    }

    void ApplyScale() => transform.localScale = Vector3.Scale(originalScale, new Vector3(scale, scale, 1));

    void OnDisable()
    {
        if (!initialized) return;
        hovered = held = selected = false;
        submittedUntil = releasePulse = velocity = 0;
        scale = 1;
        transform.localScale = originalScale;
        group.alpha = originalAlpha;
        if (graphic != null) graphic.color = originalColor;
    }

    public void OnPointerEnter(PointerEventData e) => hovered = true;
    public void OnPointerExit(PointerEventData e) { hovered = false; held = false; }
    public void OnPointerDown(PointerEventData e)
    {
        if (e.button == PointerEventData.InputButton.Left && button.IsInteractable()) held = true;
    }
    public void OnPointerUp(PointerEventData e)
    {
        if (e.button != PointerEventData.InputButton.Left) return;
        if (held && hovered && button.IsInteractable()) releasePulse = 0.025f;
        held = false;
    }
    public void OnSelect(BaseEventData e) => selected = true;
    public void OnDeselect(BaseEventData e) { selected = false; held = false; }
    public void OnSubmit(BaseEventData e)
    {
        if (button.IsInteractable()) submittedUntil = Time.unscaledTime + 0.1f;
    }
    void OnApplicationFocus(bool focused)
    {
        if (!focused) { hovered = held = false; submittedUntil = 0; }
    }
}
