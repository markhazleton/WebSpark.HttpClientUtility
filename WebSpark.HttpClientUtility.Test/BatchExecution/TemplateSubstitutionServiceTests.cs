using WebSpark.HttpClientUtility.BatchExecution;

namespace WebSpark.HttpClientUtility.Test.BatchExecution;

[TestClass]
public class TemplateSubstitutionServiceTests
{
    private readonly TemplateSubstitutionService _service = new();

    [TestMethod]
    public void Render_ReplacesPathAndBodyPlaceholders_AndPreservesMissingKeys()
    {
        var user = new BatchUserContext
        {
            UserId = "john.doe",
            Properties = new Dictionary<string, string>
            {
                ["userId"] = "42",
                ["firstName"] = "John"
            }
        };

        var path = _service.Render("/users/{userId}/profile/{missing}", user);
        var body = _service.Render("{\"firstName\":\"{firstName}\",\"missing\":\"{missing}\"}", user);

        Assert.AreEqual("/users/42/profile/{missing}", path);
        Assert.AreEqual("{\"firstName\":\"John\",\"missing\":\"{missing}\"}", body);
    }

    [TestMethod]
    public void Render_ReplacesEncodedUserName_WithoutExtraEncoding()
    {
        var user = new BatchUserContext
        {
            UserId = "john doe+admin",
            Properties = new Dictionary<string, string>
            {
                ["raw"] = "a b+c"
            }
        };

        var rendered = _service.Render("/users/{{encoded_user_name}}/{raw}", user);

        Assert.AreEqual("/users/john%20doe%2Badmin/a b+c", rendered);
    }

    [TestMethod]
    public void Render_HandlesNestedBracesAndEmptyValues_WithoutRecursiveExpansion()
    {
        var user = new BatchUserContext
        {
            UserId = "user",
            Properties = new Dictionary<string, string>
            {
                ["empty"] = string.Empty,
                ["token"] = "{inner}"
            }
        };

        var rendered = _service.Render("{{{token}}}-{empty}-{legacy_token}", user);

        Assert.AreEqual("{{{inner}}}--{legacy_token}", rendered);
    }
}
