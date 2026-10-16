using UnityEngine;

// este script es para controlar el comportamiento del objeto lanzado, detectando colisiones con el cubo objetivo, obstáculos y el jugador
public class LanzadorObjeto : MonoBehaviour
{
    public GameObject cuboObjetivo;
    public CodigoJugador codigoJugador;

    public GameManager gameManager;
    private bool yaGolpeo = false;

    void Awake()
    {
        gameManager = FindFirstObjectByType<GameManager>();
    }

    // este metodo es para detectar colisiones con el cubo objetivo, obstaculos y el jugador con trigger, y para sumar puntos al jugador si golpea el cubo objetivo
    void OnTriggerEnter(Collider otro)
    {
        if (yaGolpeo)
            return;

        if (EsObjetivo(otro))
        {
            yaGolpeo = true;

            if (codigoJugador != null)
                codigoJugador.SumarPunto();

            if (gameManager != null)
            {
                gameManager.BajarObjeto(cuboObjetivo);
            }

            Destroy(gameObject);
            return;
        }

        if (otro != null && otro.CompareTag("Obstaculo"))
        {
            Destroy(gameObject);
            return;
        }

        if (otro != null && otro.CompareTag("Player"))
        {
            Destroy(gameObject);
            return;
        }
    }

    bool EsObjetivo(Collider otro)
    {
        // proteger contra referencias nulas: si no hay objetivo asignado, no es objetivo
        if (otro == null || cuboObjetivo == null)
            return false;

        if (otro.gameObject == cuboObjetivo)
            return true;

        if (otro.transform.IsChildOf(cuboObjetivo.transform))
            return true;

        return false;
    }
}