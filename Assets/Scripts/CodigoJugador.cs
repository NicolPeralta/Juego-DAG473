using UnityEngine;
using UnityEngine.InputSystem;

public class CodigoJugador : MonoBehaviour
{
    public GameObject[] prefabs;
    public float velocidadLanzamiento = 10f;
    public float velocidadMultiplicador = 1.5f; // multiplicador global de velocidad
    public float alturaParabola = 5f;
    public float arcMultiplier = 1.2f; // multiplica la altura de la parábola para un arco más notable
    public Camera camara;
    public float tiempoEntreDisparos = 1f;

    private int puntos = 0;
    private float tiempoUltimoDisparo = -Mathf.Infinity;

    void Update()
    {
        if (Mouse.current != null ? Mouse.current.leftButton.wasPressedThisFrame : Input.GetMouseButtonDown(0))
        {
            // comprobar cooldown entre disparos
            if (Time.time - tiempoUltimoDisparo < tiempoEntreDisparos)
                return;

            Ray rayo = camara.ScreenPointToRay(Mouse.current != null ? Mouse.current.position.ReadValue() : (Vector2)Input.mousePosition);

            Vector3 objetivo;
            GameObject cubo = null;

            if (Physics.Raycast(rayo, out RaycastHit impacto))
            {
                objetivo = impacto.point;
                if (impacto.collider.CompareTag("Cubo"))
                    cubo = impacto.collider.gameObject;
            }
            else
            {
                // Si no hay colisión, proyectar sobre el plano Y=0 (suelo) o usar un punto a distancia
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

            // aplicar multiplicador de velocidad
            float velocidadBase = velocidadLanzamiento * Mathf.Max(0.01f, velocidadMultiplicador);
            Vector3 velocidadHorizontal = (distancia > 0f ? direccionHorizontal.normalized : transform.forward) * velocidadBase;

            // tiempo aproximado en horizontal (protegido)
            float tiempo = distancia / velocidadBase;
            tiempo = Mathf.Max(0.05f, tiempo);

            // altura objetivo ajustada por arcMultiplier
            float alturaDeseada = alturaParabola * Mathf.Max(0.01f, arcMultiplier);

            // ecuación para velocidad vertical inicial: y = v_y * t - 0.5 * g * t^2  => v_y = (y + 0.5*g*t^2)/t
            float g = Mathf.Abs(Physics.gravity.y);
            float velocidadVertical = (alturaDeseada + 0.5f * g * tiempo * tiempo) / tiempo;

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

    public void SumarPunto()
    {
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