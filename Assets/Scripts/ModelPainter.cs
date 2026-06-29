using UnityEngine;
using UnityEngine.InputSystem;

public class ModelPainter : MonoBehaviour
{
    [Header("Settings")] [SerializeField] private int textureSize = 1024;
    [SerializeField] private float brushSize = 0.05f;

    [Header("References")] [SerializeField]
    private Camera paintCamera;

    [SerializeField] private Color selectedColor;


    [Range(0f, 1f)] [SerializeField] private float brushMetallic = 1f;
    [Range(0f, 1f)] [SerializeField] private float brushSmoothness = 0.5f;
    [SerializeField] private bool paintMetallic = false;

    private Texture2D _metallicTexture;
    private RenderTexture _renderTexture;
    private Texture2D _paintTexture;
    private Material _targetMaterial;

    private void Start()
    {
        InitializePaintTexture();
    }

    private void InitializePaintTexture()
    {
        var renderer = GetComponent<Renderer>();
        if (renderer == null) return;

        _paintTexture = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, false);
        FillTexture(_paintTexture, Color.white);
        _paintTexture.Apply();

        _metallicTexture = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, false);
        FillTexture(_metallicTexture, new Color(0f, 0f, 0f, 0.5f));
        _metallicTexture.Apply();

        _targetMaterial = renderer.material;
        _targetMaterial.SetTexture("_MetallicGlossMap", _metallicTexture);
        _targetMaterial.SetFloat("_Metallic", 0f);
        _targetMaterial.SetFloat("_Smoothness", 0f);

        _targetMaterial.mainTexture = _paintTexture;
    }

    private void Update()
    {
        if (Mouse.current.leftButton.isPressed)
        {
            TryPaint();
        }
    }

    private void TryPaint()
    {
        Ray ray = paintCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.transform != transform) return;

            PaintAtUV(hit.textureCoord, selectedColor);
        }
    }

    private void PaintAtUV(Vector2 uv, Color color)
    {
        int centerX = (int)(uv.x * textureSize);
        int centerY = (int)(uv.y * textureSize);
        int radius = (int)(brushSize * textureSize);

        for (int x = centerX - radius; x <= centerX + radius; x++)
        for (int y = centerY - radius; y <= centerY + radius; y++)
        {
            if (x < 0 || x >= textureSize || y < 0 || y >= textureSize) continue;
            float dist = Vector2.Distance(new Vector2(x, y), new Vector2(centerX, centerY));
            if (dist > radius) continue;

            if (paintMetallic)
            {
                Color m = new Color(brushMetallic, 0f, 0f, brushSmoothness);
                _metallicTexture.SetPixel(x, y, m);
            }
            else
            {
                _paintTexture.SetPixel(x, y, color);
            }
        }

        if (paintMetallic) _metallicTexture.Apply();
        else _paintTexture.Apply();
    }

    private void FillTexture(Texture2D tex, Color color)
    {
        Color[] pixels = new Color[textureSize * textureSize];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = color;
        tex.SetPixels(pixels);
    }

    public void ClearPainting()
    {
        FillTexture(_paintTexture, Color.white);
        _paintTexture.Apply();
    }

    private void OnDestroy()
    {
        if (_targetMaterial != null)
            Destroy(_targetMaterial);
        if (_paintTexture != null)
            Destroy(_paintTexture);
    }
}