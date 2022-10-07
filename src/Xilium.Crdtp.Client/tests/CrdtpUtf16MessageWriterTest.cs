using System.Text;
using Xilium.Crdtp.Client.Logging;
using Xunit;

namespace Xilium.Crdtp.Client.Tests;

public class CrdtpUtf16MessageWriterTest
{
    [Theory]
    // Cases when overflow not happens, or there is no any win
    [InlineData("", "")]
    [InlineData("1", "1")]
    [InlineData("12", "12")]
    [InlineData("123", "123")]
    [InlineData("1234", "1234")]
    [InlineData("12345", "12345")]
    [InlineData("123456", "123456")]
    [InlineData("1234567", "1234567")]
    [InlineData("12345678", "12345678")]
    [InlineData("123456789", "123456789")]
    [InlineData("1234567890", "1234567890")]

    // Overflow (Precise)
    //[InlineData("12345678901", "1234<>8901")]
    //[InlineData("123456789012", "1234<>9012")]
    //[InlineData("1234567890123", "1234<>0123")]
    //[InlineData("12345678901234", "1234<>7890")]

    // Overflow
    // абвгдеёжзийклмнопрстуфхцчшщъыьэюя
    [InlineData("абвгдеёжзий", "абв<>ий")]
    [InlineData("абвгдеёжзийклмнопрстуфхцчшщъыьэюя", "абв<>юя")]

    // This is how it should work in precise mode.
    // [InlineData("абвгдеёжзий", "абвг<>жзий")]

    public void FormatOverflow(string input, string output)
    {
        var options = new CrdtpMessageWriterOptions
        {
            SendPrefix = null,
            ReceivePrefix = null,
            OverflowSizeInBytes = 10,
            EllipsisString = "<>",
        };

        var messageBytes = Encoding.UTF8.GetBytes(input);

        var text = CrdtpUtf16MessageWriter.Format(messageBytes, isReceive: false, in options);
        Assert.Equal(output, text);
    }

    /*
    [Theory]
    [InlineData("1", "1")]
    [InlineData("12", "12")]
    [InlineData("123", "123")]
    public void FormatSimple(string input, string output)
    {
        var options = new CrdtpMessageWriterOptions
        {
            SendPrefix = ">>>|",
            ReceivePrefix = "<<<|",
            OverflowSize = 11,
            EllipsisString = " ... ",
        };

        var message = "12345678901";
        var messageBytes = Encoding.UTF8.GetBytes(message);

        var text = CrdtpUtf16MessageWriter.Format(messageBytes, isReceive: false, in options);
        Assert.Equal(">>>|12345678901", text);
    }

    [Fact]
    public void Format()
    {
        var options = new CrdtpMessageWriterOptions
        {
            SendPrefix = ">>>|",
            ReceivePrefix = "<<<|",
            OverflowSize = 11,
            EllipsisString = " ... ",
        };

        var message = "12345678901";
        var messageBytes = Encoding.UTF8.GetBytes(message);

        var text = CrdtpUtf16MessageWriter.Format(messageBytes, isReceive: false, in options);
        Assert.Equal(">>>|12345678901", text);
    }
    */
}
