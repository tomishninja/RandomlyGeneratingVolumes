using Microsoft.MixedReality.Toolkit.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickUpAndMoveButtonBehaviour : MonoBehaviour
{
    [Header("Unity Assets"), SerializeField]
    ButtonConfigHelper buttonConfigHelper;
    public Transform ItemToSet;
    public BoundingBox boundingBox;
    public ManipulationHandler maniputionObject;

    [Header("Base Behvaiour")]
    public int anchoredButtonIconIndex;
    public int MoveableButtonIconIndex;
    public ButtonIconSet IconSet;

    [Header("Run Time Behaviour Settings")]
    public bool ObjectIsMovable = false;


    // Start is called before the first frame update
    void Start()
    {
        if (buttonConfigHelper == null)
            buttonConfigHelper = this.gameObject.GetComponent<ButtonConfigHelper>();

        if (ObjectIsMovable) 
        {
            SetToMoveable();
        }
        else
        {
            SetToAnchored();
        }
    }

    public void ButtonPressBehaviour()
    {
        if (ObjectIsMovable)
        {
            SetToAnchored();
        }
        else
        {
            SetToMoveable();
        }
    }

    public void SetToMoveable()
    {
        boundingBox.enabled = true;
        maniputionObject.enabled = true;

        buttonConfigHelper.SetQuadIcon(IconSet.QuadIcons[MoveableButtonIconIndex]);
        ObjectIsMovable = true;
    }

    public void SetToAnchored()
    {
        boundingBox.enabled = false;
        maniputionObject.enabled = false;

        buttonConfigHelper.SetQuadIcon(IconSet.QuadIcons[anchoredButtonIconIndex]);
        ObjectIsMovable = false;
    }
}
