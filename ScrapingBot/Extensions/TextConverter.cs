using System;
using System.Linq;

namespace ScrapingBot.Extensions;

public static class TextConverter {
    public static int ToInt(this string text) {
        string offers = new(text.Where(char.IsDigit).ToArray());

        try {
            int num = int.Parse(offers);
            return num;
        }
        catch(FormatException) {
            throw new FormatException($"String could not be parsed in the method {nameof(ToInt)}.");
        }
        catch(ArgumentNullException) {
            throw new ArgumentNullException($"String is null in the method {nameof(ToInt)}.");
        }
    }
}
