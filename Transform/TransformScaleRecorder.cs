using System.Collections.Generic;
using UnityEngine;

public class TransformScaleRecorder : Vector3AttributeRecorder
{
    protected override void RecordNewFrame(List<Vector3> frames)
    {
        frames.Add(transform.localScale);
    }
}
