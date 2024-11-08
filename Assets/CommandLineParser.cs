using UnityEngine;
using UnityEngine.UI;
using static NodeManager.NodeSetting;

public class CommandLineParser : MonoBehaviour
{
    public GameObject cameraObj;
    private CameraController cameraController;
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
        else if (parsed[0] == "i" || parsed[0] == "info") {
            nodeManager.HighlightNode(int.Parse(parsed[1]), int.Parse(parsed[2]));
        }
        else if (parsed[0] == "in" || parsed[0] == "infon" || parsed[0] == "infoneighbor") {
            nodeManager.HighlightNode(int.Parse(parsed[1]), int.Parse(parsed[2]), true);
        }
        else if (parsed[0] == "ic" || parsed[0] == "infoc" || parsed[0] == "infoclear") {
            nodeManager.UnhighlightAll();
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
                _inputField.interactable = false;
                cameraController.disableMovement = false;
        });
        _inputField.interactable = false;

        cameraController = cameraObj.GetComponent<CameraController>();
        if (cameraController == null) Debug.LogError("CameraController not found!");
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.Tab) && _inputField != null) {
            if (_inputField.isFocused) {
                _inputField.DeactivateInputField();
                _inputField.interactable = false;
                cameraController.disableMovement = false;
            } 
            else {
                _inputField.interactable = true;
                _inputField.ActivateInputField();
                cameraController.disableMovement = true;
            }
        }
    }
}
