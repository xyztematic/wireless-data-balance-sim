using System.Collections.Generic;
using UnityEngine;
using m4ri;

public class CoverageCalculator : MonoBehaviour
{
    public ComputeShader cs;
    public RenderTexture rt;
    public struct CoverageData {
        public int[] ranks;
        public int[] intersections;

        public CoverageData(int[] rankCoverage, int[] intersections)
        {
            this.ranks = rankCoverage;
            this.intersections = intersections;
        }
    }
    private Dictionary<List<GameObject>, int> rankLookup = new();
    private ComputeBuffer rankDataBuffer;
    private int[] rankCoverage, intersections;

    public CoverageData CalculateAndDisplayCoverage(uint dimension, float nodeRange, int maxNeighborsToLookAt, int pixelX, int pixelZ, GameObject floor, NodeManager nodeManager)
    {
        if (rankCoverage == null || rankCoverage.Length != pixelX*pixelZ) rankCoverage = new int[pixelX*pixelZ];
        if (intersections == null || intersections.Length != pixelX*pixelZ) intersections = new int[pixelX*pixelZ];
        rankLookup.Clear();
        float stepX = floor.transform.localScale.x / pixelX, stepZ = floor.transform.localScale.y / pixelZ;
        
        // TODO: Outer loop as parallel?
        int iZ = 0;
        for (float currZ = floor.transform.position.z - floor.transform.localScale.y/2f; iZ < pixelZ; currZ+=stepZ) {
            int iX = 0;
            for (float currX = floor.transform.position.x - floor.transform.localScale.x/2f; iX < pixelX; currX+=stepX) {
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
                if (rankLookup.ContainsKey(firstXNeighbors)) rankCoverage[iX+iZ*pixelX] = rankLookup[firstXNeighbors];

                // Otherwise calculate the combined rank and save it
                else if (firstXNeighbors.Count == 0) rankCoverage[iX+iZ*pixelX] = 0;
                else if (firstXNeighbors.Count == 1) rankCoverage[iX+iZ*pixelX] = firstXNeighbors[0].GetComponent<Node>().rank;
                else {
                    MatrixGF2 combined = firstXNeighbors[0].GetComponent<Node>().reducedMatrix;
                    for (int i = 1; i < firstXNeighbors.Count; i++) {
                        combined = combined.StackOnto(firstXNeighbors[i].GetComponent<Node>().reducedMatrix);
                    }
                    int combinedRank = combined.Ref();
                    rankCoverage[iX+iZ*pixelX] = combinedRank;
                    rankLookup.Add(firstXNeighbors, combinedRank);
                }
                // Save the amount of neighbors intersecting at the current position
                intersections[iX+iZ*pixelX] = firstXNeighbors.Count;

                iX++;
            }
            iZ++;
        }
        
        // Setup the buffer and inputs for the compute shader, that displays the coverage data

        int kernel = cs.FindKernel("CSMain");
        if (rt.width != pixelX || rt.height != pixelZ) rt = new RenderTexture(pixelX, pixelZ, 0);
        rt.enableRandomWrite = true;
        floor.GetComponent<MeshRenderer>().material.mainTexture = rt;
        cs.SetTexture(kernel, "Result", rt);

        cs.SetInt("ResultWidth", pixelX);
        cs.SetInt("ResultHeight", pixelZ);
        cs.SetInt("MaxRank", (int) dimension);

        if (rankDataBuffer == null || rankDataBuffer.count != rankCoverage.Length)
            rankDataBuffer = new ComputeBuffer(rankCoverage.Length, sizeof(int));
        rankDataBuffer.SetData(rankCoverage);
        cs.SetBuffer(kernel, "RankData", rankDataBuffer);
        cs.Dispatch(kernel, Mathf.CeilToInt(rt.width / 8f), Mathf.CeilToInt(rt.height / 8f), 1);
        return new CoverageData(rankCoverage, intersections);
    }
    
    void OnDestroy() {
        if (rankDataBuffer != null) rankDataBuffer.Dispose();
    }
    
}
