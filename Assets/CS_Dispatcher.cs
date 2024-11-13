using UnityEngine;

public class CS_Dispatcher : MonoBehaviour
{
    public ComputeShader cs;
    public RenderTexture rt;

    private ComputeBuffer cb;
    void Start()
    {
        cb = new ComputeBuffer(1, 1);
        cs.SetTexture(0, "Result", rt);
        //cs.SetBuffer(0, "NodePosAndRanges", cb);
        cs.Dispatch(0, rt.width / 8, rt.height / 8, 1);
        cb.Release();
    }
}
