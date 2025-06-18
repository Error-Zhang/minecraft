namespace MineCraftService.Game;

public static class WoodBlock
{
    public static int SetCutFace(int data, int cutFace)
    {
        data &= -4;
        switch (cutFace)
        {
            case 0:
            case 2:
                return data | 1;
            case 1:
            case 3:
                return data | 2;
            default:
                return data;
        }
    }
}