using UnityEngine;

public class MenuCannonVfx : MonoBehaviour
{
    public float interval = 4f;
    public float flashIntensity = 9f;
    public ParticleSystem Smoke { get; private set; }
    public Light Flash { get; private set; }
    float nextShot;
    void Awake()
    {
        var go = new GameObject("Humo rojo de portada"); go.layer = gameObject.layer;
        go.transform.SetParent(transform, false);
        Smoke = go.AddComponent<ParticleSystem>(); Smoke.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        var main = Smoke.main; main.playOnAwake = false; main.loop = true; main.useUnscaledTime = true;
        main.startLifetime = new ParticleSystem.MinMaxCurve(1.5f, 2.7f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(1.3f, 3.2f);
        main.startSize = new ParticleSystem.MinMaxCurve(.35f, .85f);
        main.startColor = new ParticleSystem.MinMaxGradient(new Color(.8f, .015f, .025f, .7f), new Color(1, .08f, .025f, .8f));
        main.simulationSpace = ParticleSystemSimulationSpace.World; main.maxParticles = 120;
        var emission = Smoke.emission; emission.rateOverTime = 0;
        var shape = Smoke.shape; shape.shapeType = ParticleSystemShapeType.Cone; shape.angle = 18; shape.radius = .3f;
        var velocity = Smoke.velocityOverLifetime; velocity.enabled = true; velocity.space = ParticleSystemSimulationSpace.World; velocity.y = .6f;
        var size = Smoke.sizeOverLifetime; size.enabled = true; size.size = new ParticleSystem.MinMaxCurve(1, AnimationCurve.Linear(0, .6f, 1, 2.4f));
        var gradient = new Gradient(); gradient.SetKeys(new[]{new GradientColorKey(Color.white,0),new GradientColorKey(Color.white,1)},new[]{new GradientAlphaKey(.8f,0),new GradientAlphaKey(0,1)});
        var color = Smoke.colorOverLifetime; color.enabled = true; color.color = gradient;
        var renderer = Smoke.GetComponent<ParticleSystemRenderer>();
        renderer.sharedMaterial = Resources.Load<Material>("DAG473Effects/Smoke");
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; renderer.receiveShadows = false;
        var lightObject = new GameObject("Destello del cañón"); lightObject.layer = gameObject.layer; lightObject.transform.SetParent(transform,false); lightObject.transform.localPosition=Vector3.forward*.3f;
        Flash = lightObject.AddComponent<Light>(); Flash.type = LightType.Point; Flash.color = new Color(1,.22f,.045f); Flash.range = 10; Flash.intensity = 0; Flash.shadows = LightShadows.None;
        Smoke.Play(); nextShot = Time.unscaledTime + 1;
    }
    public void Pulse() { Smoke.Emit(38); Flash.intensity = flashIntensity; }
    void Update()
    {
        Flash.intensity = Mathf.MoveTowards(Flash.intensity, 0, Time.unscaledDeltaTime * flashIntensity / .22f);
        if (Time.unscaledTime >= nextShot) { Pulse(); nextShot = Time.unscaledTime + Mathf.Max(1,interval); }
    }
}
