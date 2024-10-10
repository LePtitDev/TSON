using System.Text.Json;
using NUnit.Framework;
using TSONify.Tests.Helpers;
using TSONify.Tests.Models;

namespace TSONify.Tests;

public class SerializerTests
{
    [Test]
    public void Test_01()
    {
        var filePath = Path.Combine(TestUtils.GetProjectPath(), @"Resources\index.json");
        var outputPath = Path.Combine(TestContext.CurrentContext.WorkDirectory, "index.tson");
        var nuGetOrg = JsonSerializer.Deserialize<NuGetOrgJson>(File.ReadAllText(filePath));
        var serializer = new TSONSerializer();
        var data = serializer.Serialize(nuGetOrg!, typeof(NuGetOrgJson));
        File.WriteAllBytes(outputPath, data);

        var result = serializer.Deserialize(typeof(NuGetOrgJson), data);
    }
}
