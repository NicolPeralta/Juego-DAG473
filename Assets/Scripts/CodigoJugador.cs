using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class CodigoJugador : MonoBehaviour
{
    public GameObject[] prefabs;
    public float velocidadLanzamiento = 10f;
    public float velocidadMultiplicador = 1.5f;
    public float alturaParabola = 5f;
    public float arcMultiplier = 1.2f;
    public Camera camara;
    public float tiempoEntreDisparos = 1f;

    [Header("SFX")]
    public AudioClip launchSfx;    
    public AudioClip impactSfxPrefab; 

    private CannonAimer cannon;
    void Awake() => cannon = GetComponent<CannonAimer>();

    private int puntos = 0;
    private float tiempoUltimoDisparo = -Mathf.Infinity;

    void Update()
    {
        if (Time.timeScale == 0f || camara == null || Mouse.current == null) return;
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (Time.time - tiempoUltimoDisparo < tiempoEntreDisparos)
                return;

            Ray rayo = camara.ScreenPointToRay(Mouse.current.position.ReadValue());

            Vector3 objetivo;
            GameObject cubo = null;

            if (cannon != null)
            {
                objetivo = cannon.GetAimPoint(rayo, out Collider targetCollider);
                cubo = LanzadorObjeto.FindTarget(targetCollider);
            }
            else if (Physics.Raycast(rayo, out RaycastHit impacto))
            {
                objetivo = impacto.point;
                if (impacto.collider.CompareTag("Cubo"))
                    cubo = impacto.collider.gameObject;
            }
            else
            {
                Plane suelo = new Plane(Vector3.up, Vector3.zero);
                if (suelo.Raycast(rayo, out float enter))
                {
                    objetivo = rayo.GetPoint(enter);
                }
                else
                {
                    objetivo = rayo.origin + rayo.direction * 10f;
                }
            }

            LanzarObjeto(objetivo, cubo);
        }
    }

    void LanzarObjeto(Vector3 objetivo, GameObject cubo)
    {
        if (prefabs == null || prefabs.Length == 0)
            return;

        int indice = Random.Range(0, prefabs.Length);
        Vector3 origen = cannon != null && cannon.Muzzle != null ? cannon.Muzzle.position : transform.position;

        GameObject objeto = Instantiate(
            prefabs[indice],
            origen,
            Quaternion.identity
        );

        LanzadorObjeto lanzador = objeto.GetComponent<LanzadorObjeto>();
        if (lanzador == null)
            lanzador = objeto.AddComponent<LanzadorObjeto>();

        lanzador.cuboObjetivo = cubo;
        lanzador.codigoJugador = this;
        lanzador.gameManager = FindFirstObjectByType<GameManager>();
        lanzador.Launch(objetivo, Mathf.Max(0.1f, velocidadLanzamiento * velocidadMultiplicador), transform);

        if (impactSfxPrefab != null)
            lanzador.impactSfx = impactSfxPrefab;

        if (launchSfx != null)
            AudioManager.Instance?.PlaySfx(launchSfx);

        if (cannon != null)
            ProjectileEffects.RedSmoke(origen, cannon.MuzzleDirection);
        Destroy(objeto, 12f);
        tiempoUltimoDisparo = Time.time;
    }

    public void SumarPunto()
    {
        puntos++;
    }

    public int Puntos => puntos;

    public float GetCooldown01()
    {
        if (tiempoEntreDisparos <= 0f)
            return 0f;

        float elapsed = Time.time - tiempoUltimoDisparo;
        if (elapsed >= tiempoEntreDisparos)
            return 0f;

        float t = Mathf.Clamp01(elapsed / tiempoEntreDisparos);
        return 1f - t;
    }
}