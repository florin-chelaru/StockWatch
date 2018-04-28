using System.Collections.Generic;
using System.Linq;

namespace Common
{
  public static class StringUtil
  {
    public static string ToPrettyString<TKey, TValue>(this IDictionary<TKey, TValue> dict)
    {
      return $"[{string.Join(", ", from entry in dict select $"({entry.Key}, {entry.Value})")}]";
    }

    public static string ToPrettyString<T>(this IList<T> list)
    {
      return $"[{string.Join(", ", list)}]";
    }
  }
}