using System;
using UnityEngine;

namespace GeneratingRandomSDF
{
    /// <summary>
    /// Describes what action <see cref="ErrorHandlerFacade.IterationCheckFailedResetTasks"/>
    /// took when called.
    /// </summary>
    public enum ResetInstruction
    {
        /// <summary>The reset counter was cleared (a hard reset was performed).</summary>
        CounterWasReset     = 0,
        /// <summary>The reset counter was incremented (a soft reset was performed).</summary>
        CounterWasIncremented = 1
    }

    /// <summary>
    /// Centralises error-handling thresholds and counters for the volume generation pipeline.
    /// Tracks consecutive failures and decides when to reseed Unity's random number generator
    /// or perform a full reset of the current volume.
    /// </summary>
    [System.Serializable]
    public class ErrorHandlerFacade
    {
        [SerializeField] private int amountOfTimesAddingSDFsCanFailBeforeReset = 500;
        [SerializeField] private int amountOfTimesAddingSDFsCanResetBeforeHardReset = 20;
        [SerializeField] private int amountOfTimesAddingTryingToFitSDFDataBeforeHigherException = 100;

        [SerializeField] private int amountOfTimesVolumetriclyVerifingCanFailBeforeChangeRandomSeed = 100;
        [SerializeField] private int amountOfTimesVolumetriclyVerifingCanFailBeforeReseting = 500;
        [SerializeField] private int amountOfTimesVolumetriclyVerifingCanResetBeforeHardReset = 20;

        /// <summary>Number of consecutive verification failures since the last successful check.</summary>
        public int consecutiveFailures { get; private set; }
        public int amountOfResets      { get; private set; }

        SDFGenerator mainObject;
        RandomChildGeneratorFactory randomChildGeneratorFactory;

        public bool ShouldChangeRandomSeed
        {
            get => consecutiveFailures % amountOfTimesVolumetriclyVerifingCanFailBeforeChangeRandomSeed == 0;
        }

        public void IncrementFailsInARow()
        {
            consecutiveFailures++;
        }

        public void IncrementResets()
        {
            amountOfResets++;
        }

        public void Reset()
        {
            consecutiveFailures = 0;
            amountOfResets = 0;
        }

        public bool ShouldReset
        {
            get => consecutiveFailures >= amountOfTimesVolumetriclyVerifingCanFailBeforeReseting;
        }

        public void Init(SDFGenerator mainObject, ref RandomChildGeneratorFactory randomChildGeneratorFactory)
        {
            this.mainObject = mainObject;
            this.randomChildGeneratorFactory = randomChildGeneratorFactory;
        }

        public int AddSDFsFailed()
        {
            consecutiveFailures++;

            if (consecutiveFailures % amountOfTimesAddingSDFsCanFailBeforeReset == 0)
            {
                UnityEngine.Random.InitState((int)(Time.time * 7919));
                if (consecutiveFailures > 5000)
                {
                    mainObject.Reset();

                    if (amountOfResets > amountOfTimesAddingSDFsCanResetBeforeHardReset)
                    {
                        Debug.LogError("Reseting The amount of from " + randomChildGeneratorFactory.GetChildGeneratorFor(mainObject.CurrentLevel.ContainableAmountOfChildren.min, mainObject.CurrentLevel.ContainableAmountOfChildren.max, mainObject.CurrentLevel.AmountOfContainers).GenerateARandomAmountOfChildren());

                        randomChildGeneratorFactory.GetChildGeneratorFor(mainObject.CurrentLevel.ContainableAmountOfChildren.min, mainObject.CurrentLevel.ContainableAmountOfChildren.max, mainObject.CurrentLevel.AmountOfContainers).DepersistRandomValue();
                        amountOfResets = 0;
                    }
                    else
                    {
                        amountOfResets++;
                    }

                    throw new RanForTooLongException("Cant fill this region up with current items (500)");
                }
            }

            return (int)GenerationPhase.AddingShapes;
        }

        public void AddSDFTriggeredAnUnknownError()
        {
            mainObject.Reset();

            if (amountOfResets > amountOfTimesAddingSDFsCanResetBeforeHardReset)
            {
                Debug.LogError("Reseting The amount of from " + randomChildGeneratorFactory.GetChildGeneratorFor(mainObject.CurrentLevel.ContainableAmountOfChildren.min, mainObject.CurrentLevel.ContainableAmountOfChildren.max, mainObject.CurrentLevel.AmountOfContainers).GenerateARandomAmountOfChildren());

                randomChildGeneratorFactory.GetChildGeneratorFor(mainObject.CurrentLevel.ContainableAmountOfChildren.min, mainObject.CurrentLevel.ContainableAmountOfChildren.max, mainObject.CurrentLevel.AmountOfContainers).DepersistRandomValue();
                amountOfResets = 0;
            }
            else
            {
                amountOfResets++;
            }

            throw new Exception("Found Unexpected Error Disabling The System Will Restart When Reactivating");
        }

        /// <summary>
        /// Resets the main generator and decides whether to perform a hard reset of the
        /// child-count random state.
        /// </summary>
        /// <param name="currentAmountOfResets">Current value of <see cref="amountOfResets"/>.</param>
        /// <returns>
        /// <see cref="ResetInstruction.CounterWasReset"/> if the reset counter was cleared
        /// (hard reset performed); <see cref="ResetInstruction.CounterWasIncremented"/> otherwise.
        /// </returns>
        internal ResetInstruction IterationCheckFailedResetTasks(int currentAmountOfResets)
        {
            mainObject.Reset();

            if (currentAmountOfResets > amountOfTimesVolumetriclyVerifingCanResetBeforeHardReset)
            {
                Debug.LogError("Reseting The amount of from " + randomChildGeneratorFactory.GetChildGeneratorFor(mainObject.CurrentLevel.ContainableAmountOfChildren.min, mainObject.CurrentLevel.ContainableAmountOfChildren.max, mainObject.CurrentLevel.AmountOfContainers).GenerateARandomAmountOfChildren(true));

                randomChildGeneratorFactory.GetChildGeneratorFor(mainObject.CurrentLevel.ContainableAmountOfChildren.min, mainObject.CurrentLevel.ContainableAmountOfChildren.max, mainObject.CurrentLevel.AmountOfContainers).DepersistRandomValue();

                return ResetInstruction.CounterWasReset;
            }
            else
            {
                return ResetInstruction.CounterWasIncremented;
            }
        }
    }
}
