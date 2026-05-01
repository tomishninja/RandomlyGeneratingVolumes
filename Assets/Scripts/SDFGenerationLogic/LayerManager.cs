namespace GeneratingRandomSDF
{
    /// <summary>
    /// Synchronises the two state machines (<see cref="CheckingStateController"/> and
    /// <see cref="StateControllerForAddingSDFs"/>) so both always agree on which
    /// hierarchy layer (Outer → Container → Inner) is currently being processed.
    /// </summary>
    public class LayerManager
    {
        CheckingStateController stateController;
        StateControllerForAddingSDFs stateControllerForAddingSDFs;

        private enum Layer { Outer = 0, Container = 1, Inner = 2 }
        private Layer currentLayer = Layer.Outer;

        public LayerManager(ref CheckingStateController stateController, ref StateControllerForAddingSDFs stateControllerForAddingSDFs)
        {
            this.stateController = stateController;
            this.stateControllerForAddingSDFs = stateControllerForAddingSDFs;
        }

        public bool IsInner()     => this.currentLayer == Layer.Inner;
        public bool IsOuter()     => this.currentLayer == Layer.Outer;
        public bool IsContainer() => this.currentLayer == Layer.Container;

        public void SetToInner()
        {
            this.stateController.SetInner();
            this.stateControllerForAddingSDFs.SetToInner();
            this.currentLayer = Layer.Inner;
        }

        public void SetToContainer()
        {
            this.stateController.SetContainer();
            this.stateControllerForAddingSDFs.SetToContainer();
            this.currentLayer = Layer.Container;  // was incorrectly set to 2 (Inner) before
        }

        public void SetToOuter()
        {
            this.stateController.SetOuter();
            this.stateControllerForAddingSDFs.SetToOuter();
            this.currentLayer = Layer.Outer;
        }
    }
}

