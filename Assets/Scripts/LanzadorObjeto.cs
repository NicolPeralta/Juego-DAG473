using UnityEngine;
public class LanzadorObjeto : MonoBehaviour
{
    public GameObject cuboObjetivo;
    public CodigoJugador codigoJugador;
    public GameManager gameManager;
    private bool yaGolpeo = false;

    [Header("SFX")]
    public AudioClip impactSfx; 

    void Awake()
    {
        gameManager = FindFirstObjectByType<GameManager>();
    }
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

            if (impactSfx != null)
                AudioManager.Instance?.PlaySfx(impactSfx);

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
        if (otro == null || cuboObjetivo == null)
            return false;

        if (otro.gameObject == cuboObjetivo)
            return true;

        if (otro.transform.IsChildOf(cuboObjetivo.transform))
            return true;

        return false;
    }
}