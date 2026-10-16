// Enable SPINE_UNITY only after importing a compatible Spine Unity Runtime.
#if SPINE_UNITY
using Spine;
using Spine.Unity;
using UnityEngine;

namespace SpineEffectGame
{
    [RequireComponent(typeof(SkeletonAnimation), typeof(SkeletonRenderer))]
    public sealed class SpineManInteraction : ManInteractionSource
    {
        [SerializeField, Min(0.01f)] private float hitRadius = 0.3f;
        [Tooltip("Empty means all non-root bones. Specify names to exclude helper/IK bones.")]
        [SerializeField] private string[] interactiveBones = new string[0];
        private SkeletonAnimation animationComponent;
        private Bone selected;
        private Bone modified;
        private Vector3 offset;
        private float scale = 1f;
        private float baseX, baseY, baseScaleX, baseScaleY;
        public override string SelectedBoneName => selected?.Data.Name;

        private void OnEnable()
        {
            animationComponent = GetComponent<SkeletonAnimation>();
            animationComponent.Initialize(false);
            animationComponent.UpdateLocal += ApplyInteraction;
            animationComponent.UpdateComplete += RestoreLocalPose;
        }

        public override bool BeginDrag(Vector3 worldPoint)
        {
            Release();
            if (animationComponent.Skeleton == null) return false;
            float closest = hitRadius * hitRadius;
            foreach (Bone bone in animationComponent.Skeleton.Bones)
            {
                if (!bone.Active || bone.Parent == null || !IsInteractive(bone.Data.Name)) continue;
                var pose = bone.AppliedPose;
                var start = transform.TransformPoint(new Vector3(pose.WorldX, pose.WorldY, 0));
                pose.LocalToWorld(bone.Data.Length, 0, out var tipX, out var tipY);
                var end = transform.TransformPoint(new Vector3(tipX, tipY, 0));
                var segment = end - start;
                float t = segment.sqrMagnitude > 0f ? Mathf.Clamp01(Vector3.Dot(worldPoint - start, segment) / segment.sqrMagnitude) : 0f;
                float distance = (worldPoint - (start + t * segment)).sqrMagnitude;
                if (distance > closest) continue;
                closest = distance;
                selected = bone;
            }
            return selected != null;
        }

        private bool IsInteractive(string boneName)
        {
            if (interactiveBones == null || interactiveBones.Length == 0) return true;
            foreach (var candidate in interactiveBones) if (candidate == boneName) return true;
            return false;
        }

        public override void Drag(Vector3 worldOffset, float scaleMultiplier)
        {
            offset = worldOffset;
            scale = scaleMultiplier;
        }

        private void ApplyInteraction(ISkeletonRenderer renderer)
        {
            if (selected == null) return;
            modified = selected;
            var pose = modified.Pose;
            baseX = pose.X;
            baseY = pose.Y;
            baseScaleX = pose.ScaleX;
            baseScaleY = pose.ScaleY;
            // Read the current local pose after animation, not last frame's edited pose.
            var delta = transform.InverseTransformVector(offset);
            var parent = modified.Parent.AppliedPose;
            float determinant = parent.A * parent.D - parent.B * parent.C;
            if (Mathf.Abs(determinant) > 0.000001f)
            {
                pose.X += (parent.D * delta.x - parent.B * delta.y) / determinant;
                pose.Y += (-parent.C * delta.x + parent.A * delta.y) / determinant;
            }
            pose.ScaleX = baseScaleX * scale;
            pose.ScaleY = baseScaleY * scale;
        }

        private void RestoreLocalPose(ISkeletonRenderer renderer)
        {
            if (modified == null) return;
            // World pose is already calculated for rendering. Keep the next animation
            // update's local inputs clean, including properties without animation keys.
            modified.Pose.X = baseX;
            modified.Pose.Y = baseY;
            modified.Pose.ScaleX = baseScaleX;
            modified.Pose.ScaleY = baseScaleY;
            modified = null;
        }

        public override void Release()
        {
            RestoreLocalPose(null);
            selected = null;
            offset = Vector3.zero;
            scale = 1f;
        }

        private void OnDisable()
        {
            Release();
            if (animationComponent == null) return;
            animationComponent.UpdateLocal -= ApplyInteraction;
            animationComponent.UpdateComplete -= RestoreLocalPose;
        }
    }
}
#endif
