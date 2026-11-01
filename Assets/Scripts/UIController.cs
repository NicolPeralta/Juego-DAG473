using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIController : MonoBehaviour
{
    public CodigoJugador jugador;
    public Image cooldownImage;
    [Tooltip("Arrastra aquí una Image de UI para mostrar la recarga del clic derecho.")]
    public Image cooldownDerechoImage;
    public TextMeshProUGUI puntosText;
    public TextMeshProUGUI timeText;  
    public TextMeshProUGUI roundText; 

    [Header("Cuenta regresiva y anuncios de ronda")]
    [Tooltip("Fuente TMP para 3, 2, 1, ¡YA! y los anuncios de ronda. Vacío conserva la fuente predeterminada.")]
    public TMP_FontAsset fuenteCuentaRegresiva;

    private GameManager gm;

    void Awake()
    {
        gm = FindFirstObjectByType<GameManager>();
        if (cooldownDerechoImage != null)
        {
            cooldownDerechoImage.type = Image.Type.Filled;
            cooldownDerechoImage.fillMethod = Image.FillMethod.Radial360;
            cooldownDerechoImage.fillOrigin = (int)Image.Origin360.Top;
            cooldownDerechoImage.raycastTarget = false;
            cooldownDerechoImage.fillAmount = 0f;
        }
    }

    void Update()
    {
        if (jugador != null)
        {
            if (cooldownImage != null)
            {
                cooldownImage.fillAmount = jugador.GetCooldown01();
            }

            if (cooldownDerechoImage != null)
            {
                cooldownDerechoImage.fillAmount = GameSettings.SelectedDifficulty == GameSettings.Difficulty.Facil
                    ? 0f : jugador.GetRightCooldown01();
            }

            if (puntosText != null)
            {
                puntosText.text = $"Puntos: {jugador.Puntos}";
            }
        }

        if (gm == null)
            gm = FindFirstObjectByType<GameManager>();

        if (gm != null)
        {
            // Tiempo de ronda: elapsed 0 .. roundLength
            if (timeText != null)
            {
                float elapsed = gm.CurrentRoundElapsed;
                float total = Mathf.Max(0.01f, gm.RoundLength);

                int em = Mathf.FloorToInt(elapsed);
                int emMin = em / 60;
                int emSec = em % 60;

                int tm = Mathf.FloorToInt(total);
                int tmMin = tm / 60;
                int tmSec = tm % 60;

                timeText.text = string.Format("{0:00}:{1:00} / {2:00}:{3:00}", emMin, emSec, tmMin, tmSec);
            }

            // Ronda actual
            if (roundText != null)
            {
                roundText.text = $"Nivel {gm.CurrentLevel} / {gm.TotalLevels}  |  Ronda {gm.CurrentRound} / {gm.TotalRounds}";
            }
        }
    }
}
