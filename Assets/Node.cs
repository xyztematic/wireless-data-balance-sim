using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour
{
    private NodeManager nodeManager;
    private List<GameObject> neighbors;
    private Coroutine nodeLoop;

    private ulong chunkID;
    private float loopTime;

    public void Activate(float loopTime, ulong chunkID, NodeManager nodeManager) {
        this.loopTime = loopTime;
        this.chunkID = chunkID;
        this.nodeManager = nodeManager;
        neighbors = nodeManager.GetNeighborCandidates(chunkID);
        this.nodeLoop = StartCoroutine(NodeLoop());
    }
    private IEnumerator NodeLoop() {
        yield return new WaitForSeconds(loopTime);
    }
}
