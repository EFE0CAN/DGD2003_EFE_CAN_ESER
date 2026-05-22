using UnityEngine;

/// <summary>
/// Tek seferlik duman patlaması — kod ile ParticleSystem oluşturur, prefab gerekmez.
/// </summary>
public static class SmokeEffect
{
    private static Material _material;

    public static void Play(Vector3 worldPosition, float scale = 1f)
    {
        GameObject root = new GameObject("SmokeBurst");
        root.transform.position = worldPosition;

        ParticleSystem ps = root.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        ParticleSystem.MainModule main = ps.main;
        main.duration = 1.4f;
        main.loop = false;
        main.playOnAwake = false;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.8f * scale, 1.6f * scale);
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.6f * scale, 1.8f * scale);
        main.startSize = new ParticleSystem.MinMaxCurve(0.35f * scale, 0.9f * scale);
        main.startColor = new Color(0.75f, 0.75f, 0.75f, 0.55f);
        main.gravityModifier = -0.15f;
        main.maxParticles = 120;

        ParticleSystem.EmissionModule emission = ps.emission;
        emission.rateOverTime = 0f;
        emission.SetBursts(new[]
        {
            new ParticleSystem.Burst(0f, (short)(30 * scale)),
            new ParticleSystem.Burst(0.08f, (short)(18 * scale))
        });

        ParticleSystem.ShapeModule shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.25f * scale;

        ParticleSystem.SizeOverLifetimeModule sizeOverLifetime = ps.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        AnimationCurve sizeCurve = new AnimationCurve(
            new Keyframe(0f, 0.4f),
            new Keyframe(0.35f, 1f),
            new Keyframe(1f, 1.8f));
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

        ParticleSystem.ColorOverLifetimeModule colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new[]
            {
                new GradientColorKey(new Color(0.9f, 0.9f, 0.9f), 0f),
                new GradientColorKey(new Color(0.55f, 0.55f, 0.55f), 0.5f),
                new GradientColorKey(new Color(0.35f, 0.35f, 0.35f), 1f)
            },
            new[]
            {
                new GradientAlphaKey(0.65f, 0f),
                new GradientAlphaKey(0.35f, 0.55f),
                new GradientAlphaKey(0f, 1f)
            });
        colorOverLifetime.color = gradient;

        ParticleSystem.VelocityOverLifetimeModule velocity = ps.velocityOverLifetime;
        velocity.enabled = true;
        velocity.space = ParticleSystemSimulationSpace.Local;
        velocity.y = new ParticleSystem.MinMaxCurve(0.4f * scale, 1.2f * scale);

        ParticleSystem.NoiseModule noise = ps.noise;
        noise.enabled = true;
        noise.strength = 0.35f;
        noise.frequency = 0.7f;

        ParticleSystemRenderer renderer = ps.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        renderer.material = GetMaterial();

        ps.Play();
        Object.Destroy(root, 3f);
    }

    private static Material GetMaterial()
    {
        if (_material != null) return _material;

        Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
        if (shader == null) shader = Shader.Find("Particles/Standard Unlit");
        if (shader == null) shader = Shader.Find("Sprites/Default");

        _material = new Material(shader);
        _material.SetColor("_BaseColor", Color.white);
        _material.SetColor("_Color", Color.white);
        return _material;
    }
}
