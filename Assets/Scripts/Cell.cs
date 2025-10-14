using UnityEngine;

public struct Cell
{
    public int Cost;
    public Vector3 Position;
    public Vector2Int Direction;
    public bool Valid;
    public Vector2Int GridIndex;
}
