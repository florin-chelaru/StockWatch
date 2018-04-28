using System.Collections.Generic;

namespace Common
{
  public static class EqualityUtil
  {
    public static bool DictionariesAreEqual<TKey, TValue>(IDictionary<TKey, TValue> dict1,
      IDictionary<TKey, TValue> dict2)
    {
      if (ReferenceEquals(dict1, dict2))
      {
        return true;
      }

      if (dict1 == null || dict2 == null)
      {
        return false;
      }

      if (dict1.Count != dict2.Count)
      {
        return false;
      }

      foreach (var entry in dict1)
      {
        if (!dict2.TryGetValue(entry.Key, out var value))
        {
          return false;
        }

        if (!entry.Value.Equals(value))
        {
          return false;
        }
      }

      return true;
    }

    public static bool ListsAreEqual<T>(IList<T> list1, IList<T> list2)
    {
      if (ReferenceEquals(list1, list2))
      {
        return true;
      }

      if (list1 == null || list2 == null)
      {
        return false;
      }

      for (int i = 0; i < list1.Count; ++i)
      {
        if (!ReferenceEquals(list1[i], list2[i]) &&
            (list1[i] == null || list2[i] == null || !list1[i].Equals(list2[i])))
        {
          return false;
        }
      }

      return true;
    }
  }
}