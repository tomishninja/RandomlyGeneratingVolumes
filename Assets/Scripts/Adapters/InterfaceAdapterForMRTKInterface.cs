using GenoratingRandomSDF;
using UnityEngine;

public class InterfaceAdapterForMRTKInterface : MonoBehaviour
{
    [Header("Objects to save Values to")]
    [SerializeField] LogicControllerForGeneratingRandomSDFs logicController;
    SphericalVolumeHierarchyLevelDetails[] ObjectValues;

    [Header("Amounts Of Objects")]
    [SerializeField] TouchSliderValueManager SliderForAmountOfOuterObjects;
    [SerializeField] TouchSliderValueManager SliderForAmountOfCompositeObjects;
    [SerializeField] TouchSliderValueManager SliderForAmountOfLeafObjects;
    [SerializeField] TouchSliderValueManager SliderForAmountOfMiscObjects;

    [Header("Outer Object Paramters")]
    [SerializeField] SetUpColorPicker ColorPickerForOuterObjects;

    [Header("Sliders for composite objects")]
    [SerializeField] SetUpColorPicker ColorPickerForCompositeObjects;
    [SerializeField] TouchSliderValueManager SliderForMinSizeOfCompositeObjects;
    [SerializeField] TouchSliderValueManager SliderForMaxSizeOfCompositeObjects;
    [SerializeField] TouchSliderValueManager SliderForMinAmountOfChildrenForCompositeObjects;
    [SerializeField] TouchSliderValueManager SliderForMaxAmountOfChildrenForCompositeObjects;

    [Header("Sliders for single Objects")]
    [SerializeField] SetUpColorPicker ColorPickerForLeafObjects;
    [SerializeField] TouchSliderValueManager SliderForMinSizeOfLeafObjects;
    [SerializeField] TouchSliderValueManager SliderForMaxSizeOfLeafObjects;

    [Header("Sliders for the misc Objects")]
    [SerializeField] SetUpColorPicker ColorPickerForMiscObjects;
    [SerializeField] TouchSliderValueManager SliderForMinSizeOfMiscObjects;
    [SerializeField] TouchSliderValueManager SliderForMaxSizeOfMiscObjects;
    [SerializeField] TouchSliderValueManager SliderForMinAmountOfChildrenForMiscObjects;
    [SerializeField] TouchSliderValueManager SliderForMaxAmountOfChildrenForMiscObjects;

    private void Awake()
    {
        ObjectValues = logicController.GetConditionDetails();
    }

    private void Update()
    {
        if (SliderForAmountOfOuterObjects != null && SliderForAmountOfOuterObjects.HasChanged)
        {
            // Set the amount of outer objects;
        }
    }
}
