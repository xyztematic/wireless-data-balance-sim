using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using m4ri;

public class Node : MonoBehaviour
{
    private NodeManager nodeManager;
    private List<GameObject> neighbors;
    private MatrixGF2 inventory;
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
    }
    public void SetStandardBasisInventory() {
        this.inventory = MatrixGF2.Identity(this.dimension);
    }

    // TODO: Recieving data from neighboring node and integrate into inventory
}
