using UnityEngine;
using UnityEngine.UI;

public class ColorPickerManager : MonoBehaviour
{
    private Color _currentValue;
    private bool _hasChanged = false;
    [SerializeField] Button buttonToChangeColorOf;
    private ColorBlock theColor;

    public Color CurrentValue
    {
        get
        {
            _hasChanged = false;
            return _currentValue;
        }
        set
        {
            _currentValue = value;
            _hasChanged = true;
        }
    }

    public bool HasChanged { get => _hasChanged; }

    private void Start()
    {
        if (buttonToChangeColorOf == null)
        {
            buttonToChangeColorOf = this.GetComponent<Button>();
        }

        if (buttonToChangeColorOf != null)
            theColor = buttonToChangeColorOf.colors;
    }

    public void OnColorChange(Color color)
    {
        CurrentValue = color;

        if (buttonToChangeColorOf != null)
        {
            theColor.normalColor = color;
            buttonToChangeColorOf.colors = theColor;
        }
    }
}
