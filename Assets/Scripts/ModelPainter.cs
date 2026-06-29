using UnityEngine;

public class ModelPainter : MonoBehaviour
{
    [Header("Settings")] [SerializeField] private int textureSize = 1024;
    [SerializeField] private float brushSize = 0.05f;

    [Header("References")] [SerializeField]
    private Camera paintCamera;

    [SerializeField] private Color selectedColor;

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

        _targetMaterial = renderer.material;
        _targetMaterial.mainTexture = _paintTexture;
    }

    private void Update()
    {
        if (Input.GetMouseButton(0))
        {
            TryPaint();
        }
    }

    private void TryPaint()
    {
        Ray ray = paintCamera.ScreenPointToRay(Input.mousePosition);
        
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
        {
            for (int y = centerY - radius; y <= centerY + radius; y++)
            {
                if (x < 0 || x >= textureSize || y < 0 || y >= textureSize) continue;

                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(centerX, centerY));
                if (dist <= radius)
                {
                    _paintTexture.SetPixel(x, y, color);
                }
            }
        }

        _paintTexture.Apply();
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