using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NodeManager : MonoBehaviour
{
    public uint gridX, gridZ, gridDistance, dimension;
    public Vector2Int coverageTextureSize;
    public float nodeLoopTime, nodeRange, alpha;
    public GameObject floor, nodePrefab, nodeParent, rangeIndicator, statsTextField;
    public Dictionary<ulong, List<GameObject>> chunkLookup = new();
    public string saveFilename = "simdata_test";
    public bool saveSimData = false, didFileInit = false;

    private DistributionAlgorithm distrAlg = DistributionAlgorithm.MAX_DIM;
    private bool dynamicInventory = true;
    private int timestep = 0, codingMode = 1;
    private readonly ulong yBitOffset = 0x00000001_00000000ul;
    private const float SQRT3OVER2 = 0.866025404f, epsilon = 1e-3f;
    private LayoutMode currentLayout = LayoutMode.GRID_SQUARE;
    private List<GameObject> allNodes = new();
    private CoverageCalculator coverageCalculator;
    private Coroutine coverageUpdater;
    private bool[] nodeSourceMap;

    public enum DistributionAlgorithm {
        MAX_DIM,
        MAX_DIM_DIV_NEIGHBORS,
        MAX_2_DIM_DIV_NEIGHBORS,
        DIM_MINUS_ONE,
        MAX_2_DIM_DIV_NEIGHBORS_EDGE_INCLUDE
    }
    public enum NodeSetting {
        GRID_X,
        GRID_Y,
        DISTR_ALG,
        DYNAMIC_INVENTORY,
        DIMENSION,
        RANGE,
        CODED_VARIANT
    }
    public enum LayoutMode {
        GRID_SQUARE,
        GRID_HEX,
        TRUE_RANDOM
    }

    public ulong ChunkID(Vector3 worldPos) {
        ulong chunkID = 0;
        chunkID += (ulong) Mathf.Max(Mathf.FloorToInt(worldPos.x / nodeRange), 0);
        chunkID += ((ulong) Mathf.Max(Mathf.FloorToInt(worldPos.z / nodeRange), 0)) * yBitOffset;
        return chunkID;
    }
    public ulong ChunkID(float xWorldPos, float zWorldPos) {
        return (ulong) Mathf.FloorToInt(xWorldPos / nodeRange) + ((ulong) Mathf.FloorToInt(zWorldPos / nodeRange)) * yBitOffset;
    }
    public void RebuildNodes() {
        Random.InitState(42);
        chunkLookup.Clear();
        timestep = 0;
        nodeSourceMap = new bool[gridX*gridZ];
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
            allNodes[i].GetComponent<Node>().Activate(this, dimension, nodeLoopTime, nodeRange,
                tempChunkIDs[i], distrAlg, dynamicInventory, codingMode);
        }
        // Update the floor object to fit the size of the new network, including the range of the nodes
        nodeBoundsX[0] -= nodeRange;
        nodeBoundsZ[0] -= nodeRange;
        nodeBoundsX[1] += nodeRange;
        nodeBoundsZ[1] += nodeRange;
        floor.transform.localScale = new Vector3(nodeBoundsX[1] - nodeBoundsX[0], nodeBoundsZ[1] - nodeBoundsZ[0], 1f);
        floor.transform.position = new Vector3(floor.transform.localScale.x / 2f + nodeBoundsX[0], 0f, floor.transform.localScale.y / 2f + nodeBoundsX[0]);
        // Kick off the Thread to update coverage visibility
        if (coverageUpdater != null) StopCoroutine(coverageUpdater);
        if (allNodes.Count > 0) coverageUpdater = StartCoroutine(UpdateCoverage());
        Time.timeScale = 1f;
    }
    
    public void ChangeSetting(NodeSetting setting, int newValue) {
        switch (setting) {
            case NodeSetting.GRID_X:
                gridX = (uint)newValue;
                break;
            case NodeSetting.GRID_Y:
                gridZ = (uint)newValue;
                break;
            case NodeSetting.DISTR_ALG:
                distrAlg = (DistributionAlgorithm) newValue;
                break;
            case NodeSetting.DYNAMIC_INVENTORY:
                dynamicInventory = newValue != 0;
                break;
            case NodeSetting.DIMENSION:
                dimension = (uint)newValue;
                break;
            case NodeSetting.RANGE:
                nodeRange = newValue / 100f + epsilon;
                break;
            case NodeSetting.CODED_VARIANT:
                codingMode = newValue;
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

    public void SetSourceNodeRandomBasis(int x, int y) {
        int index = y * (int)gridX + x;
        allNodes[index].GetComponent<Node>().SetRandomBasisInventory();
        nodeSourceMap[index] = true;
    }
    public void SetSourceNodeStandardBasis(int x, int y) {
        int index = y * (int)gridX + x;
        allNodes[index].GetComponent<Node>().SetStandardBasisInventory();
        nodeSourceMap[index] = true;
    }

    private int[] GetNodeInvRanks() {
        int[] nodeInvRanks = new int[allNodes.Count];
        for (int i = 0; i < allNodes.Count; i++) {
            nodeInvRanks[i] = allNodes[i].GetComponent<Node>().rank;
        }
        return nodeInvRanks;
    }
    private int[] GetNodeInvLoads() {
        int[] nodeInvLoads = new int[allNodes.Count];
        for (int i = 0; i < allNodes.Count; i++) {
            nodeInvLoads[i] = allNodes[i].GetComponent<Node>().inventory.FirstZeroRow();
            if (nodeInvLoads[i] == -1) nodeInvLoads[i] = (int)dimension;
        }
        return nodeInvLoads;
    }
    private bool[] GetNodeSourceMap() {
        return nodeSourceMap;
    }
    public IEnumerator UpdateCoverage() {
        didFileInit = false;
        while (true) {
            print("Updating Coverage");
            CoverageCalculator.CoverageData cd = coverageCalculator.CalculateAndDisplayCoverage(
                dimension, nodeRange, 12, floor, alpha, this);
                
            if (saveSimData) {
                if (!didFileInit) {
                    SimulationMetricsIO.InitFileWrite(saveFilename);
                    didFileInit = true;
                }
                SimulationMetricsIO.WriteToFile(cd, GetNodeInvRanks(), GetNodeInvLoads(), GetNodeSourceMap(), dimension);
            }
            UpdateAndIncreaseTimestep();
            yield return new WaitForSeconds(nodeLoopTime);
        }
    }
    void UpdateAndIncreaseTimestep() {
        statsTextField.TryGetComponent<Text>(out Text textComponent);
        textComponent.text = "Timestep: "+(++timestep);
    }
    void Start() {
        // Hides the dummy node prefab
        nodePrefab.GetComponent<MeshRenderer>().enabled = false;
        // Get the CS_Dispatcher instance
        coverageCalculator = GetComponent<CoverageCalculator>();
    }
}
