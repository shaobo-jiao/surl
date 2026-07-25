using Surl.Api.Common;

namespace Surl.Api.UnitTests;

public class Base62EncodingTests
{
    [Theory]
    [InlineData(0, "0")]
    [InlineData(1, "1")]
    [InlineData(61, "z")]
    [InlineData(62, "10")]
    [InlineData(1234567890, "1LY7VK")]
    [InlineData(long.MaxValue, "AzL8n0Y58m7")]
    public void Encode_ValidInput_ReturnsCorrectEncodedText(long value, string expected)
    {
        string encoded = Base62Encoding.Encode(value);

        Assert.Equal(expected, encoded);
    }

    [Fact]
    public void Encode_NegativeInput_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Base62Encoding.Encode(-1));
    }

    [Theory]
    [InlineData("0", 0L)]
    [InlineData("1", 1L)]
    [InlineData("z", 61L)]
    [InlineData("10", 62L)]
    [InlineData("1LY7VK", 1234567890L)]
    [InlineData("AzL8n0Y58m7", long.MaxValue)]
    public void Decode_ValidInput_ReturnsCorrectDecodedValue(string text, long expected)
    {
        long value = Base62Encoding.Decode(text);

        Assert.Equal(expected, value);
    }

    [Fact]
    public void Decode_EmptyInput_ThrowsFormatException()
    {
        Assert.Throws<FormatException>(() => Base62Encoding.Decode(string.Empty));
    }

    [Fact]
    public void Decode_TooLongInput_ThrowsFormatException()
    {
        Assert.Throws<FormatException>(() => Base62Encoding.Decode("0123456789AB"));
    }

    [Fact]
    public void Decode_InputContainsInvalidChars_ThrowsFormatException()
    {
        Assert.Throws<FormatException>(() => Base62Encoding.Decode("0*0"));
    }

    [Fact]
    public void Decode_InputExceedingMaxValue_ThrowsOverflowException()
    {
        Assert.Throws<OverflowException>(() => Base62Encoding.Decode("AzL8n0Y58m8"));
    }
}
