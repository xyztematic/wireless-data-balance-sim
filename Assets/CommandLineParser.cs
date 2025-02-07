using UnityEngine;
using UnityEngine.UI;
using static NodeManager.NodeSetting;
using static NodeManager.LayoutMode;
using System.Diagnostics;
using System.Globalization;

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
                    // Small network static versus dynamic inventory (extreme n-1 version)
                    case 0: Parse("a 3 0 c 1 r 1 n 100 h 5 5 s 2 2 save p0_"+System.DateTime.Now.ToFileTimeUtc()); break;
                    case 1: Parse("a 3 1 c 1 r 1 n 100 h 5 5 s 2 2 save p1_"+System.DateTime.Now.ToFileTimeUtc()); break;
                    // No coding versus coding (max dim flooding, hex net, n = 1000)
                    case 100: Parse("a 0 c 0 r 1 n 1000 h 10 10 s 0 0 save p100_"+System.DateTime.Now.ToFileTimeUtc()); break;
                    case 101: Parse("a 0 c 1 r 1 n 1000 h 10 10 s 0 0 save p101_"+System.DateTime.Now.ToFileTimeUtc()); break;
                    // No coding versus coding (max dim flooding, square net, n = 1000)
                    case 102: Parse("a 0 c 0 r 1 n 1000 g 10 10 s 0 0 save p102_"+System.DateTime.Now.ToFileTimeUtc()); break;
                    case 103: Parse("a 0 c 1 r 1 n 1000 g 10 10 s 0 0 save p103_"+System.DateTime.Now.ToFileTimeUtc()); break;
                    // No coding versus coding (max dim flooding, hex net, n = 100)
                    case 104: Parse("a 0 c 0 r 1 n 100 h 10 10 s 0 0 save p104_"+System.DateTime.Now.ToFileTimeUtc()); break;
                    case 105: Parse("a 0 c 1 r 1 n 100 h 10 10 s 0 0 save p105_"+System.DateTime.Now.ToFileTimeUtc()); break;
                    // No coding versus coding (max dim flooding, square net, n = 100)
                    case 106: Parse("a 0 c 0 r 1 n 100 g 10 10 s 0 0 save p106_"+System.DateTime.Now.ToFileTimeUtc()); break;
                    case 107: Parse("a 0 c 1 r 1 n 100 g 10 10 s 0 0 save p107_"+System.DateTime.Now.ToFileTimeUtc()); break;
                    // Max dim flooding speed with different source node configs (LR no coding)
                    case 200: Parse("a 0 c 0 r 1 n 100 g 11 11 s 0 0 save p200_"+System.DateTime.Now.ToFileTimeUtc()); break;
                    case 201: Parse("a 0 c 0 r 1 n 100 g 11 11 s 0 0 s 10 10 save p201_"+System.DateTime.Now.ToFileTimeUtc()); break;
                    case 202: Parse("a 0 c 0 r 1 n 100 g 11 11 s 0 0 s 10 10 s 0 10 s 10 0 save p202_"+System.DateTime.Now.ToFileTimeUtc()); break;
                    case 203: Parse("a 0 c 0 r 1 n 100 g 11 11 s 5 5 save p203_"+System.DateTime.Now.ToFileTimeUtc()); break;
                    // Max dim flooding speed with different source node configs (RLNC)
                    case 210: Parse("a 0 c 1 r 1 n 100 g 11 11 s 0 0 save p210_"+System.DateTime.Now.ToFileTimeUtc()); break;
                    case 211: Parse("a 0 c 1 r 1 n 100 g 11 11 s 0 0 s 10 10 save p211_"+System.DateTime.Now.ToFileTimeUtc()); break;
                    case 212: Parse("a 0 c 1 r 1 n 100 g 11 11 s 0 0 s 10 10 s 0 10 s 10 0 save p212_"+System.DateTime.Now.ToFileTimeUtc()); break;
                    case 213: Parse("a 0 c 1 r 1 n 100 g 11 11 s 5 5 save p213_"+System.DateTime.Now.ToFileTimeUtc()); break;
                    // Max dim flooding speed with different source node configs (Rnd no coding)
                    case 220: Parse("a 0 c 2 r 1 n 100 g 11 11 s 0 0 save p220_"+System.DateTime.Now.ToFileTimeUtc()); break;
                    case 221: Parse("a 0 c 2 r 1 n 100 g 11 11 s 0 0 s 10 10 save p221_"+System.DateTime.Now.ToFileTimeUtc()); break;
                    case 222: Parse("a 0 c 2 r 1 n 100 g 11 11 s 0 0 s 10 10 s 0 10 s 10 0 save p222_"+System.DateTime.Now.ToFileTimeUtc()); break;
                    case 223: Parse("a 0 c 2 r 1 n 100 g 11 11 s 5 5 save p223_"+System.DateTime.Now.ToFileTimeUtc()); break;
                    // No coding versus coding (2*dim div neighbor limit) small range
                    case 300: Parse("a 2 c 0 r 1 n 100 h 10 10 s 0 0 save p300_"+System.DateTime.Now.ToFileTimeUtc()); break;
                    case 301: Parse("a 2 c 1 r 1 n 100 h 10 10 s 0 0 save p301_"+System.DateTime.Now.ToFileTimeUtc()); break;
                    case 302: Parse("a 2 c 0 r 1 n 100 g 10 10 s 0 0 save p302_"+System.DateTime.Now.ToFileTimeUtc()); break;
                    case 303: Parse("a 2 c 1 r 1 n 100 g 10 10 s 0 0 save p303_"+System.DateTime.Now.ToFileTimeUtc()); break;
                    // No coding versus coding (2*dim div neighbor limit) mid range
                    case 310: Parse("a 2 c 0 r 1.5 n 100 h 10 10 s 0 0 save p310_"+System.DateTime.Now.ToFileTimeUtc()); break;
                    case 311: Parse("a 2 c 1 r 1.5 n 100 h 10 10 s 0 0 save p311_"+System.DateTime.Now.ToFileTimeUtc()); break;
                    case 312: Parse("a 2 c 0 r 1.5 n 100 g 10 10 s 0 0 save p312_"+System.DateTime.Now.ToFileTimeUtc()); break;
                    case 313: Parse("a 2 c 1 r 1.5 n 100 g 10 10 s 0 0 save p313_"+System.DateTime.Now.ToFileTimeUtc()); break;
                    // RLNC maximum coverage (2*dim div neighbor limit) in a square net for different ranges
                    case 400: Parse("a 2 c 1 r 1 n 100 g 10 10 s 0 0 save p400_"+System.DateTime.Now.ToFileTimeUtc()); break;
                    case 401: Parse("a 2 c 1 r 1.1 n 100 g 10 10 s 0 0 save p401_"+System.DateTime.Now.ToFileTimeUtc()); break;
                    case 402: Parse("a 2 c 1 r 1.2 n 100 g 10 10 s 0 0 save p402_"+System.DateTime.Now.ToFileTimeUtc()); break;
                    case 403: Parse("a 2 c 1 r 1.3 n 100 g 10 10 s 0 0 save p403_"+System.DateTime.Now.ToFileTimeUtc()); break;
                    case 404: Parse("a 2 c 1 r 1.4 n 100 g 10 10 s 0 0 save p404_"+System.DateTime.Now.ToFileTimeUtc()); break;
                    case 405: Parse("a 2 c 1 r 1.5 n 100 g 10 10 s 0 0 save p405_"+System.DateTime.Now.ToFileTimeUtc()); break;
                    case 406: Parse("a 2 c 1 r 1.6 n 100 g 10 10 s 0 0 save p406_"+System.DateTime.Now.ToFileTimeUtc()); break;
                    case 407: Parse("a 2 c 1 r 1.7 n 100 g 10 10 s 0 0 save p407_"+System.DateTime.Now.ToFileTimeUtc()); break;
                    // RLNC maximum coverage (2*dim div neighbor limit) in a hexagonal net for different ranges
                    case 410: Parse("a 2 c 1 r 1 n 100 h 10 10 s 0 0 save p410_"+System.DateTime.Now.ToFileTimeUtc()); break;
                    case 411: Parse("a 2 c 1 r 1.1 n 100 h 10 10 s 0 0 save p411_"+System.DateTime.Now.ToFileTimeUtc()); break;
                    case 412: Parse("a 2 c 1 r 1.2 n 100 h 10 10 s 0 0 save p412_"+System.DateTime.Now.ToFileTimeUtc()); break;
                    case 413: Parse("a 2 c 1 r 1.3 n 100 h 10 10 s 0 0 save p413_"+System.DateTime.Now.ToFileTimeUtc()); break;
                    case 414: Parse("a 2 c 1 r 1.4 n 100 h 10 10 s 0 0 save p414_"+System.DateTime.Now.ToFileTimeUtc()); break;
                    case 415: Parse("a 2 c 1 r 1.5 n 100 h 10 10 s 0 0 save p415_"+System.DateTime.Now.ToFileTimeUtc()); break;
                    case 416: Parse("a 2 c 1 r 1.6 n 100 h 10 10 s 0 0 save p416_"+System.DateTime.Now.ToFileTimeUtc()); break;
                    case 417: Parse("a 2 c 1 r 1.7 n 100 h 10 10 s 0 0 save p417_"+System.DateTime.Now.ToFileTimeUtc()); break;

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
                }
                else nodeManager.ChangeSetting(DYNAMIC_INVENTORY, 1);
            }
            else if (parsed[i] == "c" || parsed[i] == "coded") {
                nodeManager.ChangeSetting(CODED_VARIANT, int.Parse(parsed[i+1]));
                i+=2;
            }
            else if (parsed[i] == "r" || parsed[i] == "range") {
                nodeManager.ChangeSetting(RANGE, Mathf.RoundToInt(float.Parse(parsed[i+1], CultureInfo.InvariantCulture)*1000));
                i+=2;
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
            else if (parsed[i] == "m" || parsed[i] == "random" || parsed[i] == "randomgrid") {
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
            else if (parsed[i] == "fast") {
                Time.timeScale = 100f;
                i++;
            }
            else if (parsed[i] == "slow") {
                Time.timeScale = 1f;
                i++;
            }
            else if (parsed[i] == "reset") {
                Time.timeScale = 1f;
                nodeManager.ChangeSetting(GRID_X, 0);
                nodeManager.ChangeSetting(GRID_Y, 0);
                nodeManager.RebuildNodes();
                i++;
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
