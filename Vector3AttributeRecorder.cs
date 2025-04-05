using System.Collections.Generic;
using System.IO;
using UnityEngine;

public abstract class Vector3AttributeRecorder : ComponentAttributeRecorder<Vector3>
{
    protected override void WriteFramesToFile(List<Vector3> frames, int fileIndex)
    {
        string filePath = Path.Combine(CurrentRecordingDirectory, $"positions_{fileIndex}.bin");
        
        using (BinaryWriter writer = new BinaryWriter(File.Open(filePath, FileMode.Create)))
        {
            writer.Write(frames.Count);
            foreach (Vector3 position in frames)
            {
                writer.Write(position.x);
                writer.Write(position.y);
                writer.Write(position.z);
            }
        }
    }

    private List<Vector3> ReadPositionsFromFile(string filePath)
    {
        List<Vector3> positions = new List<Vector3>();
        using (BinaryReader reader = new BinaryReader(File.Open(filePath, FileMode.Open)))
        {
            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                float x = reader.ReadSingle();
                float y = reader.ReadSingle();
                float z = reader.ReadSingle();
                positions.Add(new Vector3(x, y, z));
            }
        }
        return positions;
    }

    public override IReadOnlyList<Vector3> LoadFramesFromIndex(int startIndex, int count)
    {
        Debug.Log($"Loading {count} frames from index {startIndex} in directory: {CurrentRecordingDirectory}");

        List<Vector3> leftOverPositions = GetLoadedFrames();
        List<Vector3> positionsToLoad = new List<Vector3>();
        int currentIndex = 0;
        int remainingCount = count;
        int totalPositionCount = leftOverPositions.Count;
        
        if (startIndex < leftOverPositions.Count)
        {
            int inMemoryCount = Mathf.Min(remainingCount, leftOverPositions.Count - startIndex);
            positionsToLoad.AddRange(leftOverPositions.GetRange(startIndex, inMemoryCount));
            remainingCount -= inMemoryCount;
            currentIndex = leftOverPositions.Count;
        }
        
        string[] files = Directory.GetFiles(CurrentRecordingDirectory, "positions_*.bin");
        foreach (string filePath in files)
        {      
            List<Vector3> positionsFromFile = ReadPositionsFromFile(filePath);
            totalPositionCount += positionsFromFile.Count;
            
            if (remainingCount > 0 && currentIndex + positionsFromFile.Count > startIndex)
            {
                int fileStartIndex = Mathf.Max(0, startIndex - currentIndex);
                int fileCount = Mathf.Min(remainingCount, positionsFromFile.Count - fileStartIndex);
                positionsToLoad.AddRange(positionsFromFile.GetRange(fileStartIndex, fileCount));
                remainingCount -= fileCount;
            }
            currentIndex += positionsFromFile.Count;
        }
        
        TotalRecordedFrames = totalPositionCount;
        TotalRecordingTime = TotalRecordedFrames * CurrentRecordingSettings.RecordingInterval;

        return positionsToLoad.AsReadOnly();
    }

    public override IReadOnlyList<Vector3> LoadFramesFromDirectory(string directoryPath, int count)
    {
        Debug.Log($"Loading {count} frames from directory: {directoryPath}");
        
        List<Vector3> positions = new List<Vector3>();
        int remainingCount = count;
        int totalPositionCount = 0;
        string[] files = Directory.GetFiles(directoryPath, "positions_*.bin");
        
        foreach (string file in files)
        {
            List<Vector3> positionsFromFile = ReadPositionsFromFile(file);
            totalPositionCount += positionsFromFile.Count;

            if (remainingCount > 0)
            {
                int frameCountToAdd = Mathf.Min(remainingCount, positionsFromFile.Count);
                positions.AddRange(positionsFromFile.GetRange(0, frameCountToAdd));
                remainingCount -= frameCountToAdd;
            }
        }

        CurrentRecordingDirectory = directoryPath;
        TotalRecordedFrames = totalPositionCount;
        TotalRecordingTime = TotalRecordedFrames * CurrentRecordingSettings.RecordingInterval;

        return positions.AsReadOnly();
    }
}
