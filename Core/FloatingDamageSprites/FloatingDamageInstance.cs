using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;
using System;
using EditorCools;
using System.Text;

#if UNITY_EDITOR
using UnityEditor;
using System.IO;
using System.Linq;
#endif

public class FloatingDamageInstance : MonoBehaviour
{
    [Header("Sprite Settings")]
    public Sprite[] digitSprites; // 0-9
    // specialSprites: [0]=+, [1]=-, [2]=., [3]=B, [4]=icon_crit, [5]=K, [6]=M
    public Sprite[] specialSprites; // Đặt đúng thứ tự này!
    public GameObject digitSpritePrefab; // Prefab có SpriteRenderer

    public float charSpacing = 0.2f;
    [Header("Animation Settings")]
    public float moveUpDistance = 1.0f;
    public float fadeOutDuration = 0.5f;
    public float totalLifetime = 1.5f;

    private List<SpriteRenderer> currentDigits = new();
    private Dictionary<char, Sprite> digitMap = new();
    private readonly Queue<GameObject> digitPool = new();

    private Transform _cachedTransform;
    private static readonly StringBuilder sb = new StringBuilder(16);
    private Vector3[] _cachedPath;

    void Awake()
    {
        _cachedTransform = transform;
        digitMap.Clear();
        for (int i = 0; i < 10; i++)
            if (i < digitSprites.Length && digitSprites[i])
                digitMap[(char)('0' + i)] = digitSprites[i];
    }

    public void Show(int damageAmount, Vector3 startPosition, bool damageCrit, FloatingDamageMoveType moveType, Action onDespawn)
    {
        _cachedTransform.position = startPosition;
        _cachedTransform.localScale = damageCrit ? Vector3.one : Vector3.one * 0.65f;
        gameObject.SetActive(true);

        // Compact number, tránh tạo string mới nhiều lần
        sb.Clear();
        if (damageAmount >= 1_000_000)
            sb.Append(damageAmount / 1_000_000);
        else if (damageAmount >= 1_000)
            sb.Append(damageAmount / 1_000);
        else
            sb.Append(damageAmount);

        string digitsStr = sb.ToString();
        char? suffix = damageAmount >= 1_000_000 ? 'M'
                     : damageAmount >= 1_000 ? 'K'
                     : (char?)null;

        int glyphCount = digitsStr.Length + (suffix != null ? 1 : 0) + (damageCrit ? 1 : 0);
        float currentX = -glyphCount * charSpacing / 2f;
        currentDigits.Clear();

        void Spawn(Sprite sp, bool isSpecial = false)
        {
            if (!sp) return;
            GameObject go = null;
            while (digitPool.Count > 0)
            {
                var candidate = digitPool.Dequeue();
                if (!candidate.activeSelf)
                {
                    go = candidate;
                    break;
                }
            }
            if (go == null)
                go = Instantiate(digitSpritePrefab, _cachedTransform);

            go.SetActive(true);
            var sr = go.GetComponent<SpriteRenderer>();
            if (sr)
            {
                sr.color = sr.color.WithAlpha(1f);
                sr.sprite = sp;
                go.transform.localPosition = VectorExtensions.Create3D(currentX);
                currentX += isSpecial ? charSpacing * 0.3f : charSpacing;
                currentDigits.Add(sr);
            }
        }

        if (damageCrit && specialSprites?.Length > 2) Spawn(specialSprites[4]);
        foreach (var ch in digitsStr)
            if (digitMap.TryGetValue(ch, out var sp)) Spawn(sp);
        if (suffix != null && specialSprites != null)
        {
            int idx = -1;
            switch(suffix)
            {
                case '+': idx = 0; break;
                case '-': idx = 1; break;
                case '.': idx = 2; break;
                case 'B': idx = 3; break;
                case 'K': idx = 5; break;
                case 'M': idx = 6; break;
            }
            if (idx != -1 && specialSprites.Length > idx) Spawn(specialSprites[idx]);
        }

        // Tween chuyển động, scale, fade dùng lại biến cachedTransform
        switch (moveType)
        {
            case FloatingDamageMoveType.MoveUp:
                _cachedTransform.DOMoveY(_cachedTransform.position.y + moveUpDistance, totalLifetime).SetEase(Ease.OutQuad);
                break;
            case FloatingDamageMoveType.MoveUpDown:
                _cachedTransform.DOMoveY(_cachedTransform.position.y + moveUpDistance, totalLifetime * 0.6f)
                    .SetEase(Ease.OutQuad)
                    .OnComplete(() =>
                    {
                        _cachedTransform.DOMoveY(_cachedTransform.position.y, totalLifetime * 0.4f).SetEase(Ease.InBounce);
                    });
                break;
            case FloatingDamageMoveType.CurveUp:
                _cachedPath = VectorExtensions.GetPath2(_cachedTransform.position,
                    _cachedTransform.position + VectorExtensions.Create3D(0.5f, moveUpDistance, 0));
                _cachedTransform.DOPath(_cachedPath, totalLifetime, PathType.CatmullRom)
                .SetEase(Ease.OutQuad);
                break;
            case FloatingDamageMoveType.CurveDown:
                _cachedPath = VectorExtensions.GetPath2(_cachedTransform.position,
                    _cachedTransform.position + VectorExtensions.Create3D(-0.5f, -moveUpDistance, 0));
                _cachedTransform.DOPath(_cachedPath, totalLifetime, PathType.CatmullRom)
                .SetEase(Ease.InQuad);
                break;
            case FloatingDamageMoveType.Shake:
                _cachedTransform.DOShakePosition(totalLifetime, VectorExtensions.Create3D(0.2f, moveUpDistance, 0));
                break;
            case FloatingDamageMoveType.Custom:
                // Tùy chỉnh theo ý bạn
                break;
            case FloatingDamageMoveType.TossRandom:
            {
                int dir = UnityEngine.Random.value > 0.5f ? 1 : -1;
                float peakHeight = moveUpDistance * UnityEngine.Random.Range(1.1f, 1.5f);
                float horizontal = UnityEngine.Random.Range(0.7f, 1.3f) * dir;

                Vector3 start = _cachedTransform.position;
                Vector3 peak = start + VectorExtensions.Create3D(horizontal * 0.5f, peakHeight, 0);
                Vector3 end = start + VectorExtensions.Create3D(horizontal * 1.5f, -moveUpDistance * UnityEngine.Random.Range(0.5f, 0.9f), 0);

                _cachedTransform.localScale = Vector3.one;
                _cachedPath = VectorExtensions.GetPath3(_cachedTransform.position, 
                    _cachedTransform.position + VectorExtensions.Create3D(horizontal * 0.5f, peakHeight, 0), 
                    _cachedTransform.position + VectorExtensions.Create3D(horizontal * 1.5f, -moveUpDistance * UnityEngine.Random.Range(0.5f, 0.9f), 0));
                _cachedTransform.DOPath(_cachedPath, totalLifetime, PathType.CatmullRom)
                    .SetEase(Ease.OutQuad);

                _cachedTransform.DOScale(0.5f, totalLifetime).SetEase(Ease.InQuad);
            }
            break;
        }

        foreach (var sr in currentDigits)
            sr.DOFade(0f, fadeOutDuration).SetDelay(totalLifetime - fadeOutDuration);

        DOVirtual.DelayedCall(totalLifetime, () =>
        {
            foreach (var sr in currentDigits)
            {
                sr.DOFade(1f, 0);
                sr.gameObject.SetActive(false);
                digitPool.Enqueue(sr.gameObject);
            }
            currentDigits.Clear();
            onDespawn?.Invoke();
        });
    }

    [ContextMenu("Auto Import Sprites From tmp_dmg.png")]
    public void AutoImportSpritesFromTmpDmg()
    {
#if UNITY_EDITOR
        string prefabPath = AssetDatabase.GetAssetPath(gameObject);
        string folder = Path.GetDirectoryName(prefabPath);

        // Tìm đúng file tmp_dmg.png trong folder
        string[] pngs = Directory.GetFiles(folder, "tmp_dmg.png", SearchOption.TopDirectoryOnly);
        if (pngs.Length == 0)
        {
            Debug.LogWarning("Không tìm thấy file tmp_dmg.png trong folder!");
            return;
        }
        string spritePath = pngs[0];

        // Lấy tất cả sprite trong file multiple
        var sprites = AssetDatabase.LoadAllAssetsAtPath(spritePath)
            .OfType<Sprite>()
            .ToList();

        // Phân loại: tên là số thì vào digitSprites, còn lại vào specialSprites
        var digitList = sprites.Where(s => int.TryParse(s.name, out _)).OrderBy(s => int.Parse(s.name)).ToList();
        var specialList = sprites.Where(s => !int.TryParse(s.name, out _)).ToList();

        digitSprites = digitList.ToArray();
        specialSprites = specialList.ToArray();

        EditorUtility.SetDirty(this);
        Debug.Log($"Đã import {digitList.Count} digit và {specialList.Count} special sprite từ tmp_dmg.png!");
#endif
    }
}