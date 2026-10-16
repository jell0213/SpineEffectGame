using System;
using UnityEditor;
using UnityEngine;

namespace SpineEffectGame.Editor
{
    public static class DemoInteractionChecks
    {
        [MenuItem("Tools/SpineEffectGame/Run Demo Interaction Checks (Play Mode)")]
        private static void Run()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("Enter Play Mode before running interaction checks.");
                return;
            }

            var testMan = new GameObject("man-interaction-checks");
            testMan.transform.position = new Vector3(20, 0, 0);
            try
            {
                var source = testMan.AddComponent<DemoManInteraction>();
                var torso = testMan.transform.Find("torso");
                var head = torso.Find("head");
                var rest = head.localPosition;
                var restScale = head.localScale;
                var offset = new Vector3(0.1f, 0.05f, 0);

                Check(!source.BeginDrag(testMan.transform.position + new Vector3(5, 5, 0)), "Background must not select a bone.");
                for (int cycle = 0; cycle < 20; cycle++)
                {
                    Check(source.BeginDrag(head.position), "Head must be selectable.");
                    Check(source.SelectedBoneName == "head", "Overlapping torso must not capture head selection.");
                    for (int frame = 0; frame < 30; frame++) source.Drag(offset, 1.08f);
                    Check(Vector3.Distance(head.localPosition, rest + offset) < 0.0001f, "Held displacement must not accumulate.");
                    Check(Vector3.Distance(head.localScale, restScale * 1.08f) < 0.0001f, "Held scale must not accumulate.");
                    source.Release();
                    Check(Vector3.Distance(head.localPosition, rest) < 0.0001f, "Release must restore position.");
                    Check(Vector3.Distance(head.localScale, restScale) < 0.0001f, "Release must restore scale.");
                }

                var hand = torso.Find("arm-left/hand-left");
                var restHandPosition = hand.position;
                Check(source.BeginDrag(torso.position), "Torso must be selectable.");
                source.Drag(offset, 1.08f);
                Check(Vector3.Distance(hand.position, restHandPosition) > 0.01f, "Child bones must follow the selected parent.");
                source.enabled = false;
                Check(Vector3.Distance(hand.position, restHandPosition) < 0.0001f, "Disabling interaction must restore children.");
                Check(source.SelectedBoneName == null, "Disabling interaction must clear selection.");
                Debug.Log("PASS: demo picking, repeated drag/scale, release, child inheritance, disable reset. Mouse input and real Spine assets require separate checks.");
            }
            finally
            {
                UnityEngine.Object.Destroy(testMan);
            }
        }

        private static void Check(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException("Interaction check failed: " + message);
        }
    }
}
