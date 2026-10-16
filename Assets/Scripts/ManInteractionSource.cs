using UnityEngine;

namespace SpineEffectGame
{
    public abstract class ManInteractionSource : MonoBehaviour
    {
        public abstract string SelectedBoneName { get; }
        public abstract bool BeginDrag(Vector3 worldPoint);
        public abstract void Drag(Vector3 worldOffset, float scaleMultiplier);
        public abstract void Release();
    }
}
