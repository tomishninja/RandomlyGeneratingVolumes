using UnityEngine;

public class GenerateRandomSDFsForInterface : MonoBehaviour
{
    [SerializeField] InterfaceAdapterForMRTKInterface interfaceAdapter;

    [SerializeField] LogicControllerForGeneratingRandomSDFs system;

    [SerializeField] string pathToJSONFile;

    [SerializeField] bool updateFromFileOnStartup = false;

    bool shouldResetSystem = false;

    // Start is called before the first frame update
    void Start()
    {
        // Overwrite the current logic data if requested
        if (updateFromFileOnStartup)
            UpdateParametersBasedOnInputFiles();

        system.Init();
    }

    // Update is called once per frame
    void Update()
    {
        if (shouldResetSystem)
        {
            system.ResetSystem();
            shouldResetSystem = false;
        }

        system.RunProcess();
    }

    public void UpdateSystem()
    {
        shouldResetSystem = true;
    }

    public bool UpdateParametersBasedOnInputFiles()
    {
        JSONFileReaderWriter<LogicControllerForGeneratingRandomSDFs> fileReader = new JSONFileReaderWriter<LogicControllerForGeneratingRandomSDFs>(pathToJSONFile);
        LogicControllerForGeneratingRandomSDFs temp = fileReader.ReadFile();

        if (temp != null)
        {
            system = temp;
            return true;
        }

        return false;
    }

    public void writeSystemToJSONfile()
    {
        JSONFileReaderWriter<LogicControllerForGeneratingRandomSDFs> fileWriter = new JSONFileReaderWriter<LogicControllerForGeneratingRandomSDFs>(pathToJSONFile);

        fileWriter.WriteFile(this.system);
    }

    private void LinkToInterface()
    {
        if (interfaceAdapter == null)
        {

        }
    }
}
