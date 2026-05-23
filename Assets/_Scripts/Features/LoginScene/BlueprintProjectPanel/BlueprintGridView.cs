using System;
using System.Collections.Generic;
using System.Linq;
using Coffee.UIEffects;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class BlueprintGridView : MonoBehaviour
{
    [SerializeField] GameObject _defaultFileContainer;
    [SerializeField] Button _newFileButton;

    public event Action<BlueprintData> OnBlueprintSelected;
    public event Action OnNewFileSelected;

    private Dictionary<BlueprintData, GameObject> _blueprintViewlist = new();


    private void OnEnable() => _newFileButton.onClick.AddListener(() => OnNewFileSelected?.Invoke());
    private void OnDisable() => _newFileButton.onClick.RemoveAllListeners();

    private void Awake() => _defaultFileContainer.SetActive(false);

    public void UpdateBlueprintsData(List<BlueprintData> blueprintsData)
    {
        ClearView();
        blueprintsData.ForEach(data => CreateGridElement(data));
    }
    public void AddBlueprintData(BlueprintData blueprintData)
    {
        if (_blueprintViewlist.ContainsKey(blueprintData) || _blueprintViewlist.Keys.FirstOrDefault(data => data.name == blueprintData.name) != null)
            return;

        CreateGridElement(blueprintData);
    }

    public void RemoveBlueprintData(BlueprintData blueprintData) => RemoveGridElement(blueprintData);

    public void SetBlueprintActive(BlueprintData blueprint, bool isActive)
    {
        if (_blueprintViewlist.ContainsKey(blueprint))
            _blueprintViewlist[blueprint].transform.GetChild(0).GetComponentInChildren<UIEffect>().enabled = isActive;
    }

    private void ClearView()
    {
        _blueprintViewlist.Keys.ToList().ForEach((key) => RemoveGridElement(key));
        _blueprintViewlist.Clear();
    }

    private void CreateGridElement(BlueprintData data)
    {
        GameObject gridElem = GameObject.Instantiate(_defaultFileContainer, _defaultFileContainer.transform.parent);
        gridElem.SetActive(true);

        _blueprintViewlist.Add(data, gridElem);

        gridElem.GetComponentInChildren<TMP_Text>().text = data.name;
        gridElem.GetComponentInChildren<Button>().onClick.AddListener(() => OnGridElementClick(data));

    }
    private void RemoveGridElement(BlueprintData data)
    {
        GameObject go = _blueprintViewlist[data];
        go.GetComponentInChildren<Button>().onClick.RemoveAllListeners();
        GameObject.Destroy(go);

        _blueprintViewlist.Remove(data);
    }

    private void OnGridElementClick(BlueprintData data) => OnBlueprintSelected?.Invoke(data);
}
