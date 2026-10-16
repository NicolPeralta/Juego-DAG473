using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class Crosshair : MonoBehaviour
{
    [Tooltip("Preferible: arrastra un Sprite aquí (importado como Sprite).")]
    public Sprite crosshairSprite;

    [Tooltip("Alternativa: arrastra un archivo .png como Texture2D (drag & drop).")]
    public Texture2D crosshairTexture;

    [Tooltip("Escala multiplicadora del sprite")]
    public float scale = 1f;

    [Tooltip("Pivote del rectTransform (0-1)")]
    public Vector2 pivot = new Vector2(0.5f, 0.5f);

    [Tooltip("Offset en píxeles respecto al ratón")]
    public Vector2 offset = Vector2.zero;

    [Tooltip("Canvas en el que colocar la cruceta. Si es null se buscará o creará uno.")]
    public Canvas targetCanvas;

    private RectTransform rect;
    private Image image;

    void Awake()
    {
        // Si se proporcionó una textura pero no un sprite, crear Sprite en tiempo de ejecución
        if (crosshairSprite == null && crosshairTexture != null)
        {
            crosshairSprite = Sprite.Create(crosshairTexture, new Rect(0, 0, crosshairTexture.width, crosshairTexture.height), pivot, 100f);
        }

        if (targetCanvas == null)
            targetCanvas = FindFirstObjectByType<Canvas>();

        if (targetCanvas == null)
            CreateCanvas();

        GameObject go = new GameObject("Crosshair", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(targetCanvas.transform, false);

        image = go.GetComponent<Image>();
        image.raycastTarget = false; // permite pasar clics a objetos debajo

        rect = go.GetComponent<RectTransform>();
        rect.pivot = pivot;

        if (crosshairSprite != null)
        {
            image.sprite = crosshairSprite;
            rect.sizeDelta = new Vector2(crosshairSprite.rect.width, crosshairSprite.rect.height) * scale;
        }
        else
        {
            // tamaño por defecto si no hay sprite
            rect.sizeDelta = new Vector2(32f, 32f) * scale;
        }
    }

    void Update()
    {
        Vector2 mousePos = Mouse.current != null ? Mouse.current.position.ReadValue() : (Vector2)Input.mousePosition;

        if (targetCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            rect.position = mousePos + offset;
        }
        else
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                targetCanvas.transform as RectTransform,
                mousePos,
                targetCanvas.worldCamera,
                out Vector2 localPoint
            );
            rect.anchoredPosition = localPoint + offset;
        }
    }

    void CreateCanvas()
    {
        GameObject canvasGO = new GameObject("Canvas_Crosshair", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        targetCanvas = canvasGO.GetComponent<Canvas>();
        targetCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler cs = canvasGO.GetComponent<CanvasScaler>();
        cs.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
    }

    void OnEnable()
    {
        Cursor.visible = false;
    }

    void OnDisable()
    {
        Cursor.visible = true;
    }
}