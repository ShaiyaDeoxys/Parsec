using System.Linq;
using Parsec.Shaiya.Data;

namespace Parsec.Tests.Shaiya.Data;

public class SafTests
{
    [Theory]
    [InlineData(0, 100)]
    [InlineData(200, 500)]
    [InlineData(1000, 3000)]
    public void SafClearingTest(long offset, int length)
    {
        using var saf = new Saf("Shaiya/Data/clearme.saf");

        var nullData = new byte[length];
        saf.ClearBytes(offset, length);

        var newData = saf.ReadBytes(offset, length);
        Assert.True(newData.SequenceEqual(nullData));
    }
}
