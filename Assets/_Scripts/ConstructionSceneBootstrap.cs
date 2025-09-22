using UnityEngine;

public class ConstructionSceneBootstrap : MonoBehaviour
{
    [SerializeField] bool _useVsync = false;

    void Awake()
    {
        SetTargetFrameRate();
    }

    private void SetTargetFrameRate()
    {
        int targetFrameRate = (int)Screen.currentResolution.refreshRateRatio.value;
        Application.targetFrameRate = targetFrameRate;

        QualitySettings.vSyncCount = _useVsync ? 1 : 0;
    }
}
