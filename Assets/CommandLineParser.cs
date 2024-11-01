using UnityEngine;
using UnityEngine.UI;
using static NodeManager.NodeSetting;

public class CommandLineParser : MonoBehaviour
{
    private GameObject runtimeScripts;
    private NodeManager nodeManager;
    private InputField _inputField;

    void Parse(string command) {
        Debug.Log("CMD: "+command);
        command = command.ToLower();
        if (command.Length < 1) return;
        string[] parsed = command.Split(' ');
        if (parsed[0] == "g" || parsed[0] == "grid") {
            nodeManager.ChangeSetting(GRID_X, int.Parse(parsed[1]));
            nodeManager.ChangeSetting(GRID_Y, int.Parse(parsed[2]));
        }
    }

    void Start() {
        runtimeScripts = GameObject.FindGameObjectWithTag("Runtime Scripts");
        nodeManager = runtimeScripts.GetComponent<NodeManager>();
        if (nodeManager == null) {
            Debug.LogError("NodeManager not found, Command Line Disabled!");
            return;
        }

        _inputField = GetComponent<InputField>();
        _inputField.onEndEdit.AddListener(fieldValue => {
            if (Input.GetKeyDown(KeyCode.Return))
                Parse(fieldValue);
                _inputField.text = "";
        });
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.Tab) && _inputField != null) {
            if (_inputField.isFocused) _inputField.DeactivateInputField();
            else {
                _inputField.ActivateInputField();
                _inputField.Select();
            }
        }
    }
}
