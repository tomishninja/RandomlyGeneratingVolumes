using GeneratingRandomSDF;

public interface IVerification
{
    public abstract int Verify(ref ShapeHandler shapes);

    public abstract void Reset();

    public abstract void SetLayerManager(ref LayerManager layerManager);
}
