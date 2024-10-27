using System;
using System.Collections.Generic;
using UnityEngine;

public class NodeManager : MonoBehaviour
{
    public uint gridX, gridY, gridDistance;
    public GameObject floor, nodePrefab, nodeParent;
    public List<GameObject> nodes = new();

    public enum NodeSetting {
        GRID_X,
        GRID_Y,
    }

    private void OnGridChange() {
        Transform parentTransform = nodeParent.transform;
        int diff = (int) (gridX*gridY) - nodes.Count;

        if (diff > 0) {
            // Add nodes since more are needed
            for (int i = 0; i < diff; i++) {
                GameObject toAdd = Instantiate(nodePrefab, parentTransform);
                toAdd.GetComponent<MeshRenderer>().enabled = true;
                nodes.Add(toAdd);
            }
        }
        else if (diff < 0) {
            // Remove nodes since less are needed
            for (int i = 0; i < -diff; i++) {
                GameObject toDestroy = nodes[0];
                nodes.RemoveAt(0);
                DestroyImmediate(toDestroy);
            }
        }
        // Reposition all nodes
        for (int i = 0; i < nodes.Count; i++) {
            nodes[i].transform.position = gridDistance * new Vector3(i % gridX, 0, i / gridX);
        }
    }
    
    public void ChangeSetting(NodeSetting setting, int newValue) {
        switch (setting) {
            case NodeSetting.GRID_X:
                gridX = (uint)newValue;
                OnGridChange();
                break;
            case NodeSetting.GRID_Y:
                gridY = (uint)newValue;
                OnGridChange();
                break;
            default:
                Debug.LogError("Tried to change unknown setting");
                break;
            
        }
    }

    void Start() {
        nodePrefab.GetComponent<MeshRenderer>().enabled = false;
    }
}
