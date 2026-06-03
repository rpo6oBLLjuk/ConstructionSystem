using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UserViewFactory : MonoBehaviour
{
    [SerializeField] private GameObject _userViewPrefab;


    private void Awake()
    {
        if (_userViewPrefab != null)
            _userViewPrefab.SetActive(false);
    }

    public GameObject Create(UserViewData user, Transform parent, Action<UserViewData> onUserSelected)
    {
        GameObject viewObject = Instantiate(_userViewPrefab, parent);
        viewObject.SetActive(true);

        InitializeViewData(viewObject, user, onUserSelected);

        return viewObject;
    }
    public void Refresh(GameObject viewObject, UserViewData user)
    {
        Transform layout = viewObject.transform.Find("Layout");

        FillText(layout, "Group1/UserId", $"#{user.Id}");
        FillText(layout, "Group2/FullName Text (TMP)", user.FullName);
        FillText(layout, "Group2/Role Text (TMP)", user.RoleName);
        FillText(layout, "Group3/Date Text (TMP)", user.CreatedAt);
    }

    private void InitializeViewData(GameObject viewObject, UserViewData user, Action<UserViewData> onUserSelected)
    {
        Refresh(viewObject, user);

        Button selectButton = viewObject.GetComponent<Button>();
        selectButton.onClick.AddListener(() => onUserSelected?.Invoke(user));
    }
    private void FillText(Transform root, string objectName, string value)
    {
        Transform textTransform = root.Find(objectName);

        if (textTransform == null)
        {
            DebugWrapper.LogWarning(this, $"Text object <b>{objectName}</b> not found in user view prefab");
            return;
        }

        TMP_Text text = textTransform.GetComponent<TMP_Text>();

        if (text == null)
        {
            DebugWrapper.LogWarning(this, $"TMP_Text component not found on <b>{objectName}</b>");
            return;
        }

        text.text = value;
    }
}