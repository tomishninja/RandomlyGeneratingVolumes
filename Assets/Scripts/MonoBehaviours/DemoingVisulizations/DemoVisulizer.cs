using GenoratingRandomSDF;
using System;
using UnityEngine;

public class DemoVisulizer : MonoBehaviour, VisulizationAdapter
{
    public HierachicalObjects[] volumeParameters;
    [SerializeField] public Material mat;

    /// Needs to be updated for final study
    [SerializeField] VisulizationHandeler[] visulizations;

    // the object used to render this shape
    [SerializeField] public MeshRenderer renderer;

    public int AmountOfShapes { get => volumeParameters.Length; }

    public string Condition { get; set; }

    // Start is called before the first frame update
    void Start()
    {
        for (int index = 0; index < visulizations.Length; index++)
        {
            visulizations[index].Init();
        }

        if (mat != null && (volumeParameters != null && volumeParameters.Length > 0))
            SetUpShader();

        if (renderer == null)
            renderer = this.GetComponent<MeshRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (mat != null && (volumeParameters != null && volumeParameters.Length > 0))
            SetUpShader();//Maybe comment out later
    }

    private void SetUpShader()
    {
        mat.SetVectorArray("_SphereDetails", this.getShapeDetailsArray());
        mat.SetColorArray("_Colors", this.getColorArray());
        mat.SetFloatArray("_Importance", this.getImportanceArray());
        mat.SetFloat("_NoiseSuppression", 1f);
    }

    // Sets up the varibles for the ittal condition
    public void SetUpVisulization(ShapeHandeler shapes, string CurrentCondition = null)
    {
        // Set the amount of shapes 
        this.volumeParameters = shapes.GetShapesAsArray();

        // Set the material
        if (CurrentCondition == null)
            this.SetMat(this.volumeParameters.Length, Condition);
        else
            this.SetMat(this.volumeParameters.Length, CurrentCondition);

        // Set up the hash
        //itterationDetails.hash.SetInShader(this.mat, "_HashLineA", "_HashLineB", "_HashLineC");

        // Set shader variables
        mat.SetVectorArray("_SphereDetails", this.getShapeDetailsArray());
        mat.SetColorArray("_Colors", this.getColorArray());
        mat.SetFloatArray("_Importance", this.getImportanceArray());
    }

    /// <summary>
    /// This function should set the material based on a selection of materials that meet the required critia
    /// </summary>
    /// <param name="amountOfObjects">The amount of regions requried to render</param>
    /// <param name="ConditionName">The amount of conditiosn requierd to render</param>
    /// <exception cref="ArgumentNullException">
    /// Will be thrown if condition name is set to null
    /// </exception>
    /// <remarks>
    /// Will need to change for the final study
    /// </remarks>
    private void SetMat(int amountOfObjects, string ConditionName)
    {
        // if condition name is set to null do nothing
        if (ConditionName == null) throw new ArgumentNullException("Condition Name was set to null");

        for (int index = 0; index < visulizations.Length; index++)
        {
            if (ConditionName.ToLower().Equals(visulizations[index].Name.ToLower()))
            {
                this.mat = visulizations[index].GetMaterial(amountOfObjects);
                renderer.material = this.mat;
            }
        }
    }

    private Vector4[] getShapeDetailsArray()
    {
        Vector4[] output = new Vector4[volumeParameters.Length];

        for (int index = 0; index < output.Length; index++)
        {
            output[index] = this.volumeParameters[index].getPosAndSizeVetor4();
        }

        return output;
    }

    private float[] getImportanceArray()
    {
        float[] output = new float[volumeParameters.Length];

        for (int index = 0; index < output.Length; index++)
        {
            output[index] = this.volumeParameters[index].importance;
        }

        return output;
    }

    private Color[] getColorArray()
    {
        Color[] output = new Color[volumeParameters.Length];

        for (int index = 0; index < output.Length; index++)
        {
            output[index] = this.volumeParameters[index].color;
        }

        return output;
    }

    public Material GetMaterial()
    {
        return this.mat;
    }
}
