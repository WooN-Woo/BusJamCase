using UnityEngine;

namespace BusJam
{
    // moves the camera back for the level fits on screen
    [RequireComponent(typeof(Camera))]
    public class CameraFitter : MonoBehaviour
    {
        [SerializeField] private float tiltAngle = 48f;
        [SerializeField] private float padding = 0.8f;

        private Camera cachedCamera;

        public void Fit(Bounds area)
        {
            if (cachedCamera == null)
                cachedCamera = GetComponent<Camera>();

            transform.rotation = Quaternion.Euler(tiltAngle, 0f, 0f);

            // how big the area looks from the camera's angle
            float angle = tiltAngle * Mathf.Deg2Rad;
            float halfHeight = area.size.z * Mathf.Sin(angle) * 0.5f + padding;
            float halfWidth = area.size.x * 0.5f + padding;

            // distance so both width and height fit
            float tanHalfFov = Mathf.Tan(cachedCamera.fieldOfView * 0.5f * Mathf.Deg2Rad);
            float distanceForHeight = halfHeight / tanHalfFov;
            float distanceForWidth = halfWidth / (tanHalfFov * cachedCamera.aspect);
            float distance = Mathf.Max(distanceForHeight, distanceForWidth);

            cachedCamera.orthographic = false;
            transform.position = area.center - transform.forward * distance;
        }
    }
}
