using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BlueprintLineHandler : MonoBehaviour, IPointerClickHandler
{
    public event Action<BlueprintLineHandler, Vector2> PointerClicked;
    public Image SelfImage { get; private set; }
    public CanvasGroup CanvasGroup { get; private set; }
    public TMP_Text Text { get; private set; }

    private void Awake()
    {
        SelfImage ??= GetComponent<Image>();
        CanvasGroup ??= GetComponent<CanvasGroup>();
        Text ??= GetComponentInChildren<TMP_Text>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
            PointerClicked?.Invoke(this, eventData.position);
    }
}
