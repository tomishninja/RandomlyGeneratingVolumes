using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class DropDownManager : MonoBehaviour
{
    public RectTransform[] tabPanels;
    public RectTransform[] buttonsToMove;
    public float animationDuration = 0.5f;
    public Ease easingType = Ease.OutQuad;

    private RectTransform activePanel;
    private float defaultHeight;
    private Vector2[] originalButtonPositions;
    private Vector2[] targetButtonPositions;

    private void Start()
    {
        foreach (var panel in tabPanels)
        {
            panel.gameObject.SetActive(false);
        }

        // Store the original positions of the buttons
        originalButtonPositions = new Vector2[buttonsToMove.Length];
        for (int i = 0; i < buttonsToMove.Length; i++)
        {
            originalButtonPositions[i] = buttonsToMove[i].anchoredPosition;
        }
    }

    public void TogglePanel(RectTransform panel)
    {
        if (activePanel == panel)
        {
            // Panel is already open; close it
            ClosePanel();
            return;
        }

        if (activePanel != null)
        {
            ClosePanel();
        }

        // Open the new panel
        activePanel = panel;
        activePanel.gameObject.SetActive(true);
        defaultHeight = activePanel.sizeDelta.y;
        activePanel.sizeDelta = new Vector2(activePanel.sizeDelta.x, 0f);

        // Calculate the target position for buttons below the active panel
        for (int i = 0; i < buttonsToMove.Length; i++)
        {
            if (buttonsToMove[i].anchoredPosition.y < activePanel.anchoredPosition.y)
            {
                targetButtonPositions[i] = buttonsToMove[i].anchoredPosition - new Vector2(0f, defaultHeight);
            }
            else
            {
                targetButtonPositions[i] = buttonsToMove[i].anchoredPosition;
            }
        }

        // Animate the opening of the new panel
        activePanel.DOSizeDelta(new Vector2(activePanel.sizeDelta.x, defaultHeight), animationDuration)
            .SetEase(easingType);

        for (int i = 0; i < buttonsToMove.Length; i++)
        {
            buttonsToMove[i].DOAnchorPosY(targetButtonPositions[i].y, animationDuration)
                .SetEase(easingType);
        }
    }

    private void ClosePanel()
    {
        // Animate the closing of the active panel
        activePanel.DOSizeDelta(new Vector2(activePanel.sizeDelta.x, 0f), animationDuration)
            .SetEase(easingType)
            .OnComplete(() =>
            {
                activePanel.gameObject.SetActive(false);
                activePanel = null;


                // Move the buttons back to their original positions
                MoveButtons(false);
            });
    }

    private void MoveButtons(bool moveUp)
    {
        for (int i = 0; i < buttonsToMove.Length; i++)
        {
            float yOffset = moveUp ? defaultHeight : -defaultHeight;
            buttonsToMove[i].DOAnchorPosY(buttonsToMove[i].anchoredPosition.y + yOffset, animationDuration)
                .SetEase(easingType);
        }
    }
}
