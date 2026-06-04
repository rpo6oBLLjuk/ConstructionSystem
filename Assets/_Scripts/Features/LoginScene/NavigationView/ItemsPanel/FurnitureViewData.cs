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

    public string FilePath { get; set; }
    public string ThumbnailPath { get; set; }

    public double Price { get; set; }

    public bool IsAvailable { get; set; }

    public string CreatedAt { get; set; }
    public string UpdatedAt { get; set; }

    public Furniture SourceFurniture { get; set; }
    public Sprite Preview { get; set; }
    public GameObject ViewObject { get; set; }
}