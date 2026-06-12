using System.Collections.Generic;
using rpoboBLLjuk.SpaceCanvas;
using UnityEngine;
using Zenject;

public class ConstructionRoomBuilder : MonoBehaviour
{
    [Header("References")]
    [Inject] private ConstructionManager _constructionManager;
    [Inject] private ConstructionSystemConfig _config;

    [Header("Parents")]
    [SerializeField] private Transform _roomRoot;

    [Header("Materials")]
    [SerializeField] private Material _floorMaterial;
    [SerializeField] private Material _wallMaterial;
    [SerializeField] private Material _baseboardMaterial;

    private GameObject _floorObject;
    private GameObject _wallsRoot;
    private GameObject _baseboardsRoot;


    private void OnEnable() => _constructionManager.ProjectLoaded += Build;
    private void OnDisable() => _constructionManager.ProjectLoaded -= Build;

    public void Build(ProjectData projectData)
    {
        if (projectData == null)
        {
            DebugWrapper.LogError(this, "Project data is null.");
            return;
        }

        if (projectData.Points == null || projectData.Points.Length < 3)
        {
            DebugWrapper.LogError(this, "Project points are empty or incorrect.");
            return;
        }

        if (_config == null)
        {
            DebugWrapper.LogError(this, "Construction config is not assigned.");
            return;
        }

        Clear();

        BuildFloor(projectData.Points);
        BuildWalls(projectData.Points);
        BuildBaseboards(projectData.Points);
    }
    public void Clear()
    {
        if (_floorObject != null)
            Destroy(_floorObject);

        if (_wallsRoot != null)
            Destroy(_wallsRoot);

        if (_baseboardsRoot != null)
            Destroy(_baseboardsRoot);

        _floorObject = null;
        _wallsRoot = null;
        _baseboardsRoot = null;
    }

    private void BuildFloor(Vector2[] points)
    {
        Mesh mesh = new();
        mesh.name = "Floor_Mesh";

        Vector3[] vertices = new Vector3[points.Length];

        for (int i = 0; i < points.Length; i++)
            vertices[i] = _config.BlueprintPointToWorld(points[i]);

        int[] triangles = Triangulate(points);

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        _floorObject = new GameObject("Floor");
        _floorObject.transform.SetParent(GetRoomRoot(), false);

        MeshFilter meshFilter = _floorObject.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = _floorObject.AddComponent<MeshRenderer>();
        MeshCollider meshCollider = _floorObject.AddComponent<MeshCollider>();

        meshFilter.sharedMesh = mesh;
        meshCollider.sharedMesh = mesh;

        if (_floorMaterial != null)
            meshRenderer.material = _floorMaterial;
    }
    private void BuildWalls(Vector2[] points)
    {
        _wallsRoot = new GameObject("Walls");
        _wallsRoot.transform.SetParent(GetRoomRoot(), false);

        float height = _config.WallHeightMeters;

        for (int i = 0; i < points.Length; i++)
        {
            Vector3 start = _config.BlueprintPointToWorld(points[i]);
            Vector3 end = _config.BlueprintPointToWorld(points[(i + 1) % points.Length]);

            BuildWallSegment(i, start, end, height);
        }
    }
    private void BuildBaseboards(Vector2[] points)
    {
        if (_config.BaseboardHeightMeters <= 0f || _config.BaseboardWidthMeters <= 0f)
            return;

        _baseboardsRoot = new GameObject("Baseboards");
        _baseboardsRoot.transform.SetParent(GetRoomRoot(), false);

        float height = _config.BaseboardHeightMeters;
        float width = _config.BaseboardWidthMeters;

        bool clockwise = IsClockwise(points);

        for (int i = 0; i < points.Length; i++)
        {
            Vector3 start = _config.BlueprintPointToWorld(points[i]);
            Vector3 end = _config.BlueprintPointToWorld(points[(i + 1) % points.Length]);

            BuildBaseboardSegment(i, start, end, height, width, clockwise);
        }
    }

    private void BuildBaseboardSegment(int index, Vector3 start, Vector3 end, float height, float width, bool clockwise)
    {
        Vector3 direction = end - start;

        if (direction.sqrMagnitude <= 0.001f)
            return;

        direction.Normalize();

        Vector3 side = clockwise
            ? Vector3.Cross(Vector3.up, direction).normalized
            : Vector3.Cross(direction, Vector3.up).normalized;

        Vector3 startCorner = start;
        Vector3 endCorner = end;

        Vector3 startFloor = start + side * width;
        Vector3 endFloor = end + side * width;

        Vector3 startWall = start + Vector3.up * height;
        Vector3 endWall = end + Vector3.up * height;

        Vector3 startFloorMiter = startFloor - direction * width;
        Vector3 endFloorMiter = endFloor + direction * width;

        Mesh mesh = new();
        mesh.name = $"Baseboard_{index}_Mesh";

        Vector3[] vertices =
        {
            startCorner,      // 0
            endCorner,        // 1

            startFloor,       // 2
            endFloor,         // 3

            startWall,        // 4
            endWall,          // 5

            startFloorMiter,  // 6
            endFloorMiter     // 7
        };

        int[] triangles =
        {
            // Main diagonal face
            2, 5, 3,
            2, 4, 5,

            // Start 45-degree extension
            6, 4, 2,

            // End 45-degree extension
            3, 5, 7,

            // Start miter side face
            0, 4, 6,

            // End miter side face
            1, 7, 5
        };

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        GameObject baseboardObject = new($"Baseboard_{index}");
        baseboardObject.transform.SetParent(_baseboardsRoot.transform, false);

        MeshFilter meshFilter = baseboardObject.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = baseboardObject.AddComponent<MeshRenderer>();
        MeshCollider meshCollider = baseboardObject.AddComponent<MeshCollider>();

        meshFilter.sharedMesh = mesh;
        meshCollider.sharedMesh = mesh;

        meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.TwoSided;

        if (_baseboardMaterial != null)
            meshRenderer.material = _baseboardMaterial;
    }

    private void BuildWallSegment(int index, Vector3 start, Vector3 end, float height)
    {
        Vector3 direction = end - start;

        if (direction.sqrMagnitude <= 0.001f)
            return;

        Mesh mesh = new();
        mesh.name = $"Wall_{index}_Mesh";

        Vector3 startTop = start + Vector3.up * height;
        Vector3 endTop = end + Vector3.up * height;

        Vector3[] vertices =
        {
            start,
            end,
            endTop,
            startTop
        };

        int[] triangles =
        {
            0, 2, 1,
            0, 3, 2
        };

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        GameObject wallObject = new($"Wall_{index}");
        wallObject.transform.SetParent(_wallsRoot.transform, false);

        MeshFilter meshFilter = wallObject.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = wallObject.AddComponent<MeshRenderer>();
        MeshCollider meshCollider = wallObject.AddComponent<MeshCollider>();

        meshFilter.sharedMesh = mesh;
        meshCollider.sharedMesh = mesh;

        meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.TwoSided;

        if (_wallMaterial != null)
            meshRenderer.material = _wallMaterial;
    }
    private int[] Triangulate(Vector2[] points)
    {
        List<int> result = new();
        List<int> indexes = new();

        bool clockwise = IsClockwise(points);

        for (int i = 0; i < points.Length; i++)
            indexes.Add(clockwise ? i : points.Length - 1 - i);

        int guard = 0;

        while (indexes.Count > 3 && guard < 5000)
        {
            guard++;

            bool earFound = false;

            for (int i = 0; i < indexes.Count; i++)
            {
                int previousIndex = indexes[(i - 1 + indexes.Count) % indexes.Count];
                int currentIndex = indexes[i];
                int nextIndex = indexes[(i + 1) % indexes.Count];

                Vector2 previous = points[previousIndex];
                Vector2 current = points[currentIndex];
                Vector2 next = points[nextIndex];

                if (!IsConvex(previous, current, next))
                    continue;

                if (ContainsAnyPoint(points, indexes, previousIndex, currentIndex, nextIndex))
                    continue;

                result.Add(previousIndex);
                result.Add(currentIndex);
                result.Add(nextIndex);

                indexes.RemoveAt(i);
                earFound = true;

                break;
            }

            if (!earFound)
                break;
        }

        if (indexes.Count == 3)
        {
            result.Add(indexes[0]);
            result.Add(indexes[1]);
            result.Add(indexes[2]);
        }

        return result.ToArray();
    }

    private bool IsClockwise(Vector2[] points)
    {
        float sum = 0f;

        for (int i = 0; i < points.Length; i++)
        {
            Vector2 current = points[i];
            Vector2 next = points[(i + 1) % points.Length];

            sum += (next.x - current.x) * (next.y + current.y);
        }

        return sum > 0f;
    }
    private bool IsConvex(Vector2 previous, Vector2 current, Vector2 next)
    {
        Vector2 a = current - previous;
        Vector2 b = next - current;

        return Cross(a, b) < 0f;
    }

    private bool ContainsAnyPoint(Vector2[] points, List<int> indexes, int previousIndex, int currentIndex, int nextIndex)
    {
        Vector2 a = points[previousIndex];
        Vector2 b = points[currentIndex];
        Vector2 c = points[nextIndex];

        foreach (int index in indexes)
        {
            if (index == previousIndex || index == currentIndex || index == nextIndex)
                continue;

            if (IsPointInTriangle(points[index], a, b, c))
                return true;
        }

        return false;
    }
    private bool IsPointInTriangle(Vector2 point, Vector2 a, Vector2 b, Vector2 c)
    {
        float area = Mathf.Abs(Cross(b - a, c - a));
        float area1 = Mathf.Abs(Cross(a - point, b - point));
        float area2 = Mathf.Abs(Cross(b - point, c - point));
        float area3 = Mathf.Abs(Cross(c - point, a - point));

        return Mathf.Approximately(area, area1 + area2 + area3);
    }
    private float Cross(Vector2 a, Vector2 b)
    {
        return a.x * b.y - a.y * b.x;
    }

    private Transform GetRoomRoot()
    {
        if (_roomRoot != null)
            return _roomRoot;

        return transform;
    }
}