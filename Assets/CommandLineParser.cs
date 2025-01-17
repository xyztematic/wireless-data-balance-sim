using UnityEngine;
using UnityEngine.UI;
using static NodeManager.NodeSetting;
using static NodeManager.LayoutMode;
using System.Diagnostics;

public class CommandLineParser : MonoBehaviour
{
    public GameObject cameraObj;
    private CameraController cameraController;
    private GameObject runtimeScripts;
    private NodeManager nodeManager;
    private InputField _inputField;

    void Parse(string command) {
        print("CMD: "+command);
        command = command.ToLower();
        if (command.Length < 1) return;
        string[] parsed = command.Split(' ');
        int i = 0;
        while (i < parsed.Length) {
            
            // Command presets (typing stuff is hard :/)
            if (parsed[i] == "p" || parsed[i] == "preset") {
                switch (int.Parse(""+parsed[i+1])) {
                    case 0: Parse("a 0 n 100 g 10 10 s 0 0 save sim_"+System.DateTime.Now.ToFileTimeUtc()); break;
                    case 1: Parse("a 0 n 100 g 10 10 s 0 0 s 9 9 save sim_"+System.DateTime.Now.ToFileTimeUtc()); break;
                    case 2: Parse("a 1 0 n 20 g 1 5 s 0 0 save sim_"+System.DateTime.Now.ToFileTimeUtc()); break;
                    case 3: Parse("a 1 1 n 20 g 1 5 s 0 0 save sim_"+System.DateTime.Now.ToFileTimeUtc()); break;
                    case 4: Parse("a 1 0 n 20 h 5 5 s 0 0 save sim_"+System.DateTime.Now.ToFileTimeUtc()); break;
                    case 5: Parse("a 1 1 n 20 h 5 5 s 0 0 save sim_"+System.DateTime.Now.ToFileTimeUtc()); break;
                    
                    default: break;
                }
                break;
            }
            else if (parsed[i] == "a" || parsed[i] == "alg" || parsed[i] == "algorithm") {
                nodeManager.ChangeSetting(DISTR_ALG, int.Parse(""+parsed[i+1]));
                i+=2;
                if (int.TryParse(parsed[i], out int next)) {
                    nodeManager.ChangeSetting(DYNAMIC_INVENTORY, next);
                    i++;
                    if (int.TryParse(parsed[i], out next)) {
                        nodeManager.ChangeSetting(REDUNDANCY_BONUS, next);
                        i++;
                    }
                    else nodeManager.ChangeSetting(REDUNDANCY_BONUS, 0);
                }
                else nodeManager.ChangeSetting(DYNAMIC_INVENTORY, 1);
            }
            else if (parsed[i] == "n" || parsed[i] == "dim" || parsed[i] == "dimension") {
                nodeManager.ChangeSetting(DIMENSION, int.Parse(parsed[i+1]));
                i+=2;
            }
            else if (parsed[i] == "g" || parsed[i] == "grid" || parsed[i] == "squaregrid") {
                nodeManager.ChangeSetting(GRID_X, int.Parse(parsed[i+1]));
                nodeManager.ChangeSetting(GRID_Y, int.Parse(parsed[i+2]));
                nodeManager.ChangeLayout(GRID_SQUARE);
                nodeManager.RebuildNodes();
                i+=3;
            }
            else if (parsed[i] == "h" || parsed[i] == "hex" || parsed[i] == "hexgrid") {
                nodeManager.ChangeSetting(GRID_X, int.Parse(parsed[i+1]));
                nodeManager.ChangeSetting(GRID_Y, int.Parse(parsed[i+2]));
                nodeManager.ChangeLayout(GRID_HEX);
                nodeManager.RebuildNodes();
                i+=3;
            }
            else if (parsed[i] == "r" || parsed[i] == "random" || parsed[i] == "randomgrid") {
                nodeManager.ChangeSetting(GRID_X, int.Parse(parsed[i+1]));
                nodeManager.ChangeSetting(GRID_Y, int.Parse(parsed[i+2]));
                nodeManager.ChangeLayout(TRUE_RANDOM);
                nodeManager.RebuildNodes();
                i+=3;
            }
            else if (parsed[i] == "s" || parsed[i] == "source") {
                nodeManager.SetSourceNodeRandomBasis(int.Parse(parsed[i+1]), int.Parse(parsed[i+2]));
                i+=3;
            }
            else if (parsed[i] == "std" || parsed[i] == "sourcestd") {
                nodeManager.SetSourceNodeStandardBasis(int.Parse(parsed[i+1]), int.Parse(parsed[i+2]));
                i+=3;
            }
            else if (parsed[i] == "i" || parsed[i] == "info") {
                nodeManager.HighlightNode(int.Parse(parsed[i+1]), int.Parse(parsed[i+2]));
                i+=3;
            }
            else if (parsed[i] == "in" || parsed[i] == "infon" || parsed[i] == "infoneighbor") {
                nodeManager.HighlightNode(int.Parse(parsed[i+1]), int.Parse(parsed[i+2]), true);
                i+=3;
            }
            else if (parsed[i] == "ic" || parsed[i] == "infoc" || parsed[i] == "infoclear") {
                nodeManager.UnhighlightAll();
                i+=1;
            }
            else if (parsed[i] == "saveto" || parsed[i] == "save") {
                nodeManager.saveFilename = parsed[i+1].Replace(".","").Replace("/","");
                nodeManager.saveSimData = true;
                nodeManager.didFileInit = false;
                i+=2;
            }
            else if (parsed[i] == "stop" || parsed[i] == "end") {
                nodeManager.saveSimData = false;
                Time.timeScale = 0f;
                ProcessStartInfo start = new ProcessStartInfo("/usr/bin/python3", Application.dataPath+"/Python/graphSimData.py "+Application.persistentDataPath+"/Recordings/"+nodeManager.saveFilename);
                start.UseShellExecute = false;
                Process process = Process.Start(start);
                break;
            }
            else {
                print("No command detected!");
                break;
            }
            
        }
    }

    void Start() {
        runtimeScripts = GameObject.FindGameObjectWithTag("Runtime Scripts");
        nodeManager = runtimeScripts.GetComponent<NodeManager>();
        if (nodeManager == null) {
            UnityEngine.Debug.LogError("NodeManager not found, Command Line Disabled!");
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
        if (cameraController == null) UnityEngine.Debug.LogError("CameraController not found!");
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
