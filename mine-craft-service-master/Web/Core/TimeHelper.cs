namespace MineCraftService.Web;

public static class TimeHelper
{
    /// <summary>
    /// 获取当前东八区本地时间，精确到秒（毫秒为 0）
    /// </summary>
    public static DateTime GetChinaLocalTime()
    {
        var utcNow = DateTime.UtcNow.AddHours(8);
        return new DateTime(
            utcNow.Year,
            utcNow.Month,
            utcNow.Day,
            utcNow.Hour,
            utcNow.Minute,
            utcNow.Second,
            DateTimeKind.Unspecified // 如果你希望保留 +8:00 可改为 Local
        );
    }
}