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
    public BoxCollider bounds;

    [Header("Base Behvaiour")]
    public int anchoredButtonIconIndex;
    public int MoveableButtonIconIndex;
    public ButtonIconSet IconSet;

    public string AnchoredLabel;
    public string MovableLabel;

    public GameObject[] MovableObjects;
    public GameObject[] AnchoredObjects;

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
        bounds.enabled = true;

        buttonConfigHelper.SetQuadIcon(IconSet.QuadIcons[MoveableButtonIconIndex]);
        buttonConfigHelper.MainLabelText = MovableLabel;
        ObjectIsMovable = true;

        if (this.MovableObjects != null)
        {
            for(int index = 0; index < this.MovableObjects.Length; index++)
            {
                this.MovableObjects[index].SetActive(true);
            }
        }

        if (this.AnchoredObjects != null)
        {
            for (int index = 0; index < this.AnchoredObjects.Length; index++)
            {
                this.AnchoredObjects[index].SetActive(false);
            }
        }
    }

    public void SetToAnchored()
    {
        boundingBox.enabled = false;
        maniputionObject.enabled = false;
        bounds.enabled = false;

        buttonConfigHelper.SetQuadIcon(IconSet.QuadIcons[anchoredButtonIconIndex]);
        buttonConfigHelper.MainLabelText = AnchoredLabel;
        ObjectIsMovable = false;

        if (this.AnchoredObjects != null)
        {
            for (int index = 0; index < this.AnchoredObjects.Length; index++)
            {
                this.AnchoredObjects[index].SetActive(true);
            }
        }

        if (this.MovableObjects != null)
        {
            for (int index = 0; index < this.MovableObjects.Length; index++)
            {
                this.MovableObjects[index].SetActive(false);
            }
        }
    }
}
