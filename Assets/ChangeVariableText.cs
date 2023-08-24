using UnityEngine;
using UnityEngine.UI;

public class ChangeVariableText : MonoBehaviour
{
    [SerializeField] Text text;

    public void ChangeText(string text)
    {
        if (this.text != null)
            this.text.text = text;
    }
}
