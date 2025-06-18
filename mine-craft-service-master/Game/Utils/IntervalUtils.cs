namespace MineCraftService.Game;

public static class IntervalUtils
{
	public static float Normalize(float t)
	{
		return t - MathF.Floor(t);
	}

	public static float Add(float t, float interval)
	{
		return Normalize(t + interval);
	}

	public static float Interval(float t1, float t2)
	{
		return Normalize(t2 - t1);
	}

	public static float Midpoint(float t1, float t2, float factor = 0.5f)
	{
		return Add(t1, Interval(t1, t2) * factor);
	}

}