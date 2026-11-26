using UnityEngine;

namespace BusJam
{
    public class InputHandler : MonoBehaviour
    {
        [SerializeField] private Camera mainCamera;
        [SerializeField] private Board board;

        private void Update()
        {
            if (!Input.GetMouseButtonDown(0))
                return;

            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (!Physics.Raycast(ray, out RaycastHit hit, 200f))
                return;

            Passenger passenger = hit.collider.GetComponentInParent<Passenger>();
            if (passenger != null)
                board.HandleTap(passenger);
        }
    }
}
