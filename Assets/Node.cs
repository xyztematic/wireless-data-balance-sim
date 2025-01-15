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
    private bool isSource = false, dynamicInventory;
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
        while (true) {
            int compare = redundancyBonus;
            switch (distrAlg) {
                case MAX_DIM: compare += dimension; break;
                case MAX_DIM_DIV_NEIGHBORS: compare += dimension / neighborCount; break;
                default: compare += dimension; break;
            }
            if (compare > dimension) compare = dimension;
            if (rank <= compare) {
                for (int i = 0; i < neighborCount; i++) {
                    StepRecieveBuffer();
                    HighlightNode(Color.HSVToRGB(0f, (float)rank/dimension, 1f));
                }
            }
            else if (rank < dimension) {
                StepRecieveBuffer(overwriteMode: dynamicInventory);
                HighlightNode(Color.HSVToRGB(0f, (float)rank/dimension, 1f));
            }
            
            if (rank > 0 && rank <= compare || isSource) BroadcastLRLC();
            
            yield return new WaitForSeconds(loopTime);
        }
    }
    private IEnumerator NodeLoopTimed() {

        while (true) {
            float t1 = Time.realtimeSinceStartup;
            if (rank > 0) BroadcastLRLC();
            float t2 = Time.realtimeSinceStartup;

            for (int i = 0; i < neighborCount; i++) {
                StepRecieveBuffer();
                HighlightNode(Color.HSVToRGB(0f, (float)rank/dimension, 1f));
            }
            float t3 = Time.realtimeSinceStartup;
            //print(Mathf.Round((t2 - t1)*1000000) + " " + Mathf.Round((t3 - t2)*1000000));
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
        this.isSource = true;
        HighlightNode(Color.HSVToRGB(0f, (float)rank/dimension, 1f));
        print("Finished setting random basis inventory!");
    }
    public void SetStandardBasisInventory() {
        print("Setting standard basis inventory...");
        this.inventory = MatrixGF2.Identity(this.dimension);
        this.reducedMatrix = MatrixGF2.Identity(this.dimension);
        this.firstZeroRow = this.dimension;
        this.rank = this.dimension;
        this.isSource = true;
        HighlightNode(Color.HSVToRGB(0f, (float)rank/dimension, 1f));
        print("Finished setting standard basis inventory!");
    }

    // Dequeues the front item of the recieve-buffer and integrates it into the nodes inventory
    private void StepRecieveBuffer(bool overwriteMode = false) {
        int[] topItem;
        int rowToOverwrite = Random.Range(0, inventory.FirstZeroRow());
        if (!this.recieveBuffer.TryDequeue(out topItem) || topItem.Length != this.dimension) return;
        if (overwriteMode)
            this.reducedMatrix.WriteRow(rowToOverwrite, topItem);
        else
            this.reducedMatrix.WriteRow(reducedMatrix.FirstZeroRow(), topItem);
        // If the recieved information is a linear combination of some entries of the inventory, don't save it
        // TODO: SLOW! Could be more optimal with gauss_from_row or even manual computation
        int newRank = reducedMatrix.Ref();
        if (this.rank == newRank && !overwriteMode) {
            //print(reducedMatrix.ToString());
            //print(firstZeroRow);
            return;
        }
        if (overwriteMode)
            this.inventory.WriteRow(rowToOverwrite, topItem);
        else
            this.inventory.WriteRow(firstZeroRow, topItem);
        this.firstZeroRow = this.inventory.FirstZeroRow();
        this.rank = newRank;
        //print(this.rank);
    }
    // Sends a random linear combination of at most log2(dimension)+1 rows from the inventory to all neighbors
    // TODO: Could be faster. Optimize Parallel.For (i.e. by accumulating and %2 or djb)
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
            for (int choices = this.firstZeroRow; choices > this.firstZeroRow - this.logDimension; choices--) {
                int randomChoice = Random.Range(0, choices);
                pickedRowIndices.Add(toPickFromIndices[randomChoice]);
                toPickFromIndices.RemoveAt(randomChoice);
            }

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
