using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// A vertical scrollbar that jumps to a clicked track position instead of
/// moving a page at a time. Dragging the existing handle remains unchanged.
/// </summary>
public sealed class StatusMenuScrollbar : Scrollbar
{
    public override void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left &&
            IsActive() && IsInteractable() && handleRect != null &&
            !RectTransformUtility.RectangleContainsScreenPoint(
                handleRect, eventData.position, eventData.pressEventCamera))
        {
            RectTransform slidingArea = handleRect.parent as RectTransform;
            if (slidingArea != null &&
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    slidingArea, eventData.position, eventData.pressEventCamera, out Vector2 local))
            {
                float height = slidingArea.rect.height;
                float handleHeight = height * size;
                float travel = height - handleHeight;
                if (travel > 0.001f)
                {
                    // BottomToTop: 0 = bottom, 1 = top. Center the handle
                    // on the pointer when possible, and clamp at both ends.
                    value = Mathf.Clamp01(
                        (local.y - slidingArea.rect.yMin - handleHeight * 0.5f) / travel);
                }
            }
        }

        // After jumping, the pointer is over the handle (except at the
        // clamped ends). Keep Unity's standard press/drag/release handling.
        base.OnPointerDown(eventData);
    }
}
