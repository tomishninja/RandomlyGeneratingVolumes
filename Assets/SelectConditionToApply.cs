using Microsoft.MixedReality.Toolkit.UI;
using UnityEngine;

public class SelectConditionToApply : MonoBehaviour
{
    [SerializeField] InteractableToggleCollection interactableToggleCollection;

    [SerializeField] InterfaceAdapterForMRTKInterface interfaceAdapterForMRTKInterface;

    private void Start()
    {
        // if these are null try to fill them in
        if (interactableToggleCollection == null)
        {
            interactableToggleCollection = this.GetComponent<InteractableToggleCollection>();
        }

        if (interfaceAdapterForMRTKInterface == null)
        {
            interfaceAdapterForMRTKInterface = FindAnyObjectByType<InterfaceAdapterForMRTKInterface>();
        }
    }

    public void SelectCondition()
    {
        interfaceAdapterForMRTKInterface.ObjectValueIndex = interactableToggleCollection.CurrentIndex;

        interfaceAdapterForMRTKInterface.SetNewValuesBasedOnCurrent();
    }
}
