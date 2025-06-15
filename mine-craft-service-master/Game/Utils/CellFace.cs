using Engine;

namespace MineCraftService.Game;

public struct CellFace
{
    public static readonly Point3[] FaceToPoint3 =
    [
        new(0, 0, 1),
        new(1, 0, 0),
        new(0, 0, -1),
        new(-1, 0, 0),
        new(0, 1, 0),
        new(0, -1, 0)
    ];
   
}