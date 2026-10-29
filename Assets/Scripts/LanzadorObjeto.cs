using UnityEngine;

public class LanzadorObjeto : MonoBehaviour
{
    public GameObject cuboObjetivo;
    public CodigoJugador codigoJugador;
    public GameManager gameManager;
    public AudioClip impactSfx;
    private bool yaGolpeo;
    private bool inFlight;
    private Vector3 aimPoint;
    private float flightSpeed;
    private Transform owner;
    private Rigidbody body;

    public Vector3 AimPoint => aimPoint;

    public void Launch(Vector3 target, float speed, Transform launcher)
    {
        aimPoint = target;
        flightSpeed = Mathf.Max(0.1f, speed);
        owner = launcher;
        body = GetComponent<Rigidbody>();
        if (body != null)
        {
            body.linearVelocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
            body.useGravity = false;
            body.isKinematic = true;
        }
        // Sweep the projectile's centre instead of allowing its large visual
        // collider to score early, to one side of the cursor.
        foreach (var collider in GetComponentsInChildren<Collider>()) collider.enabled = false;
        inFlight = true;
    }

    void FixedUpdate()
    {
        if (!inFlight || yaGolpeo) return;
        Vector3 start = body != null ? body.position : transform.position;
        Vector3 delta = aimPoint - start;
        float remaining = delta.magnitude;
        if (remaining <= 0.0001f) { FinishFlight(); return; }
        Vector3 direction = delta / remaining;
        float distance = Mathf.Min(flightSpeed * Time.fixedDeltaTime, remaining);
        RaycastHit? nearest = null;
        // RaycastAll keeps the sweep correct even with many overlapping colliders.
        foreach (var hit in Physics.RaycastAll(start, direction, distance + 0.0001f, ~0, QueryTriggerInteraction.Collide))
        {
            if (hit.transform.IsChildOf(transform) || (owner != null && hit.transform.IsChildOf(owner))) continue;
            if (hit.collider.GetComponentInParent<LanzadorObjeto>() != null) continue;
            if (hit.collider.isTrigger && FindTarget(hit.collider) == null && !hit.collider.CompareTag("Obstaculo")) continue;
            if (!nearest.HasValue || hit.distance < nearest.Value.distance) nearest = hit;
        }
        if (nearest.HasValue)
        {
            var hit = nearest.Value;
            transform.position = hit.point;
            HandleHit(hit.collider, hit.point);
            if (!yaGolpeo) FinishFlight();
            return;
        }
        Vector3 next = start + direction * distance;
        if (body != null) body.position = next;
        else transform.position = next;
        if (distance >= remaining) FinishFlight();
    }

    void FinishFlight()
    {
        inFlight = false;
        yaGolpeo = true;
        Destroy(gameObject);
    }

    void Awake() => gameManager = FindFirstObjectByType<GameManager>();

    void OnTriggerEnter(Collider other)
    {
        if (inFlight) return;
        HandleHit(other, other != null ? other.ClosestPoint(transform.position) : transform.position);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (inFlight) return;
        HandleHit(collision.collider, collision.contactCount > 0 ? collision.GetContact(0).point : transform.position);
    }

    void HandleHit(Collider other, Vector3 point)
    {
        if (yaGolpeo || other == null) return;
        GameObject target = FindTarget(other);
        if (target == null && cuboObjetivo != null && other.transform.IsChildOf(cuboObjetivo.transform)) target = cuboObjetivo;
        if (target != null)
        {
            yaGolpeo = true;
            ProjectileEffects.Shatter(point, ProjectileEffects.ProjectileColor(gameObject));
            if (codigoJugador != null) codigoJugador.SumarPunto();
            if (gameManager != null) gameManager.BajarObjeto(target);
            AudioManager.Instance?.PlaySfx(impactSfx);
            Destroy(gameObject);
        }
        else if (other.CompareTag("Obstaculo") || other.CompareTag("Player"))
        {
            yaGolpeo = true;
            Destroy(gameObject);
        }
    }

    public static GameObject FindTarget(Collider collider)
    {
        for (Transform current = collider != null ? collider.transform : null; current != null; current = current.parent)
            if (current.CompareTag("Cubo")) return current.gameObject;
        return null;
    }
}
