using UnityEngine;

public class HIdeObject : MonoBehaviour
{
    [SerializeField] GameObject[] gameObjects;
    [SerializeField] bool toggle = false;

    public void Start()
    {
        ToggleObjects(toggle);
    }

    public void ToggleEverything()
    {
        toggle = !toggle;
        ToggleObjects(toggle);
    }

    private void ToggleObjects(bool isActive)
    {
        for (int index = 0; index < gameObjects.Length; index++)
        {
            gameObjects[index].SetActive(isActive);
        }
    }
}
