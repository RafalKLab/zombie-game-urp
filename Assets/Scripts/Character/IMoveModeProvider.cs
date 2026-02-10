using UnityEngine;

public interface IMoveModeProvider
{
    bool IsRunning { get; }
    float RunSpeed { get; }
    float WalkSpeed { get; }
}
