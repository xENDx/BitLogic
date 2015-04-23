using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BitLogic {
    internal class common {
        
        internal static void copy<T>(T[] from, T[] to) { copy(from, to, 0, 0, Math.Min(from.Length, to.Length)); }
        internal static void copy<T>(T[] from, T[] to, int fromIndex, int toIndex) { copy(from, to, fromIndex, toIndex, Math.Min(from.Length - fromIndex, to.Length - toIndex)); }
        internal static void copy<T>(T[] from, T[] to, int fromIndex, int toIndex, int length)
        {
            while (length-- > 0)
                to[toIndex++] = from[fromIndex++];
        }

        internal static T[] expand_linear<T>(T[] array, int left, int right) {
            T[] r = new T[array.Length + left + right];
            int copymin = Math.Max(0, -left);
            copy(array, r, copymin, Math.Max(0, left), array.Length - copymin - Math.Max(0, -right));
            return r;
        }
    }
}
