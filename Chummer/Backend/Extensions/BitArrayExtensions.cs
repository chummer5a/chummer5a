using System.Collections;

namespace Chummer.Backend
{
    static class BitArrayExtensions
    {
        public static int FirstMatching(this BitArray array, bool value, int skip = 0)
        {
            for (; skip < array.Count; skip++)
            {
                if (array[skip] == value) return skip;
            }

            return -1;
        }

    }
}
