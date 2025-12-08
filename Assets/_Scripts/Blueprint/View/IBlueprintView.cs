public interface IBlueprintView
{
    public void OnEnable() { }
    public void OnDisable() { }

    public void Awake() { }
    public void Start() { }
    public void Update() { }

    public void OnDestroy() { }

    public void OnViewLayerValueChanged(BlueprintViewLayers blueprintViewLayers);
}
