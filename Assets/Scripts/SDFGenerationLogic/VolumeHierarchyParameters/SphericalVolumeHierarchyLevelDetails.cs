using UnityEngine;


/// <summary>
/// Describes a single study condition: the full three-layer sphere hierarchy.
/// <list type="bullet">
///   <item><description><b>Outer</b> – the single large enclosing sphere.</description></item>
///   <item><description><b>Container</b> – mid-level spheres nested inside the outer sphere.</description></item>
///   <item><description><b>Countable</b> – small inner spheres that participants are asked to count.</description></item>
///   <item><description><b>NotCountable</b> – additional small inner spheres that are visual distractors.</description></item>
/// </list>
/// </summary>
[System.Serializable]
public class SphericalVolumeHierarchyLevelDetails
{
    // input fields for amounts
    [SerializeField] int amountOfOuters = 1;
    [SerializeField] int amountOfCountables = 0;
    [SerializeField] int amountOfContainers = 0;
    [SerializeField] int amountOfNotCountables = 0;

    public int AmountOfOuters { get => this.amountOfOuters; }
    public int AmountOfCountables { get => this.amountOfCountables; }
    public int AmountOfContainers { get => this.amountOfContainers; }
    public int AmountOfNotCountables { get => this.amountOfNotCountables; }

    // Input fields volume details
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
}
