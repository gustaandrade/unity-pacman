using UnityEngine;

public interface IMazeEntity
{
    void Move();
    float DistanceFromTile(Vector3 target);
    bool TargetWasOvershot();
    void UpdateAnimation();
}
