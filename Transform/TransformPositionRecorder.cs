using System.Collections.Generic;
using UnityEngine;

public class TransformPositionRecorder : Vector3AttributeRecorder
{
    protected override void RecordNewFrame(List<Vector3> frames)
    {
        frames.Add(transform.position);
    }
}
