using System.Collections;

namespace Chummer.Backend
{
    static class BitArrayExtensions
    {
        public static int FirstMatching(this BitArray array, bool value, int skip = 0, int max = int.MaxValue)
        {
            if (max > array.Count)
                max = array.Count;
            for (; skip < max; skip++)
            {
                if (array[skip] == value) return skip;
            }

            return -1;
        }
    }
}
