using System.Collections;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.UI;

public class SceneDebug : MonoBehaviour
{
    [SerializeField] TMP_Text debugText;
    [SerializeField] Toggle autoDebugToggle;
    [SerializeField] Button forceDebugButton;

    MeshFilter[] _meshFilters;
    SkinnedMeshRenderer[] _skinnedMeshes;
    [SerializeField] bool _enableAutoDebug = true;

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

    private void Start() => OnToggleValueChanged(_enableAutoDebug);

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
            yield return new WaitForSeconds(0.1f);
            CountTrianglesInScene();
        }

    }

    void OnToggleValueChanged(bool value)
    {
        _enableAutoDebug = value;
        if (_enableAutoDebug)
            _coroutine = StartCoroutine(AutoDebugTicker());
        else if(_coroutine != null)
            StopCoroutine(_coroutine);
    }
    void OnButtonClick() => CountTrianglesInScene();
}
