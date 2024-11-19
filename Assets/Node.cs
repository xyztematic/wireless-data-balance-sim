using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using m4ri;
using System.Collections.Concurrent;

public class Node : MonoBehaviour
{
    private NodeManager nodeManager;
    private List<GameObject> neighbors;
    // Inventory of this node, represented as a bit-matrix of size "dimension"
    private MatrixGF2 inventory;
    // Tracks the first empty row of the bit-matrix for easy insertion
    private int firstZeroRow;
    // Stores incoming data sent from neighboring nodes in a thread-safe queue for later processing
    private ConcurrentQueue<int[]> recieveBuffer;
    private Coroutine nodeLoop;
    private int dimension;
    private float loopTime, nodeRange;
    private ulong chunkID;

    public bool active = false;

    public void Activate(uint dimension, float loopTime, float nodeRange, ulong chunkID, NodeManager nodeManager) {
        this.dimension = (int) dimension;
        this.loopTime = loopTime;
        this.nodeRange = nodeRange;
        this.chunkID = chunkID;
        this.nodeManager = nodeManager;

        // Get neighbor candidates from NodeManager and remove all candidates out of range
        neighbors = nodeManager.GetNeighborCandidates(chunkID);
        neighbors.RemoveAll(go => Vector3.Distance(go.transform.position, this.transform.position) > nodeRange);
        neighbors.Remove(gameObject); // This node should not be a neighbor of itself

        // Initialize empty inventory
        inventory = new MatrixGF2(this.dimension, this.dimension);
        // Start the node loop in a separate thread and mark this node as active
        this.nodeLoop = StartCoroutine(NodeLoop());
        active = true;
    }
    private IEnumerator NodeLoop() {
        yield return new WaitForSeconds(loopTime);
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
    public void SetSpecificInventory(MatrixGF2 matrix) {
        if (matrix.nrows != this.dimension || matrix.ncols != this.dimension) {
            Debug.LogError("Wrong marix dimension when trying to set specific Node inventory");
            return;
        }
        this.inventory = matrix;
        this.firstZeroRow = matrix.FirstZeroRow();
    }
    public void SetRandomBasisInventory() {
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
        this.firstZeroRow = dimension;
    }
    public void SetStandardBasisInventory() {
        this.inventory = MatrixGF2.Identity(this.dimension);
        this.firstZeroRow = dimension;
    }

    // TODO: Recieving data from neighboring node and integrate into inventory
    public void StepRecieveBuffer() {
        int[] topItem;
        if (!this.recieveBuffer.TryDequeue(out topItem) || topItem.Length != dimension) return;
        inventory.WriteRow(firstZeroRow, topItem);
        firstZeroRow = inventory.FirstZeroRow();
    }
}
