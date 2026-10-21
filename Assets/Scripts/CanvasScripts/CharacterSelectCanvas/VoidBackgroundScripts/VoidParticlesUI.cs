
using System;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(CanvasRenderer))]
public class VoidParticlesUI : MaskableGraphic
{
    [Header("Particle Amount")]
    [SerializeField, Range(1, 200)]
    private int particleCount = 55;

    [Header("Particle Size")]
    [SerializeField, Min(0.1f)]
    private float minSize = 3f;

    [SerializeField, Min(0.1f)]
    private float maxSize = 7f;

    [Header("Particle Movement")]
    [SerializeField, Min(0f)]
    private float minSpeed = 3f;

    [SerializeField, Min(0f)]
    private float maxSpeed = 10f;

    [Header("Particle Appearance")]
    [SerializeField, Range(0f, 1f)]
    private float opacity = 0.55f;

    [Header("Particle Generation")]
    [SerializeField]
    private int randomSeed = 25;

    private struct Particle
    {
        public Vector2 position;
        public Vector2 velocity;
        public float size;
        public float brightness;
    }

    private Particle[] particles;
    private Texture2D softCircleTexture;

    public override Texture mainTexture
    {
        get
        {
            return softCircleTexture != null
                ? softCircleTexture
                : Texture2D.whiteTexture;
        }
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        // The particles must never block UI buttons.
        raycastTarget = false;

        GenerateSoftCircle();
        GenerateParticles();

        SetAllDirty();
    }

    private void GenerateSoftCircle()
    {
        if (softCircleTexture != null)
            return;

        const int resolution = 32;

        softCircleTexture = new Texture2D(
            resolution,
            resolution,
            TextureFormat.RGBA32,
            false
        );

        softCircleTexture.wrapMode = TextureWrapMode.Clamp;
        softCircleTexture.filterMode = FilterMode.Bilinear;

        Color[] pixels = new Color[
            resolution * resolution
        ];

        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                float u = (x + 0.5f) / resolution;
                float v = (y + 0.5f) / resolution;

                Vector2 offset = new Vector2(
                    u - 0.5f,
                    v - 0.5f
                );

                float distance = offset.magnitude * 2f;

                // Soft circular transparency.
                float alpha = Mathf.Clamp01(
                    1f - distance
                );

                alpha = alpha * alpha *
                    (3f - 2f * alpha);

                pixels[y * resolution + x] =
                    new Color(1f, 1f, 1f, alpha);
            }
        }

        softCircleTexture.SetPixels(pixels);
        softCircleTexture.Apply();

        SetMaterialDirty();
    }

    private void GenerateParticles()
    {
        System.Random random = new System.Random(
            randomSeed
        );

        particles = new Particle[particleCount];

        float smallestSize = Mathf.Min(minSize, maxSize);
        float largestSize = Mathf.Max(minSize, maxSize);

        float slowestSpeed = Mathf.Min(minSpeed, maxSpeed);
        float fastestSpeed = Mathf.Max(minSpeed, maxSpeed);

        for (int i = 0; i < particles.Length; i++)
        {
            Particle particle = new Particle();

            // Random positions across the screen.
            particle.position = new Vector2(
                (float)random.NextDouble(),
                (float)random.NextDouble()
            );

            // Every particle has its own direction.
            float angle =
                (float)random.NextDouble() *
                Mathf.PI * 2f;

            // And its own speed.
            float speed = Mathf.Lerp(
                slowestSpeed,
                fastestSpeed,
                (float)random.NextDouble()
            );

            particle.velocity = new Vector2(
                Mathf.Cos(angle),
                Mathf.Sin(angle)
            ) * speed;

            particle.size = Mathf.Lerp(
                smallestSize,
                largestSize,
                (float)random.NextDouble()
            );

            particle.brightness = Mathf.Lerp(
                0.45f,
                1f,
                (float)random.NextDouble()
            );

            particles[i] = particle;
        }

        SetVerticesDirty();
    }

    private void Update()
    {
        if (particles == null ||
            particles.Length != particleCount)
        {
            GenerateParticles();
        }

        Rect rect = rectTransform.rect;

        float width = Mathf.Max(1f, rect.width);
        float height = Mathf.Max(1f, rect.height);

        float deltaTime = Time.unscaledDeltaTime;

        for (int i = 0; i < particles.Length; i++)
        {
            Particle particle = particles[i];

            // Move independently.
            particle.position.x +=
                particle.velocity.x *
                deltaTime / width;

            particle.position.y +=
                particle.velocity.y *
                deltaTime / height;

            // Wrap around screen edges.
            particle.position.x = Mathf.Repeat(
                particle.position.x, 1f
            );

            particle.position.y = Mathf.Repeat(
                particle.position.y, 1f
            );

            particles[i] = particle;
        }

        // Update our single UI mesh.
        SetVerticesDirty();
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        if (particles == null)
            return;

        Rect rect = rectTransform.rect;

        for (int i = 0; i < particles.Length; i++)
        {
            Particle particle = particles[i];

            float x = rect.xMin +
                particle.position.x * rect.width;

            float y = rect.yMin +
                particle.position.y * rect.height;

            float halfSize = particle.size * 0.5f;

            Color particleColor = color;

            particleColor.a *=
                opacity * particle.brightness;

            int startIndex = vh.currentVertCount;

            vh.AddVert(
                new Vector3(x - halfSize, y - halfSize),
                particleColor,
                new Vector2(0f, 0f)
            );

            vh.AddVert(
                new Vector3(x - halfSize, y + halfSize),
                particleColor,
                new Vector2(0f, 1f)
            );

            vh.AddVert(
                new Vector3(x + halfSize, y + halfSize),
                particleColor,
                new Vector2(1f, 1f)
            );

            vh.AddVert(
                new Vector3(x + halfSize, y - halfSize),
                particleColor,
                new Vector2(1f, 0f)
            );

            vh.AddTriangle(
                startIndex,
                startIndex + 1,
                startIndex + 2
            );

            vh.AddTriangle(
                startIndex,
                startIndex + 2,
                startIndex + 3
            );
        }
    }

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();

        particleCount = Mathf.Clamp(
            particleCount, 1, 200
        );

        if (Application.isPlaying &&
            isActiveAndEnabled)
        {
            GenerateParticles();
        }
    }
#endif

    protected override void OnDestroy()
    {
        if (softCircleTexture != null)
        {
            if (Application.isPlaying)
                Destroy(softCircleTexture);
            else
                DestroyImmediate(softCircleTexture);
        }

        base.OnDestroy();
    }
}
