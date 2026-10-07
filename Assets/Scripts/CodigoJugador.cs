using UnityEngine;
using UnityEngine.InputSystem;


public class CodigoJugador : MonoBehaviour
{
    public GameObject[] prefabs;
    public float velocidadLanzamiento = 10f;
    public float alturaParabola = 5f;
    public Camera camara;
    public float tiempoEntreDisparos = 1f;

    private int puntos = 0;
    private float tiempoUltimoDisparo = -Mathf.Infinity;

    void Update()
    {
        //este codigo es para detectar el click del mouse y lanzar un objeto hacia el cubo que se haya clickeado
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            // comprobar cooldown entre disparos
            if (Time.time - tiempoUltimoDisparo < tiempoEntreDisparos)
                return;

            Ray rayo = camara.ScreenPointToRay(Mouse.current.position.ReadValue());

            if (Physics.Raycast(rayo, out RaycastHit impacto))
            {
                if (impacto.collider.CompareTag("Cubo"))
                {
                    LanzarObjeto(impacto.point, impacto.collider.gameObject);
                }
            }
        }
    }

    //este codigo es para detectar el click del mouse y lanzar un objeto hacia el cubo que se haya clickeado
    void LanzarObjeto(Vector3 objetivo, GameObject cubo)
    {
        if (prefabs.Length == 0)
            return;

        int indice = Random.Range(0, prefabs.Length);

        GameObject objeto = Instantiate(
            prefabs[indice],
            transform.position,
            Quaternion.identity
        );

        Rigidbody rb = objeto.GetComponent<Rigidbody>();

        if (rb != null)
        {
            Vector3 direccion = objetivo - transform.position;
            Vector3 direccionHorizontal = new Vector3(direccion.x, 0f, direccion.z);

            float distancia = direccionHorizontal.magnitude;

            Vector3 velocidadHorizontal = direccionHorizontal.normalized * velocidadLanzamiento;

            float tiempo = distancia / velocidadLanzamiento;

            float velocidadVertical = (alturaParabola + 0.5f * Mathf.Abs(Physics.gravity.y) * tiempo * tiempo) / tiempo;

            Vector3 velocidad = velocidadHorizontal;
            velocidad.y = velocidadVertical;

            rb.linearVelocity = velocidad;
        }

        LanzadorObjeto lanzador = objeto.GetComponent<LanzadorObjeto>();
        if (lanzador == null)
            lanzador = objeto.AddComponent<LanzadorObjeto>();

        lanzador.cuboObjetivo = cubo;
        lanzador.codigoJugador = this;
        lanzador.gameManager = FindFirstObjectByType<GameManager>();

        // actualizar tiempo del último disparo (aplicar cooldown)
        tiempoUltimoDisparo = Time.time;
    }
    //
    public void SumarPunto()
    {
        //este codigo es para sumar un punto al jugador cuando golpea el cubo
        puntos++;
        Debug.Log("+1 punto");
        Debug.Log("Puntos: " + puntos);
    }

    // Propiedad pública para que la UI lea los puntos
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