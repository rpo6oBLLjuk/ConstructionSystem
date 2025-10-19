using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BlueprintVisualizer : MonoBehaviour
{
    [SerializeField] Image lineImage;
    [SerializeField] List<RectTransform> visualPoints;

    [SerializeField] float Height = 5;

    List<Vector2> blueprintPoints = new();
    List<Image> lines = new();

    [SerializeField] bool loop = true;


    private void Awake()
    {
        lines.Clear();
        for (int i = 0; i < visualPoints.Count; i++)
        {
            lines.Add(GameObject.Instantiate(lineImage, lineImage.transform.parent).GetComponent<Image>());
        }
        lineImage.gameObject.SetActive(false);
    }
    private void Update()
    {
        UpdatePoints();
        SetImage();
    }

    void UpdatePoints()
    {
        blueprintPoints.Clear();
        visualPoints.ForEach(value => blueprintPoints.Add(value.anchoredPosition));
    }

    void SetImage()
    {
        for (int i = 0; i < visualPoints.Count - 1; i++)
            ConfugurateLine(lines[i], blueprintPoints[i], blueprintPoints[i + 1]);
        if (loop)
            ConfugurateLine(lines[^1], blueprintPoints[^1], blueprintPoints[0]);
    }

    private void ConfugurateLine(Image line, Vector2 startPosition, Vector2 endPosition)
    {
        Vector2 direction = endPosition - startPosition;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // Позиция по центру между точками
        line.rectTransform.anchoredPosition = (startPosition + endPosition) / 2;

        line.rectTransform.rotation = Quaternion.Euler(0, 0, angle);
        line.rectTransform.sizeDelta = new Vector2(direction.magnitude + Height, Height);
    }
}
