using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RawImage))]
public class EmbeddedSpiralFloorUI : MonoBehaviour
{
    [Header("Texture Quality")]
    [SerializeField] private int textureWidth = 768;
    [SerializeField] private int textureHeight = 256;

    [Header("Overall Look")]
    [SerializeField] private Color spiralTint = new Color(0.55f, 0.85f, 1f, 1f);
    [SerializeField, Range(0f, 1f)] private float overallOpacity = 0.22f;

    [Header("Shape")]
    [SerializeField, Range(1f, 4f)] private float verticalCompression = 2.1f;
    [SerializeField, Range(0.2f, 1.2f)] private float visibleRadius = 0.92f;
    [SerializeField, Range(0f, 0.5f)] private float centerSoftHole = 0.05f;

    [Header("Spiral")]
    [SerializeField, Range(1f, 8f)] private float spiralTwist = 3.6f;
    [SerializeField, Range(1f, 8f)] private float spiralSharpness = 3.0f;
    [SerializeField, Range(0f, 1f)] private float spiralStrength = 0.55f;
    [SerializeField, Range(0f, 1f)] private float secondarySpiralStrength = 0.25f;

    [Header("Soft Fill")]
    [SerializeField, Range(0f, 1f)] private float hazeStrength = 0.18f;
    [SerializeField, Range(0f, 1f)] private float centerGlowStrength = 0.10f;

    [Header("Noise")]
    [SerializeField] private int seed = 17;
    [SerializeField, Range(0f, 1f)] private float noiseStrength = 0.16f;
    [SerializeField, Range(0.5f, 8f)] private float noiseScale = 3.0f;

    private RawImage rawImage;
    private Texture2D generatedTexture;

    private void Awake()
    {
        rawImage = GetComponent<RawImage>();
        rawImage.raycastTarget = false;
    }

    private void OnEnable()
    {
        if (rawImage == null)
            rawImage = GetComponent<RawImage>();

        GenerateTexture();
        ApplyTint();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (!isActiveAndEnabled)
            return;

        if (rawImage == null)
            rawImage = GetComponent<RawImage>();

        GenerateTexture();
        ApplyTint();
    }
#endif

    private void ApplyTint()
    {
        if (rawImage == null)
            return;

        rawImage.color = new Color(
            spiralTint.r,
            spiralTint.g,
            spiralTint.b,
            overallOpacity
        );
    }

    private void GenerateTexture()
    {
        textureWidth = Mathf.Clamp(textureWidth, 64, 2048);
        textureHeight = Mathf.Clamp(textureHeight, 64, 2048);

        ReleaseTexture();

        generatedTexture = new Texture2D(
            textureWidth,
            textureHeight,
            TextureFormat.RGBA32,
            false
        );

        generatedTexture.name = "Generated Embedded Spiral Floor";
        generatedTexture.wrapMode = TextureWrapMode.Clamp;
        generatedTexture.filterMode = FilterMode.Bilinear;

        Color[] pixels = new Color[textureWidth * textureHeight];

        float seedOffsetX = seed * 0.173f + 11.3f;
        float seedOffsetY = seed * 0.291f + 27.8f;

        for (int y = 0; y < textureHeight; y++)
        {
            for (int x = 0; x < textureWidth; x++)
            {
                float u = (x + 0.5f) / textureWidth;
                float v = (y + 0.5f) / textureHeight;

                // Normalized centered coordinates
                float px = (u - 0.5f) * 2f;
                float py = (v - 0.5f) * 2f;

                // Compress vertically to make it feel like a floor plane
                float cy = py * verticalCompression;

                float radius = Mathf.Sqrt(px * px + cy * cy);
                float angle = Mathf.Atan2(cy, px);

                // Main spiral band
                float spiralPhase = angle + radius * spiralTwist * 6f;
                float spiralBand = 0.5f + 0.5f * Mathf.Cos(spiralPhase);
                spiralBand = Mathf.Pow(spiralBand, spiralSharpness);

                // Secondary softer swirl for richness
                float secondaryPhase = angle * 1.65f + radius * spiralTwist * 3.3f + 1.35f;
                float secondaryBand = 0.5f + 0.5f * Mathf.Cos(secondaryPhase);
                secondaryBand = Mathf.Pow(secondaryBand, spiralSharpness * 0.65f);

                // Radial fade so it feels embedded, not sticker-like
                float outerFade = 1f - Mathf.SmoothStep(visibleRadius * 0.72f, visibleRadius, radius);
                float innerFade = Mathf.SmoothStep(centerSoftHole, centerSoftHole + 0.12f, radius);
                float radialMask = outerFade * innerFade;

                // Broad soft fill beneath the spiral
                float haze = (1f - Mathf.SmoothStep(0.15f, visibleRadius, radius));
                haze *= hazeStrength;

                // Gentle center presence
                float centerGlow = Mathf.Exp(-radius * radius * 5.5f) * centerGlowStrength;

                // Subtle breakup so it doesn't look mathematically perfect
                float noise = Mathf.PerlinNoise(
                    u * noiseScale + seedOffsetX,
                    v * noiseScale + seedOffsetY
                );

                float noiseMul = Mathf.Lerp(
                    1f - noiseStrength,
                    1f + noiseStrength,
                    noise
                );

                float alpha =
                    ((spiralBand * spiralStrength) +
                    (secondaryBand * secondarySpiralStrength) +
                    haze +
                    centerGlow) *
                    radialMask *
                    noiseMul;

                alpha = Mathf.Clamp01(alpha);

                pixels[y * textureWidth + x] = new Color(1f, 1f, 1f, alpha);
            }
        }

        generatedTexture.SetPixels(pixels);
        generatedTexture.Apply();

        rawImage.texture = generatedTexture;
        rawImage.raycastTarget = false;
    }

    private void ReleaseTexture()
    {
        if (generatedTexture == null)
            return;

        if (rawImage != null && rawImage.texture == generatedTexture)
            rawImage.texture = null;

        if (Application.isPlaying)
            Destroy(generatedTexture);
        else
            DestroyImmediate(generatedTexture);

        generatedTexture = null;
    }

    private void OnDestroy()
    {
        ReleaseTexture();
    }
}