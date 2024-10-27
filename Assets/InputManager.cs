using System;
using UnityEngine;
using UnityEngine.UI;

public class InputManager : MonoBehaviour
{
    public NodeManager.NodeSetting setting;

    private GameObject runtimeScripts;
    private InputField inputField;
    
    void Start() {
        runtimeScripts = GameObject.FindGameObjectWithTag("Runtime Scripts");
        inputField = GetComponent<InputField>();
    }

    public void TextFieldChanged() {
        if (runtimeScripts != null && inputField != null) {
            try
            {
                runtimeScripts.GetComponent<NodeManager>().ChangeSetting(setting, int.Parse(inputField.text));
                Debug.Log("Changed Setting "+ setting.ToString() + " to "+int.Parse(inputField.text));
            }
            catch (System.Exception e)
            {
                Debug.LogError(e.StackTrace);
                throw;
            }
        }
    }
    
}
