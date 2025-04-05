using UnityEngine;

public class TransformScaleReplaySource : Vector3AttributeReplaySource
{
    protected override void PlayAttributeValue(Vector3 value)
    {
        transform.localScale = value;
    }
}
