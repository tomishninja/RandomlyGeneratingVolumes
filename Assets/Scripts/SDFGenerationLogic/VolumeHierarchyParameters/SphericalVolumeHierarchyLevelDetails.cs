using UnityEngine;


[System.Serializable]
public class SphericalVolumeHierarchyLevelDetails
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

    // Input feilds volume details
    [SerializeField] private VolumetricSphereArtifact OuterObjectInputInfo;
    [SerializeField] private VolumetricSphereArtifact CountableInputInformation;
    [SerializeField] private VolumetricSphereArtifact ContainableInputInformation;
    [SerializeField] private VolumetricSphereArtifact NotCountableInputInformation;

    // Required information for the outer
    public float OuterRadius { get => OuterObjectInputInfo.Radius; }
    public Color OuterColor { get => OuterObjectInputInfo.color; }
    public int OuterImportance { get => OuterObjectInputInfo.importance; }

    // Required information for the inner
    public float CountableRadius { get => CountableInputInformation.Radius; }
    public MinAndMaxFloat CountableRadiusRange { get => CountableInputInformation.minAndMaxRadiusMultipers; }
    public Color CountableColor { get => CountableInputInformation.color; }
    public int CountableImportance { get => CountableInputInformation.importance; }

    // Required information for the containers
    public float ContainableRadius { get => ContainableInputInformation.Radius; }
    public MinAndMaxFloat ContainableRadiusRange { get => ContainableInputInformation.minAndMaxRadiusMultipers; }
    public Color ContainableColor { get => ContainableInputInformation.color; }
    public MinAndMaxInt ContainableAmountOfChildren { get => ContainableInputInformation.amountOfChildrenRange; }
    public int ContainableImportance { get => ContainableInputInformation.importance; }

    // Required information for the containers
    public float NotCountableRadius { get => NotCountableInputInformation.Radius; }
    public MinAndMaxFloat NotCountableRadiusRange { get => NotCountableInputInformation.minAndMaxRadiusMultipers; }
    public Color NotCountableColor { get => NotCountableInputInformation.color; }
    public int NotCountableImportance { get => NotCountableInputInformation.importance; }

    public int CountAll()
    {
        return this.amountOfOuters + this.amountOfCountables + this.amountOfContainers + this.amountOfNotCountables;
    }

    public override bool Equals(object obj)
    {
        if (obj == null || GetType() != obj.GetType())
            return false;

        SphericalVolumeHierarchyLevelDetails other = (SphericalVolumeHierarchyLevelDetails)obj;

        // Compare amount fields
        if (amountOfOuters != other.amountOfOuters ||
            amountOfCountables != other.amountOfCountables ||
            amountOfContainers != other.amountOfContainers ||
            amountOfNotCountables != other.amountOfNotCountables)
        {
            return false;
        }

        // Compare OuterObjectInputInfo
        if (!OuterObjectInputInfo.Equals(other.OuterObjectInputInfo))
            return false;

        // Compare CountableInputInformation
        if (!CountableInputInformation.Equals(other.CountableInputInformation))
            return false;

        // Compare ContainableInputInformation
        if (!ContainableInputInformation.Equals(other.ContainableInputInformation))
            return false;

        // Compare NotCountableInputInformation
        if (!NotCountableInputInformation.Equals(other.NotCountableInputInformation))
            return false;

        return true;
    }

    public override int GetHashCode()
    {
        int hash = 17;
        hash = hash * 23 + amountOfOuters.GetHashCode();
        hash = hash * 23 + amountOfCountables.GetHashCode();
        hash = hash * 23 + amountOfContainers.GetHashCode();
        hash = hash * 23 + amountOfNotCountables.GetHashCode();

        // Combine the hash codes of the VolumetricSphereArtifact objects
        hash = hash * 23 + OuterObjectInputInfo.GetHashCode();
        hash = hash * 23 + CountableInputInformation.GetHashCode();
        hash = hash * 23 + ContainableInputInformation.GetHashCode();
        hash = hash * 23 + NotCountableInputInformation.GetHashCode();

        return hash;
    }
}
