using System;
using UnityEngine;

[System.Serializable]
public class VisulizationHandeler
{
    [SerializeField] string name;
    public string Name { get => this.name; }
    [SerializeField] MaterialHandler[] handler;

    /// <summary>
    /// Sets up the arrays so they are more efficent for later. 
    /// </summary>
    public void Init()
    {
        Array.Sort<MaterialHandler>(this.handler, new Comparison<MaterialHandler>((a, b) => a.CompareTo(b)));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="amountOfRegions">
    /// The amount of regions that this visulisation will need to render
    /// </param>
    /// <returns>
    /// retuns the appropriate material for the region that is specified
    /// </returns>
    public Material GetMaterial(int amountOfRegions)
    {
        //loop though all of the visluations and work 
        for (int index = 0; index < handler.Length; index++)
        {
            if (amountOfRegions <= handler[index].AmountOfRegions)
            {
                return handler[index].mat;
            }
        }
        return null;
    }

}
