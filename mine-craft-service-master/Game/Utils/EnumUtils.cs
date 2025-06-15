using Engine;

namespace MineCraftService.Game;

public struct NamesValues
{
    public ReadOnlyList<string> Names;

    public ReadOnlyList<int> Values;
}

public static class Cache
{
    public static readonly Dictionary<Type, NamesValues> NamesValuesByType = new ();

    public static NamesValues Query(Type type)
    {
        lock (NamesValuesByType)
        {
            NamesValues namesValues;
            if (!NamesValuesByType.TryGetValue(type, out NamesValues value))
            {
                namesValues = default;
                namesValues.Names = new ReadOnlyList<string>(new List<string>(Enum.GetNames(type)));
                namesValues.Values = new ReadOnlyList<int>(new List<int>(Enum.GetValues(type).Cast<int>()));
                value = namesValues;
                NamesValuesByType.Add(type, value);
            }

            namesValues = value;
            return namesValues;
        }
    }
}
public static class EnumUtils
{
    public static string GetEnumName(Type type, int value)
    {
        int num = GetEnumValues(type).IndexOf(value);
        if (num >= 0)
        {
            return GetEnumNames(type)[num];
        }

        return "<invalid enum>";
    }

    public static IList<string> GetEnumNames(Type type)
    {
        return Cache.Query(type).Names;
    }

    public static IList<int> GetEnumValues(Type type)
    {
        return Cache.Query(type).Values;
    }
}