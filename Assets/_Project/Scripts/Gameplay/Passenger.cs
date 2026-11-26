using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace BusJam
{
    public class Passenger : MonoBehaviour
    {
        private static readonly int ColorId = Shader.PropertyToID("_Color");

        [SerializeField] private Renderer bodyRenderer;
        [SerializeField] private Renderer outlineRenderer;
        [SerializeField] private float moveSpeed = 7f;

        private MaterialPropertyBlock propertyBlock;

        public GameColor PassengerColor { get; private set; }
        public Vector2Int GridPosition { get; private set; }
        public bool IsOnGrid { get; private set; }
        public bool IsMoving { get; private set; }

        public void Setup(GameColor color, Vector2Int gridPosition)
        {
            PassengerColor = color;
            GridPosition = gridPosition;
            IsOnGrid = true;

            if (propertyBlock == null)
                propertyBlock = new MaterialPropertyBlock();

            propertyBlock.SetColor(ColorId, GameColors.ToColor(color));
            bodyRenderer.SetPropertyBlock(propertyBlock);
        }

        public void SetReachable(bool reachable)
        {
            outlineRenderer.enabled = reachable;
        }

        public void LeaveGrid()
        {
            IsOnGrid = false;
            SetReachable(false);
        }

        public void MoveAlong(List<Vector3> points, Action onArrived)
        {
            transform.DOComplete();

            IsMoving = true;
            transform.DOPath(points.ToArray(), moveSpeed)
                .SetSpeedBased()
                .SetEase(Ease.Linear)
                .SetLookAt(0.01f)
                .SetLink(gameObject)
                .OnComplete(() =>
                {
                    IsMoving = false;
                    onArrived?.Invoke();
                });
        }

        public void PlayBlockedShake()
        {
            if (IsMoving)
                return;

            transform.DOComplete();
            transform.DOPunchPosition(Vector3.right * 0.25f, 0.45f, 10, 1f).SetLink(gameObject);
        }
    }
}
