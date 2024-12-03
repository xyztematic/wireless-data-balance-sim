using System.Collections.Generic;
using UnityEngine;
using m4ri;

public class CS_Dispatcher : MonoBehaviour
{
    public ComputeShader cs;
    public RenderTexture rt;
    public Dictionary<List<GameObject>, int> rankLookup = new();

    private ComputeBuffer cb;
    private nodePosition[] nodePositions;
    struct nodePosition {
        float x, y;
        public nodePosition(float _x, float _y) {
            x = _x;
            y = _y;
        }
    }

    void Dispatch(int nodeCount, float nodeRange, float floorSizeX, float floorSizeZ, int pixelX, int pixelY, NodeManager nodeManager, List<GameObject> nodeGameObjects)
    {
        int kernel = cs.FindKernel("CSMain");
        nodePositions = new nodePosition[nodeCount];
        float stepX = floorSizeX / pixelX, stepZ = floorSizeZ / pixelY;
        
        // TODO: Outer loop as parallel?
        for (float currZ = 0f; currZ <= floorSizeZ; currZ+=stepZ) {
            for (float currX = 0f; currX <= floorSizeX; currX+=stepX) {
            
                Vector3 currWorldPos = new Vector3(currX, 0f, currZ);
                ulong currentChunk = nodeManager.ChunkID(currX, currZ);
                List<GameObject> neighborCandidates = nodeManager.GetNeighborCandidates(currentChunk);
                List<GameObject> firstFourNeighbors = new(4);
                int found = 0;
                foreach (GameObject neighborCandidate in neighborCandidates) {
                    if (Vector3.Distance(currWorldPos, neighborCandidate.transform.position) < nodeRange) {
                        firstFourNeighbors.Add(neighborCandidate);
                        if (found++ >= 4) break;
                    }
                }
                if (rankLookup.ContainsKey(firstFourNeighbors)) return;

                // Calculate the combined rank
                MatrixGF2 combined = 
                
            
            }
        }
        cs.SetTexture(kernel, "Result", rt);
        cs.SetInt("NodeCount", nodeCount);
        cs.SetFloat("NodeRange", nodeRange);
        cs.SetFloat("FloorSizeX", floorSizeX);
        cs.SetFloat("FloorSizeY", floorSizeZ);
        cs.Dispatch(kernel, rt.width / 8, rt.height / 8, 1);
        
    }
    void OnDestroy() {
        if (cb != null) cb.Release();
    }
}
