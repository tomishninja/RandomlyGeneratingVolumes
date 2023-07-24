using System.Collections.Generic;

public class RandomChildGeneratorFactory
{

    List<RandomChildrenAllocationSystem> ExistingSystems;

    public RandomChildGeneratorFactory()
    {
        ExistingSystems = new List<RandomChildrenAllocationSystem>();
    }

    public RandomChildrenAllocationSystem GetChildGenoratorFor(int min, int max, int parentsAvalible)
    {
        for (int index = 0; index < ExistingSystems.Count; index++)
        {
            if (ExistingSystems[index].MatchesParameters(min, max, parentsAvalible))
            {
                return ExistingSystems[index];
            }
        }

        // if no match was found then a new object needs to be created and saved into this system.
        RandomChildrenAllocationSystem newAllocationSystem = new RandomChildrenAllocationSystem(min, max, parentsAvalible);
        ExistingSystems.Add(newAllocationSystem);
        return newAllocationSystem;
    }
}
