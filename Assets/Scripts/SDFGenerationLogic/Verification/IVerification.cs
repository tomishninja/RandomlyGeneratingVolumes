using GenoratingRandomSDF;

public interface IVerification
{
    public abstract int Verify(ref ShapeHandeler shapes);

    public abstract void Reset();

    public abstract void SetLayerManager(ref LayerManager layerManager);
}
