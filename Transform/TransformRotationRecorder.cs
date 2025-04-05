using System.Collections.Generic;
using UnityEngine;

public class TransformRotationRecorder : Vector3AttributeRecorder
{
    protected override void RecordNewFrame(List<Vector3> frames)
    {
        frames.Add(transform.eulerAngles);
    }
}
