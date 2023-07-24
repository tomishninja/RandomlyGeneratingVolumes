namespace GenoratingRandomSDF
{
    public class LayerManager
    {
        CheckingStateContoller stateController;
        StateControllerForAddingSDFs stateControllerForAddingSDFs;
        int currentLayer = 0;

        public LayerManager(ref CheckingStateContoller stateController, ref StateControllerForAddingSDFs stateControllerForAddingSDFs)
        {
            this.stateController = stateController;
            this.stateControllerForAddingSDFs = stateControllerForAddingSDFs;
            this.currentLayer = 0;
        }

        public bool IsInner()
        {
            return this.currentLayer == 2;
        }

        public bool IsOuter()
        {
            return this.currentLayer == 0;
        }

        public bool IsContainer()
        {
            return this.currentLayer == 1;
        }

        public void SetToInner()
        {
            this.stateController.SetInner();
            this.stateControllerForAddingSDFs.SetToInner();
            this.currentLayer = 2;
        }

        public void SetToContainer()
        {
            this.stateController.SetContainer();
            this.stateControllerForAddingSDFs.SetToContainer();
            this.currentLayer = 2;
        }

        public void SetToOuter()
        {
            this.stateController.SetOuter();
            this.stateControllerForAddingSDFs.SetToOuter();
            this.currentLayer = 0;
        }
    }
}

