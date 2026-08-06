namespace Surl.Api.IntegrationTests.Helpers;

public static class RandomHelper
{
    /// <summary>
    /// Returns a list of distinct integers of given number in given range
    /// </summary>
    /// <param name="number"></param>
    /// <param name="minValue"></param>
    /// <param name="maxValue"></param>
    /// <returns></returns>
    public static List<int> DistinctIntegers(int number, int minValue, int maxValue)
    {
        var set = new HashSet<int>();
        while (set.Count < number)
        {
            set.Add(Random.Shared.Next(minValue, maxValue));
        }
        return set.ToList();
    }
}
