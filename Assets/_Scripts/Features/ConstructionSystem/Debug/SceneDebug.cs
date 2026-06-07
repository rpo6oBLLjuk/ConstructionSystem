using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SceneDebug : MonoBehaviour
{
    [SerializeField] TMP_Text debugText;
    [SerializeField] TMP_Text fpsText;
    [SerializeField] Toggle autoDebugToggle;
    [SerializeField] Button forceDebugButton;

    [SerializeField] private float fpsUpdateDelay = 0.25f;

    MeshFilter[] _meshFilters;
    SkinnedMeshRenderer[] _skinnedMeshes;
    [SerializeField] bool _enableAutoDebug = true;

    List<float> _previousDeltaTimes = new();
    private Coroutine _coroutine;


    private void OnEnable()
    {
        autoDebugToggle.onValueChanged.AddListener(OnToggleValueChanged);
        forceDebugButton.onClick.AddListener(OnButtonClick);

        autoDebugToggle.isOn = _enableAutoDebug;
    }
    private void OnDisable()
    {
        autoDebugToggle.onValueChanged.RemoveListener(OnToggleValueChanged);
        forceDebugButton.onClick.RemoveListener(OnButtonClick);
    }

    private void Awake() => StartCoroutine(FPSCounterTicker());
    private void Start() => OnToggleValueChanged(_enableAutoDebug);

    private void Update()
    {
        _previousDeltaTimes.Add(Time.deltaTime);
    }

    void CountTrianglesInScene()
    {
        int totalTriangles = 0;
        int totalVertices = 0;

        _meshFilters = FindObjectsByType<MeshFilter>(FindObjectsSortMode.None);
        _skinnedMeshes = FindObjectsByType<SkinnedMeshRenderer>(FindObjectsSortMode.None);

        foreach (MeshFilter mf in _meshFilters)
        {
            if (mf.sharedMesh != null)
            {
                totalTriangles += mf.sharedMesh.triangles.Length / 3;
                totalVertices += mf.sharedMesh.vertexCount;
            }
        }

        foreach (SkinnedMeshRenderer smr in _skinnedMeshes)
        {
            if (smr.sharedMesh != null)
            {
                totalTriangles += smr.sharedMesh.triangles.Length / 3;
                totalVertices += smr.sharedMesh.vertexCount;
            }
        }

        UpdateDebugText($"Total triangles in scene: {totalTriangles.ToString("N0", CultureInfo.InvariantCulture)}\nTotal vertices in scene: {totalVertices.ToString("N0", CultureInfo.InvariantCulture)}");
    }
    void UpdateDebugText(string text) => debugText.text = text;

    IEnumerator AutoDebugTicker()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.25f);
            CountTrianglesInScene();
        }

    }
    IEnumerator FPSCounterTicker()
    {
        while (true)
        {
            yield return new WaitForSeconds(fpsUpdateDelay);
            fpsText.text = $"FPS: {CalculateFPSCount()}";
        }
    }

    float CalculateFPSCount()
    {
        float deltaTimesSum = 0;
        _previousDeltaTimes.ForEach(deltaTime => deltaTimesSum += deltaTime);

        float fpsCount = _previousDeltaTimes.Count / deltaTimesSum;
        _previousDeltaTimes.Clear();

        return fpsCount;
    }

    void OnToggleValueChanged(bool value)
    {
        _enableAutoDebug = value;
        if (_enableAutoDebug)
            _coroutine = StartCoroutine(AutoDebugTicker());
        else if (_coroutine != null)
            StopCoroutine(_coroutine);
    }
    void OnButtonClick() => CountTrianglesInScene();
}
