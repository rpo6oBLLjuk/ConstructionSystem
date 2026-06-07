using Coffee.UIEffects;
using UnityEngine;

public class FurnitureViewData
{
    public int Id { get; set; }
    public bool IsNew { get; set; } = false;

    public string Name { get; set; }
    public string Description { get; set; }

    public int FurnitureTypeId { get; set; }
    public string FurnitureTypeName { get; set; }

    public int ColorTypeId { get; set; }
    public string ColorTypeName { get; set; }

    public string Manufacturer { get; set; }

    public float Width { get; set; }
    public float Height { get; set; }
    public float Depth { get; set; }

    public bool HasModel { get; set; } = false;
    public bool HasPreview { get; set; } = false;

    public bool ModelOrPreviewChanged { get; set; } = false;

    public double Price { get; set; }

    public bool IsAvailable { get; set; }

    public string CreatedAt { get; set; }
    public string UpdatedAt { get; set; }

    public Furniture SourceFurniture { get; set; }
    public Sprite Preview { get; set; }
    public GameObject ViewObject { get; set; }
    public UIEffect UIEffect { get; set; }
}