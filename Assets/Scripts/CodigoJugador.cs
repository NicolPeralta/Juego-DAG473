using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class CodigoJugador : MonoBehaviour
{
    public GameObject[] prefabs;
    public float velocidadLanzamiento = 12f, velocidadMultiplicador = 1.5f;
    public float alturaParabola = 6f, arcMultiplier = 1.2f;
    public Camera camara;
    public float tiempoEntreDisparos = 1f;
    [Min(0f)] public float tiempoEntreDisparosDerechos = 5f;
    public AudioClip launchSfx, impactSfxPrefab;
    [Range(2, 3)] public int defaultVolleySize = 2;
    public float volleySeparation = .28f;
    CannonAimer cannon;
    GameManager manager;
    int puntos;
    float lastShot = -Mathf.Infinity;
    float lastRightShot = -Mathf.Infinity;
    public int Puntos => puntos;
    public float ShotSpeed => Mathf.Max(1, velocidadLanzamiento * velocidadMultiplicador);

    void Awake()
    {
        cannon = GetComponent<CannonAimer>();
        manager = FindFirstObjectByType<GameManager>();
    }

    // Height follows flight time squared: close shots have a very shallow arc.
    public float ArcHeight(Vector3 start, Vector3 end)
    {
        float duration = Vector3.Distance(start, end) / ShotSpeed;
        float height = 9.81f * duration * duration / 8f;
        return Mathf.Min(Mathf.Max(0, alturaParabola), height * Mathf.Max(0, arcMultiplier));
    }

    void Update()
    {
        if (Time.timeScale == 0 || camara == null || Mouse.current == null) return;
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;
        bool spreadShot = Mouse.current.rightButton.wasPressedThisFrame && UsesSplitShotControls;
        if (!Mouse.current.leftButton.wasPressedThisFrame && !spreadShot) return;
        Ray ray = camara.ScreenPointToRay(Mouse.current.position.ReadValue());
        Vector3 target = cannon != null ? cannon.GetAimPoint(ray, out _) : ray.GetPoint(50);
        if (Mouse.current.leftButton.wasPressedThisFrame) TryFireAt(target);
        if (spreadShot) TryFireAt(target, true);
    }

    bool UsesSplitShotControls => GameSettings.SelectedDifficulty == GameSettings.Difficulty.Medio
        || GameSettings.SelectedDifficulty == GameSettings.Difficulty.Dificil;

    public bool TryFireAt(Vector3 target) => TryFireAt(target, false);

    public bool TryFireAt(Vector3 target, bool spreadShot)
    {
        if (Time.timeScale == 0 || (manager != null && !manager.IsPlaying)) return false;
        if (spreadShot && !UsesSplitShotControls) return false;
        float lastFireTime = spreadShot ? lastRightShot : lastShot;
        float cooldown = spreadShot ? tiempoEntreDisparosDerechos : tiempoEntreDisparos;
        if (Time.time - lastFireTime < cooldown || prefabs == null || prefabs.Length == 0) return false;
        Vector3 origin = cannon != null && cannon.Muzzle != null ? cannon.Muzzle.position : transform.position;
        spreadShot &= UsesSplitShotControls;
        int count = UsesSplitShotControls ? (spreadShot ? 3 : 1)
            : (manager != null ? manager.CandiesPerShot : Mathf.Clamp(defaultVolleySize, 2, 3));
        Vector3 direction = (target - origin).normalized;
        Vector3 side = Vector3.Cross(Vector3.up, direction).normalized;
        // Medium/hard: the centre shot follows the cursor; the others fan out horizontally.
        for (int i = 0; i < count; i++)
        {
            var prefab = prefabs[Random.Range(0, prefabs.Length)];
            if (prefab == null) continue;
            Vector3 start = UsesSplitShotControls ? origin
                : origin + side * ((i - (count - 1) * .5f) * volleySeparation);
            float angle = spreadShot && i > 0 ? (i == 1 ? -27f : 27f) : 0f;
            Vector3 shotTarget = angle == 0f ? target
                : origin + Quaternion.AngleAxis(angle, Vector3.up) * (target - origin);
            var candy = Instantiate(prefab, start, Quaternion.identity);
            var shot = candy.GetComponent<LanzadorObjeto>();
            if (shot == null) shot = candy.AddComponent<LanzadorObjeto>();
            shot.codigoJugador = this;
            shot.gameManager = manager;
            shot.impactSfx = impactSfxPrefab;
            shot.Launch(shotTarget, ShotSpeed, transform, ArcHeight(start, shotTarget));
            Destroy(candy, 12f);
        }
        AudioManager.Instance?.PlaySfx(launchSfx);
        ProjectileEffects.RedSmoke(origin, cannon != null ? cannon.MuzzleDirection : direction);
        if (spreadShot) lastRightShot = Time.time;
        else lastShot = Time.time;
        return true;
    }

    public void SumarPunto() => puntos++;
    public float GetCooldown01() => tiempoEntreDisparos <= 0 ? 0 : 1f - Mathf.Clamp01((Time.time - lastShot) / tiempoEntreDisparos);
    public float GetRightCooldown01() => tiempoEntreDisparosDerechos <= 0 ? 0 : 1f - Mathf.Clamp01((Time.time - lastRightShot) / tiempoEntreDisparosDerechos);
}
