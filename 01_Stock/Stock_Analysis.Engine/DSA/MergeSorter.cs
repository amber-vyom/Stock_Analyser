namespace Stock_Analysis.Engine.DSA;

public static class MergeSorter
{
    public static List<T> Sort<T>(
        IList<T> items,
        Comparison<T> comparison)
    {
        var working = new List<T>(items);

        if (working.Count < 2)
        {
            return working;
        }

        T[] buffer = new T[working.Count];

        SortRange(working, buffer, 0, working.Count - 1, comparison);

        return working;
    }

    private static void SortRange<T>(
        List<T> items,
        T[] buffer,
        int left,
        int right,
        Comparison<T> comparison)
    {
        if (left >= right)
        {
            return;
        }

        int middle = left + (right - left) / 2;

        SortRange(items, buffer, left, middle, comparison);
        SortRange(items, buffer, middle + 1, right, comparison);

        Merge(items, buffer, left, middle, right, comparison);
    }

    private static void Merge<T>(
        List<T> items,
        T[] buffer,
        int left,
        int middle,
        int right,
        Comparison<T> comparison)
    {
        int i = left;
        int j = middle + 1;
        int k = left;

        while (i <= middle && j <= right)
        {
            if (comparison(items[i], items[j]) <= 0)
            {
                buffer[k++] = items[i++];
            }
            else
            {
                buffer[k++] = items[j++];
            }
        }

        while (i <= middle)
        {
            buffer[k++] = items[i++];
        }

        while (j <= right)
        {
            buffer[k++] = items[j++];
        }

        for (int index = left; index <= right; index++)
        {
            items[index] = buffer[index];
        }
    }
}
