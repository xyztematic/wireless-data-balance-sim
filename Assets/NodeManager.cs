using System.Collections.Generic;
using UnityEngine;

public class NodeManager : MonoBehaviour
{
    public uint gridX, gridY, gridDistance;
    public float nodeLoopTime, nodeRange;
    public GameObject floor, nodePrefab, nodeParent, rangeIndicator;
    public Dictionary<ulong, List<GameObject>> chunkLookup = new();
    public Color highlightColor1 = Color.red, highlightColor2 = Color.magenta;

    private readonly ulong yBitOffset = 0x00000001_00000000ul;
    private List<GameObject> allNodes = new();
    private List<GameObject> highlightedNodes = new();

    public enum NodeSetting {
        GRID_X,
        GRID_Y
    }

    public ulong ChunkID(Vector3 worldPos) {
        ulong chunkID = 0;
        chunkID += (ulong) Mathf.FloorToInt(worldPos.x / nodeRange);
        chunkID += ((ulong) Mathf.FloorToInt(worldPos.z / nodeRange)) * yBitOffset;
        return chunkID;
    }
    private void OnGridChange() {
        chunkLookup.Clear();
        highlightedNodes.Clear();
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
        // Reposition all nodes, add them to the chunk dictionary and activate them after
        float y = nodePrefab.transform.position.y;
        List<ulong> tempChunkIDs = new(allNodes.Count);
        for (int i = 0; i < allNodes.Count; i++) {
            Vector3 newPosition = gridDistance * new Vector3(i % gridX, y / gridDistance, i / gridX);
            allNodes[i].transform.position = newPosition;

            ulong chunkID = ChunkID(newPosition);
            if (!chunkLookup.ContainsKey(chunkID)) {
                chunkLookup.Add(chunkID, new());
            }
            chunkLookup[chunkID].Add(allNodes[i]);
            tempChunkIDs.Add(chunkID);
        }
        for (int i = 0; i < allNodes.Count; i++) {

            allNodes[i].GetComponent<Node>().Activate(nodeLoopTime, nodeRange, tempChunkIDs[i], this);
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
            neighborCandidates.AddRange(chunkLookup[neighborChunkID]);
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
    public void HighlightNode(int x, int y, bool alsoHighlightNeighbors = false) {
        UnhighlightAll();
        int index = y * (int)gridX + x;
        if (allNodes.Count < index) return;
        if (alsoHighlightNeighbors) {
            highlightedNodes.AddRange(allNodes[index].GetComponent<Node>().HighlightNeighbors(highlightColor2));
        }
        allNodes[index].GetComponent<MeshRenderer>().material.color = highlightColor1;
        highlightedNodes.Add(allNodes[index]);

        // add the range circle visualizer
        rangeIndicator.transform.position = new Vector3(allNodes[index].transform.position.x, 0, allNodes[index].transform.position.z);
        float scaleToSet = 2 * allNodes[index].GetComponent<Node>().GetNodeRange();
        rangeIndicator.transform.localScale = scaleToSet * Vector3.one;
    }
    public void UnhighlightAll() {
        foreach (GameObject go in highlightedNodes) {
            go.GetComponent<MeshRenderer>().material.color = Color.white;
        }
        highlightedNodes.Clear();
        rangeIndicator.transform.localScale = Vector3.zero;
    }
    void Start() {
        nodePrefab.GetComponent<MeshRenderer>().enabled = false;
    }
}
