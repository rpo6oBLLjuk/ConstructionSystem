using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class CustomTMP_Dropdown : TMP_Dropdown
{
    [SerializeField] private bool _hideOnClick = true;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public override void OnSubmit(BaseEventData eventData)
    {
        base.OnSubmit(eventData);

        Show();
    }
}
