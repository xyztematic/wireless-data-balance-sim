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

    public void Dispatch(uint dimension, float nodeRange, int maxNeighborsToLookAt, float floorSizeX, float floorSizeZ, int pixelX, int pixelZ, GameObject floor, NodeManager nodeManager)
    {
        int kernel = cs.FindKernel("CSMain");
        if (ranks == null || ranks.Length != pixelX*pixelZ) ranks = new int[pixelX*pixelZ];
        rankLookup.Clear();
        float stepX = floorSizeX / pixelX, stepZ = floorSizeZ / pixelZ;
        
        // TODO: Outer loop as parallel?
        int iZ = 0;
        for (float currZ = 0f; iZ < pixelZ; currZ+=stepZ) {
            int iX = 0;
            for (float currX = 0f; iX < pixelX; currX+=stepX) {
                Vector2 currWorldPos = new Vector3(currX, currZ);
                ulong currentChunk = nodeManager.ChunkID(currX, currZ);
                List<GameObject> neighborCandidates = nodeManager.GetNeighborCandidates(currentChunk);
                List<GameObject> firstXNeighbors = new(maxNeighborsToLookAt);
                int found = 0;
                foreach (GameObject neighborCandidate in neighborCandidates) {
                    Vector2 neighborCandidateWorldPos = new Vector2(
                        neighborCandidate.transform.position.x,
                        neighborCandidate.transform.position.z);
                    if (Vector2.Distance(currWorldPos, neighborCandidateWorldPos) < nodeRange) {
                        firstXNeighbors.Add(neighborCandidate);
                        if (found++ >= maxNeighborsToLookAt) break;
                    }
                }
                // If the rank has already been calculated, grab it from the dictionary
                if (rankLookup.ContainsKey(firstXNeighbors)) ranks[iX+iZ*pixelX] = rankLookup[firstXNeighbors];

                // Otherwise calculate the combined rank and save it
                else if (firstXNeighbors.Count == 0) ranks[iX+iZ*pixelX] = 0;
                else if (firstXNeighbors.Count == 1) ranks[iX+iZ*pixelX] = firstXNeighbors[0].GetComponent<Node>().rank;
                else {
                    MatrixGF2 combined = firstXNeighbors[0].GetComponent<Node>().reducedMatrix;
                    for (int i = 1; i < firstXNeighbors.Count; i++) {
                        combined = combined.StackOnto(firstXNeighbors[i].GetComponent<Node>().reducedMatrix);
                    }
                    int combinedRank = combined.Ref();
                    ranks[iX+iZ*pixelX] = combinedRank;
                    rankLookup.Add(firstXNeighbors, combinedRank);
                }
                iX++;
            }
            iZ++;
        }
        //for (int i = 0; i < pixelX; i++) print(ranks[i]);
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
        cs.Dispatch(kernel, Mathf.CeilToInt(rt.width / 8f), Mathf.CeilToInt(rt.height / 8f), 1);
        
    }
    
    void OnDestroy() {
        if (rankDataBuffer != null) rankDataBuffer.Dispose();
    }
    
}
