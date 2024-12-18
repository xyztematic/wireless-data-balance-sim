using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeManager : MonoBehaviour
{
    public uint gridX, gridZ, gridDistance, dimension;
    public Vector2Int coverageTextureSize;
    public float nodeLoopTime, nodeRange;
    public GameObject floor, nodePrefab, nodeParent, rangeIndicator;
    public Dictionary<ulong, List<GameObject>> chunkLookup = new();
    public Color highlightColor1 = Color.red, highlightColor2 = Color.magenta;

    private readonly ulong yBitOffset = 0x00000001_00000000ul;
    private const float SQRT3OVER2 = 0.866025404f;
    private LayoutMode currentLayout = LayoutMode.GRID_SQUARE;
    private List<GameObject> allNodes = new();
    private List<GameObject> highlightedNodes = new();
    private CoverageCalculator coverageCalculator;
    private Coroutine coverageUpdater;

    public enum NodeSetting {
        GRID_X,
        GRID_Y
    }
    public enum LayoutMode {
        GRID_SQUARE,
        GRID_HEX,
        TRUE_RANDOM
    }

    public ulong ChunkID(Vector3 worldPos) {
        ulong chunkID = 0;
        chunkID += (ulong) Mathf.FloorToInt(worldPos.x / nodeRange);
        chunkID += ((ulong) Mathf.FloorToInt(worldPos.z / nodeRange)) * yBitOffset;
        return chunkID;
    }
    public ulong ChunkID(float xWorldPos, float zWorldPos) {
        return (ulong) Mathf.FloorToInt(xWorldPos / nodeRange) + ((ulong) Mathf.FloorToInt(zWorldPos / nodeRange)) * yBitOffset;
    }
    public void RebuildNodes() {
        UnhighlightAll();
        chunkLookup.Clear();
        Transform parentTransform = nodeParent.transform;
        int diff = (int) (gridX*gridZ) - allNodes.Count;

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
        Vector2 nodeBoundsX = new Vector2(gridX * gridDistance, 0);
        Vector2 nodeBoundsZ = new Vector2(gridZ * gridDistance, 0);
        List<ulong> tempChunkIDs = new(allNodes.Count);
        for (int i = 0; i < allNodes.Count; i++) {
            Vector3 newPosition;
            switch (currentLayout) {
                case LayoutMode.GRID_SQUARE:
                    newPosition = gridDistance * new Vector3(i % gridX, y / gridDistance, i / gridX);
                    break;
                case LayoutMode.GRID_HEX:
                    newPosition = gridDistance * new Vector3((i % gridX)+((i/gridX)%2)/2f, y / gridDistance, i / gridX * SQRT3OVER2);
                    break;
                case LayoutMode.TRUE_RANDOM:
                    newPosition = gridDistance * new Vector3(gridX*UnityEngine.Random.value, y / gridDistance, gridZ*UnityEngine.Random.value);
                    break;
                default:
                    newPosition = Vector3.zero;
                    break;
            }
            allNodes[i].transform.position = newPosition;

            if (newPosition.x < nodeBoundsX[0]) nodeBoundsX[0] = newPosition.x;
            else if (newPosition.x > nodeBoundsX[1]) nodeBoundsX[1] = newPosition.x;
            if (newPosition.z < nodeBoundsZ[0]) nodeBoundsZ[0] = newPosition.z;
            else if (newPosition.z > nodeBoundsZ[1]) nodeBoundsZ[1] = newPosition.z;


            ulong chunkID = ChunkID(newPosition);
            if (!chunkLookup.ContainsKey(chunkID)) {
                chunkLookup.Add(chunkID, new());
            }
            chunkLookup[chunkID].Add(allNodes[i]);
            tempChunkIDs.Add(chunkID);
        }
        for (int i = 0; i < allNodes.Count; i++) {
            allNodes[i].GetComponent<Node>().Activate(dimension, nodeLoopTime, nodeRange, tempChunkIDs[i], this, i == allNodes.Count/2);
        }
        // Update the floor object to fit the size of the new network
        // NOTE: localScale y and z is swapped, because the plane is rotated
        // TODO: wrong sizing
        floor.transform.localScale = new Vector3(nodeBoundsX[1] - nodeBoundsX[0], nodeBoundsZ[1] - nodeBoundsZ[0], 1f);
        floor.transform.localScale *= 1.01f;
        floor.transform.position = new Vector3(floor.transform.localScale.x / 2f + nodeBoundsX[0], 0f, floor.transform.localScale.y / 2f + nodeBoundsX[0]);
        // Kick off the Thread to update coverage visibility
        if (coverageUpdater != null) StopCoroutine(coverageUpdater);
        coverageUpdater = StartCoroutine(UpdateCoverage());
    }
    
    public void ChangeSetting(NodeSetting setting, int newValue) {
        switch (setting) {
            case NodeSetting.GRID_X:
                gridX = (uint)newValue;
                break;
            case NodeSetting.GRID_Y:
                gridZ = (uint)newValue;
                break;
            
            default:
                Debug.LogError("Tried to change unknown setting");
                break;
            
        }
    }
    public void ChangeLayout(LayoutMode layout) {
        currentLayout = layout;
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

    public void SetSourceNodeRandomBasis(int x, int y) {
        int index = y * (int)gridX + x;
        allNodes[index].GetComponent<Node>().SetRandomBasisInventory();
    }
    public void SetSourceNodeStandardBasis(int x, int y) {
        int index = y * (int)gridX + x;
        allNodes[index].GetComponent<Node>().SetStandardBasisInventory();
    }

    public IEnumerator UpdateCoverage() {
        int timeStep = 0;
        while (true) {
            print("Updating Coverage");
            int[] coverageData = coverageCalculator.CalculateCoverage(dimension, nodeRange, 6, floor.transform.localScale.x, floor.transform.localScale.y,
                coverageTextureSize.x, coverageTextureSize.y, floor, this);
            
            SimulationMetrics.WriteToFile(SimulationMetrics.Metrics.ALL, timeStep, coverageData);
            
            yield return new WaitForSeconds(nodeLoopTime);
        }
    }
    
    void Start() {
        // Hides the dummy node prefab
        nodePrefab.GetComponent<MeshRenderer>().enabled = false;
        // Get the CS_Dispatcher instance
        coverageCalculator = GetComponent<CoverageCalculator>();
    }
}
