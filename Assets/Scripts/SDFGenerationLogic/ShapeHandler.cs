using profiler;
using UnityEngine;


namespace GeneratingRandomSDF
{
    /// <summary>
    /// Flat array that holds all <see cref="HierarchicalObjects"/> across the entire three-layer
    /// hierarchy (outer, container, inner) for a single generation cycle.
    /// Provides a cursor (<see cref="CurrentShape"/>) that drives sequential adding and
    /// verification passes over each shape.
    /// </summary>
    [System.Serializable]
    public class ShapeHandler
    {
        private int currentShapeIndex = 0;
        [SerializeField] HierarchicalObjects[] Shapes;
        profiler.AbstractProfiler profiler;

        internal HierarchicalObjects CurrentShape { get => Shapes[currentShapeIndex]; }

        public int Length { get => Shapes.Length; }

        internal bool ShapesAreRemainingToVerify { get => currentShapeIndex < (this.Shapes.Length - 1); }

        internal int AmountOfShapesRemaining { get => (this.Shapes.Length - currentShapeIndex); }    

        /// <summary>
        /// Move the interal incremenor to the next shape
        /// </summary>
        /// <returns>
        /// true if it could incrment to a non null space and false if it couldn't
        /// </returns>
        internal bool IncrementCurrentShapeToNextShape()
        {
            if (this.Shapes.Length > (currentShapeIndex + 1) && this.Shapes[currentShapeIndex + 1] != null)
            {
                currentShapeIndex++;
                return true;
            }
            return false; 
        }

        internal void Set(int index, HierarchicalObjects shape)
        {
              if (index < 0 || index >= Shapes.Length) throw new System.ArgumentException("The index " + index + " is out of range");
              //if (shape == null) throw new System.NullReferenceException("The shape at " + index + " is null");

            Shapes[index] = shape;
        }

        internal void SetCurrent(HierarchicalObjects shape)
        {
            if (shape == null) throw new System.ArgumentException("The Provided Shape is null");

            Shapes[currentShapeIndex] = shape;
        }

        internal HierarchicalObjects Get(int index)
        {
            return this.Shapes[index];
        }

        public HierarchicalObjects[] GetShapesAsArray()
        {
            return Shapes;
        }

        public void Reset(int amountOfShapesRequired)
        {
            this.Shapes = new HierarchicalObjects[amountOfShapesRequired];
            this.currentShapeIndex = 0;
        }

        internal void Empty()
        {
            if (this.Shapes != null && currentShapeIndex >= 0 && currentShapeIndex < Shapes.Length && Shapes[currentShapeIndex].Children != null)
            {
                // Empty the shape array where needed
                for (int childrenIndex = 0; childrenIndex < this.Shapes[currentShapeIndex].Children.Length; childrenIndex++)
                {
                    for (int ShapesIndex = 0; ShapesIndex < this.Shapes.Length; ShapesIndex++)
                    {
                        if (this.Shapes[ShapesIndex] != null && this.Shapes[ShapesIndex].Equals(this.Shapes[currentShapeIndex].Children[childrenIndex]))
                        {
                            this.Shapes[ShapesIndex] = null;
                            break;
                        }
                    }
                }

                // Clear the parent array
                if (this.Shapes[currentShapeIndex].Children != null)
                    this.Shapes[currentShapeIndex].Children = new HierarchicalObjects[this.Shapes[currentShapeIndex].Children.Length];
                else
                    this.Shapes[currentShapeIndex].Children = new HierarchicalObjects[0];
            }
        }

        internal void MoveSDF(HierarchicalObjects parent, int index)
        {
            // Run though the various sets of illigal parameters that can be thrown and thrown expections accordingly
            if (parent == null) throw new System.ArgumentException("A parent Volume is require to move this sdf");
            if (this.Shapes[index] == null) throw new System.ArgumentException("The Shape at " + index + " has not yet been itiationaized");

            // Just make sure the parent also contiains this shape and where it is
            int parentIndex = 0;
            bool isInParent = false;
            // if we are removing a existing child then we need to find the matching one in the other array
            for (; parentIndex < parent.Children.Length; parentIndex++)
            {
                if (parent.Children[parentIndex].Equals(this.Shapes[index]))
                {
                    isInParent = true;
                    break;
                }
            }

            // if the parent object hasn't been found then 
            if (!isInParent)
            {
                throw new System.ArgumentException("Shape does not exist in parent object");
            }

            // loop though the 
            int amountOfFails = 0;
            bool failed = false;

            do
            {
                // don't keep looping though this unless it fails
                failed = false;

                try
                {
                    // create the shape we are trying to create
                    HierarchicalObjects shape = null;

                    // create the new child and take a guess of some maybe appropriate parameters
                    shape = parent.MoveChild(parentIndex);

                    this.Shapes[index] = shape;
                    parent.Children[parentIndex] = shape;
                }
                catch (RanForTooLongException)
                {
                    profiler.Increment(DataGenerationDataProfiler.COULD_NOT_FIT_ALL_OBJECTS_IN_TIME);

                    // the random generator is stuggling to fit the current item
                    // this likly means a earlier one is in its road so we need to start again
                    if (amountOfFails > 100)
                    {
                        amountOfFails = 0;

                        UnityEngine.Random.InitState((int)(Time.time * 7919));

                        //Random.InitState(System.Convert.ToInt32(System.DateTime.Now.Ticks));
                        throw new RanForTooLongException("Cant fill this region up with current items");
                    }

                    // repeat the loop
                    failed = true;
                    amountOfFails++;
                }
                catch (System.Exception ex)
                {
                    Debug.LogError("Unexpected Error \n" + ex.Message);

                    profiler.Increment(DataGenerationDataProfiler.UNEXPECTED_ERROR);

                    if (parent.Children != null)
                        parent.Children = new HierarchicalObjects[parent.Children.Length];
                    else
                        parent.Children = new HierarchicalObjects[0];

                    // This is normally caused by a unforseen error so it it happens I want to get out of this loop as fast as possible
                }
            } while (failed);
        }
    }
}

