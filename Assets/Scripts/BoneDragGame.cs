using UnityEngine;

namespace SpineEffectGame
{
    public sealed class BoneDragGame : MonoBehaviour
    {
        [SerializeField] private Camera gameCamera;
        [SerializeField] private ManInteractionSource man;
        [SerializeField, Min(0f)] private float maxDisplacement = 0.18f;
        [SerializeField, Range(1f, 1.25f)] private float heldScale = 1.08f;
        private Vector3 pressPoint;
        private Plane dragPlane;
        private bool holding;
        private GUIStyle heading;
        private GUIStyle text;

        private void Start()
        {
            if (gameCamera == null) gameCamera = Camera.main;
            if (gameCamera == null)
            {
                Debug.LogError("BoneDragGame requires a camera.", this);
                enabled = false;
                return;
            }

            if (man == null) man = FindFirstObjectByType<ManInteractionSource>();
            if (man == null)
            {
                var demo = new GameObject("man");
                man = demo.AddComponent<DemoManInteraction>();
            }
        }

        private void Update()
        {
            if (man == null || !man.isActiveAndEnabled)
            {
                StopDragging();
                return;
            }

            if (Input.GetMouseButtonDown(0))
            {
                dragPlane = new Plane(-gameCamera.transform.forward, man.transform.position);
                if (PointerOnPlane(out pressPoint)) holding = man.BeginDrag(pressPoint);
            }

            if (!holding) return;
            if (!Input.GetMouseButton(0))
            {
                StopDragging();
                return;
            }

            if (PointerOnPlane(out var point))
                man.Drag(Vector3.ClampMagnitude(point - pressPoint, maxDisplacement), heldScale);
        }

        private bool PointerOnPlane(out Vector3 point)
        {
            var ray = gameCamera.ScreenPointToRay(Input.mousePosition);
            if (dragPlane.Raycast(ray, out var distance))
            {
                point = ray.GetPoint(distance);
                return true;
            }
            point = default;
            return false;
        }

        private void StopDragging()
        {
            if (man != null) man.Release();
            holding = false;
        }

        private void OnDisable() => StopDragging();
        private void OnApplicationFocus(bool focused) { if (!focused) StopDragging(); }
        private void OnApplicationPause(bool paused) { if (paused) StopDragging(); }

        private void OnGUI()
        {
            if (heading == null)
            {
                heading = new GUIStyle(GUI.skin.label) { fontSize = 28, fontStyle = FontStyle.Bold };
                heading.normal.textColor = new Color(0.93f, 0.96f, 1f);
                text = new GUIStyle(GUI.skin.label) { fontSize = 16, wordWrap = true };
                text.normal.textColor = new Color(0.7f, 0.8f, 0.9f);
            }
            float width = Mathf.Max(160f, Mathf.Min(Screen.width - 48f, 600f));
            GUI.Label(new Rect(24, 20, width, 42), "MAN / BONE PLAYGROUND", heading);
            GUI.Label(new Rect(24, 66, width, 60), "Hold left mouse on a body part. Drag gently. Release to reset.", text);
            var selected = man != null ? man.SelectedBoneName : null;
            GUI.Label(new Rect(24, Screen.height - 76, width, 60),
                (man is DemoManInteraction ? "DEMO RIG (Spine asset not connected)" : "SPINE RIG") +
                "\n" + (string.IsNullOrEmpty(selected) ? "Select a body part" : "Holding: " + selected), text);
        }
    }
}
