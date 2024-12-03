using System.Collections.Generic;
using UnityEngine;
using m4ri;

public class CS_Dispatcher : MonoBehaviour
{
    public ComputeShader cs;
    public RenderTexture rt;
    private Dictionary<List<GameObject>, int> rankLookup = new();
    private ComputeBuffer rankDataBuffer;
    private int[] ranks;

    public void Dispatch(uint dimension, float nodeRange, float floorSizeX, float floorSizeZ, int pixelX, int pixelZ, GameObject floor, NodeManager nodeManager)
    {
        int kernel = cs.FindKernel("CSMain");
        if (ranks == null || ranks.Length != pixelX*pixelZ) ranks = new int[pixelX*pixelZ];
        float stepX = floorSizeX / pixelX, stepZ = floorSizeZ / pixelZ;
        
        // TODO: Outer loop as parallel?
        int iX = 0, iZ = 0;
        for (float currZ = 0f; currZ < floorSizeZ; currZ+=stepZ) {
            for (float currX = 0f; currX < floorSizeX; currX+=stepX) {
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
                // If the rank has already been calculated, grab it from the dictionary
                if (rankLookup.ContainsKey(firstFourNeighbors)) ranks[iX+iZ*pixelX] = rankLookup[firstFourNeighbors];

                // Otherwise calculate the combined rank and save it
                else if (firstFourNeighbors.Count == 0) ranks[iX+iZ*pixelX] = 0;
                else if (firstFourNeighbors.Count == 1) ranks[iX+iZ*pixelX] = firstFourNeighbors[0].GetComponent<Node>().rank;
                else {
                    MatrixGF2 combined = firstFourNeighbors[0].GetComponent<Node>().reducedMatrix;
                    for (int i = 1; i < firstFourNeighbors.Count; i++) {
                        combined = combined.StackOnto(firstFourNeighbors[i].GetComponent<Node>().reducedMatrix);
                    }
                    int combinedRank = combined.Ref();
                    ranks[iX+iZ*pixelX] = combinedRank;
                    rankLookup.Add(firstFourNeighbors, combinedRank);
                }
                
            
            }
        }
        if (rt.width != pixelX || rt.height != pixelZ) rt = new RenderTexture(pixelX, pixelZ, 0);
        rt.enableRandomWrite = true;
        floor.GetComponent<MeshRenderer>().material.mainTexture = rt;
        cs.SetTexture(kernel, "Result", rt);

        cs.SetInt("ResultWidth", pixelX);
        cs.SetInt("ResultHeight", pixelZ);
        cs.SetInt("MaxRank", (int) dimension);

        if (rankDataBuffer == null || rankDataBuffer.count != ranks.Length)
            rankDataBuffer = new ComputeBuffer(ranks.Length, sizeof(int));
        rankDataBuffer.SetData(ranks);
        cs.SetBuffer(kernel, "RankData", rankDataBuffer);
        cs.Dispatch(kernel, rt.width / 8, rt.height / 8, 1);
        
    }
    void OnDestroy() {
        if (rankDataBuffer != null) rankDataBuffer.Dispose();
    }
}
