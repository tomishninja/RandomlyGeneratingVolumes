namespace GeneratingRandomSDF
{
    public static class ShapeArrayFriendlyFunctions
    {
        public static void EmptyShapeArrayOfChildrenOfCurrent(HierarchicalObjects[] shapes, int currentShapeIndex)
        {
            if (shapes != null && currentShapeIndex >= 0 && currentShapeIndex < shapes.Length && shapes[currentShapeIndex].Children != null)
            {
                // Empty the shape array where needed
                for (int childrenIndex = 0; childrenIndex < shapes[currentShapeIndex].Children.Length; childrenIndex++)
                {
                    for (int ShapesIndex = 0; ShapesIndex < shapes.Length; ShapesIndex++)
                    {
                        if (shapes[ShapesIndex] != null && shapes[ShapesIndex].Equals(shapes[currentShapeIndex].Children[childrenIndex]))
                        {
                            shapes[ShapesIndex] = null;
                            break;
                        }
                    }
                }

                // Clear the parent array
                if (shapes[currentShapeIndex].Children != null)
                    shapes[currentShapeIndex].Children = new HierarchicalObjects[shapes[currentShapeIndex].Children.Length];
                else
                    shapes[currentShapeIndex].Children = new HierarchicalObjects[0];
            }
        }
    }
}

