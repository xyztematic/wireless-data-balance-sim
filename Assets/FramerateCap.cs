using UnityEngine;

public class FramerateCap : MonoBehaviour
{
    public int framerateCap = 60;
    void Start()
    {
        Application.targetFrameRate = framerateCap;
    }
}
