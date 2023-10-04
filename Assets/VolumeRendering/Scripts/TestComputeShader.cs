using UnityEngine;

public class TestComputeShader : MonoBehaviour
{
    public ComputeShader computeShader;
    public RenderTexture renderTexture;

    // Start is called before the first frame update
    void Start()
    {
        if (renderTexture == null)
            renderTexture = new RenderTexture(256, 256, 24);
        renderTexture.enableRandomWrite = true;
        renderTexture.Create();

        computeShader.SetTexture(0, "Result", renderTexture);
        computeShader.SetVector("Resultion", new Vector4(256, 256, 0, 0));
        computeShader.Dispatch(0, 256 / 8, 256 / 8, 1);
    }
}
