using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public GameObject[] objetos, easyObjects, mediumObjects, hardObjects;
    public float alturaSubida = 4f;
    public float velocidadMovimiento = 5f, tiempoVisible = 3f, tiempoEntreObjetos = 2f;
    public float roundLength = 30f;
    [Min(1)] public int levelCount = 3;
    public const int RoundsPerLevel = 3;
    public bool IsPlaying { get; private set; }
    public int CurrentLevel { get; private set; } = 1;
    public int CurrentRound { get; private set; } = 1;
    public int TotalRounds => RoundsPerLevel;
    public int TotalLevels => Mathf.Max(1, levelCount);
    public int CandiesPerShot => CurrentLevel == 1 ? 2 : 3;
    public float RoundLength => roundLength;
    public float CurrentRoundElapsed { get; private set; }
    public float GameDuration => TotalLevels * RoundsPerLevel * roundLength;
    public IReadOnlyCollection<GameObject> ActiveTargets => activeTargets;
    public GameObject ActiveTarget
    {
        get { foreach (var target in activeTargets) return target; return null; }
    }
    readonly List<GameObject> targets = new List<GameObject>();
    readonly Dictionary<GameObject, Vector3> origins = new Dictionary<GameObject, Vector3>();
    readonly HashSet<GameObject> scored = new HashSet<GameObject>();
    readonly HashSet<GameObject> activeTargets = new HashSet<GameObject>();
    class Appearance
    {
        public GameObject target;
        public Vector3 start;
        public int phase;
        public float elapsed;
    }
    GameObject previous;
    Coroutine appearances;

    void Start()
    {
        GameSettings.ApplyDifficulty(GameSettings.SelectedDifficulty);
        var chosen = GameSettings.SelectedDifficulty == GameSettings.Difficulty.Facil ? easyObjects : mediumObjects;
        if (chosen == null || chosen.Length == 0) chosen = objetos;
        if (objetos != null)
            foreach (var target in objetos)
                if (target != null && !origins.ContainsKey(target)) origins.Add(target, target.transform.position);
        if (chosen != null)
            foreach (var target in chosen)
            {
                if (target == null || targets.Contains(target) || targets.Count >= GameSettings.TargetCount) continue;
                targets.Add(target);
                if (!origins.ContainsKey(target)) origins.Add(target, target.transform.position);
            }
        foreach (var pair in origins) pair.Key.SetActive(targets.Contains(pair.Key));
        StartCoroutine(RunLevels());
    }

    public void ApplyProgression()
    {
        float progression = 1f + (CurrentLevel - 1) * .2f + (CurrentRound - 1) * .06f;
        velocidadMovimiento = Mathf.Min(18f, GameSettings.VelocidadMovimiento * progression);
        tiempoVisible = Mathf.Max(.7f, GameSettings.TiempoVisible / progression);
        tiempoEntreObjetos = Mathf.Max(.3f, GameSettings.TiempoEntreObjetos / progression);
    }

    IEnumerator RunLevels()
    {
        for (int level = 1; level <= TotalLevels; level++)
        {
            CurrentLevel = level;
            for (int round = 1; round <= RoundsPerLevel; round++)
            {
                CurrentRound = round;
                CurrentRoundElapsed = 0;
                ApplyProgression();
                var banner = gameObject.AddComponent<StartCountdown>();
                yield return banner.Play($"NIVEL {level}\nRONDA {round} / {RoundsPerLevel}", level == 1 && round == 1);
                IsPlaying = true;
                appearances = StartCoroutine(ControlTargets());
                while (CurrentRoundElapsed < roundLength)
                {
                    yield return null;
                    CurrentRoundElapsed = Mathf.Min(roundLength, CurrentRoundElapsed + Time.deltaTime);
                }
                IsPlaying = false;
                if (appearances != null) StopCoroutine(appearances);
                activeTargets.Clear();
                foreach (var shot in FindObjectsByType<LanzadorObjeto>(FindObjectsSortMode.None)) Destroy(shot.gameObject);
                yield return LowerAll();
            }
        }
        var finish = gameObject.AddComponent<StartCountdown>();
        yield return finish.PlayMessage("¡PARTIDA COMPLETADA!", 2f);
        Time.timeScale = 1;
        SceneManager.LoadScene("MainMenu");
    }

    IEnumerator ControlTargets()
    {
        while (IsPlaying)
        {
            if (targets.Count == 0) { yield return null; continue; }
            var available = new List<GameObject>(targets);
            var wave = new List<Appearance>();
            int count = Mathf.Clamp(GameSettings.ConcurrentTargets, 1, available.Count);
            for (int i = 0; i < count; i++)
            {
                int index = Random.Range(0, available.Count);
                if (i == 0 && available.Count > 1 && available[index] == previous) index = (index + 1) % available.Count;
                var target = available[index]; available.RemoveAt(index);
                if (i == 0) previous = target;
                scored.Remove(target); activeTargets.Add(target);
                wave.Add(new Appearance { target = target, start = origins[target] });
            }
            // One coroutine owns the entire wave, so a round break stops all children together.
            bool complete = false;
            while (!complete)
            {
                complete = true;
                foreach (var item in wave)
                {
                    if (item.phase == 3) continue;
                    complete = false;
                    if (item.phase < 2 && (scored.Contains(item.target) || (item.phase == 1 && item.elapsed >= tiempoVisible)))
                    {
                        activeTargets.Remove(item.target);
                        item.phase = 2; item.elapsed = 0; item.start = item.target.transform.position;
                    }
                    if (item.phase == 1) { item.elapsed += Time.deltaTime; continue; }
                    item.elapsed = Mathf.Min(1, item.elapsed + Time.deltaTime * velocidadMovimiento);
                    Vector3 end = origins[item.target] + (item.phase == 0 ? Vector3.up * alturaSubida : Vector3.zero);
                    item.target.transform.position = Vector3.Lerp(item.start, end, Mathf.SmoothStep(0, 1, item.elapsed));
                    if (item.elapsed >= 1) { item.phase++; item.elapsed = 0; }
                }
                yield return null;
            }
            yield return new WaitForSeconds(tiempoEntreObjetos);
        }
    }

    IEnumerator LowerAll()
    {
        var starts = new Dictionary<GameObject, Vector3>();
        foreach (var target in targets) starts[target] = target.transform.position;
        float t = 0;
        while (t < 1)
        {
            t = Mathf.Min(1, t + Time.deltaTime / .35f);
            foreach (var target in targets) target.transform.position = Vector3.Lerp(starts[target], origins[target], Mathf.SmoothStep(0, 1, t));
            yield return null;
        }
    }

    // One score per appearance, even when a volley lands simultaneously.
    public bool TryHitTarget(GameObject hit)
    {
        if (!IsPlaying || hit == null) return false;
        foreach (var target in activeTargets)
            if (hit == target || hit.transform.IsChildOf(target.transform)) return scored.Add(target);
        return false;
    }
    public void BajarObjeto(GameObject hit) => TryHitTarget(hit);
}
