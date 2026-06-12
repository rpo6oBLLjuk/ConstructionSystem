using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

public class BlueprintOverlayPreviewRenderer : MonoBehaviour
{
    [Inject] private BlueprintManager _blueprintManager;

    [Header("Blueprint")]
    [SerializeField] private RectTransform _blueprintRoot;

    [Header("Settings")]
    [SerializeField] private float _padding = 50f;
    [SerializeField] private float _minCaptureSize = 128f;
    [SerializeField] private float _maxAspectRatio = 2f;

    public async UniTask<Sprite> RenderSprite()
    {
        Texture2D texture = await RenderTexture2D();

        if (texture == null)
            return null;

        Rect rect = new(0, 0, texture.width, texture.height);
        Vector2 pivot = new(0.5f, 0.5f);

        return Sprite.Create(texture, rect, pivot);
    }
    public async UniTask<Texture2D> RenderTexture2D()
    {
        if (_blueprintRoot == null)
        {
            DebugWrapper.LogError(this, "Blueprint root is not assigned.");
            return null;
        }

        if (_blueprintManager == null)
        {
            DebugWrapper.LogError(this, "BlueprintManager is not injected.");
            return null;
        }

        Vector2[] points = _blueprintManager.BlueprintPoints.ToArray();

        if (points == null || points.Length == 0)
        {
            DebugWrapper.LogWarning(this, "Blueprint points are empty.");
            return null;
        }

        await UniTask.WaitForEndOfFrame();

        Rect screenRect = BuildScreenRect(points);

        if (screenRect.width <= 0 || screenRect.height <= 0)
        {
            DebugWrapper.LogError(this, "Capture rect has incorrect size.");
            return null;
        }

        Texture2D texture = new(Mathf.RoundToInt(screenRect.width), Mathf.RoundToInt(screenRect.height), TextureFormat.RGBA32, false);

        texture.ReadPixels(screenRect, 0, 0);
        texture.Apply();

        return texture;
    }

    private Rect BuildScreenRect(IReadOnlyList<Vector2> localPoints)
    {
        Vector2 firstPoint = LocalPointToScreenPoint(localPoints[0]);

        float minX = firstPoint.x;
        float maxX = firstPoint.x;
        float minY = firstPoint.y;
        float maxY = firstPoint.y;

        for (int i = 1; i < localPoints.Count; i++)
        {
            Vector2 screenPoint = LocalPointToScreenPoint(localPoints[i]);

            minX = Mathf.Min(minX, screenPoint.x);
            maxX = Mathf.Max(maxX, screenPoint.x);

            minY = Mathf.Min(minY, screenPoint.y);
            maxY = Mathf.Max(maxY, screenPoint.y);
        }

        minX -= _padding;
        maxX += _padding;
        minY -= _padding;
        maxY += _padding;

        ApplyMinCaptureSize(ref minX, ref maxX, ref minY, ref maxY);
        ApplyAspectLimit(ref minX, ref maxX, ref minY, ref maxY);

        minX = Mathf.Clamp(minX, 0, Screen.width);
        maxX = Mathf.Clamp(maxX, 0, Screen.width);

        minY = Mathf.Clamp(minY, 0, Screen.height);
        maxY = Mathf.Clamp(maxY, 0, Screen.height);

        return new Rect(minX, minY, maxX - minX, maxY - minY);
    }
    private Vector2 LocalPointToScreenPoint(Vector2 localPoint)
    {
        Vector3 worldPoint = _blueprintRoot.TransformPoint(new Vector3(localPoint.x, localPoint.y, 0));
        return RectTransformUtility.WorldToScreenPoint(null, worldPoint);
    }

    private void ApplyMinCaptureSize(ref float minX, ref float maxX, ref float minY, ref float maxY)
    {
        float width = maxX - minX;
        float height = maxY - minY;

        if (width < _minCaptureSize)
        {
            float difference = _minCaptureSize - width;
            minX -= difference * 0.5f;
            maxX += difference * 0.5f;
        }

        if (height < _minCaptureSize)
        {
            float difference = _minCaptureSize - height;
            minY -= difference * 0.5f;
            maxY += difference * 0.5f;
        }
    }
    private void ApplyAspectLimit(ref float minX, ref float maxX, ref float minY, ref float maxY)
    {
        float width = maxX - minX;
        float height = maxY - minY;

        if (width <= 0 || height <= 0)
            return;

        float aspect = width / height;

        if (aspect > _maxAspectRatio)
        {
            float targetHeight = width / _maxAspectRatio;
            float difference = targetHeight - height;

            minY -= difference * 0.5f;
            maxY += difference * 0.5f;
        }
        else if (aspect < 1f / _maxAspectRatio)
        {
            float targetWidth = height / _maxAspectRatio;
            float difference = targetWidth - width;

            minX -= difference * 0.5f;
            maxX += difference * 0.5f;
        }
    }
}