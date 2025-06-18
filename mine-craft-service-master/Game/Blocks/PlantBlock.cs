namespace MineCraftService.Game;

public class PlantBlock
{
    public static int SetIsWild(int data, bool isWild)
    {
        if (!isWild)
        {
            return data & -9;
        }
        return data | 8;
    }
    public static int SetSize(int data, int size)
    {
        size = Math.Clamp(size, 0, 7);
        return (data & -8) | (size & 7);
    }
}