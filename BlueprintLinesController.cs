using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


[Serializable]
public class BlueprintLinesController
{
    BlueprintManager _blueprintManager;
    [SerializeField] private float Height = 5f;

    [SerializeField] BlueprintLineHandler _defaultLine;
    [SerializeField] List<BlueprintLineHandler> _lines;


    public void Awake(BlueprintManager blueprintManager)
    {
        _blueprintManager = blueprintManager;
        _defaultLine.gameObject.SetActive(false);
    }

    public void Update()
    {

    }

    public void CreateNewLine(int index, Vector2 startPosition, Vector2 endPosition)
    {
        GameObject lineInstance = GameObject.Instantiate(_defaultLine.gameObject);
        _lines.Insert(index, lineInstance.GetComponent<BlueprintLineHandler>());
        lineInstance.SetActive(true);

        ConfugurateLine(lineInstance.GetComponent<Image>(), startPosition, endPosition);
    }

    private void ConfugurateLine(Image line, Vector2 startPosition, Vector2 endPosition)
    {
        line.rectTransform.anchoredPosition = (startPosition + endPosition) / 2;

        Vector2 direction = endPosition - startPosition;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        line.rectTransform.rotation = Quaternion.Euler(0, 0, angle);

        line.rectTransform.sizeDelta = new Vector2(direction.magnitude + Height, Height);
    }
}
