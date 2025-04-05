using UnityEngine;

public class TranformRotationReplaySource : Vector3AttributeReplaySource
{
    protected override void PlayAttributeValue(Vector3 value)
    {
        transform.eulerAngles = value;
    }
}
