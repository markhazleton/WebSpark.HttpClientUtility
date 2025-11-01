namespace WebSpark.HttpClientUtility.Test.QueryString;

[TestClass]
public class QueryStringParametersListTests
{
    [TestMethod]
    public void Constructor_CreatesEmptyList()
    {
        // Arrange & Act
        var queryString = new QueryStringParametersList();

        // Assert
        Assert.AreEqual(string.Empty, queryString.GetQueryStringPostfix());
    }

    [TestMethod]
    public void Add_ValidKeyValue_AddsParameter()
    {
        // Arrange
        var queryString = new QueryStringParametersList();

        // Act
        queryString.Add("key1", "value1");

        // Assert
        Assert.AreEqual("key1=value1", queryString.GetQueryStringPostfix());
    }

    [TestMethod]
    public void Add_MultipleParameters_ConcatenatesWithAmpersand()
    {
        // Arrange
        var queryString = new QueryStringParametersList();

        // Act
        queryString.Add("key1", "value1");
        queryString.Add("key2", "value2");
        queryString.Add("key3", "value3");

        // Assert
        Assert.AreEqual("key1=value1&key2=value2&key3=value3", queryString.GetQueryStringPostfix());
    }

    [TestMethod]
    public void Add_SpecialCharacters_EncodesCorrectly()
    {
        // Arrange
        var queryString = new QueryStringParametersList();

        // Act
        queryString.Add("key with spaces", "value with spaces & symbols");
        queryString.Add("search", "C# programming");

        // Assert
        var result = queryString.GetQueryStringPostfix();
        Assert.IsTrue(result.Contains("key%20with%20spaces"));
        Assert.IsTrue(result.Contains("value%20with%20spaces%20%26%20symbols"));
        Assert.IsTrue(result.Contains("C%23%20programming"));
    }

    [TestMethod]
    public void Add_EmptyValue_AddsParameter()
    {
        // Arrange
        var queryString = new QueryStringParametersList();

        // Act
        queryString.Add("emptyKey", "");

        // Assert
        Assert.AreEqual("emptyKey=", queryString.GetQueryStringPostfix());
    }

    [TestMethod]
    public void Add_NullKey_ThrowsArgumentException()
    {
        // Arrange
        var queryString = new QueryStringParametersList();

        // Act & Assert
        Assert.ThrowsExactly<ArgumentException>(() => queryString.Add(null, "value"));
    }

    [TestMethod]
    public void Add_EmptyKey_ThrowsArgumentException()
    {
        // Arrange
        var queryString = new QueryStringParametersList();

        // Act & Assert
        Assert.ThrowsExactly<ArgumentException>(() => queryString.Add("", "value"));
    }

    [TestMethod]
    public void Add_WhitespaceKey_ThrowsArgumentException()
    {
        // Arrange
        var queryString = new QueryStringParametersList();

        // Act & Assert
        Assert.ThrowsExactly<ArgumentException>(() => queryString.Add("   ", "value"));
    }

    [TestMethod]
    public void Add_NullValue_ThrowsArgumentNullException()
    {
        // Arrange
        var queryString = new QueryStringParametersList();

        // Act & Assert
        Assert.ThrowsExactly<ArgumentNullException>(() => queryString.Add("key", null));
    }

    [TestMethod]
    public void Add_DuplicateKeys_AllowsMultipleValues()
    {
        // Arrange
        var queryString = new QueryStringParametersList();

        // Act
        queryString.Add("filter", "type:book");
        queryString.Add("filter", "author:smith");

        // Assert
        Assert.AreEqual("filter=type%3Abook&filter=author%3Asmith", queryString.GetQueryStringPostfix());
    }

    [TestMethod]
    public void Add_UrlUnsafeCharacters_EncodedProperly()
    {
        // Arrange
        var queryString = new QueryStringParametersList();

        // Act
        queryString.Add("query", "SELECT * FROM users WHERE id = 1");
        queryString.Add("callback", "javascript:alert('test')");

        // Assert
        var result = queryString.GetQueryStringPostfix();
        Assert.IsTrue(result.Contains("SELECT%20"));
        Assert.IsTrue(result.Contains("javascript%3Aalert"));
        Assert.IsFalse(result.Contains(" ")); // Spaces should be encoded
        Assert.IsFalse(result.Contains(":")); // Colons should be encoded in values
    }

    [TestMethod]
    public void GetQueryStringPostfix_EmptyList_ReturnsEmptyString()
    {
        // Arrange
        var queryString = new QueryStringParametersList();

        // Act
        var result = queryString.GetQueryStringPostfix();

        // Assert
        Assert.AreEqual(string.Empty, result);
    }

    [TestMethod]
    public void GetQueryStringPostfix_SingleParameter_NoLeadingAmpersand()
    {
        // Arrange
        var queryString = new QueryStringParametersList();
        queryString.Add("single", "parameter");

        // Act
        var result = queryString.GetQueryStringPostfix();

        // Assert
        Assert.IsFalse(result.StartsWith("&"));
        Assert.AreEqual("single=parameter", result);
    }

    [TestMethod]
    public void GetQueryStringPostfix_RepeatedCalls_ReturnsSameResult()
    {
        // Arrange
        var queryString = new QueryStringParametersList();
        queryString.Add("key1", "value1");
        queryString.Add("key2", "value2");

        // Act
        var result1 = queryString.GetQueryStringPostfix();
        var result2 = queryString.GetQueryStringPostfix();

        // Assert
        Assert.AreEqual(result1, result2);
        Assert.AreEqual("key1=value1&key2=value2", result1);
    }
}
