using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace rpoboBLLjuk.SpaceCanvas
{
    public class ConstructionPlacementCollisionChecker : MonoBehaviour
    {
        public ConstructionCollisionCheckResult CurrentResult { get; private set; }

        [Header("Collision")]
        [SerializeField] private bool _isBoxCollider = true;
        [SerializeField] private LayerMask _obstacleLayerMask = ~0;
        [SerializeField] private QueryTriggerInteraction _triggerInteraction = QueryTriggerInteraction.Collide;

        [Space]
        [SerializeField] private float _gap = 0.01f;
        [SerializeField] private int _maxResolveIterations = 6;
        [SerializeField] private int _overlapBufferSize = 64;

        [Header("Detailed Mesh Check")]
        [SerializeField] private float _pointInsideEpsilon = 0.0005f;
        [SerializeField] private int _vertexStep = 1;

        private Collider[] _overlapBuffer;
        private readonly HashSet<Collider> _processedColliders = new();

        private bool _meshCheckInProgress;
        private int _meshCheckRequestId;


        private void Awake() => _overlapBuffer = new Collider[_overlapBufferSize];

        public ConstructionCollisionCheckResult Check(GameObject activeObject)
        {
            if (_isBoxCollider)
                return CheckBox(activeObject);

            if (_meshCheckInProgress)
                return CurrentResult;

            CheckMeshAsync(activeObject).Forget();
            return CurrentResult;
        }

        public async UniTask<ConstructionCollisionCheckResult> CheckAsync(GameObject activeObject)
        {
            if (_isBoxCollider)
                return CheckBox(activeObject);

            if (_meshCheckInProgress)
                return CurrentResult;

            return await CheckMeshAsync(activeObject);
        }

        //public ConstructionCollisionCheckResult CheckAtPosition(GameObject activeObject, Vector3 position)
        //{
        //    Vector3 previousPosition = activeObject.transform.position;

        //    activeObject.transform.position = position;
        //    ConstructionCollisionCheckResult result = Check(activeObject);
        //    activeObject.transform.position = previousPosition;

        //    return result;
        //}

        public Collider GetActiveCollider(GameObject activeObject)
        {
            return activeObject
                .GetComponent<ConstructionFurnitureCollisionHandler>()
                .GetVisualizationCollider(_isBoxCollider);
        }

        private ConstructionCollisionCheckResult CheckBox(GameObject activeObject)
        {
            ConstructionFurnitureCollisionHandler activeData = activeObject.GetComponent<ConstructionFurnitureCollisionHandler>();

            BoxCollider activeCollider = activeData.BoundsCollider;
            BoxCollider boundsCollider = activeData.BoundsCollider;

            Vector3 originalPosition = activeObject.transform.position;
            Quaternion rotation = activeObject.transform.rotation;

            Vector3 suggestedPosition = originalPosition;

            bool hasCollision = false;
            bool hasResolvedPosition = false;

            int totalCollisionCount = 0;
            int iterationsUsed = 0;

            for (int i = 0; i < _maxResolveIterations; i++)
            {
                iterationsUsed = i + 1;

                Vector3 iterationCorrection = Vector3.zero;
                int iterationCollisionCount = CheckBoxPosition(
                    activeObject,
                    activeCollider,
                    boundsCollider,
                    suggestedPosition,
                    rotation,
                    ref iterationCorrection
                );

                if (iterationCollisionCount == 0)
                {
                    hasResolvedPosition = hasCollision;
                    break;
                }

                hasCollision = true;
                totalCollisionCount += iterationCollisionCount;

                if (iterationCorrection.sqrMagnitude <= 0.000001f)
                    break;

                suggestedPosition += iterationCorrection;
            }

            if (hasCollision)
                hasResolvedPosition = !HasBoxCollisionAtPosition(activeObject, activeCollider, boundsCollider, suggestedPosition, rotation);

            CurrentResult = new ConstructionCollisionCheckResult(
                hasCollision,
                hasResolvedPosition,
                totalCollisionCount,
                iterationsUsed,
                originalPosition,
                suggestedPosition,
                activeCollider
            );

            return CurrentResult;
        }

        private async UniTask<ConstructionCollisionCheckResult> CheckMeshAsync(GameObject activeObject)
        {
            _meshCheckInProgress = true;

            int requestId = ++_meshCheckRequestId;

            try
            {
                ConstructionFurnitureCollisionHandler activeData = activeObject.GetComponent<ConstructionFurnitureCollisionHandler>();

                MeshCollider activeCollider = activeData.DetailedCollider;
                BoxCollider boundsCollider = activeData.BoundsCollider;

                Vector3 originalPosition = activeObject.transform.position;
                Quaternion rotation = activeObject.transform.rotation;

                if (activeCollider == null || activeCollider.sharedMesh == null)
                {
                    CurrentResult = new ConstructionCollisionCheckResult(
                        false,
                        false,
                        0,
                        1,
                        originalPosition,
                        originalPosition,
                        activeData.GetVisualizationCollider(_isBoxCollider)
                    );

                    return CurrentResult;
                }

                List<MeshPairCheckData> pairs = BuildMeshCheckPairs(activeObject, activeCollider, boundsCollider, originalPosition, rotation);
                int vertexStep = Mathf.Max(1, _vertexStep);
                float epsilon = _pointInsideEpsilon;

                int collisionCount = await CountMeshCollisionsAsync(pairs, vertexStep, epsilon);

                await UniTask.SwitchToMainThread();

                if (requestId != _meshCheckRequestId)
                    return CurrentResult;

                bool hasCollision = collisionCount > 0;

                CurrentResult = new ConstructionCollisionCheckResult(
                    hasCollision,
                    false,
                    collisionCount,
                    1,
                    originalPosition,
                    originalPosition,
                    activeData.GetVisualizationCollider(_isBoxCollider)
                );

                return CurrentResult;
            }
            catch (Exception e)
            {
                this.LogError(e.Message);
                return CurrentResult;
            }
            finally
            {
                _meshCheckInProgress = false;
            }
        }

        private int CheckBoxPosition(
            GameObject activeObject,
            BoxCollider activeCollider,
            BoxCollider boundsCollider,
            Vector3 activePosition,
            Quaternion activeRotation,
            ref Vector3 correction)
        {
            int collisionCount = 0;
            int candidateCount = GetOverlapCandidates(boundsCollider, activePosition, activeRotation);

            _processedColliders.Clear();

            for (int i = 0; i < candidateCount; i++)
            {
                Collider candidate = _overlapBuffer[i];

                if (candidate == null)
                    continue;

                if (IsOwnCollider(activeObject, candidate))
                    continue;

                Collider obstacleCollider = GetBoxObstacleCollider(candidate);

                if (obstacleCollider == null)
                    continue;

                if (_processedColliders.Contains(obstacleCollider))
                    continue;

                _processedColliders.Add(obstacleCollider);

                if (Physics.ComputePenetration(
                    activeCollider,
                    activePosition,
                    activeRotation,
                    obstacleCollider,
                    obstacleCollider.transform.position,
                    obstacleCollider.transform.rotation,
                    out Vector3 direction,
                    out float distance
                ))
                {
                    correction += direction * (distance + _gap);
                    collisionCount++;
                }
            }

            return collisionCount;
        }

        private bool HasBoxCollisionAtPosition(
            GameObject activeObject,
            BoxCollider activeCollider,
            BoxCollider boundsCollider,
            Vector3 activePosition,
            Quaternion activeRotation)
        {
            Vector3 correction = Vector3.zero;

            return CheckBoxPosition(
                activeObject,
                activeCollider,
                boundsCollider,
                activePosition,
                activeRotation,
                ref correction
            ) > 0;
        }

        private List<MeshPairCheckData> BuildMeshCheckPairs(
            GameObject activeObject,
            MeshCollider activeCollider,
            BoxCollider boundsCollider,
            Vector3 activePosition,
            Quaternion activeRotation)
        {
            List<MeshPairCheckData> pairs = new();
            int candidateCount = GetOverlapCandidates(boundsCollider, activePosition, activeRotation);

            MeshSnapshot activeMeshSnapshot = CreateMeshSnapshot(activeCollider);

            if (!activeMeshSnapshot.IsValid)
                return pairs;

            _processedColliders.Clear();

            for (int i = 0; i < candidateCount; i++)
            {
                Collider candidate = _overlapBuffer[i];

                if (candidate == null)
                    continue;

                if (IsOwnCollider(activeObject, candidate))
                    continue;

                Collider obstacleCollider = GetDetailedObstacleCollider(candidate);

                if (obstacleCollider == null)
                    continue;

                if (_processedColliders.Contains(obstacleCollider))
                    continue;

                _processedColliders.Add(obstacleCollider);

                MeshTargetSnapshot targetSnapshot = CreateTargetSnapshot(obstacleCollider);

                if (!targetSnapshot.IsValid)
                    continue;

                pairs.Add(new MeshPairCheckData(activeMeshSnapshot, targetSnapshot));

                if (obstacleCollider is MeshCollider obstacleMeshCollider)
                {
                    MeshSnapshot obstacleMeshSnapshot = CreateMeshSnapshot(obstacleMeshCollider);

                    if (obstacleMeshSnapshot.IsValid)
                        pairs.Add(new MeshPairCheckData(obstacleMeshSnapshot, MeshTargetSnapshot.FromMesh(activeMeshSnapshot)));
                }
            }

            return pairs;
        }

        private async UniTask<int> CountMeshCollisionsAsync(List<MeshPairCheckData> pairs, int vertexStep, float epsilon)
        {
            int collisionCount = 0;

            await UniTask.SwitchToThreadPool();

            for (int i = 0; i < pairs.Count; i++)
            {
                if (HasAnyMeshVertexInsideTarget(pairs[i].Source, pairs[i].Target, vertexStep, epsilon))
                    collisionCount++;
            }

            await UniTask.SwitchToMainThread();

            return collisionCount;
        }

        private bool HasAnyMeshVertexInsideTarget(MeshSnapshot source, MeshTargetSnapshot target, int vertexStep, float epsilon)
        {
            for (int i = 0; i < source.Vertices.Length; i += vertexStep)
            {
                Vector3 worldPoint = source.LocalToWorld.MultiplyPoint3x4(source.Vertices[i]);

                if (IsWorldPointInsideTarget(worldPoint, target, epsilon))
                    return true;
            }

            return false;
        }

        private bool IsWorldPointInsideTarget(Vector3 worldPoint, MeshTargetSnapshot target, float epsilon)
        {
            return target.Type switch
            {
                MeshTargetType.Box => IsWorldPointInsideBox(worldPoint, target.Box, epsilon),
                MeshTargetType.Mesh => IsWorldPointInsideMesh(worldPoint, target.Mesh, epsilon),
                _ => false
            };
        }

        private bool IsWorldPointInsideBox(Vector3 worldPoint, BoxSnapshot box, float epsilon)
        {
            Vector3 localPoint = box.WorldToLocal.MultiplyPoint3x4(worldPoint);

            Vector3 half = box.Size * 0.5f;

            return localPoint.x >= box.Center.x - half.x - epsilon &&
                   localPoint.x <= box.Center.x + half.x + epsilon &&
                   localPoint.y >= box.Center.y - half.y - epsilon &&
                   localPoint.y <= box.Center.y + half.y + epsilon &&
                   localPoint.z >= box.Center.z - half.z - epsilon &&
                   localPoint.z <= box.Center.z + half.z + epsilon;
        }

        private bool IsWorldPointInsideMesh(Vector3 worldPoint, MeshSnapshot mesh, float epsilon)
        {
            Vector3 localPoint = mesh.WorldToLocal.MultiplyPoint3x4(worldPoint);

            Vector3[] directions =
            {
                new(1f, 0.173f, 0.317f),
                new(0.271f, 1f, 0.139f),
                new(0.113f, 0.419f, 1f)
            };

            int insideVotes = 0;

            for (int i = 0; i < directions.Length; i++)
            {
                if (IsPointInsideMeshByRay(localPoint, directions[i].normalized, mesh.Vertices, mesh.Triangles, epsilon))
                    insideVotes++;
            }

            return insideVotes >= 2;
        }

        private bool IsPointInsideMeshByRay(
            Vector3 origin,
            Vector3 direction,
            Vector3[] vertices,
            int[] triangles,
            float epsilon)
        {
            int hitCount = 0;

            for (int i = 0; i < triangles.Length; i += 3)
            {
                Vector3 a = vertices[triangles[i]];
                Vector3 b = vertices[triangles[i + 1]];
                Vector3 c = vertices[triangles[i + 2]];

                if (RayIntersectsTriangle(origin, direction, a, b, c, out float distance))
                {
                    if (distance > epsilon)
                        hitCount++;
                }
            }

            return hitCount % 2 == 1;
        }

        private bool RayIntersectsTriangle(
            Vector3 origin,
            Vector3 direction,
            Vector3 a,
            Vector3 b,
            Vector3 c,
            out float distance)
        {
            distance = 0f;

            const float epsilon = 0.0000001f;

            Vector3 edge1 = b - a;
            Vector3 edge2 = c - a;

            Vector3 h = Vector3.Cross(direction, edge2);
            float determinant = Vector3.Dot(edge1, h);

            if (determinant > -epsilon && determinant < epsilon)
                return false;

            float inverseDeterminant = 1f / determinant;

            Vector3 s = origin - a;
            float u = inverseDeterminant * Vector3.Dot(s, h);

            if (u < 0f || u > 1f)
                return false;

            Vector3 q = Vector3.Cross(s, edge1);
            float v = inverseDeterminant * Vector3.Dot(direction, q);

            if (v < 0f || u + v > 1f)
                return false;

            distance = inverseDeterminant * Vector3.Dot(edge2, q);

            return distance > epsilon;
        }

        private int GetOverlapCandidates(BoxCollider boundsCollider, Vector3 position, Quaternion rotation)
        {
            Vector3 center = position + rotation * Vector3.Scale(boundsCollider.center, boundsCollider.transform.lossyScale);
            Vector3 halfExtents = Vector3.Scale(boundsCollider.size * 0.5f, boundsCollider.transform.lossyScale);

            return Physics.OverlapBoxNonAlloc(
                center,
                halfExtents,
                _overlapBuffer,
                rotation,
                _obstacleLayerMask,
                _triggerInteraction
            );
        }

        private Collider GetBoxObstacleCollider(Collider candidate)
        {
            ConstructionFurnitureCollisionHandler handler = candidate.GetComponentInParent<ConstructionFurnitureCollisionHandler>();

            if (handler == null)
                return candidate;

            return handler.BoundsCollider;
        }

        private Collider GetDetailedObstacleCollider(Collider candidate)
        {
            ConstructionFurnitureCollisionHandler handler = candidate.GetComponentInParent<ConstructionFurnitureCollisionHandler>();

            if (handler == null)
                return candidate;

            return handler.DetailedCollider != null ? handler.DetailedCollider : handler.BoundsCollider;
        }

        private bool IsOwnCollider(GameObject activeObject, Collider candidate)
        {
            return candidate.transform.root == activeObject.transform.root;
        }

        private MeshSnapshot CreateMeshSnapshot(MeshCollider meshCollider)
        {
            Mesh mesh = meshCollider.sharedMesh;

            if (mesh == null)
                return MeshSnapshot.Invalid;

            return new MeshSnapshot(
                mesh.vertices,
                mesh.triangles,
                meshCollider.transform.localToWorldMatrix,
                meshCollider.transform.worldToLocalMatrix
            );
        }

        private MeshTargetSnapshot CreateTargetSnapshot(Collider collider)
        {
            if (collider is MeshCollider meshCollider)
            {
                MeshSnapshot meshSnapshot = CreateMeshSnapshot(meshCollider);
                return MeshTargetSnapshot.FromMesh(meshSnapshot);
            }

            if (collider is BoxCollider boxCollider)
            {
                BoxSnapshot boxSnapshot = new(
                    boxCollider.center,
                    boxCollider.size,
                    boxCollider.transform.worldToLocalMatrix
                );

                return MeshTargetSnapshot.FromBox(boxSnapshot);
            }

            return MeshTargetSnapshot.Invalid;
        }

        private enum MeshTargetType
        {
            None,
            Box,
            Mesh
        }

        private readonly struct MeshPairCheckData
        {
            public readonly MeshSnapshot Source;
            public readonly MeshTargetSnapshot Target;

            public MeshPairCheckData(MeshSnapshot source, MeshTargetSnapshot target)
            {
                Source = source;
                Target = target;
            }
        }

        private readonly struct MeshSnapshot
        {
            public readonly bool IsValid;
            public readonly Vector3[] Vertices;
            public readonly int[] Triangles;
            public readonly Matrix4x4 LocalToWorld;
            public readonly Matrix4x4 WorldToLocal;

            public static MeshSnapshot Invalid => new(false, null, null, Matrix4x4.identity, Matrix4x4.identity);

            public MeshSnapshot(Vector3[] vertices, int[] triangles, Matrix4x4 localToWorld, Matrix4x4 worldToLocal)
                : this(true, vertices, triangles, localToWorld, worldToLocal)
            {
            }

            private MeshSnapshot(bool isValid, Vector3[] vertices, int[] triangles, Matrix4x4 localToWorld, Matrix4x4 worldToLocal)
            {
                IsValid = isValid;
                Vertices = vertices;
                Triangles = triangles;
                LocalToWorld = localToWorld;
                WorldToLocal = worldToLocal;
            }
        }

        private readonly struct BoxSnapshot
        {
            public readonly Vector3 Center;
            public readonly Vector3 Size;
            public readonly Matrix4x4 WorldToLocal;

            public BoxSnapshot(Vector3 center, Vector3 size, Matrix4x4 worldToLocal)
            {
                Center = center;
                Size = size;
                WorldToLocal = worldToLocal;
            }
        }

        private readonly struct MeshTargetSnapshot
        {
            public readonly bool IsValid;
            public readonly MeshTargetType Type;
            public readonly MeshSnapshot Mesh;
            public readonly BoxSnapshot Box;

            public static MeshTargetSnapshot Invalid => new(false, MeshTargetType.None, MeshSnapshot.Invalid, default);

            private MeshTargetSnapshot(bool isValid, MeshTargetType type, MeshSnapshot mesh, BoxSnapshot box)
            {
                IsValid = isValid;
                Type = type;
                Mesh = mesh;
                Box = box;
            }

            public static MeshTargetSnapshot FromMesh(MeshSnapshot mesh)
            {
                return new MeshTargetSnapshot(mesh.IsValid, MeshTargetType.Mesh, mesh, default);
            }

            public static MeshTargetSnapshot FromBox(BoxSnapshot box)
            {
                return new MeshTargetSnapshot(true, MeshTargetType.Box, MeshSnapshot.Invalid, box);
            }
        }
    }
}