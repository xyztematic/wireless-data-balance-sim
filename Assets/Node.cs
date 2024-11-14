using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour
{
    private NodeManager nodeManager;
    private List<GameObject> neighbors;
    private Coroutine nodeLoop;

    private ulong chunkID;
    private float loopTime, nodeRange;
    public bool active = false;

    public void Activate(float loopTime, float nodeRange, ulong chunkID, NodeManager nodeManager) {
        this.loopTime = loopTime;
        this.nodeRange = nodeRange;
        this.chunkID = chunkID;
        this.nodeManager = nodeManager;
        neighbors = nodeManager.GetNeighborCandidates(chunkID);
        neighbors.RemoveAll(go => Vector3.Distance(go.transform.position, this.transform.position) > nodeRange);
        neighbors.Remove(gameObject); // this node should not be a neighbor of itself
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
        return nodeRange;
    }
}
