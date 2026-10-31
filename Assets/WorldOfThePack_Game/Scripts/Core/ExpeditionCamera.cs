using UnityEngine;

namespace WorldOfThePack.Game
{
    [ExecuteAlways]
    [RequireComponent(typeof(Camera))]
    public class ExpeditionCamera : MonoBehaviour
    {
        [Header("Corridor Framing")]
        [Tooltip("Minimum horizontal world width visible in viewport to guarantee the 42% corridor is framed")]
        [SerializeField] private float minHorizontalWidth = 5.5f;
        [SerializeField] private float defaultVerticalSize = 6.2f;

        [Header("Tracking Anchor (Centered Battlefield Framing)")]
        [SerializeField] private float verticalLead = 0.55f;
        [SerializeField] private float smoothTime = 0.25f;

        private Camera cam;
        private float currentVelocityY;

        private void Awake()
        {
            cam = GetComponent<Camera>();
            cam.orthographic = true;
            UpdateFraming();
        }

        private void LateUpdate()
        {
            UpdateFraming();
            UpdateTracking();
        }

        public void UpdateFraming()
        {
            if (cam == null) cam = GetComponent<Camera>();
            if (cam == null) return;

            float aspect = Mathf.Max(0.1f, cam.aspect);
            float requiredVerticalForWidth = (minHorizontalWidth * 0.5f) / aspect;
            cam.orthographicSize = Mathf.Max(defaultVerticalSize, requiredVerticalForWidth);
        }

        private void UpdateTracking()
        {
            if (!Application.isPlaying) return;

            Vector3 target = transform.position;
            target.x = 0f;
            target.z = -10f;

            if (FormationController.Instance != null)
            {
                float targetY = FormationController.Instance.AnchorPosition.y + verticalLead;
                target.y = Mathf.SmoothDamp(transform.position.y, targetY, ref currentVelocityY, smoothTime);
            }

            transform.position = target;
        }
    }
}
