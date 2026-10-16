using UnityEngine;
using UnityEngine.UI;

public class RoundUI : MonoBehaviour
{
    public Canvas targetCanvas;
    public Vector2 anchoredPosition = new Vector2(100, -10); // desde top-left (ajusta en inspector)
    public int fontSize = 18;
    public Color textColor = Color.white;

    private Text roundText;
    private GameManager gm;

    void Awake()
    {
        gm = FindFirstObjectByType<GameManager>();

        if (targetCanvas == null)
            targetCanvas = FindFirstObjectByType<Canvas>();

        if (targetCanvas == null)
            CreateCanvas();

        CreateText();
    }

    void Update()
    {
        if (gm == null)
            gm = FindFirstObjectByType<GameManager>();

        if (gm == null || roundText == null)
            return;

        roundText.text = $"Ronda {gm.CurrentRound} / {gm.TotalRounds}";
    }

    void CreateCanvas()
    {
        GameObject canvasGO = new GameObject("Canvas_RoundUI", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        targetCanvas = canvasGO.GetComponent<Canvas>();
        targetCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler cs = canvasGO.GetComponent<CanvasScaler>();
        cs.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
    }

    void CreateText()
    {
        GameObject go = new GameObject("RoundText", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        go.transform.SetParent(targetCanvas.transform, false);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(0f, 1f);
        rt.pivot = new Vector2(0f, 1f);
        rt.anchoredPosition = anchoredPosition;

        roundText = go.GetComponent<Text>();
        roundText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        roundText.fontSize = fontSize;
        roundText.color = textColor;
        roundText.alignment = TextAnchor.MiddleLeft;
        roundText.text = "Ronda 1 / 1";
    }
}