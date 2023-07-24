using GenoratingRandomSDF;

public class CheckingStateContoller
{
    IVerification outer;
    IVerification container;
    IVerification inner;

    IVerification current;

    public CheckingStateContoller(ref IVerification outer, ref IVerification container, ref IVerification inner)
    {
        this.outer = outer;
        this.container = container;
        this.inner = inner;

        current = outer;
    }

    public IVerification Get()
    {
        return current;
    }

    public void SetOuter()
    {
        current = outer;
    }

    public void SetContainer()
    {
        current = container;
    }

    public void SetInner()
    {
        current = inner;
    }

    public void Reset()
    {
        outer.Reset();
        container.Reset();
        inner.Reset();
        current = outer;
    }

    public void SetLayerManager(ref LayerManager layerManager)
    {
        outer.SetLayerManager(ref layerManager);
        inner.SetLayerManager(ref layerManager);
        container.SetLayerManager(ref layerManager);
    }

}
