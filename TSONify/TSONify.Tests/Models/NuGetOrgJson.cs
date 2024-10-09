using System.Text.Json.Serialization;

namespace TSONify.Tests.Models;

internal class NuGetOrgJson
{
    [JsonPropertyName("@id")]
    public string? Id { get; set; }

    [JsonPropertyName("@type")]
    public string[]? Type { get; set; }

    [JsonPropertyName("commitId")]
    public string? CommitId { get; set; }

    [JsonPropertyName("commitTimeStamp")]
    public string? CommitTimeStamp { get; set; }

    [JsonPropertyName("count")]
    public int Count { get; set; }

    [JsonPropertyName("nuget:lastCreated")]
    public string? NuGetLastCreated { get; set; }

    [JsonPropertyName("nuget:lastDeleted")]
    public string? NuGetLastDeleted { get; set; }

    [JsonPropertyName("nuget:lastEdited")]
    public string? NuGetLastEdited { get; set; }

    [JsonPropertyName("items")]
    public NuGetOrgItemJson[]? Items { get; set; }

    [JsonPropertyName("@context")]
    public NuGetOrgContextJson? Context { get; set; }

    public class NuGetOrgItemJson
    {
        [JsonPropertyName("@id")]
        public string? Id { get; set; }

        [JsonPropertyName("@type")]
        public string? Type { get; set; }

        [JsonPropertyName("commitId")]
        public string? CommitId { get; set; }

        [JsonPropertyName("commitTimeStamp")]
        public string? CommitTimeStamp { get; set; }

        [JsonPropertyName("count")]
        public int Count { get; set; }
    }

    public class NuGetOrgContextJson
    {
        [JsonPropertyName("@vocab")]
        public string? Vocab { get; set; }

        [JsonPropertyName("nuget")]
        public string? Nuget { get; set; }

        [JsonPropertyName("items")]
        public NuGetOrgContextItemsJson? Items { get; set; }

        [JsonPropertyName("parent")]
        public NuGetOrgContextDetailJson? Parent { get; set; }

        [JsonPropertyName("commitTimeStamp")]
        public NuGetOrgContextDetailJson? CommitTimeStamp { get; set; }

        [JsonPropertyName("nuget:lastCreated")]
        public NuGetOrgContextDetailJson? NugetLastCreated { get; set; }

        [JsonPropertyName("nuget:lastDeleted")]
        public NuGetOrgContextDetailJson? NugetLastDeleted { get; set; }

        [JsonPropertyName("nuget:lastEdited")]
        public NuGetOrgContextDetailJson? NugetLastEdited { get; set; }
    }

    public class NuGetOrgContextItemsJson
    {
        [JsonPropertyName("@id")]
        public string? Id { get; set; }

        [JsonPropertyName("@container")]
        public string? Container { get; set; }
    }

    public class NuGetOrgContextDetailJson
    {
        [JsonPropertyName("@type")]
        public string? Type { get; set; }
    }
}
