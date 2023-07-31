using UnityEngine;

    public class InterfaceAdapterForMRTKInterface : MonoBehaviour
    {
        public int ObjectValueIndex = -1;

        [Header("Objects to save Values to")]
        LogicControllerForGeneratingRandomSDFs logicController;
        SphericalVolumeHierarchyLevelDetails[] ObjectValues;

        [Header("Amounts Of Objects")]
        [SerializeField] TouchSliderValueManager SliderForAmountOfOuterObjects;
        [SerializeField] TouchSliderValueManager SliderForAmountOfCompositeObjects;
        [SerializeField] TouchSliderValueManager SliderForAmountOfLeafObjects;
        [SerializeField] TouchSliderValueManager SliderForAmountOfMiscObjects;

        [Header("Outer Object Paramters")]

        [Header("Sliders for composite objects")]
        [SerializeField] TouchSliderValueManager SliderForMinSizeOfCompositeObjects;
        [SerializeField] TouchSliderValueManager SliderForMaxSizeOfCompositeObjects;
        [SerializeField] TouchSliderValueManager SliderForMinAmountOfChildrenForCompositeObjects;
        [SerializeField] TouchSliderValueManager SliderForMaxAmountOfChildrenForCompositeObjects;

        [Header("Sliders for single Objects")]
        [SerializeField] TouchSliderValueManager SliderForMinSizeOfLeafObjects;
        [SerializeField] TouchSliderValueManager SliderForMaxSizeOfLeafObjects;

        [Header("Sliders for the misc Objects")]
        [SerializeField] TouchSliderValueManager SliderForMinSizeOfMiscObjects;
        [SerializeField] TouchSliderValueManager SliderForMaxSizeOfMiscObjects;
        [SerializeField] TouchSliderValueManager SliderForMinAmountOfChildrenForMiscObjects;
        [SerializeField] TouchSliderValueManager SliderForMaxAmountOfChildrenForMiscObjects;

        [Header("Color Picker Object")]
        [SerializeField] SetUpColorPicker ColorPicker;



    private void Awake()
        {
            ObjectValues = logicController.GetConditionDetails();
        }

        private void Update()
        {
            //Sliders

            // If you required to update the values of the objects
            if (SliderForAmountOfOuterObjects != null && SliderForAmountOfOuterObjects.HasChanged)
            {
                if (ObjectValueIndex >= 0 && ObjectValueIndex < ObjectValues.Length)
                {
                    // Set the amount of outer objects;
                    ObjectValues[ObjectValueIndex].AmountOfOuters = Mathf.RoundToInt(SliderForAmountOfOuterObjects.Value);
                }
                else if (ObjectValueIndex < 0)
                {
                    int newValue = Mathf.RoundToInt(SliderForAmountOfOuterObjects.Value);
                    for (int index = 0; index < ObjectValues.Length; index++)
                    {
                        ObjectValues[index].AmountOfOuters = newValue;
                    }
                }
            }

            // If you required to update the values of the objects
            if (SliderForAmountOfCompositeObjects != null && SliderForAmountOfCompositeObjects.HasChanged)
            {
                if (ObjectValueIndex >= 0 && ObjectValueIndex < ObjectValues.Length)
                {
                    // Set the amount of outer objects;
                    ObjectValues[ObjectValueIndex].amountOfContainers = Mathf.RoundToInt(SliderForAmountOfCompositeObjects.Value);
                }
                else if (ObjectValueIndex < 0)
                {
                    int newValue = Mathf.RoundToInt(SliderForAmountOfCompositeObjects.Value);
                    for (int index = 0; index < ObjectValues.Length; index++)
                    {
                        ObjectValues[index].amountOfContainers = newValue;
                    }
                }
            }

            // If you required to update the values of the objects
            if (SliderForAmountOfLeafObjects != null && SliderForAmountOfLeafObjects.HasChanged)
            {
                if (ObjectValueIndex >= 0 && ObjectValueIndex < ObjectValues.Length)
                {
                    // Set the amount of outer objects;
                    ObjectValues[ObjectValueIndex].AmountOfCountables = Mathf.RoundToInt(SliderForAmountOfLeafObjects.Value);
                }
                else if (ObjectValueIndex < 0)
                {
                    int newValue = Mathf.RoundToInt(SliderForAmountOfLeafObjects.Value);
                    for (int index = 0; index < ObjectValues.Length; index++)
                    {
                        ObjectValues[index].AmountOfCountables = newValue;
                    }
                }
            }

            // If you required to update the values of the objects
            if (SliderForAmountOfMiscObjects != null && SliderForAmountOfMiscObjects.HasChanged)
            {
                if (ObjectValueIndex >= 0 && ObjectValueIndex < ObjectValues.Length)
                {
                    // Set the amount of outer objects;
                    ObjectValues[ObjectValueIndex].AmountOfNotCountables = Mathf.RoundToInt(SliderForAmountOfMiscObjects.Value);
                }
                else if (ObjectValueIndex < 0)
                {
                    int newValue = Mathf.RoundToInt(SliderForAmountOfMiscObjects.Value);
                    for (int index = 0; index < ObjectValues.Length; index++)
                    {
                        ObjectValues[index].AmountOfNotCountables = newValue;
                    }
                }
            }

            ///
            /// Compiste Objects Sliders
            /// 

            // If you required to update the values of the objects
            if (SliderForMinSizeOfCompositeObjects != null && SliderForMinSizeOfCompositeObjects.HasChanged)
            {
                if (ObjectValueIndex >= 0 && ObjectValueIndex < ObjectValues.Length)
                {
                    // Set the amount of outer objects;
                    ObjectValues[ObjectValueIndex].ContainableRadiusRange.min = SliderForMinSizeOfCompositeObjects.Value;
                }
                else if (ObjectValueIndex < 0)
                {
                    for (int index = 0; index < ObjectValues.Length; index++)
                    {
                        ObjectValues[index].ContainableRadiusRange.min = SliderForMinSizeOfCompositeObjects.Value;
                    }
                }
        }

        // If you required to update the values of the objects
        if (SliderForMaxSizeOfCompositeObjects != null && SliderForMaxSizeOfCompositeObjects.HasChanged)
        {
            if (ObjectValueIndex >= 0 && ObjectValueIndex < ObjectValues.Length)
            {
                // Set the amount of outer objects;
                ObjectValues[ObjectValueIndex].ContainableRadiusRange.max = SliderForMaxSizeOfCompositeObjects.Value;
            }
            else if (ObjectValueIndex < 0)
            {
                for (int index = 0; index < ObjectValues.Length; index++)
                {
                    ObjectValues[index].ContainableRadiusRange.max = SliderForMaxSizeOfCompositeObjects.Value;
                }
            }
        }

        // If you required to update the values of the objects
        if (SliderForMinAmountOfChildrenForCompositeObjects != null && SliderForMinAmountOfChildrenForCompositeObjects.HasChanged)
        {
            if (ObjectValueIndex >= 0 && ObjectValueIndex < ObjectValues.Length)
            {
                // Set the amount of outer objects;
                ObjectValues[ObjectValueIndex].ContainableAmountOfChildren.min = Mathf.RoundToInt(SliderForMinAmountOfChildrenForCompositeObjects.Value);
            }
            else if (ObjectValueIndex < 0)
            {
                int newValue = Mathf.RoundToInt(SliderForMinAmountOfChildrenForCompositeObjects.Value);
                for (int index = 0; index < ObjectValues.Length; index++)
                {
                    ObjectValues[index].ContainableAmountOfChildren.min = newValue;
                }
            }
        }

        // If you required to update the values of the objects
        if (SliderForMaxAmountOfChildrenForCompositeObjects != null && SliderForMaxAmountOfChildrenForCompositeObjects.HasChanged)
        {
            if (ObjectValueIndex >= 0 && ObjectValueIndex < ObjectValues.Length)
            {
                // Set the amount of outer objects;
                ObjectValues[ObjectValueIndex].ContainableAmountOfChildren.max = Mathf.RoundToInt(SliderForMaxAmountOfChildrenForCompositeObjects.Value);
            }
            else if (ObjectValueIndex < 0)
            {
                int newValue = Mathf.RoundToInt(SliderForMaxAmountOfChildrenForCompositeObjects.Value);
                for (int index = 0; index < ObjectValues.Length; index++)
                {
                    ObjectValues[index].ContainableAmountOfChildren.max = newValue;
                }
            }
        }

        ///
        /// Misc Objects Sliders
        ///

        // If you required to update the values of the objects
        if (SliderForMinSizeOfMiscObjects != null && SliderForMinSizeOfMiscObjects.HasChanged)
        {
            if (ObjectValueIndex >= 0 && ObjectValueIndex < ObjectValues.Length)
            {
                // Set the amount of outer objects;
                ObjectValues[ObjectValueIndex].NotCountableRadiusRange.min = SliderForMinSizeOfMiscObjects.Value;
            }
            else if (ObjectValueIndex < 0)
            {
                for (int index = 0; index < ObjectValues.Length; index++)
                {
                    ObjectValues[index].NotCountableRadiusRange.min = SliderForMinSizeOfMiscObjects.Value;
                }
            }
        }

        // If you required to update the values of the objects
        if (SliderForMaxSizeOfMiscObjects != null && SliderForMaxSizeOfMiscObjects.HasChanged)
        {
            if (ObjectValueIndex >= 0 && ObjectValueIndex < ObjectValues.Length)
            {
                // Set the amount of outer objects;
                ObjectValues[ObjectValueIndex].NotCountableRadiusRange.max = SliderForMaxSizeOfMiscObjects.Value;
            }
            else if (ObjectValueIndex < 0)
            {
                for (int index = 0; index < ObjectValues.Length; index++)
                {
                    ObjectValues[index].NotCountableRadiusRange.max = SliderForMaxSizeOfMiscObjects.Value;
                }
            }
        }

        // If you required to update the values of the objects
        if (SliderForMinAmountOfChildrenForMiscObjects != null && SliderForMinAmountOfChildrenForMiscObjects.HasChanged)
        {
            if (ObjectValueIndex >= 0 && ObjectValueIndex < ObjectValues.Length)
            {
                // Set the amount of outer objects;
                ObjectValues[ObjectValueIndex].NotCountableAmountOfChildren.min = Mathf.RoundToInt(SliderForMinAmountOfChildrenForCompositeObjects.Value);
            }
            else if (ObjectValueIndex < 0)
            {
                int newValue = Mathf.RoundToInt(SliderForMinAmountOfChildrenForCompositeObjects.Value);
                for (int index = 0; index < ObjectValues.Length; index++)
                {
                    ObjectValues[index].NotCountableAmountOfChildren.min = newValue;
                }
            }
        }

        // If you required to update the values of the objects
        if (SliderForMaxAmountOfChildrenForMiscObjects != null && SliderForMaxAmountOfChildrenForMiscObjects.HasChanged)
        {
            if (ObjectValueIndex >= 0 && ObjectValueIndex < ObjectValues.Length)
            {
                // Set the amount of outer objects;
                ObjectValues[ObjectValueIndex].NotCountableAmountOfChildren.max = Mathf.RoundToInt(SliderForMaxAmountOfChildrenForMiscObjects.Value);
            }
            else if (ObjectValueIndex < 0)
            {
                int newValue = Mathf.RoundToInt(SliderForMaxAmountOfChildrenForMiscObjects.Value);
                for (int index = 0; index < ObjectValues.Length; index++)
                {
                    ObjectValues[index].NotCountableAmountOfChildren.max = newValue;
                }
            }
        }

        ///
        /// Leaf Objects Sliders
        ///

        // If you required to update the values of the objects
        if (SliderForMinSizeOfLeafObjects != null && SliderForMinSizeOfLeafObjects.HasChanged)
        {
            if (ObjectValueIndex >= 0 && ObjectValueIndex < ObjectValues.Length)
            {
                // Set the amount of outer objects;
                ObjectValues[ObjectValueIndex].CountableRadiusRange.min = SliderForMinSizeOfLeafObjects.Value;
            }
            else if (ObjectValueIndex < 0)
            {
                for (int index = 0; index < ObjectValues.Length; index++)
                {
                    ObjectValues[index].CountableRadiusRange.min = SliderForMinSizeOfLeafObjects.Value;
                }
            }
        }

        // If you required to update the values of the objects
        if (SliderForMaxSizeOfLeafObjects != null && SliderForMaxSizeOfLeafObjects.HasChanged)
        {
            if (ObjectValueIndex >= 0 && ObjectValueIndex < ObjectValues.Length)
            {
                // Set the amount of outer objects;
                ObjectValues[ObjectValueIndex].CountableRadiusRange.max = SliderForMaxSizeOfLeafObjects.Value;
            }
            else if (ObjectValueIndex < 0)
            {
                for (int index = 0; index < ObjectValues.Length; index++)
                {
                    ObjectValues[index].CountableRadiusRange.max = SliderForMaxSizeOfLeafObjects.Value;
                }
            }
        }
    }


    /// <summary>
    /// Sets all of the sliders based on the input values form the main set of values
    /// </summary>
    public void SetNewValuesBasedOnCurrent()
    {
        if (SliderForAmountOfOuterObjects != null)
        {
            SliderForAmountOfOuterObjects.Value = ObjectValues[ObjectValueIndex].AmountOfOuters;
        }

        if (SliderForAmountOfCompositeObjects != null)
        {
            SliderForAmountOfCompositeObjects.Value = ObjectValues[ObjectValueIndex].amountOfContainers;
        }

        if (SliderForAmountOfLeafObjects != null)
        {
            SliderForAmountOfLeafObjects.Value = ObjectValues[ObjectValueIndex].AmountOfCountables;
        }

        if (SliderForAmountOfMiscObjects != null)
        {
            SliderForAmountOfMiscObjects.Value = ObjectValues[ObjectValueIndex].AmountOfNotCountables;
        }

        if (SliderForMinSizeOfCompositeObjects != null)
        {
            SliderForMinSizeOfCompositeObjects.Value = ObjectValues[ObjectValueIndex].ContainableRadiusRange.min;
        }

        if (SliderForMaxSizeOfCompositeObjects != null)
        {
            SliderForMaxSizeOfCompositeObjects.Value = ObjectValues[ObjectValueIndex].ContainableRadiusRange.max;
        }

        if (SliderForMinAmountOfChildrenForCompositeObjects != null)
        {
            SliderForMinAmountOfChildrenForCompositeObjects.Value = ObjectValues[ObjectValueIndex].ContainableAmountOfChildren.min;
        }

        if (SliderForMaxAmountOfChildrenForCompositeObjects != null)
        {
            SliderForMaxAmountOfChildrenForCompositeObjects.Value = ObjectValues[ObjectValueIndex].ContainableAmountOfChildren.max;
        }

        if (SliderForMinSizeOfMiscObjects != null)
        {
            SliderForMinSizeOfMiscObjects.Value = ObjectValues[ObjectValueIndex].NotCountableRadiusRange.min;
        }

        if (SliderForMaxSizeOfMiscObjects != null)
        {
            SliderForMaxSizeOfMiscObjects.Value = ObjectValues[ObjectValueIndex].NotCountableRadiusRange.max;
        }

        if (SliderForMinAmountOfChildrenForMiscObjects != null)
        {
            SliderForMinAmountOfChildrenForMiscObjects.Value = ObjectValues[ObjectValueIndex].NotCountableAmountOfChildren.min;
        }

        if (SliderForMaxAmountOfChildrenForMiscObjects != null)
        {
            SliderForMaxAmountOfChildrenForMiscObjects.Value = ObjectValues[ObjectValueIndex].NotCountableAmountOfChildren.max;
        }

        if (SliderForMinSizeOfLeafObjects != null)
        {
            SliderForMinSizeOfLeafObjects.Value = ObjectValues[ObjectValueIndex].CountableRadiusRange.min;
        }

        if (SliderForMaxSizeOfLeafObjects != null)
        {
            SliderForMaxSizeOfLeafObjects.Value = ObjectValues[ObjectValueIndex].CountableRadiusRange.max;
        }
    }
}
