using UnityEngine;

public class ChangeColorOnHover : MonoBehaviour
{
    public Material mat;

    public Color Outside;

    public Color HovingInside;

    public void HoverIn()
    {
        this.mat.color = HovingInside;
    }

    public void HoverOut()
    {
        this.mat.color = Outside;
    }
}
