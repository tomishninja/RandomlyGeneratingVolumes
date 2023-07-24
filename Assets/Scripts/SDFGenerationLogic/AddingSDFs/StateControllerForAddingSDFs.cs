namespace GenoratingRandomSDF
{
    public class StateControllerForAddingSDFs
    {
        private enum Level
        {
            outer = 0,
            inner = 1,
            container = 2,
        }

        AbstractAddSDFLogic outer = null;
        AbstractAddSDFLogic container = null;
        AbstractAddSDFLogic inner = null;

        AbstractAddSDFLogic current = null;

        public StateControllerForAddingSDFs(AbstractAddSDFLogic outer, AbstractAddSDFLogic inner, AbstractAddSDFLogic container)
        {
            this.outer = outer;
            this.inner = inner;
            this.container = container;

            // set current to the outer for the time being
            current = outer;
        }

        public void SetCurrentShapeDetails(SphericalVolumeHierarchyLevelDetails currentLevel)
        {
            outer.SetCurrentShapeDetails(currentLevel);
            inner.SetCurrentShapeDetails(currentLevel);
            container.SetCurrentShapeDetails(currentLevel);
            current.SetCurrentShapeDetails(currentLevel);
        }

        public AbstractAddSDFLogic Get()
        {
            return current;
        }

        public void SetToInner()
        {
            current = inner;
        }

        public void SetToContainer()
        {
            current = container;
        }

        public int GetTheAmountOfOuters()
        {
            return outer.AmountOfSDFsAdded;
        }

        public int GetTheAmountOfinners()
        {
            return inner.AmountOfSDFsAdded;
        }

        public int GetTheAmountOfContained()
        {
            return container.AmountOfSDFsAdded;
        }

        public void SetToOuter()
        {
            current = outer;
        }
    }
}
