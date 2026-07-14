namespace Surl.Api.Common;

/// <summary>
/// Base62 encoding that converts a long integer to its Base62 text representation and vice versa
/// </summary>
public static class Base62Encoding
{
    public const string Charset = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";

    public static string Encode(long val)
    {
        if (val < 0) throw new ArgumentOutOfRangeException(nameof(val), val, "Non-negative value expected");
        if (val == 0) return Charset[..1];

        // algo: e.g. 11111 => 179*62 + 13 = (2*62 + 55) * 62 + 13 => 2*62^2 + 55*62 + 13
        var text = new char[11]; // max long integer: 9,223,372,036,854,775,807 => AzL8n0Y58m7
        int index = 10;

        while (val > 0)
        {
            long quotient = val / 62;
            int reminder = (int)(val - quotient * 62);
            text[index--] = Charset[reminder];
            val = quotient;
        }
        return new string(text, index + 1, text.Length - index - 1); // encoded text starts from current index + 1;
    }

    public static long Decode(string text)
    {
        if (string.IsNullOrEmpty(text) || text.Length > 11) // quick check on obvious invalid input 
            throw new FormatException("Invalid input for Base62 decoding");

        long val = 0;
        for (int i = 0; i < text.Length; i++)
        {
            int cv = Charset.IndexOf(text[i]);
            if (cv == -1) throw new FormatException("Invalid character in Base62 Encoding");

            // check overflow, whether val*62 + cv > long.MaxValue <==> whether val > (long.MaxValue - cv) / 62
            if (val > (long.MaxValue - cv) / 62)
                throw new OverflowException("code exceeding maximum value of long integer");

            val = val * 62 + cv;
        }
        return val;
    }

    public static bool TryDecode(string text, out long val)
    {
        try
        {
            val = Decode(text);
            return true;
        }
        catch
        {
            val = -1;
            return false;
        }
    }
}
