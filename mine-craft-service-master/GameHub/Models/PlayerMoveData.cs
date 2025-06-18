namespace MineCraftService.GameHub;

public class PlayerMoveData
{
    public int PlayerId { get; set; }
    public float X { get; set; }
    public float Y { get; set; }
    
    public float Z { get; set; }
    public float Yaw { get; set; }
    public float Pitch { get; set; }
}