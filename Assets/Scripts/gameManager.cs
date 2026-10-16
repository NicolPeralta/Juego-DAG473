using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public GameObject[] objetos;

    // valores por defecto (se sobrescriben desde GameSettings)
    public float alturaSubida = 4f;
    public float velocidadMovimiento = 5f;
    public float tiempoVisible = 3f;
    public float tiempoEntreObjetos = 2f;

    // parámetros de juego (se ajustan desde GameSettings)
    public float GameDuration { get; private set; } = 180f;

    // rondas
    public float roundLength = 30f; // cada ronda dura 30 segundos
    private int totalRounds = 1;
    private int currentRound = 1;

    private int targetCount = 6;

    private Vector3[] posicionesIniciales;
    private bool[] objetosGolpeados;
    private List<int> gestionIndices = new List<int>();
    private int ultimoObjeto = -1;
    private bool juegoActivo = true;

    private float startTime;
    private float nextRoundTime;

    // Exponer tiempo restante y rondas para la UI
    public float RemainingTime
    {
        get
        {
            if (!juegoActivo) return 0f;
            float elapsed = Time.time - startTime;
            return Mathf.Max(0f, GameDuration - elapsed);
        }
    }

    // Tiempo restante de la ronda actual (usado por TimeUI)
    public float CurrentRoundRemaining
    {
        get
        {
            if (!juegoActivo) return 0f;
            return Mathf.Max(0f, nextRoundTime - Time.time);
        }
    }

    public float RoundLength => roundLength;

    public int CurrentRound => currentRound;
    public int TotalRounds => totalRounds;

    void Start()
    {
        // aplicar ajustes desde GameSettings (GameSettings es estático)
        GameDuration = GameSettings.GameDuration;
        targetCount = GameSettings.TargetCount;
        tiempoVisible = GameSettings.TiempoVisible;
        tiempoEntreObjetos = GameSettings.TiempoEntreObjetos;
        velocidadMovimiento = GameSettings.VelocidadMovimiento;

        if (objetos == null || objetos.Length == 0)
        {
            Debug.LogWarning("GameManager: no hay objetos asignados.");
            return;
        }

        // calcular rondas totales según duración total del juego
        totalRounds = Mathf.Max(1, Mathf.CeilToInt(GameDuration / Mathf.Max(0.01f, roundLength)));
        currentRound = 1;

        // seleccionar índices únicos a gestionar (targetCount o menos)
        int count = Mathf.Clamp(targetCount, 1, objetos.Length);
        gestionIndices.Clear();
        List<int> pool = new List<int>();
        for (int i = 0; i < objetos.Length; i++) pool.Add(i);
        for (int i = 0; i < count; i++)
        {
            int r = Random.Range(0, pool.Count);
            gestionIndices.Add(pool[r]);
            pool.RemoveAt(r);
        }

        posicionesIniciales = new Vector3[objetos.Length];
        objetosGolpeados = new bool[objetos.Length];

        for (int i = 0; i < objetos.Length; i++)
        {
            posicionesIniciales[i] = objetos[i].transform.position;
            objetosGolpeados[i] = false;
            // asegurar que los que no gestionamos queden abajo
            if (!gestionIndices.Contains(i))
            {
                objetos[i].transform.position = posicionesIniciales[i];
            }
        }

        // iniciar el bucle principal que levanta objetos uno a la vez
        StartCoroutine(ControlarObjetos());
        juegoActivo = true;
        startTime = Time.time;
        nextRoundTime = startTime + roundLength;
        StartCoroutine(MonitorGameEnd());
    }

    IEnumerator ControlarObjetos()
    {
        while (juegoActivo)
        {
            if (gestionIndices.Count == 0)
            {
                yield return null;
                continue;
            }

            int indice;
            do
            {
                indice = gestionIndices[Random.Range(0, gestionIndices.Count)];
            }
            while (gestionIndices.Count > 1 && indice == ultimoObjeto);

            ultimoObjeto = indice;

            // administrar secuencialmente: subir, esperar tiempoVisible (o hasta que sea golpeado), bajar
            yield return StartCoroutine(AdministrarObjeto(indice));

            // esperar un tiempo entre objetos antes de elegir el siguiente
            float waited = 0f;
            while (waited < tiempoEntreObjetos && juegoActivo)
            {
                waited += Time.deltaTime;
                yield return null;
            }
        }
    }

    IEnumerator AdministrarObjeto(int indice)
    {
        GameObject objeto = objetos[indice];

        Vector3 posicionAbajo = posicionesIniciales[indice];
        Vector3 posicionArriba = posicionAbajo + Vector3.up * alturaSubida;

        objetosGolpeados[indice] = false;

        // Subir
        yield return StartCoroutine(MoverObjeto(objeto, posicionAbajo, posicionArriba));
        float tiempo = 0f;

        while (tiempo < tiempoVisible && !objetosGolpeados[indice] && juegoActivo)
        {
            tiempo += Time.deltaTime;
            yield return null;
        }

        // Bajar
        yield return StartCoroutine(MoverObjeto(objeto, posicionArriba, posicionAbajo));
        objetosGolpeados[indice] = false;
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

        // dejar todo en estado abajo y esperar un poco en tiempo real
        foreach (int idx in gestionIndices)
        {
            if (idx >= 0 && idx < objetos.Length)
            {
                objetos[idx].transform.position = posicionesIniciales[idx];
            }
        }

        yield return new WaitForSecondsRealtime(0.5f);
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    void AdvanceRound()
    {
        // no avanzar más allá de totalRounds
        if (currentRound >= totalRounds)
            return;

        currentRound++;
        // reducir tiempoVisible en 0.1 cada ronda (mínimo 0.1)
        tiempoVisible = Mathf.Max(0.1f, tiempoVisible - 0.1f);
        Debug.Log($"Ronda {currentRound}/{totalRounds} - nuevo tiempoVisible: {tiempoVisible:F2}s");
    }

    // llamada por LanzadorObjeto cuando golpea (mantener funcionalidad)
    public void BajarObjeto(GameObject objetoGolpeado)
    {
        if (objetoGolpeado == null)
            return;
        for (int i = 0; i < objetos.Length; i++)
        {
            if (objetos[i] == objetoGolpeado)
            {
                objetosGolpeados[i] = true;
                return;
            }
        }
        for (int i = 0; i < objetos.Length; i++)
        {
            if (objetoGolpeado.transform.IsChildOf(objetos[i].transform))
            {
                objetosGolpeados[i] = true;
                return;
            }
        }
        for (int i = 0; i < objetos.Length; i++)
        {
            if (objetoGolpeado.transform.root.gameObject == objetos[i])
            {
                objetosGolpeados[i] = true;
                return;
            }
        }
    }
}