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
    private Vector3 launchPosition;
    private float flightDuration;
    private float flightElapsed;
    private float arcHeight;
    private Transform owner;
    private Rigidbody body;

    public Vector3 AimPoint => aimPoint;

    public void Launch(Vector3 target, float speed, Transform launcher, float height)
    {
        aimPoint = target;
        launchPosition = transform.position;
        arcHeight = Mathf.Max(0.1f, height);
        flightDuration = Mathf.Max(0.1f, Vector3.Distance(launchPosition, target) / Mathf.Max(0.1f, speed));
        flightElapsed = 0;
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

    public Vector3 PositionOnArc(float normalizedTime)
    {
        float t = Mathf.Clamp01(normalizedTime);
        return Vector3.Lerp(launchPosition, aimPoint, t) + Vector3.up * (4f * arcHeight * t * (1f - t));
    }

    void FixedUpdate()
    {
        if (!inFlight || yaGolpeo) return;
        float endTime = Mathf.Min(flightElapsed + Time.fixedDeltaTime, flightDuration);
        // Subdivide the analytic curve so fast shots cannot skip thin targets.
        while (flightElapsed < endTime)
        {
            Vector3 start = PositionOnArc(flightElapsed / flightDuration);
            flightElapsed = Mathf.Min(flightElapsed + 0.005f, endTime);
            Vector3 next = PositionOnArc(flightElapsed / flightDuration);
            Vector3 delta = next - start;
            float distance = delta.magnitude;
            RaycastHit? nearest = null;
            if (distance > 0.000001f)
            {
                foreach (var hit in Physics.RaycastAll(start, delta / distance, distance + 0.00001f, ~0, QueryTriggerInteraction.Collide))
                {
                    if (hit.transform.IsChildOf(transform) || (owner != null && hit.transform.IsChildOf(owner))) continue;
                    if (hit.collider.GetComponentInParent<LanzadorObjeto>() != null) continue;
                    if (hit.collider.isTrigger && FindTarget(hit.collider) == null && !hit.collider.CompareTag("Obstaculo")) continue;
                    if (!nearest.HasValue || hit.distance < nearest.Value.distance) nearest = hit;
                }
            }
            if (nearest.HasValue)
            {
                var hit = nearest.Value;
                transform.position = hit.point;
                HandleHit(hit.collider, hit.point);
                if (!yaGolpeo) FinishFlight();
                return;
            }
            if (body != null) body.position = next;
            else transform.position = next;
        }
        if (flightElapsed >= flightDuration)
        {
            // A curve ending exactly on a mesh may touch it without crossing a
            // front-facing triangle. Confirm contact at the endpoint as well.
            foreach (var collider in Physics.OverlapSphere(aimPoint, 0.01f, ~0, QueryTriggerInteraction.Collide))
            {
                if (FindTarget(collider) == null) continue;
                transform.position = aimPoint;
                HandleHit(collider, aimPoint);
                if (yaGolpeo) return;
            }
            FinishFlight();
        }
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
