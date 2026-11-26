using System;
using DG.Tweening;
using UnityEngine;

namespace BusJam
{
    public class Bus : MonoBehaviour
    {
        private static readonly int ColorId = Shader.PropertyToID("_Color");

        [SerializeField] private Renderer bodyRenderer;
        [SerializeField] private Renderer[] seatRenderers;
        [SerializeField] private Transform door;
        [SerializeField] private float moveSpeed = 14f;

        private MaterialPropertyBlock propertyBlock;
        private Tween moveTween;
        private Tween punchTween;
        private int reservedSeats;
        private int seatedPassengers;

        public GameColor BusColor { get; private set; }
        public bool IsAtStop { get; set; }
        public bool HasFreeSeat => reservedSeats < LevelData.BusCapacity;
        public bool IsFull => seatedPassengers >= LevelData.BusCapacity;
        public Vector3 DoorPosition => door.position;

        public void Setup(GameColor color)
        {
            BusColor = color;
            propertyBlock = new MaterialPropertyBlock();
            SetRendererColor(bodyRenderer, GameColors.ToColor(color));

            foreach (Renderer seat in seatRenderers)
                SetRendererColor(seat, new Color(0.15f, 0.15f, 0.18f));
        }

        public void ReserveSeat()
        {
            reservedSeats++;
        }

        public void SeatPassenger()
        {
            if (seatedPassengers < seatRenderers.Length)
                SetRendererColor(seatRenderers[seatedPassengers], Color.white);

            seatedPassengers++;

            punchTween?.Complete();
            punchTween = transform.DOPunchScale(Vector3.one * 0.15f, 0.25f, 6).SetLink(gameObject);
        }

        public void MoveTo(Vector3 target, Action onArrived)
        {
            moveTween?.Kill();
            moveTween = transform.DOMove(target, moveSpeed)
                .SetSpeedBased()
                .SetEase(Ease.InOutSine)
                .SetLink(gameObject)
                .OnComplete(() => onArrived?.Invoke());
        }

        private void SetRendererColor(Renderer target, Color color)
        {
            propertyBlock.SetColor(ColorId, color);
            target.SetPropertyBlock(propertyBlock);
        }
    }
}
