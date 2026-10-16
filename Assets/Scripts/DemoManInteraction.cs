using System.Collections.Generic;
using UnityEngine;

namespace SpineEffectGame
{
    // A visible fallback for interaction design; this is not a Spine skeleton.
    public sealed class DemoManInteraction : ManInteractionSource
    {
        private sealed class Part
        {
            public Transform Bone;
            public Vector3 RestPosition;
            public Vector3 RestScale;
            public SpriteRenderer Renderer;
            public Color Color;
        }

        private readonly List<Part> parts = new List<Part>();
        private Part selected;
        private Texture2D texture;
        private Sprite circle;
        public override string SelectedBoneName => selected?.Bone.name;

        private void Awake()
        {
            texture = new Texture2D(64, 64, TextureFormat.RGBA32, false);
            texture.name = "Demo circle (generated)";
            var pixels = new Color[64 * 64];
            for (int y = 0; y < 64; y++)
                for (int x = 0; x < 64; x++)
                {
                    float distance = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(32, 32));
                    pixels[y * 64 + x] = new Color(1f, 1f, 1f, Mathf.Clamp01(31.5f - distance));
                }
            texture.SetPixels(pixels);
            texture.Apply();
            circle = Sprite.Create(texture, new Rect(0, 0, 64, 64), Vector2.one * 0.5f, 64);

            var blue = new Color(0.25f, 0.65f, 0.85f);
            var pale = new Color(0.96f, 0.75f, 0.53f);
            Part torso = AddPart("torso", transform, new Vector2(0, 0.25f), new Vector2(1.4f, 1.85f), blue, 1);
            AddPart("head", torso.Bone, new Vector2(0, 1.45f), new Vector2(0.95f, 1.05f), pale, 3);
            Part leftArm = AddPart("arm-left", torso.Bone, new Vector2(-0.95f, 0.25f), new Vector2(0.42f, 1.35f), blue, 2);
            AddPart("hand-left", leftArm.Bone, new Vector2(0, -0.78f), new Vector2(0.46f, 0.48f), pale, 4);
            Part rightArm = AddPart("arm-right", torso.Bone, new Vector2(0.95f, 0.25f), new Vector2(0.42f, 1.35f), blue, 2);
            AddPart("hand-right", rightArm.Bone, new Vector2(0, -0.78f), new Vector2(0.46f, 0.48f), pale, 4);
            Part leftLeg = AddPart("leg-left", torso.Bone, new Vector2(-0.4f, -1.5f), new Vector2(0.52f, 1.6f), blue, 0);
            AddPart("foot-left", leftLeg.Bone, new Vector2(-0.12f, -0.85f), new Vector2(0.7f, 0.36f), pale, 4);
            Part rightLeg = AddPart("leg-right", torso.Bone, new Vector2(0.4f, -1.5f), new Vector2(0.52f, 1.6f), blue, 0);
            AddPart("foot-right", rightLeg.Bone, new Vector2(0.12f, -0.85f), new Vector2(0.7f, 0.36f), pale, 4);
            AddDecoration("eye-left", torso.Bone.Find("head"), new Vector2(-0.19f, 0.06f), Vector2.one * 0.1f);
            AddDecoration("eye-right", torso.Bone.Find("head"), new Vector2(0.19f, 0.06f), Vector2.one * 0.1f);
        }

        private Part AddPart(string boneName, Transform parent, Vector2 position, Vector2 size, Color color, int order)
        {
            var bone = new GameObject(boneName).transform;
            bone.SetParent(parent, false);
            bone.localPosition = position;
            var visual = new GameObject("visual").transform;
            visual.SetParent(bone, false);
            visual.localScale = new Vector3(size.x, size.y, 1);
            var renderer = visual.gameObject.AddComponent<SpriteRenderer>();
            renderer.sprite = circle;
            renderer.color = color;
            renderer.sortingOrder = order;
            var part = new Part { Bone = bone, RestPosition = bone.localPosition, RestScale = bone.localScale, Renderer = renderer, Color = color };
            parts.Add(part);
            return part;
        }

        private void AddDecoration(string label, Transform parent, Vector2 position, Vector2 size)
        {
            var eye = new GameObject(label).transform;
            eye.SetParent(parent, false);
            eye.localPosition = position;
            eye.localScale = new Vector3(size.x, size.y, 1);
            var renderer = eye.gameObject.AddComponent<SpriteRenderer>();
            renderer.sprite = circle;
            renderer.color = new Color(0.08f, 0.1f, 0.16f);
            renderer.sortingOrder = 5;
        }

        public override bool BeginDrag(Vector3 worldPoint)
        {
            Release();
            foreach (var part in parts)
            {
                var local = part.Renderer.transform.InverseTransformPoint(worldPoint);
                if (local.x * local.x + local.y * local.y > 0.25f) continue;
                if (selected == null || part.Renderer.sortingOrder >= selected.Renderer.sortingOrder) selected = part;
            }
            if (selected == null) return false;
            selected.Renderer.color = Color.Lerp(selected.Color, Color.white, 0.35f);
            return true;
        }

        public override void Drag(Vector3 worldOffset, float scaleMultiplier)
        {
            if (selected == null) return;
            selected.Bone.localPosition = selected.RestPosition + selected.Bone.parent.InverseTransformVector(worldOffset);
            selected.Bone.localScale = selected.RestScale * scaleMultiplier;
        }

        public override void Release()
        {
            if (selected == null) return;
            selected.Bone.localPosition = selected.RestPosition;
            selected.Bone.localScale = selected.RestScale;
            selected.Renderer.color = selected.Color;
            selected = null;
        }

        private void OnDisable() => Release();
        private void OnDestroy()
        {
            if (circle != null) Destroy(circle);
            if (texture != null) Destroy(texture);
        }
    }
}
