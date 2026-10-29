using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
public class CannonAimer : MonoBehaviour
{
    public Camera aimCamera;
    public Vector3 offsetEuler = new Vector3(0, 90, 0);
    public bool onlyRotateY = false;
    public bool smooth = true;
    public float rotationSpeed = 180f;
    public float aimDistance = 50f;
    public bool useClosestPointOnRay = false;
    public LayerMask groundLayerMask = ~0;
    public float minElevation = -15f;
    public float maxElevation = 80f;
    public Vector3 barrelPivotLocalPosition = new Vector3(1.59f, 2.34f, 0.1f);
    public Vector3 muzzleLocalPosition = new Vector3(-1.72f, 2.34f, 0.1f);
    public Transform Muzzle { get; private set; }
    public Transform BarrelPivot { get; private set; }
    public Vector3 MuzzleDirection => BarrelPivot != null ? -BarrelPivot.right : transform.forward;

    void Awake()
    {
        if (aimCamera == null) aimCamera = Camera.main;
        var body = GetComponent<Rigidbody>();
        if (body != null) { body.isKinematic = true; body.useGravity = false; }

        BarrelPivot = new GameObject("Elevacion del tubo").transform;
        BarrelPivot.SetParent(transform, false);
        BarrelPivot.localPosition = barrelPivotLocalPosition;
        // Keep the carriage and wheels level; only the barrel tilts.
        foreach (string name in new[] { "Cylinder", "Cylinder.002", "Cylinder.003", "Cylinder.004" })
        {
            var part = transform.Find(name);
            if (part != null) part.SetParent(BarrelPivot, true);
        }
        Muzzle = new GameObject("Boca del canon").transform;
        Muzzle.SetParent(BarrelPivot, false);
        Muzzle.localPosition = muzzleLocalPosition - barrelPivotLocalPosition;
    }

    void Update()
    {
        if (Time.timeScale == 0 || aimCamera == null || Mouse.current == null) return;
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;
        Ray ray = aimCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        AimAt(GetAimPoint(ray, out _), Time.deltaTime);
    }

    public void AimAt(Vector3 point, float deltaTime)
    {
        if (BarrelPivot == null) return;
        Vector3 direction = point - BarrelPivot.position;
        var shooter = GetComponent<CodigoJugador>();
        if (shooter != null) direction.y += 4f * Mathf.Max(0.1f, shooter.alturaParabola * shooter.arcMultiplier);
        Vector3 horizontal = Vector3.ProjectOnPlane(direction, Vector3.up);
        if (horizontal.sqrMagnitude < 0.0001f) return;
        Quaternion yaw = Quaternion.LookRotation(horizontal, Vector3.up) * Quaternion.Euler(offsetEuler);
        transform.rotation = smooth ? Quaternion.RotateTowards(transform.rotation, yaw, rotationSpeed * deltaTime) : yaw;
        float elevation = onlyRotateY ? 0 : Mathf.Clamp(Mathf.Atan2(direction.y, horizontal.magnitude) * Mathf.Rad2Deg, minElevation, maxElevation);
        Quaternion pitch = Quaternion.AngleAxis(-elevation, Vector3.forward);
        BarrelPivot.localRotation = smooth ? Quaternion.RotateTowards(BarrelPivot.localRotation, pitch, rotationSpeed * deltaTime) : pitch;
    }

    public Vector3 GetAimPoint(Ray ray, out Collider hitCollider)
    {
        hitCollider = null;
        float nearest = float.PositiveInfinity;
        Vector3 point = ray.GetPoint(aimDistance);
        foreach (var hit in Physics.RaycastAll(ray, aimDistance, groundLayerMask, QueryTriggerInteraction.Collide))
        {
            if (hit.transform.IsChildOf(transform) || hit.collider.GetComponentInParent<LanzadorObjeto>() != null) continue;
            if (hit.collider.isTrigger && LanzadorObjeto.FindTarget(hit.collider) == null && !hit.collider.CompareTag("Obstaculo")) continue;
            if (hit.distance >= nearest) continue;
            nearest = hit.distance;
            point = hit.point;
            hitCollider = hit.collider;
        }
        return point;
    }
}
