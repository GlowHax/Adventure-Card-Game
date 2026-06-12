using UnityEngine;

[DefaultExecutionOrder(1000)]
public class CameraOffsetter : MonoBehaviour
{
    public Vector3 positionOffset;
    public Vector3 rotationOffset;

    void LateUpdate()
    {
        if (positionOffset != Vector3.zero)
        {
            transform.position += positionOffset;
        }
        
        if (rotationOffset != Vector3.zero)
        {
            transform.eulerAngles += rotationOffset;
        }
    }
}
