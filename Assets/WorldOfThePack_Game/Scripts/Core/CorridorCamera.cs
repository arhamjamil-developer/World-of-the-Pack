using UnityEngine;

namespace WorldOfThePack.Game
{
    [ExecuteAlways]
    [RequireComponent(typeof(Camera))]
    public class CorridorCamera : MonoBehaviour
    {
        [Header("Corridor Framing")]
        [Tooltip("Minimum horizontal world width visible in the viewport so the 42% lane is fully framed")]
        [SerializeField] private float minHorizontalWidth = 5.5f;
        [SerializeField] private float defaultVerticalSize = 6.5f;
        [SerializeField] private Vector3 targetFocus = new Vector3(0f, 0f, -10f);

        private Camera cam;

        private void Awake()
        {
            cam = GetComponent<Camera>();
            cam.orthographic = true;
            UpdateFraming();
        }

        private void LateUpdate()
        {
            UpdateFraming();
        }

        public void UpdateFraming()
        {
            if (cam == null) cam = GetComponent<Camera>();
            if (cam == null) return;

            // Camera is locked to center X=0
            transform.position = targetFocus;

            // Calculate orthographic size to guarantee the minHorizontalWidth is visible across any aspect ratio
            float aspect = Mathf.Max(0.1f, cam.aspect);
            float requiredVerticalForWidth = (minHorizontalWidth * 0.5f) / aspect;

            cam.orthographicSize = Mathf.Max(defaultVerticalSize, requiredVerticalForWidth);
        }
    }
}
