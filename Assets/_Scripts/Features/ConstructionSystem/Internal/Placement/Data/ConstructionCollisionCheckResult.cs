using UnityEngine;

namespace rpoboBLLjuk.SpaceCanvas
{
    public readonly struct ConstructionCollisionCheckResult
    {
        public bool HasCollision { get; }
        public bool IsValid => !HasCollision;

        public bool HasResolvedPosition { get; }
        public bool CanUseSuggestedPosition => HasCollision && HasResolvedPosition;

        public int CollisionCount { get; }
        public int IterationsUsed { get; }

        public Vector3 OriginalPosition { get; }
        public Vector3 SuggestedPosition { get; }
        public Vector3 Correction { get; }

        public Collider ActiveCollider { get; }


        public ConstructionCollisionCheckResult(
            bool hasCollision,
            bool hasResolvedPosition,
            int collisionCount,
            int iterationsUsed,
            Vector3 originalPosition,
            Vector3 suggestedPosition,
            Collider activeCollider)
        {
            HasCollision = hasCollision;
            HasResolvedPosition = hasResolvedPosition;

            CollisionCount = collisionCount;
            IterationsUsed = iterationsUsed;

            OriginalPosition = originalPosition;
            SuggestedPosition = suggestedPosition;
            Correction = suggestedPosition - originalPosition;

            ActiveCollider = activeCollider;
        }

        public static ConstructionCollisionCheckResult Valid(Vector3 position, Collider activeCollider)
        {
            return new ConstructionCollisionCheckResult(
                false,
                false,
                0,
                0,
                position,
                position,
                activeCollider
            );
        }
    }
}