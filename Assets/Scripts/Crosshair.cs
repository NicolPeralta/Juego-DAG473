using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class Crosshair : MonoBehaviour
{
    [Tooltip("Preferible: arrastra un Sprite aqu� (importado como Sprite).")]
    public Sprite crosshairSprite;

    [Tooltip("Alternativa: arrastra un archivo .png como Texture2D (drag & drop).")]
    public Texture2D crosshairTexture;

    [Tooltip("Escala multiplicadora del sprite")]
    public float scale = 1f;

    [Tooltip("Pivote del rectTransform (0-1)")]
    public Vector2 pivot = new Vector2(0.5f, 0.5f);

    [Tooltip("Offset en p�xeles respecto al rat�n")]
    public Vector2 offset = Vector2.zero;

    [Tooltip("Canvas en el que colocar la cruceta. Si es null se buscar� o crear� uno.")]
    public Canvas targetCanvas;

    private RectTransform rect;
    private Image image;

    void Awake()
    {
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
        image.raycastTarget = false; 

        rect = go.GetComponent<RectTransform>();
        rect.pivot = pivot;

        if (crosshairSprite != null)
        {
            image.sprite = crosshairSprite;
            rect.sizeDelta = new Vector2(crosshairSprite.rect.width, crosshairSprite.rect.height) * scale;
        }
        else
        {
            rect.sizeDelta = new Vector2(32f, 32f) * scale;
        }
    }

    void Update()
    {
        UpdateCursor();
        if (Time.timeScale == 0f || Mouse.current == null) return;
        Vector2 mousePos = Mouse.current.position.ReadValue();

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
        UpdateCursor();
    }

    void UpdateCursor()
    {
        bool inMenu = Time.timeScale == 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = inMenu;
        if (image != null) image.enabled = !inMenu;
    }

    void OnDisable()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        if (image != null) image.enabled = false;
    }
}
