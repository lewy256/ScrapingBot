using System;

namespace ScrapingBot.Extensions;

public static class ArraySlice {
    public static T[] Slice<T>(this T[] source, int index, int length) {
        if(index + length > source.Length) {
            length = source.Length - index;
        }

        T[] slice = new T[length];
        Array.Copy(source, index, slice, 0, length);
        return slice;
    }
}
