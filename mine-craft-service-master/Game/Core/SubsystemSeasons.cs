namespace MineCraftService.Game;

public class SubsystemSeasons
{

	public static readonly float SummerStart = 0f;

	public static readonly float AutumnStart = 0.25f;

	public static readonly float WinterStart = 0.5f;

	public static readonly float SpringStart = 0.75f;

	public static readonly float MidSummer = IntervalUtils.Midpoint(SummerStart, AutumnStart);

	public static readonly float MidAutumn = IntervalUtils.Midpoint(AutumnStart, WinterStart);

	public static readonly float MidWinter = IntervalUtils.Midpoint(WinterStart, SpringStart);

	public static readonly float MidSpring = IntervalUtils.Midpoint(SpringStart, SummerStart);
}