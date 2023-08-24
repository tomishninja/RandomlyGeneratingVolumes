using UnityEngine;
using UnityEngine.UI;

public enum DataType
{
    String = 0,
    Integer,
    Float
}

public class DataConverter : MonoBehaviour
{
    [SerializeField] InputField inputField;
    [SerializeField] DataType dataType;
    public string PreviousInput { get; set; }
    DataType previousDataType;

    private void Start()
    {
        inputField.onEndEdit.AddListener(OnDataTypeChanged);
        this.previousDataType = this.dataType;
    }

    private void Update()
    {
        if (dataType != previousDataType)
        {
            HandleChange();
            dataType = previousDataType;
        }
    }

    private void OnDataTypeChanged(string input)
    {
        switch (dataType)
        {
            case DataType.String:
                PreviousInput = FormatAsString(input);
                break;
            case DataType.Integer:
                PreviousInput = FormatAsInteger(input);
                break;
            case DataType.Float:
                PreviousInput = RemoveTrailingZeros(FormatAsFloat(input));
                break;
        }

        inputField.text = PreviousInput;
    }

    private string FormatAsString(string input)
    {
        return input;
    }

    private string FormatAsInteger(string input)
    {
        if (int.TryParse(input, out int intValue))
        {
            return intValue.ToString();
        }
        else
        {
            return PreviousInput;
        }
    }

    private string FormatAsFloat(string input)
    {
        if (float.TryParse(input, out float floatValue))
        {
            return floatValue.ToString("F6"); // Format as floating-point with 2 decimal places
        }
        else
        {
            return PreviousInput;
        }
    }

    public string RemoveTrailingZeros(string input)
    {
        if (input.Contains("."))
        {
            while (input.EndsWith("0") && input.Contains(".") && !input.EndsWith("."))
            {
                input = input.Substring(0, input.Length - 1);
            }
        }

        Debug.Log(input);

        return input;
    }

    public void HandleChange()
    {
        string input = inputField.text;

        switch (dataType)
        {
            case DataType.String:
                input = FormatAsString(input);
                break;
            case DataType.Integer:
                input = FormatAsInteger(input);
                break;
            case DataType.Float:
                input = RemoveTrailingZeros(FormatAsFloat(input));
                break;
        }
        
        if (input == PreviousInput)
        {
            inputField.text = string.Empty;
        }
        else
        {
            inputField.text = input;
        }
    }
}
