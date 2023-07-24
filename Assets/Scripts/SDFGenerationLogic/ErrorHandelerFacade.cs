using System;
using UnityEngine;

namespace GenoratingRandomSDF
{
    [System.Serializable]
    public class ErrorHandelerFacade
    {
        [SerializeField] private int amountOfTimesAddingSDFsCanFailBeforeReset = 500;
        [SerializeField] private int amountOfTimesAddingSDFsCanResetBeforeHardReset = 20;
        [SerializeField] private int amountOfTimesAddingTryingToFitSDFDataBeforeHigherException = 100;

        [SerializeField] private int amountOfTimesVolumetriclyVerifingCanFailBeforeChangeRandomSeed = 100;
        [SerializeField] private int amountOfTimesVolumetriclyVerifingCanFailBeforeReseting = 500;
        [SerializeField] private int amountOfTimesVolumetriclyVerifingCanResetBeforeHardReset = 20;

        public int failsInARow { get; private set; }
        public int amountOfResets { get; private set; }

        SDFGenorator mainObject;
        RandomChildGeneratorFactory randomChildGeneratorFactory;

        public bool ShouldChangeRandomSeed
        {
            get => failsInARow % amountOfTimesVolumetriclyVerifingCanFailBeforeChangeRandomSeed == 0;
        }

        public void IncrementFailsInARow()
        {
            failsInARow++;
        }

        public void IncrementResets()
        {
            amountOfResets++;
        }

        public void Reset()
        {
            failsInARow = 0;
            amountOfResets = 0;
        }

        public bool ShouldReset
        {
            get => failsInARow >= amountOfTimesVolumetriclyVerifingCanFailBeforeReseting;
        }

        public void Init(SDFGenorator mainObject, ref RandomChildGeneratorFactory randomChildGeneratorFactory)
        {
            this.mainObject = mainObject;
            this.randomChildGeneratorFactory = randomChildGeneratorFactory;
        }

        public int AddSDFsFailed()
        {
            // Change the pixels and set the z index to zero so this can start again
            failsInARow++;

            if (failsInARow % amountOfTimesAddingSDFsCanFailBeforeReset == 0)
            {
                UnityEngine.Random.InitState((int)(Time.time * 7919));
                if (failsInARow > 5000)
                {

                    // go back to how everything used to be
                    mainObject.Reset();

                    if (amountOfResets > amountOfTimesAddingSDFsCanResetBeforeHardReset)
                    {
                        // Tell the user what is going on
                        Debug.LogError("Reseting The amount of from " + randomChildGeneratorFactory.GetChildGenoratorFor(mainObject.CurrentLevel.ContainableAmountOfChildren.min, mainObject.CurrentLevel.ContainableAmountOfChildren.max, mainObject.CurrentLevel.AmountOfContainers).GenerateARadomAmountOfChildren());


                        randomChildGeneratorFactory.GetChildGenoratorFor(mainObject.CurrentLevel.ContainableAmountOfChildren.min, mainObject.CurrentLevel.ContainableAmountOfChildren.max, mainObject.CurrentLevel.AmountOfContainers).DepersistsRandomValue();
                        amountOfResets = 0;
                    }
                    else
                    {
                        amountOfResets++;
                    }

                    throw new RanForTooLongException("Cant fill this region up with current items (500)");
                }
            }

            // change the current subroutine to 0;
            return 0;
        }

        public void AddSDFTriggeredAnUnknownError()
        {
            // go back to how everything used to be
            mainObject.Reset();

            if (amountOfResets > amountOfTimesAddingSDFsCanResetBeforeHardReset)
            {
                // Tell the user what is going on
                Debug.LogError("Reseting The amount of from " + randomChildGeneratorFactory.GetChildGenoratorFor(mainObject.CurrentLevel.ContainableAmountOfChildren.min, mainObject.CurrentLevel.ContainableAmountOfChildren.max, mainObject.CurrentLevel.AmountOfContainers).GenerateARadomAmountOfChildren());


                randomChildGeneratorFactory.GetChildGenoratorFor(mainObject.CurrentLevel.ContainableAmountOfChildren.min, mainObject.CurrentLevel.ContainableAmountOfChildren.max, mainObject.CurrentLevel.AmountOfContainers).DepersistsRandomValue();
                amountOfResets = 0;
            }
            else
            {
                amountOfResets++;
            }

            throw new Exception("Found Unexpected Error Disabling The System Will Restart When Reactivating");
        }



        internal int ItterationCheckFailedResetTasks(int amountOfResets)
        {
            // go back to how everything used to be
            mainObject.Reset();

            if (amountOfResets > amountOfTimesVolumetriclyVerifingCanResetBeforeHardReset)
            {
                // Tell the user what is going on
                Debug.LogError("Reseting The amount of from " + randomChildGeneratorFactory.GetChildGenoratorFor(mainObject.CurrentLevel.ContainableAmountOfChildren.min, mainObject.CurrentLevel.ContainableAmountOfChildren.max, mainObject.CurrentLevel.AmountOfContainers).GenerateARadomAmountOfChildren(true));


                randomChildGeneratorFactory.GetChildGenoratorFor(mainObject.CurrentLevel.ContainableAmountOfChildren.min, mainObject.CurrentLevel.ContainableAmountOfChildren.max, mainObject.CurrentLevel.AmountOfContainers).DepersistsRandomValue();
                
                // reset the amount of resets
                return 0;
            }
            else
            {
                // increment the amount of resets
                return 1;
            }
        }
    }

}
