using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIController : MonoBehaviour
{
    public CodigoJugador jugador;
    public Image cooldownImage;
    public TextMeshProUGUI puntosText;
    public TextMeshProUGUI timeText;  
    public TextMeshProUGUI roundText; 

    private GameManager gm;

    void Awake()
    {
        gm = FindFirstObjectByType<GameManager>();
    }

    void Update()
    {
        if (jugador != null)
        {
            if (cooldownImage != null)
            {
                cooldownImage.fillAmount = jugador.GetCooldown01();
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
                roundText.text = $"Ronda {gm.CurrentRound} / {gm.TotalRounds}";
            }
        }
    }
}