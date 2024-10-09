using System.Runtime.CompilerServices;
using NUnit.Framework;

namespace TSONify.Tests.Helpers;

internal static class TestUtils
{
    public static string GetProjectPath([CallerFilePath] string filePath = "")
    {
        if (string.IsNullOrEmpty(filePath))
            Assert.Fail();

        do
        {
            filePath = Path.GetDirectoryName(filePath)!;
            if (string.IsNullOrEmpty(filePath))
                Assert.Fail();
        } while (Path.GetFileName(filePath) != "TSONify.Tests");

        return filePath;
    }
}
