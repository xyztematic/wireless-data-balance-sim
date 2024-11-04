using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour
{
    private NodeManager nodeManager;
    private List<GameObject> neighbors;
    private Coroutine nodeLoop;

    private int chunkX, chunkY;
    private float loopTime;

    public void Activate(float loopTime, NodeManager nodeManager) {
        this.loopTime = loopTime;
        this.nodeManager = nodeManager;
        this.neighbors = PopulateNeighbors();
        this.nodeLoop = StartCoroutine(NodeLoop());
    }
    private List<GameObject> PopulateNeighbors() {
        return new();
    }
    private IEnumerator NodeLoop() {
        yield return new WaitForSeconds(loopTime);
    }
}
