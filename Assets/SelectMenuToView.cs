using Microsoft.MixedReality.Toolkit.UI;
using UnityEngine;

public class SelectMenuToView : MonoBehaviour
{
    [SerializeField] InteractableToggleCollection interactableToggleCollection;

    [SerializeField] GameObject[] menuList;

    public void SelectMenu()
    {
        if (menuList != null)
        {
            for (int index = 0; index < menuList.Length; index++)
            {
                if (menuList[index] != null)
                {
                    menuList[index].SetActive(interactableToggleCollection.CurrentIndex == index);
                }
            }
        }
    }
}
