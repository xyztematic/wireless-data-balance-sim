using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using m4ri;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using static NodeManager.DistributionAlgorithm;

public class Node : MonoBehaviour
{
    private NodeManager nodeManager;
    private List<GameObject> neighbors;
    private int neighborCount;
    // Inventory of this node, represented as a bit-matrix of size "dimension"
    public MatrixGF2 inventory;
    private int firstZeroRow;
    public MatrixGF2 reducedMatrix;
    public int rank;
    // Stores incoming data sent from neighboring nodes in a thread-safe queue for later processing
    public ConcurrentQueue<int[]> recieveBuffer;
    private MeshRenderer meshRenderer;
    private int dimension, logDimension, redundancyBonus;
    private int[] toBroadcast;
    private float loopTime, nodeRange;
    private bool dynamicInventory;
    private NodeManager.DistributionAlgorithm distrAlg;

    public bool active = false;

    public void Activate(NodeManager nodeManager, uint dimension, float loopTime, float nodeRange, ulong chunkID,
        NodeManager.DistributionAlgorithm distrAlg, bool dynamicInventory, int redundancyBonus) {
        this.nodeManager = nodeManager;
        this.dimension = (int) dimension;
        this.logDimension = Mathf.FloorToInt(Mathf.Log(this.dimension, 2f)+1);
        this.firstZeroRow = 0;
        this.rank = 0;
        toBroadcast = new int[this.dimension];
        this.loopTime = loopTime;
        this.nodeRange = nodeRange;
        this.distrAlg = distrAlg;
        this.dynamicInventory = dynamicInventory;
        this.redundancyBonus = redundancyBonus;
        this.meshRenderer = gameObject.GetComponent<MeshRenderer>();
        // Get neighbor candidates from NodeManager and remove all candidates out of range
        neighbors = nodeManager.GetNeighborCandidates(chunkID);
        neighbors.RemoveAll(go => Vector3.Distance(go.transform.position, this.transform.position) > nodeRange);
        neighbors.Remove(gameObject); // This node should not be a neighbor of itself
        this.neighborCount = neighbors.Count;
        // Initialize empty inventory and reduced matrix
        inventory = new MatrixGF2(this.dimension, this.dimension);
        reducedMatrix = new MatrixGF2(this.dimension, this.dimension);
        // Initialize the recieve buffer, where incoming data is queued
        recieveBuffer = new();
        // Start the node loop which handles sendingand receiving data
        StartCoroutine(NodeLoop());
        this.active = true;
    }
    
    private IEnumerator NodeLoop() {
        yield return new WaitForSeconds(Random.Range(0f, loopTime));
        print(distrAlg.ToString());
        while (true) {
            bool notFull = false;
            for (int i = 0; i < neighborCount; i++) {
                switch (distrAlg) {
                    case MAX_DIM: notFull = rank < dimension; break;
                    case MAX_DIM_DIV_NEIGHBORS: notFull = rank <= dimension / neighborCount + 1; break;
                    default: break;
                }
            
                if (notFull) {
                    StepRecieveBuffer();
                }
                else if (rank < dimension) {
                    if (dynamicInventory) StepRecieveBuffer(overwriteMode: true);
                }
            }
            HighlightNode(Color.HSVToRGB(0f, (float)rank/dimension, 1f));
            
            if (rank > 0) BroadcastLRLC();
            
            yield return new WaitForSeconds(loopTime);
        }
    }

    public void HighlightNode(Color highlightColor) {
        meshRenderer.material.color = highlightColor;
    }
    public List<GameObject> HighlightNeighbors(Color highlightColor) {
        foreach (GameObject neighbor in neighbors) {
            neighbor.GetComponent<MeshRenderer>().material.color = highlightColor;
        }
        return neighbors;
    }

    public float GetNodeRange() {
        return this.nodeRange;
    }

    // Methods for setting the inventory of the node. Use only for configuring the source node of a network
    public void SetRandomBasisInventory() {
        print("Setting random basis inventory...");
        MatrixGF2 rndMatrix = new MatrixGF2(this.dimension, this.dimension);
        rndMatrix.Randomize();
        MatrixGF2 copy = rndMatrix.Copy();
        int rank = copy.Ref();
        while (rank != this.dimension) {
            rndMatrix.Randomize();
            copy = rndMatrix.Copy();
            rank = copy.Ref();
        }
        this.inventory = rndMatrix;
        this.reducedMatrix = copy;
        this.firstZeroRow = dimension;
        this.rank = rank;
        HighlightNode(Color.HSVToRGB(0f, (float)rank/dimension, 1f));
        print("Finished setting random basis inventory!");
    }
    public void SetStandardBasisInventory() {
        print("Setting standard basis inventory...");
        this.inventory = MatrixGF2.Identity(this.dimension);
        this.reducedMatrix = MatrixGF2.Identity(this.dimension);
        this.firstZeroRow = this.dimension;
        this.rank = this.dimension;
        HighlightNode(Color.HSVToRGB(0f, (float)rank/dimension, 1f));
        print("Finished setting standard basis inventory!");
    }

    // Dequeues the front item of the recieve-buffer and integrates it into the nodes inventory
    private void StepRecieveBuffer(bool overwriteMode = false) {
        int[] topItem; int newRank;
        
        // Try fetching from the receive buffer. Return on wrong format.
        if (!this.recieveBuffer.TryDequeue(out topItem) || topItem.Length != this.dimension) return;

        // Standard mode (Node is not deemed full by the algorithm). Insert into first empty row.
        if (!overwriteMode) {
            this.reducedMatrix.WriteRow(reducedMatrix.FirstZeroRow(), topItem);
        
            // If insert would increase rank, insert, otherwise return
            newRank = reducedMatrix.Ref();
            if (this.rank == newRank) return;
            this.inventory.WriteRow(firstZeroRow, topItem);
        }
        // Overwrite mode (Node is deemed full), overwrites a random row. This prevents blocking of information relay.
        else {
            int rowToOverwrite = Random.Range(0, inventory.FirstZeroRow());
            this.inventory.WriteRow(rowToOverwrite, topItem);
            // We need to recompute the reduced matrix from scratch after an overwrite
            this.reducedMatrix = this.inventory.Copy();
            newRank = reducedMatrix.Ref();
        }

        this.firstZeroRow = this.inventory.FirstZeroRow();
        this.rank = newRank;
    }
    // Sends a random linear combination of at most log2(dimension)+1 rows from the inventory to all neighbors
    // Uses fast-forwarding: If not already chosen, adds the last received row into the random linear combination
    private void BroadcastLRLC() {
        if (this.firstZeroRow < this.logDimension) {
            Parallel.For(0, this.dimension, (x) => {
                int write = 0;
                for (int i = 0; i < this.firstZeroRow; i++) {
                    write ^= this.inventory[i,x];
                }
                toBroadcast[x] = write;
            });
            
        }
        else {
            List<int> toPickFromIndices = new(Enumerable.Range(0, this.firstZeroRow));
            List<int> pickedRowIndices = new();
            bool isLastRowIncluded = false;
            for (int choices = this.firstZeroRow; choices > this.firstZeroRow - this.logDimension; choices--) {
                int randomChoice = Random.Range(0, choices);
                pickedRowIndices.Add(toPickFromIndices[randomChoice]);
                if (toPickFromIndices[randomChoice] == this.firstZeroRow - 1) isLastRowIncluded = true;
                toPickFromIndices.RemoveAt(randomChoice);
            }
            if (!isLastRowIncluded) pickedRowIndices.Add(this.firstZeroRow - 1);

            Parallel.For(0, this.dimension, (x) => {
                int write = 0;
                foreach (int i in pickedRowIndices) {
                    write ^= this.inventory[i,x];
                }
                toBroadcast[x] = write;
            });

        }
        foreach (GameObject neighbor in this.neighbors) {
            neighbor.TryGetComponent<Node>(out Node n);
            if (n == null || n.recieveBuffer == null) continue;
            n.recieveBuffer.Enqueue(toBroadcast);
        }
    }
}
