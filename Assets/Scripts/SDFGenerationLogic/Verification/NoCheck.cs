using GenoratingRandomSDF;

public class NoCheck : IVerification
{
    private readonly int subProcessToReturn;
    LayerManager layerManager;

    public NoCheck(int subProcessToReturn = 0)
    {
        this.subProcessToReturn = subProcessToReturn;
    }

    public void Reset()
    {
        // Do Nothing
    }

    public int Verify(ref ShapeHandeler shapes)
    {
        return subProcessToReturn;
    }

    public void SetLayerManager(ref LayerManager layerManager)
    {
        this.layerManager = layerManager;
    }
}
