using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class BlueprintLinesController
{
    BlueprintManager _blueprintManager;
    [SerializeField] float Height = 5f;
    [SerializeField] bool loop = true;

    [SerializeField] BlueprintLineHandler _defaultLine;
    [SerializeField] List<BlueprintLineHandler> _lines;


    public void Awake(BlueprintManager blueprintManager)
    {
        _blueprintManager = blueprintManager;
        _defaultLine.gameObject.SetActive(false);
    }

    public void Update()
    {
        for (int i = 0; i < _blueprintManager.BlueprintPoints.Count - 1; i++)
            ConfigurateLine(_lines[0].SelfImage, _blueprintManager.BlueprintPoints[i], _blueprintManager.BlueprintPoints[i + 1]);
        if(loop)
            ConfigurateLine(_lines[^1].SelfImage, _blueprintManager.BlueprintPoints[^1], _blueprintManager.BlueprintPoints[0]);

    }

    public void CreateNewLine(int index, Vector2 startPosition, Vector2 endPosition)
    {
        GameObject lineInstance = GameObject.Instantiate(_defaultLine.gameObject, _defaultLine.transform.parent);
        lineInstance.SetActive(true);

        _lines.Insert(index, lineInstance.GetComponent<BlueprintLineHandler>());

        ConfigurateLine(lineInstance.GetComponent<Image>(), startPosition, endPosition);
    }

    private void ConfigurateLine(Image line, Vector2 startPosition, Vector2 endPosition)
    {
        line.rectTransform.anchoredPosition = (startPosition + endPosition) / 2;

        Vector2 direction = endPosition - startPosition;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        line.rectTransform.rotation = Quaternion.Euler(0, 0, angle);

        line.rectTransform.sizeDelta = new Vector2(direction.magnitude + Height, Height);
    }
}
