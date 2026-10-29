using UnityEngine;

public static class ProjectileEffects
{
    static Material smokeMaterial;
    static Material fragmentMaterial;
    static Mesh fragmentMesh;

    public static void RedSmoke(Vector3 position, Vector3 direction)
    {
        if (smokeMaterial == null) smokeMaterial = Resources.Load<Material>("DAG473Effects/Smoke");
        var particles = Create("Humo rojo del canon", position, smokeMaterial, 1.6f, 30);
        particles.transform.rotation = Quaternion.LookRotation(direction);
        var main = particles.main;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.7f, 1.5f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(1.2f, 3.2f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.3f, 0.65f);
        main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
        main.startColor = new ParticleSystem.MinMaxGradient(new Color(0.65f, 0.015f, 0.025f, 0.8f), new Color(1f, 0.08f, 0.04f, 0.9f));
        var shape = particles.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 22f;
        shape.radius = 0.2f;
        var velocity = particles.velocityOverLifetime;
        velocity.enabled = true;
        velocity.space = ParticleSystemSimulationSpace.World;
        velocity.y = 0.8f;
        var size = particles.sizeOverLifetime;
        size.enabled = true;
        size.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0, 0.6f, 1, 2.8f));
        Fade(particles);
        particles.Play();
    }

    public static void Shatter(Vector3 position, Color color)
    {
        if (fragmentMaterial == null) fragmentMaterial = Resources.Load<Material>("DAG473Effects/Fragments");
        var particles = Create("Fragmentos del proyectil", position, fragmentMaterial, 1.4f, 32);
        var main = particles.main;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.45f, 1.2f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(2f, 5f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.08f, 0.22f);
        main.gravityModifier = 0.65f;
        color.a = 1;
        main.startColor = new ParticleSystem.MinMaxGradient(color, Color.Lerp(color, Color.white, 0.35f));
        main.startRotation3D = true;
        main.startRotationX = new ParticleSystem.MinMaxCurve(0f, 6.28f);
        main.startRotationY = new ParticleSystem.MinMaxCurve(0f, 6.28f);
        main.startRotationZ = new ParticleSystem.MinMaxCurve(0f, 6.28f);
        var shape = particles.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.18f;
        var renderer = particles.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Mesh;
        renderer.mesh = GetFragmentMesh();
        var rotation = particles.rotationOverLifetime;
        rotation.enabled = true;
        rotation.z = new ParticleSystem.MinMaxCurve(-4f, 4f);
        var size = particles.sizeOverLifetime;
        size.enabled = true;
        size.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.EaseInOut(0, 1, 1, 0));
        Fade(particles);
        particles.Play();
    }

    public static Color ProjectileColor(GameObject projectile)
    {
        var renderer = projectile.GetComponentInChildren<Renderer>();
        var material = renderer != null ? renderer.sharedMaterial : null;
        if (material != null && material.HasProperty("_BaseColor")) return material.GetColor("_BaseColor");
        if (material != null && material.HasProperty("_Color")) return material.color;
        return new Color(1f, 0.7f, 0.2f);
    }

    static ParticleSystem Create(string name, Vector3 position, Material material, float duration, short count)
    {
        var go = new GameObject(name);
        go.transform.position = position;
        var particles = go.AddComponent<ParticleSystem>();
        particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        var main = particles.main;
        main.playOnAwake = false;
        main.loop = false;
        main.duration = duration;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = 64;
        main.stopAction = ParticleSystemStopAction.Destroy;
        var emission = particles.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, count) });
        var renderer = particles.GetComponent<ParticleSystemRenderer>();
        renderer.sharedMaterial = material;
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        renderer.receiveShadows = false;
        return particles;
    }

    static void Fade(ParticleSystem particles)
    {
        var gradient = new Gradient();
        gradient.SetKeys(new[] { new GradientColorKey(Color.white, 0), new GradientColorKey(Color.white, 1) },
            new[] { new GradientAlphaKey(0.9f, 0), new GradientAlphaKey(0.65f, 0.35f), new GradientAlphaKey(0, 1) });
        var color = particles.colorOverLifetime;
        color.enabled = true;
        color.color = gradient;
    }

    static Mesh GetFragmentMesh()
    {
        if (fragmentMesh != null) return fragmentMesh;
        fragmentMesh = new Mesh { name = "Fragmento" };
        fragmentMesh.vertices = new[] { new Vector3(-.5f,-.5f,-.5f), new Vector3(.5f,-.5f,-.5f), new Vector3(.5f,.5f,-.5f), new Vector3(-.5f,.5f,-.5f), new Vector3(-.5f,-.5f,.5f), new Vector3(.5f,-.5f,.5f), new Vector3(.5f,.5f,.5f), new Vector3(-.5f,.5f,.5f) };
        fragmentMesh.triangles = new[] { 0,2,1,0,3,2, 4,5,6,4,6,7, 0,1,5,0,5,4, 3,7,6,3,6,2, 0,4,7,0,7,3, 1,2,6,1,6,5 };
        fragmentMesh.RecalculateNormals();
        fragmentMesh.RecalculateBounds();
        return fragmentMesh;
    }
}
