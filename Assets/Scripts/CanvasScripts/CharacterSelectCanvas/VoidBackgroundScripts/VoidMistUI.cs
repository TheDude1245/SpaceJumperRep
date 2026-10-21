
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RawImage))]
public class VoidMistUI : MonoBehaviour
{
    [Header("Mist Appearance")]
    [SerializeField]
    private Color mistColor =
        new Color(0.3f, 0.6f, 0.9f, 1f);

    [SerializeField, Range(0f, 1f)]
    private float opacity = 0.25f;

    [Header("Mist Movement")]
    [SerializeField]
    private Vector2 driftSpeed =
        new Vector2(0.005f, 0.001f);

    [Header("Mist Generation")]
    [SerializeField] private int seed = 10;

    [SerializeField] private int textureWidth = 256;
    [SerializeField] private int textureHeight = 128;

    private RawImage rawImage;
    private Texture2D mistTexture;
    private Vector2 uvOffset;

    private void Awake()
    {
        rawImage = GetComponent<RawImage>();
        rawImage.raycastTarget = false;

        GenerateMistTexture();
    }

    private void OnEnable()
    {
        if (rawImage == null)
            rawImage = GetComponent<RawImage>();

        if (mistTexture == null)
            GenerateMistTexture();
    }

    private void Update()
    {
        if (rawImage == null)
            return;

        // Keep drifting even when gameplay is paused.
        uvOffset += driftSpeed * Time.unscaledDeltaTime;

        rawImage.uvRect = new Rect(
            uvOffset.x,
            uvOffset.y,
            1f,
            1f
        );

        rawImage.color = new Color(
            mistColor.r,
            mistColor.g,
            mistColor.b,
            opacity
        );
    }

    private void GenerateMistTexture()
    {
        textureWidth = Mathf.Max(32, textureWidth);
        textureHeight = Mathf.Max(32, textureHeight);

        mistTexture = new Texture2D(
            textureWidth,
            textureHeight,
            TextureFormat.RGBA32,
            false
        );

        mistTexture.wrapMode = TextureWrapMode.Repeat;
        mistTexture.filterMode = FilterMode.Bilinear;

        Color[] pixels = new Color[
            textureWidth * textureHeight
        ];

        float offsetX = seed * 0.371f + 17f;
        float offsetY = seed * 0.613f + 29f;

        for (int y = 0; y < textureHeight; y++)
        {
            for (int x = 0; x < textureWidth; x++)
            {
                float u = x / (float)(textureWidth - 1);
                float v = y / (float)(textureHeight - 1);

                // Broad, soft clouds.
                float largeClouds = TileableNoise(
                    u, v, 3f, 2f, offsetX, offsetY
                );

                // Smaller details.
                float smallClouds = TileableNoise(
                    u, v, 8f, 5f,
                    offsetX + 46f,
                    offsetY + 73f
                );

                float noise =
                    largeClouds * 0.75f +
                    smallClouds * 0.25f;

                // Create soft transparent fog shapes.
                float alpha = Mathf.InverseLerp(
                    0.38f,
                    0.68f,
                    noise
                );

                alpha = Mathf.SmoothStep(0f, 1f, alpha);

                pixels[y * textureWidth + x] =
                    new Color(1f, 1f, 1f, alpha);
            }
        }

        mistTexture.SetPixels(pixels);
        mistTexture.Apply();

        rawImage.texture = mistTexture;
        rawImage.raycastTarget = false;
    }

    private float TileableNoise(
        float u,
        float v,
        float frequencyX,
        float frequencyY,
        float offsetX,
        float offsetY
    )
    {
        float x = u * frequencyX + offsetX;
        float y = v * frequencyY + offsetY;

        float a = Mathf.PerlinNoise(x, y);

        float b = Mathf.PerlinNoise(
            x - frequencyX, y
        );

        float c = Mathf.PerlinNoise(
            x, y - frequencyY
        );

        float d = Mathf.PerlinNoise(
            x - frequencyX,
            y - frequencyY
        );

        float top = Mathf.Lerp(a, b, u);
        float bottom = Mathf.Lerp(c, d, u);

        return Mathf.Lerp(top, bottom, v);
    }

    private void OnDestroy()
    {
        if (mistTexture != null)
            Destroy(mistTexture);
    }
}
