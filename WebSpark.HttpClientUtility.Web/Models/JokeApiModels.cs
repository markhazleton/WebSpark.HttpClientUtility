using System.Text.Json.Serialization;

namespace WebSpark.HttpClientUtility.Web.Models;

/// <summary>
/// Response from the Joke API
/// </summary>
public class JokeResponse
{
    [JsonPropertyName("error")]
    public bool Error { get; set; }

    [JsonPropertyName("category")]
    public string Category { get; set; } = string.Empty;

[JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("joke")]
    public string? Joke { get; set; }

    [JsonPropertyName("setup")]
    public string? Setup { get; set; }

    [JsonPropertyName("delivery")]
    public string? Delivery { get; set; }

    [JsonPropertyName("flags")]
    public JokeFlags? Flags { get; set; }

  [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("safe")]
    public bool Safe { get; set; }

    [JsonPropertyName("lang")]
    public string Lang { get; set; } = string.Empty;

/// <summary>
    /// Gets the full joke text, handling both single and two-part jokes
    /// </summary>
    public string FullJoke => Type == "single" ? Joke ?? string.Empty : $"{Setup}\n{Delivery}";
}

public class JokeFlags
{
    [JsonPropertyName("nsfw")]
    public bool Nsfw { get; set; }

    [JsonPropertyName("religious")]
    public bool Religious { get; set; }

    [JsonPropertyName("political")]
    public bool Political { get; set; }

    [JsonPropertyName("racist")]
    public bool Racist { get; set; }

    [JsonPropertyName("sexist")]
    public bool Sexist { get; set; }

    [JsonPropertyName("explicit")]
    public bool Explicit { get; set; }
}

/// <summary>
/// Multiple jokes response
/// </summary>
public class JokesResponse
{
    [JsonPropertyName("error")]
    public bool Error { get; set; }

    [JsonPropertyName("amount")]
    public int Amount { get; set; }

    [JsonPropertyName("jokes")]
    public List<JokeResponse> Jokes { get; set; } = new();
}
