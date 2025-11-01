namespace WebSpark.HttpClientUtility.StringConverter;

/// <summary>
/// Interface for converting between string representations and strongly-typed models.
/// </summary>
/// <remarks>
/// This interface abstracts the serialization and deserialization operations, 
/// allowing the application to switch between different JSON libraries (such as 
/// System.Text.Json and Newtonsoft.Json) without changing the consuming code.
/// </remarks>
public interface IStringConverter
{
    /// <summary>
    /// Converts a string representation to a strongly-typed model.
    /// </summary>
    /// <typeparam name="T">The target type to convert to.</typeparam>
    /// <param name="value">The string value to convert from.</param>
    /// <returns>An instance of type T created from the provided string value.</returns>
    /// <exception cref="System.Text.Json.JsonException">Thrown when the deserialization fails with System.Text.Json.</exception>
    /// <exception cref="Newtonsoft.Json.JsonException">Thrown when the deserialization fails with Newtonsoft.Json.</exception>
    T ConvertFromString<T>(string value);

    /// <summary>
    /// Converts a strongly-typed model to its string representation.
    /// </summary>
    /// <typeparam name="T">The type of the model to convert from.</typeparam>
    /// <param name="model">The model to convert to a string.</param>
    /// <returns>The string representation of the provided model.</returns>
    /// <exception cref="System.Text.Json.JsonException">Thrown when the serialization fails with System.Text.Json.</exception>
    /// <exception cref="Newtonsoft.Json.JsonException">Thrown when the serialization fails with Newtonsoft.Json.</exception>
    string ConvertFromModel<T>(T model);
}
