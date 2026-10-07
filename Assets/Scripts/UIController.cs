using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIController : MonoBehaviour
{
    public CodigoJugador jugador;
    public Image cooldownImage;
    public TextMeshProUGUI puntosText;

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
    }
}