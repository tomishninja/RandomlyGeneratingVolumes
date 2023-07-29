using Microsoft.MixedReality.Toolkit.Examples.Demos;
using Microsoft.MixedReality.Toolkit.UI;
using UnityEngine;

public class TouchSliderValueManager : MonoBehaviour
{
    ShowSliderValue showSliderValue;

    float value;

    [SerializeField] float min = 0;
    [SerializeField] float max = 1;

    public bool HasChanged { get; private set; }

    public float Value { get
        {
            HasChanged = false;
            return value;
        }
    } 

    // Start is called before the first frame update
    void Start()
    {
        if (showSliderValue == null)
        {
            // Check if the class exists in children
            bool classExistsInChildren = CheckIfClassExistsInChildren();

            // If the class exists in children, get the reference
            if (classExistsInChildren)
            {
                showSliderValue = GetComponentInChildren<ShowSliderValue>();
            }
        }

        // Make sure the visulizer and this look the same
        if (showSliderValue != null)
        {
            showSliderValue.min = this.min;
            showSliderValue.max = this.max;
        }
    }

    public void OnSliderUpdated(SliderEventData eventData)
    {
        value = (min + (eventData.NewValue * (max - min)));
    }

    // Boolean flag to indicate if the class is found in children
    private bool foundShowSliderValueClass = false;

    // Call this method to start the search
    public bool CheckIfClassExistsInChildren()
    {
        // Reset the flag before starting the search
        foundShowSliderValueClass = false;

        // Start the recursive search from the root object
        SearchInChildrenRecursive(transform);

        // Return the result after the search is completed
        return foundShowSliderValueClass;
    }

    // Recursive method to search for the class in children
    private void SearchInChildrenRecursive(Transform currentTransform)
    {
        // Check if the current object has the desired class attached
        Component component = currentTransform.GetComponent<ShowSliderValue>();

        // If the class is found, set the flag to true and return
        if (component != null)
        {
            foundShowSliderValueClass = true;
            return;
        }

        // Continue searching in children if the class is not found yet
        for (int i = 0; i < currentTransform.childCount; i++)
        {
            Transform childTransform = currentTransform.GetChild(i);
            SearchInChildrenRecursive(childTransform);

            // If the class is found in any of the children, return immediately
            if (foundShowSliderValueClass)
                return;
        }
    }
}
