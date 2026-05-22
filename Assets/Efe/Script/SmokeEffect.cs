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

    /// <summary>
    /// Karakterin tüm gövdesini saran beyaz-gri duman (süre bitince vb.).
    /// </summary>
    public static void PlayBodySmokeAura(Transform target, float duration, float scale = 1.35f)
    {
        if (target == null || duration <= 0f) return;

        float bodyHeight = GetBodyHeight(target);
        float h = bodyHeight * scale;

        GameObject root = new GameObject("BodySmokeAura");
        root.transform.SetParent(target, false);
        root.transform.localPosition = Vector3.zero;
        root.transform.localRotation = Quaternion.identity;

        // Gövde boyunca + etrafında çoklu emiter — her yönde görünür duman
        Vector3[] localOffsets =
        {
            new Vector3(0f, h * 0.92f, 0f),
            new Vector3(0f, h * 0.58f, 0f),
            new Vector3(0f, h * 0.28f, 0f),
            new Vector3(0f, h * 0.05f, 0f),
            new Vector3(h * 0.32f, h * 0.5f, 0f),
            new Vector3(-h * 0.32f, h * 0.5f, 0f),
            new Vector3(0f, h * 0.5f, h * 0.28f),
            new Vector3(0f, h * 0.5f, -h * 0.28f),
        };

        float[] radiusMul = { 0.55f, 0.75f, 0.7f, 0.65f, 0.6f, 0.6f, 0.65f, 0.65f };
        float[] rateMul = { 1.1f, 1.4f, 1.25f, 1f, 1.15f, 1.15f, 1.2f, 1.2f };

        for (int i = 0; i < localOffsets.Length; i++)
            CreateBodySmokeEmitter(root.transform, localOffsets[i], duration, scale, radiusMul[i], rateMul[i]);

        // Tüm vücudu saran büyük dış hacim
        CreateBodySmokeEmitter(root.transform, new Vector3(0f, h * 0.48f, 0f), duration, scale * 1.15f, 1.35f, 2.2f, wrapVolume: true);

        Object.Destroy(root, duration + 1.2f);
    }

    public static void PlayRedAuraAround(Transform target, float duration, float scale = 1.35f)
        => PlayBodySmokeAura(target, duration, scale);

    private static float GetBodyHeight(Transform target)
    {
        CharacterController cc = target.GetComponent<CharacterController>();
        if (cc == null)
            cc = target.GetComponentInParent<CharacterController>();

        if (cc != null && cc.height > 0.1f)
            return cc.height;

        return 1.75f;
    }

    private static void CreateBodySmokeEmitter(
        Transform parent,
        Vector3 localOffset,
        float duration,
        float scale,
        float radiusMul,
        float rateMul,
        bool wrapVolume = false)
    {
        GameObject go = new GameObject(wrapVolume ? "SmokeWrap" : "SmokePoint");
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localOffset;
        go.transform.localRotation = Quaternion.identity;

        ParticleSystem ps = go.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        ParticleSystem.MainModule main = ps.main;
        main.duration = duration;
        main.loop = true;
        main.playOnAwake = false;
        main.simulationSpace = ParticleSystemSimulationSpace.Local;
        main.startLifetime = new ParticleSystem.MinMaxCurve(
            (wrapVolume ? 1.4f : 0.9f) * scale,
            (wrapVolume ? 2.4f : 1.8f) * scale);
        main.startSpeed = new ParticleSystem.MinMaxCurve(
            0.15f * scale,
            (wrapVolume ? 1.1f : 0.85f) * scale);
        main.startSize = new ParticleSystem.MinMaxCurve(
            (wrapVolume ? 0.55f : 0.35f) * scale,
            (wrapVolume ? 1.35f : 1.05f) * scale);
        main.startColor = new ParticleSystem.MinMaxGradient(
            new Color(0.96f, 0.96f, 0.97f, wrapVolume ? 0.5f : 0.65f),
            new Color(0.78f, 0.78f, 0.8f, wrapVolume ? 0.38f : 0.52f));
        main.gravityModifier = -0.12f;
        main.maxParticles = wrapVolume ? 350 : 180;

        ParticleSystem.EmissionModule emission = ps.emission;
        emission.rateOverTime = (wrapVolume ? 70f : 38f) * scale * rateMul;

        ParticleSystem.ShapeModule shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = (wrapVolume ? 0.95f : 0.42f) * scale * radiusMul;
        shape.radiusThickness = wrapVolume ? 1f : 0.65f;

        ParticleSystem.SizeOverLifetimeModule sizeOverLifetime = ps.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(
            new Keyframe(0f, 0.35f),
            new Keyframe(0.25f, 0.85f),
            new Keyframe(0.55f, 1.15f),
            new Keyframe(1f, 1.75f)));

        ParticleSystem.ColorOverLifetimeModule colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        colorOverLifetime.color = CreateWhiteGrayGradient();

        ParticleSystem.VelocityOverLifetimeModule velocity = ps.velocityOverLifetime;
        velocity.enabled = true;
        velocity.space = ParticleSystemSimulationSpace.Local;
        velocity.radial = new ParticleSystem.MinMaxCurve(
            (wrapVolume ? 0.25f : 0.4f) * scale,
            (wrapVolume ? 0.9f : 1.2f) * scale);
        velocity.y = new ParticleSystem.MinMaxCurve(
            (wrapVolume ? 0.1f : 0.25f) * scale,
            (wrapVolume ? 0.55f : 0.85f) * scale);

        ParticleSystem.RotationOverLifetimeModule rotation = ps.rotationOverLifetime;
        rotation.enabled = true;
        rotation.z = new ParticleSystem.MinMaxCurve(-120f, 120f);

        ParticleSystem.NoiseModule noise = ps.noise;
        noise.enabled = true;
        noise.strength = wrapVolume ? 0.55f : 0.7f;
        noise.frequency = wrapVolume ? 0.55f : 0.9f;
        noise.scrollSpeed = 0.35f;
        noise.quality = ParticleSystemNoiseQuality.High;

        ParticleSystemRenderer renderer = ps.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        renderer.material = GetMaterial();
        renderer.maxParticleSize = 4f * scale;

        ps.Play();
    }

    private static Gradient CreateWhiteGrayGradient()
    {
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new[]
            {
                new GradientColorKey(new Color(1f, 1f, 1f), 0f),
                new GradientColorKey(new Color(0.88f, 0.88f, 0.9f), 0.35f),
                new GradientColorKey(new Color(0.62f, 0.62f, 0.65f), 0.7f),
                new GradientColorKey(new Color(0.42f, 0.42f, 0.45f), 1f)
            },
            new[]
            {
                new GradientAlphaKey(0f, 0f),
                new GradientAlphaKey(0.55f, 0.12f),
                new GradientAlphaKey(0.75f, 0.35f),
                new GradientAlphaKey(0.45f, 0.65f),
                new GradientAlphaKey(0f, 1f)
            });
        return gradient;
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
