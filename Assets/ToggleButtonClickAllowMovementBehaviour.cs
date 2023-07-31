using Microsoft.MixedReality.Toolkit.UI;
using UnityEngine;

public class ToggleButtonClickAllowMovementBehaviour : MonoBehaviour
{
    [SerializeField] Interactable interactable;
    bool toggle = false;

    [SerializeField] ManipulationHandler manipulationHandler;
    [SerializeField] BoundingBox boundingBox;

    // Start is called before the first frame update
    void Start()
    {
        if (interactable == null)
            interactable = GetComponent<Interactable>();

        toggle = interactable.IsToggled;
        if (manipulationHandler != null)
            manipulationHandler.enabled = toggle;
        if (boundingBox != null)
            boundingBox.enabled = toggle;
    }

    public void ButtonPressed()
    {
        toggle = !toggle;
        if (manipulationHandler != null)
            manipulationHandler.enabled = toggle;
        if (boundingBox != null)
            boundingBox.enabled = toggle;
    }
}
