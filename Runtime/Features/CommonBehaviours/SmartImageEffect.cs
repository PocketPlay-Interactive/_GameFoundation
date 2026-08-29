using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
[RequireComponent(typeof(Graphic))]
public class SmartImageEffect : MonoBehaviour
{
    private enum EffectType
    {
        None,
        Shine,
        Tiling
    }

    [SerializeField] private EffectType effect = EffectType.Shine;
    [SerializeField] private Color shineColor = Color.white;
    [SerializeField, Range(0.01f, 1f)] private float shineWidth = 0.18f;
    [SerializeField] private float shineAngle = -25f;
    [SerializeField] private float shineSpeed = 1f;
    [SerializeField] private Vector2 tilingScale = Vector2.one;
    [SerializeField] private Vector2 tilingSpeed = new(0.2f, 0f);

    private static readonly int ShineColor = Shader.PropertyToID("_ShineColor");
    private static readonly int ShineWidth = Shader.PropertyToID("_ShineWidth");
    private static readonly int ShineAngle = Shader.PropertyToID("_ShineAngle");
    private static readonly int ShinePosition = Shader.PropertyToID("_ShinePosition");
    private static readonly int TilingOffset = Shader.PropertyToID("_TilingOffset");
    private static readonly int TilingScale = Shader.PropertyToID("_TilingScale");
    private static readonly int UvRect = Shader.PropertyToID("_UvRect");

    private Graphic graphic;
    private Material material;
    private EffectType appliedEffect;

    private void OnEnable()
    {
        graphic = GetComponent<Graphic>();
        EnsureMaterial();
        ApplyStaticProperties();
        graphic.SetVerticesDirty();
    }

    private void OnValidate()
    {
        if (!isActiveAndEnabled)
            return;

        graphic = GetComponent<Graphic>();
        EnsureMaterial();
        ApplyStaticProperties();
        graphic.SetVerticesDirty();
    }

    private void Update()
    {
        if (material == null)
            return;

        float time = Application.isPlaying ? Time.unscaledTime : (float)UnityEditorSafeTime();
        material.SetFloat(ShinePosition, Mathf.Repeat(time * shineSpeed, 1.6f) - 0.3f);
        material.SetVector(TilingOffset, new Vector4(tilingSpeed.x * time, tilingSpeed.y * time, 0f, 0f));
    }

    private void OnDisable()
    {
        if (graphic != null && graphic.material == material)
            graphic.material = null;

        if (material != null)
        {
            DestroyMaterial(material);
            material = null;
        }
    }

    private void EnsureMaterial()
    {
        if (graphic == null)
            return;

        if (effect == EffectType.None)
        {
            if (graphic.material == material)
                graphic.material = null;

            if (material != null)
            {
                DestroyMaterial(material);
                material = null;
            }

            return;
        }

        Shader shader = Shader.Find(GetShaderName(effect));
        if (shader == null)
            return;

        if (material == null || appliedEffect != effect || material.shader != shader)
        {
            if (material != null)
                DestroyMaterial(material);

            material = new Material(shader) { name = $"Smart Image Effect ({effect})" };
            appliedEffect = effect;
        }

        graphic.material = material;
    }

    private void ApplyStaticProperties()
    {
        if (material == null)
            return;

        material.SetColor(ShineColor, shineColor);
        material.SetFloat(ShineWidth, shineWidth);
        material.SetFloat(ShineAngle, shineAngle);
        material.SetVector(TilingScale, new Vector4(Mathf.Max(0.001f, tilingScale.x), Mathf.Max(0.001f, tilingScale.y), 0f, 0f));
        material.SetVector(UvRect, GetUvRect());
    }

    private Vector4 GetUvRect()
    {
        if (graphic is Image image && image.sprite != null)
        {
            Texture texture = image.sprite.texture;
            Rect rect = image.sprite.textureRect;
            return new Vector4(rect.x / texture.width, rect.y / texture.height, rect.width / texture.width, rect.height / texture.height);
        }

        return new Vector4(0f, 0f, 1f, 1f);
    }

    private static string GetShaderName(EffectType type)
    {
        return type switch
        {
            EffectType.Shine => "Dann/UI/Effects/Shine",
            EffectType.Tiling => "Dann/UI/Effects/Tiling",
            _ => null
        };
    }

    private static void DestroyMaterial(Material target)
    {
        if (Application.isPlaying)
            Destroy(target);
        else
            DestroyImmediate(target);
    }

    private static double UnityEditorSafeTime()
    {
#if UNITY_EDITOR
        return UnityEditor.EditorApplication.timeSinceStartup;
#else
        return Time.unscaledTime;
#endif
    }
}
