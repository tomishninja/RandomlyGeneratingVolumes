using UnityEngine;

[System.Serializable]
public class VolumeHierarachyLevelDetails
{
    // input feilds for amounts
    [SerializeField] int amountOfOuters = 1;
    [SerializeField] int amountOfCountables = 0;
    [SerializeField] int amountOfContainers = 0;
    [SerializeField] int amountOfNotCountables = 0;

    public int AmountOfOuters { get => this.amountOfOuters; }
    public int AmountOfCountables { get => this.amountOfCountables; }
    public int AmountOfContainers { get => this.amountOfContainers; }
    public int AmountOfNotCountables { get => this.amountOfNotCountables; }
}
