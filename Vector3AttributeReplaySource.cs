using UnityEngine;

public abstract class Vector3AttributeReplaySource : ComponentAttributeReplaySource<Vector3>
{
    protected override void OnPlaybackUpdate(Vector3 currentFrame, Vector3 nextFrame, float time)
    {
        Vector3 interpolatedValue = Vector3.Lerp(currentFrame, nextFrame, time);
        PlayAttributeValue(interpolatedValue);
    }

    protected abstract void PlayAttributeValue(Vector3 value);
}
