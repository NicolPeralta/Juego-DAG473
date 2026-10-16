using UnityEngine;
using UnityEngine.UI;

public class TimeUI : MonoBehaviour
{
    public Canvas targetCanvas;
    public Vector2 size = new Vector2(300, 24);
    public Vector2 anchoredPosition = new Vector2(0, -10); // desde top-center
    public Color backgroundColor = new Color(0f, 0f, 0f, 0.5f);
    public Color fillColor = new Color(0.2f, 0.7f, 0.2f, 1f);

    private Image fillImage;
    private GameManager gm;

    void Awake()
    {
        gm = FindFirstObjectByType<GameManager>();

        if (targetCanvas == null)
            targetCanvas = FindFirstObjectByType<Canvas>();

        if (targetCanvas == null)
            CreateCanvas();

        CreateBar();
    }

    void Update()
    {
        if (gm == null)
            gm = FindFirstObjectByType<GameManager>();

        if (gm == null || fillImage == null)
            return;

        // usa elapsed de la ronda (0 .. roundLength)
        float elapsed = gm.CurrentRoundElapsed;
        float total = gm.RoundLength > 0f ? gm.RoundLength : 1f;
        float t = Mathf.Clamp01(elapsed / total);
        fillImage.fillAmount = t;
    }

    void CreateCanvas()
    {
        GameObject canvasGO = new GameObject("Canvas_TimeUI", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        targetCanvas = canvasGO.GetComponent<Canvas>();
        targetCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler cs = canvasGO.GetComponent<CanvasScaler>();
        cs.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
    }

    void CreateBar()
    {
        Transform parent = targetCanvas.transform;

        // Root container anchored top-center
        GameObject container = new GameObject("TimeBarRoot", typeof(RectTransform), typeof(CanvasRenderer));
        container.transform.SetParent(parent, false);
        RectTransform crt = container.GetComponent<RectTransform>();
        crt.anchorMin = new Vector2(0.5f, 1f);
        crt.anchorMax = new Vector2(0.5f, 1f);
        crt.pivot = new Vector2(0.5f, 1f);
        crt.anchoredPosition = anchoredPosition;
        crt.sizeDelta = size;

        // Background
        GameObject bg = new GameObject("TimeBar_BG", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        bg.transform.SetParent(container.transform, false);
        Image bgImg = bg.GetComponent<Image>();
        bgImg.color = backgroundColor;
        RectTransform bgRt = bg.GetComponent<RectTransform>();
        bgRt.anchorMin = Vector2.zero;
        bgRt.anchorMax = Vector2.one;
        bgRt.offsetMin = Vector2.zero;
        bgRt.offsetMax = Vector2.zero;

        // Fill (Image filled)
        GameObject fill = new GameObject("TimeBar_Fill", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        fill.transform.SetParent(container.transform, false);
        fillImage = fill.GetComponent<Image>();
        fillImage.color = fillColor;
        fillImage.type = Image.Type.Filled;
        fillImage.fillMethod = Image.FillMethod.Horizontal;
        fillImage.fillOrigin = (int)Image.OriginHorizontal.Left;
        fillImage.fillAmount = 0f;
        RectTransform fRt = fill.GetComponent<RectTransform>();
        fRt.anchorMin = Vector2.zero;
        fRt.anchorMax = Vector2.one;
        fRt.offsetMin = Vector2.zero;
        fRt.offsetMax = Vector2.zero;
    }
}