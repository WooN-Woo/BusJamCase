using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace BusJam
{
    public class WaitingArea : MonoBehaviour
    {
        [SerializeField] private int slotCount = 5;
        [SerializeField] private float slotSpacing = 1f;
        [SerializeField] private GameObject slotPrefab;

        [Header("Warning")]
        [SerializeField] private Color warningColor = new Color(1f, 0.25f, 0.2f);
        [SerializeField] private float warningBlinkTime = 0.3f;

        private readonly List<GameObject> slotObjects = new List<GameObject>();
        private readonly List<Renderer> slotRenderers = new List<Renderer>();
        private Passenger[] slots;
        private MaterialPropertyBlock propertyBlock;
        private Color normalColor;
        private Tween warningTween;

        public bool IsFull => FindFreeSlot() < 0;

        public void Build()
        {
            Clear();
            slots = new Passenger[slotCount];

            for (int i = 0; i < slotCount; i++)
            {
                GameObject slot = Instantiate(slotPrefab, GetSlotPosition(i), Quaternion.identity, transform);
                slotObjects.Add(slot);
                slotRenderers.Add(slot.GetComponentInChildren<Renderer>());
            }
            normalColor = slotRenderers[0].sharedMaterial.color;
            RefreshWarning();
        }

        public void Clear()
        {
            warningTween?.Kill();
            foreach (GameObject slot in slotObjects)
                Destroy(slot);
            slotObjects.Clear();
            slotRenderers.Clear();
            slots = null;
        }

        public int FindFreeSlot()
        {
            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i] == null)
                    return i;
            }
            return -1;
        }

        private int CountFreeSlots()
        {
            int count = 0;
            foreach (Passenger passenger in slots)
            {
                if (passenger == null)
                    count++;
            }
            return count;
        }

        // 1 slot left -> slots blink red as a warning
        private void RefreshWarning()
        {
            bool warn = CountFreeSlots() <= 1;
            bool isWarning = warningTween != null && warningTween.IsActive();
            if (warn == isWarning)
                return;

            if (warn)
            {
                warningTween = DOVirtual.Float(0f, 1f, warningBlinkTime, blink => SetSlotColor(Color.Lerp(normalColor, warningColor, blink)))
                    .SetLoops(-1, LoopType.Yoyo);
            }
            else
            {
                warningTween.Kill();
                SetSlotColor(normalColor);
            }
        }

        private void SetSlotColor(Color color)
        {
            if (propertyBlock == null)
                propertyBlock = new MaterialPropertyBlock();

            propertyBlock.SetColor("_Color", color);
            foreach (Renderer slotRenderer in slotRenderers)
                slotRenderer.SetPropertyBlock(propertyBlock);
        }

        public void Reserve(int slotIndex, Passenger passenger)
        {
            slots[slotIndex] = passenger;
            RefreshWarning();
        }

        public void Remove(Passenger passenger)
        {
            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i] == passenger)
                    slots[i] = null;
            }
            RefreshWarning();
        }

        // someone of this color already sitting in a slot?
        public Passenger FindWaitingPassenger(GameColor color)
        {
            foreach (Passenger passenger in slots)
            {
                if (passenger != null && !passenger.IsMoving && passenger.PassengerColor == color)
                    return passenger;
            }
            return null;
        }

        public Vector3 GetSlotPosition(int slotIndex)
        {
            float offset = (slotCount - 1) * 0.5f;
            return transform.position + Vector3.right * ((slotIndex - offset) * slotSpacing);
        }
    }
}
