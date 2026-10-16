using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Controla la lógica de aparición de los objetos (muñequitos) por rondas,
/// tiempos y dificultad. Ahora permite asignar desde el inspector los objetos
/// concretos que se activarán en cada dificultad: `easyObjects`, `mediumObjects`, `hardObjects`.
/// </summary>
public class GameManager : MonoBehaviour
{
    [Header("Todos los objetos posibles en la escena (para posiciones iniciales)")]
    public GameObject[] objetos;

    [Header("Asignar manualmente los objetos por dificultad (inspector)")]
    public GameObject[] easyObjects;
    public GameObject[] mediumObjects;
    public GameObject[] hardObjects;

    // valores por defecto (se sobrescriben desde GameSettings)
    public float alturaSubida = 4f;
    public float velocidadMovimiento = 5f;
    public float tiempoVisible = 3f;
    public float tiempoEntreObjetos = 2f;
    public float GameDuration { get; private set; } = 180f;

    [Header("Rondas")]
    public float roundLength = 30f; // cada ronda dura 30 segundos
    private int totalRounds = 1;
    private int currentRound = 1;

    // Objetos gestionados según la dificultad (exactamente los que quieres en inspector)
    private List<GameObject> gestionObjects = new List<GameObject>();

    // Estado interno
    private Dictionary<GameObject, Vector3> posicionesIniciales = new Dictionary<GameObject, Vector3>();
    private Dictionary<GameObject, bool> objetosGolpeados = new Dictionary<GameObject, bool>();
    private GameObject ultimoObjeto = null;
    private bool juegoActivo = true;

    private float startTime;
    private float nextRoundTime;
    private float lastRoundStartTime;

    // Exponer tiempo transcurrido en ronda para UI
    public float CurrentRoundElapsed
    {
        get
        {
            if (!juegoActivo) return 0f;
            return Mathf.Clamp(Time.time - lastRoundStartTime, 0f, roundLength);
        }
    }

    public float RoundLength => roundLength;
    public int CurrentRound => currentRound;
    public int TotalRounds => totalRounds;

    void Start()
    {
        // aplicar ajustes desde GameSettings (estáticos)
        GameDuration = GameSettings.GameDuration;
        tiempoVisible = GameSettings.TiempoVisible;
        tiempoEntreObjetos = GameSettings.TiempoEntreObjetos;
        velocidadMovimiento = GameSettings.VelocidadMovimiento;

        if (objetos == null || objetos.Length == 0)
        {
            Debug.LogWarning("GameManager: no hay objetos asignados en 'objetos'.");
        }

        // calcular rondas totales según duración total del juego
        totalRounds = Mathf.Max(1, Mathf.CeilToInt(GameDuration / Mathf.Max(0.01f, roundLength)));
        currentRound = 1;

        // construir diccionario de posiciones iniciales para todos los objetos conocidos
        posicionesIniciales.Clear();
        if (objetos != null)
        {
            foreach (var o in objetos)
            {
                if (o != null && !posicionesIniciales.ContainsKey(o))
                    posicionesIniciales[o] = o.transform.position;
            }
        }

        // Determinar la lista de objetos gestionados según la dificultad seleccionada
        GameSettings.Difficulty diff = GameSettings.SelectedDifficulty;
        GameObject[] chosen = diff == GameSettings.Difficulty.Facil ? easyObjects
                              : diff == GameSettings.Difficulty.Medio ? mediumObjects
                              : hardObjects;

        // Si no se asignaron en inspector, caer en fallback: usar primeros N de 'objetos'
        if (chosen == null || chosen.Length == 0)
        {
            int fallbackCount = GameSettings.TargetCount;
            Debug.LogWarning($"GameManager: no hay objetos asignados para dificultad {diff}. Usando primeros {fallbackCount} de 'objetos' como fallback.");
            chosen = new GameObject[Mathf.Min(fallbackCount, objetos != null ? objetos.Length : 0)];
            if (objetos != null)
                for (int i = 0; i < chosen.Length; i++) chosen[i] = objetos[i];
        }

        // construir gestionObjects preservando el orden dado en inspector (no eliminar duplicados)
        gestionObjects.Clear();
        foreach (var g in chosen)
        {
            if (g == null) continue;
            if (!gestionObjects.Contains(g))
                gestionObjects.Add(g);

            // asegure posición inicial almacenada para objetos elegidos que no estaban en 'objetos'
            if (!posicionesIniciales.ContainsKey(g))
                posicionesIniciales[g] = g.transform.position;
        }

        // inicializar estados golpeados y activar/desactivar objetos en escena conforme al set elegido
        objetosGolpeados.Clear();
        // si 'objetos' contiene la lista completa de GameObjects en escena, desactivar los no gestionados;
        // en caso contrario, intentamos no tocar objetos fuera de la lista 'objetos'.
        HashSet<GameObject> gestionSet = new HashSet<GameObject>(gestionObjects);
        if (objetos != null)
        {
            foreach (var o in objetos)
            {
                if (o == null) continue;
                bool shouldBeActive = gestionSet.Contains(o);
                if (o.activeSelf != shouldBeActive)
                    o.SetActive(shouldBeActive);

                // asegurar posición inicial
                if (posicionesIniciales.ContainsKey(o))
                    o.transform.position = posicionesIniciales[o];

                objetosGolpeados[o] = false;
            }
        }
        else
        {
            // si no hay 'objetos' definidos, sólo asegurar inicialización para gestionObjects
            foreach (var o in gestionObjects)
            {
                if (o == null) continue;
                if (!o.activeSelf) o.SetActive(true);
                if (posicionesIniciales.ContainsKey(o))
                    o.transform.position = posicionesIniciales[o];
                objetosGolpeados[o] = false;
            }
        }

        // iniciar lógica
        StartCoroutine(ControlarObjetos());
        juegoActivo = true;
        startTime = Time.time;
        lastRoundStartTime = startTime;
        nextRoundTime = startTime + roundLength;
        StartCoroutine(MonitorGameEnd());
    }

    IEnumerator ControlarObjetos()
    {
        while (juegoActivo)
        {
            if (gestionObjects.Count == 0)
            {
                yield return null;
                continue;
            }

            GameObject seleccionado;
            int attempts = 0;
            do
            {
                seleccionado = gestionObjects[Random.Range(0, gestionObjects.Count)];
                attempts++;
            } while (gestionObjects.Count > 1 && seleccionado == ultimoObjeto && attempts < 10);

            ultimoObjeto = seleccionado;

            // administrar secuencialmente: subir, esperar tiempoVisible (o hasta que sea golpeado), bajar
            yield return StartCoroutine(AdministrarObjeto(seleccionado));

            // esperar un tiempo entre objetos antes de elegir el siguiente
            float waited = 0f;
            while (waited < tiempoEntreObjetos && juegoActivo)
            {
                waited += Time.deltaTime;
                yield return null;
            }
        }
    }

    IEnumerator AdministrarObjeto(GameObject objeto)
    {
        if (objeto == null || !juegoActivo || !posicionesIniciales.ContainsKey(objeto) || !objeto.activeSelf)
            yield break;

        Vector3 posicionAbajo = posicionesIniciales[objeto];
        Vector3 posicionArriba = posicionAbajo + Vector3.up * alturaSubida;

        objetosGolpeados[objeto] = false;

        // Subir
        yield return StartCoroutine(MoverObjeto(objeto, posicionAbajo, posicionArriba));
        float tiempo = 0f;

        while (tiempo < tiempoVisible && !objetosGolpeados[objeto] && juegoActivo)
        {
            tiempo += Time.deltaTime;
            yield return null;
        }

        // Bajar
        yield return StartCoroutine(MoverObjeto(objeto, posicionArriba, posicionAbajo));
        objetosGolpeados[objeto] = false;
    }

    IEnumerator MoverObjeto(GameObject objeto, Vector3 inicio, Vector3 destino)
    {
        float tiempo = 0f;
        while (tiempo < 1f && juegoActivo)
        {
            tiempo += Time.deltaTime * velocidadMovimiento;
            objeto.transform.position = Vector3.Lerp(inicio, destino, Mathf.Clamp01(tiempo));
            yield return null;
        }
        objeto.transform.position = destino;
    }

    IEnumerator MonitorGameEnd()
    {
        while (juegoActivo)
        {
            // comprobar fin de ronda y avanzar si corresponde
            if (Time.time >= nextRoundTime)
            {
                AdvanceRound();
                nextRoundTime += roundLength;
            }

            // finalizar por tiempo total únicamente
            if (GameDuration > 0f && Time.time - startTime >= GameDuration)
                break;

            yield return null;
        }

        juegoActivo = false;
        Debug.Log("Juego finalizado por tiempo.");

        // reactivar todos los objetos y restaurar posiciones iniciales
        if (objetos != null)
        {
            foreach (var o in objetos)
            {
                if (o == null) continue;
                o.SetActive(true);
                if (posicionesIniciales.ContainsKey(o))
                    o.transform.position = posicionesIniciales[o];
            }
        }
        else
        {
            foreach (var o in gestionObjects)
            {
                if (o == null) continue;
                if (posicionesIniciales.ContainsKey(o))
                    o.transform.position = posicionesIniciales[o];
            }
        }

        yield return new WaitForSecondsRealtime(0.5f);
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    void AdvanceRound()
    {
        if (currentRound >= totalRounds)
            return;

        currentRound++;
        tiempoVisible = Mathf.Max(0.1f, tiempoVisible - 0.1f);
        lastRoundStartTime = Time.time;
        Debug.Log($"Ronda {currentRound}/{totalRounds} - nuevo tiempoVisible: {tiempoVisible:F2}s");
    }

    /// <summary>
    /// Marca como golpeado el muñeco correspondiente. Se busca coincidencia directa,
    /// luego por hijo/raíz para soportar colisiones sobre partes hijas del muñeco.
    /// </summary>
    public void BajarObjeto(GameObject objetoGolpeado)
    {
        if (objetoGolpeado == null) return;

        // Coincidencia directa
        if (objetosGolpeados.ContainsKey(objetoGolpeado))
        {
            objetosGolpeados[objetoGolpeado] = true;
            return;
        }

        // Si colisiona en una parte hija, buscar el padre gestionado
        foreach (var g in gestionObjects)
        {
            if (g == null) continue;
            if (objetoGolpeado.transform.IsChildOf(g.transform) || objetoGolpeado.transform.root.gameObject == g)
            {
                objetosGolpeados[g] = true;
                return;
            }
        }
    }
}