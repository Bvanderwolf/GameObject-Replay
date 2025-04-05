using System.Collections.Generic;

public interface IComponentAttributeReplayDataSource
{
    int TotalRecordedFrames { get;  }
    float TotalRecordingTime { get;  }
    float RecordingInterval { get; }
}

public interface IComponentAttributeReplayDataSource<out T> : IComponentAttributeReplayDataSource
{
    public IReadOnlyList<T> LoadFramesFromIndex(int startIndex, int count);
    public IReadOnlyList<T> LoadFramesFromDirectory(string directoryPath, int count);
}
