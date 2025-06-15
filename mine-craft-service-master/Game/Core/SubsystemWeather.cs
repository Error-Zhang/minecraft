using Engine;

namespace MineCraftService.Game;

public class SubsystemWeather
{
    public static readonly Func<int, int, bool> IsPlaceFrozen = (temperature, y) => temperature + GetTemperatureAdjustmentAtHeight(y) <= 0;
    public static readonly Func<int, int> GetTemperatureAdjustmentAtHeight = y=> (int)MathF.Round((y > 64) ? (-0.0008f * MathUtils.Sqr(y - 64)) : (0.1f * (64 - y)));
}