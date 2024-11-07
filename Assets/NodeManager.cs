using System.Collections.Generic;
using UnityEngine;

public class NodeManager : MonoBehaviour
{
    public uint gridX, gridY, gridDistance;
    public float nodeLoopTime, nodeRange;
    public GameObject floor, nodePrefab, nodeParent;
    public List<GameObject> allNodes = new();
    public Dictionary<ulong, List<GameObject>> chunkLookup = new();

    public enum NodeSetting {
        GRID_X,
        GRID_Y
    }

    public ulong ChunkID(Vector3 worldPos) {
        ulong chunkID = 0;
        chunkID += (ulong) Mathf.FloorToInt(worldPos.x / nodeRange);
        chunkID += ((ulong) Mathf.FloorToInt(worldPos.y / nodeRange)) << sizeof(int);
        return chunkID;
    }
    private void OnGridChange() {
        chunkLookup = new();
        Transform parentTransform = nodeParent.transform;
        int diff = (int) (gridX*gridY) - allNodes.Count;

        if (diff > 0) {
            // Add nodes since more are needed
            for (int i = 0; i < diff; i++) {
                GameObject toAdd = Instantiate(nodePrefab, parentTransform);
                toAdd.GetComponent<MeshRenderer>().enabled = true;
                allNodes.Add(toAdd);
            }
        }
        else if (diff < 0) {
            // Remove nodes since less are needed
            for (int i = 0; i < -diff; i++) {
                GameObject toDestroy = allNodes[0];
                allNodes.RemoveAt(0);
                DestroyImmediate(toDestroy);
            }
        }
        // Reposition all nodes, add them to the chunk dictionary and activate them
        float y = nodePrefab.transform.position.y;
        for (int i = 0; i < allNodes.Count; i++) {
            Vector3 newPosition = gridDistance * new Vector3(i % gridX, y / gridDistance, i / gridX);
            allNodes[i].transform.position = newPosition;

            ulong chunkID = ChunkID(newPosition);
            if (!chunkLookup.ContainsKey(chunkID)) {
                chunkLookup.Add(chunkID, new());
            }
            chunkLookup[chunkID].Add(allNodes[i]);

            allNodes[i].GetComponent<Node>().Activate(nodeLoopTime, chunkID, this);
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

    public List<GameObject> GetNeighborCandidates(ulong chunkID) {
        List<GameObject> neighborCandidates = new();
        foreach (ulong neighborChunkID in GetNeighboringChunkIDs(chunkID)) {
            neighborCandidates.AddRange(chunkLookup[chunkID]);
        }
        return neighborCandidates;
    }

    public List<ulong> GetNeighboringChunkIDs(ulong chunkID) {
        List<ulong> neighboringChunkIDs = new List<ulong>(9)
        {
            chunkID - 0x00000001_00000001ul,
            chunkID - 0x00000001_00000000ul,
            chunkID - 0x00000001_00000000ul + 1ul,
            chunkID - 1ul,
            chunkID,
            chunkID + 1ul,
            chunkID + 0x00000001_00000000ul - 1ul,
            chunkID + 0x00000001_00000000ul,
            chunkID + 0x00000001_00000001ul
        };
        List<ulong> entriesToRemove = new();
        for (int i = 0; i < neighboringChunkIDs.Count; i++) {
            if (!chunkLookup.ContainsKey(neighboringChunkIDs[i])) {
                entriesToRemove.Add(neighboringChunkIDs[i]);
            }
        }
        neighboringChunkIDs.RemoveAll(x => entriesToRemove.Contains(x));
        return neighboringChunkIDs;
    }
    void Start() {
        nodePrefab.GetComponent<MeshRenderer>().enabled = false;
    }
}
