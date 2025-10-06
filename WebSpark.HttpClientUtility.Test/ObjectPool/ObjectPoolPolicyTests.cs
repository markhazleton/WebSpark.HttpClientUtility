using Microsoft.Extensions.ObjectPool;
using System.Text;
using WebSpark.HttpClientUtility.ObjectPool;

namespace WebSpark.HttpClientUtility.Test.ObjectPool;

[TestClass]
public class ObjectPoolPolicyTests
{
    [TestClass]
    public class HttpRequestMessagePoolPolicyTests
    {
        private HttpRequestMessagePoolPolicy _policy = null!;

        [TestInitialize]
        public void Setup()
        {
            _policy = new HttpRequestMessagePoolPolicy();
        }

        [TestMethod]
        public void Create_ReturnsNewHttpRequestMessage()
        {
            // Act
            var request = _policy.Create();

            // Assert
            Assert.IsNotNull(request);
            Assert.IsInstanceOfType(request, typeof(HttpRequestMessage));
            Assert.AreEqual(HttpMethod.Get, request.Method);
            Assert.IsNull(request.RequestUri);
        }

        [TestMethod]
        public void Return_ValidHttpRequestMessage_ReturnsTrue()
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Post, "https://example.com");
            request.Headers.Add("Custom-Header", "Value");
            request.Content = new StringContent("test content");

            // Act
            var result = _policy.Return(request);

            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual(HttpMethod.Get, request.Method); // Should be reset to default
            Assert.IsNull(request.RequestUri); // Should be cleared
            Assert.IsNull(request.Content); // Should be cleared
            Assert.AreEqual(0, request.Headers.Count()); // Headers should be cleared
        }

        [TestMethod]
        public void Return_NullHttpRequestMessage_ReturnsFalse()
        {
            // Act
            var result = _policy.Return(null!);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void Return_HttpRequestMessageWithContent_DisposesContent()
        {
            // Arrange
            var request = new HttpRequestMessage();
            var content = new StringContent("test");
            request.Content = content;

            // Act
            var result = _policy.Return(request);

            // Assert
            Assert.IsTrue(result);
            Assert.IsNull(request.Content);
        }

        [TestMethod]
        public void Return_HttpRequestMessageWithHeaders_ClearsHeaders()
        {
            // Arrange
            var request = new HttpRequestMessage();
            request.Headers.Add("X-Custom-Header", "Value");
            request.Headers.Add("X-Another-Header", "AnotherValue");

            // Act
            var result = _policy.Return(request);

            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual(0, request.Headers.Count());
        }

        [TestMethod]
        public void Return_HttpRequestMessageWithVersion_ResetsVersion()
        {
            // Arrange
            var request = new HttpRequestMessage();
            request.Version = new Version(2, 0);

            // Act
            var result = _policy.Return(request);

            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual(new Version(1, 1), request.Version);
        }

        [TestMethod]
        public void Return_MultipleOperations_WorksCorrectly()
        {
            // Arrange
            var request1 = _policy.Create();
            var request2 = _policy.Create();

            request1.Method = HttpMethod.Post;
            request1.RequestUri = new Uri("https://example.com");

            request2.Method = HttpMethod.Put;
            request2.Content = new StringContent("content");

            // Act
            var result1 = _policy.Return(request1);
            var result2 = _policy.Return(request2);

            // Assert
            Assert.IsTrue(result1);
            Assert.IsTrue(result2);

            // Both should be reset to defaults
            Assert.AreEqual(HttpMethod.Get, request1.Method);
            Assert.AreEqual(HttpMethod.Get, request2.Method);
            Assert.IsNull(request1.RequestUri);
            Assert.IsNull(request2.Content);
        }
    }

    [TestClass]
    public class StringBuilderPoolPolicyTests
    {
        [TestMethod]
        public void Constructor_DefaultParameters_CreatesPolicy()
        {
            // Act
            var policy = new StringBuilderPoolPolicy();

            // Assert
            Assert.IsNotNull(policy);
        }

        [TestMethod]
        public void Constructor_CustomParameters_CreatesPolicy()
        {
            // Act
            var policy = new StringBuilderPoolPolicy(512, 16384);

            // Assert
            Assert.IsNotNull(policy);
        }

        [TestMethod]
        public void Create_ReturnsNewStringBuilderWithInitialCapacity()
        {
            // Arrange
            var policy = new StringBuilderPoolPolicy(1024);

            // Act
            var sb = policy.Create();

            // Assert
            Assert.IsNotNull(sb);
            Assert.IsTrue(sb.Capacity >= 1024);
            Assert.AreEqual(0, sb.Length);
        }

        [TestMethod]
        public void Return_ValidStringBuilder_ClearsContentAndReturnsTrue()
        {
            // Arrange
            var policy = new StringBuilderPoolPolicy();
            var sb = new StringBuilder();
            sb.Append("test content");

            // Act
            var result = policy.Return(sb);

            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual(0, sb.Length);
        }

        [TestMethod]
        public void Return_NullStringBuilder_ReturnsFalse()
        {
            // Arrange
            var policy = new StringBuilderPoolPolicy();

            // Act
            var result = policy.Return(null!);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void Return_StringBuilderExceedsMaxCapacity_ReturnsFalse()
        {
            // Arrange
            var policy = new StringBuilderPoolPolicy(256, 1024); // maxCapacity = 1024
            var sb = new StringBuilder(2048); // Exceeds max capacity

            // Act
            var result = policy.Return(sb);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void Return_StringBuilderWithinMaxCapacity_ReturnsTrue()
        {
            // Arrange
            var policy = new StringBuilderPoolPolicy(256, 1024); // maxCapacity = 1024
            var sb = new StringBuilder(512); // Within max capacity
            sb.Append("some content");

            // Act
            var result = policy.Return(sb);

            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual(0, sb.Length);
            Assert.AreEqual(512, sb.Capacity); // Capacity should be preserved
        }

        [TestMethod]
        public void Return_StringBuilderAtMaxCapacity_ReturnsTrue()
        {
            // Arrange
            var policy = new StringBuilderPoolPolicy(256, 1024); // maxCapacity = 1024
            var sb = new StringBuilder(1024); // Exactly at max capacity

            // Act
            var result = policy.Return(sb);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void Return_StringBuilderCapacityGrowthScenario_HandlesCorrectly()
        {
            // Arrange
            var policy = new StringBuilderPoolPolicy(256, 1024);
            var sb = new StringBuilder(256);
            
            // Grow the StringBuilder beyond max capacity
            var largeString = new string('x', 2000);
            sb.Append(largeString);

            // Act
            var result = policy.Return(sb);

            // Assert
            Assert.IsFalse(result); // Should reject due to capacity growth
        }

        [TestMethod]
        public void PoolingScenario_CreateReturnCreate_WorksCorrectly()
        {
            // Arrange
            var policy = new StringBuilderPoolPolicy(256, 1024);

            // Act
            var sb1 = policy.Create();
            sb1.Append("test content");
            
            var returnResult = policy.Return(sb1);
            
            var sb2 = policy.Create();

            // Assert
            Assert.IsTrue(returnResult);
            Assert.AreEqual(0, sb1.Length); // Should be cleared
            Assert.IsNotNull(sb2);
            Assert.AreEqual(0, sb2.Length); // New instance should be empty
        }
    }
}