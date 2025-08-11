using System;
using System.Threading.Tasks;
using NUnit.Framework;
using TaxCalculator.Services.Services;
using TaxCalculator.Services.Interfaces;

namespace TaxCalculator.Tests.Unit.Services
{
    [TestFixture]
    public class CacheServiceTests
    {
        private ICacheService _cacheService;

        [SetUp]
        public void Setup()
        {
            _cacheService = new CacheService();
        }

        [Test]
        public async Task SetAsync_ValidKeyValue_StoresSuccessfully()
        {
            // Arrange
            var key = "test-key";
            var value = "test-value";
            var expiration = TimeSpan.FromMinutes(5);

            // Act
            await _cacheService.SetAsync(key, value, expiration);

            // Assert
            var retrieved = await _cacheService.GetAsync<string>(key);
            Assert.That(retrieved, Is.EqualTo(value));
        }

        [Test]
        public async Task GetAsync_NonExistentKey_ReturnsNull()
        {
            // Arrange
            var nonExistentKey = "non-existent-key";

            // Act
            var result = await _cacheService.GetAsync<string>(nonExistentKey);

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task GetAsync_ExpiredItem_ReturnsNull()
        {
            // Arrange
            var key = "expired-key";
            var value = "test-value";
            var expiration = TimeSpan.FromMilliseconds(50); // Very short expiration

            // Act
            await _cacheService.SetAsync(key, value, expiration);
            
            // Wait for expiration
            await Task.Delay(100);
            
            var result = await _cacheService.GetAsync<string>(key);

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task GetAsync_ValidItem_ReturnsCorrectValue()
        {
            // Arrange
            var key = "valid-key";
            var value = "test-value";
            var expiration = TimeSpan.FromMinutes(5);

            // Act
            await _cacheService.SetAsync(key, value, expiration);
            var result = await _cacheService.GetAsync<string>(key);

            // Assert
            Assert.That(result, Is.EqualTo(value));
        }

        [Test]
        public async Task SetAsync_OverwriteExistingKey_UpdatesValue()
        {
            // Arrange
            var key = "update-key";
            var originalValue = "original-value";
            var updatedValue = "updated-value";
            var expiration = TimeSpan.FromMinutes(5);

            // Act
            await _cacheService.SetAsync(key, originalValue, expiration);
            await _cacheService.SetAsync(key, updatedValue, expiration);
            var result = await _cacheService.GetAsync<string>(key);

            // Assert
            Assert.That(result, Is.EqualTo(updatedValue));
        }

        [Test]
        public async Task RemoveAsync_ExistingKey_RemovesItem()
        {
            // Arrange
            var key = "remove-key";
            var value = "test-value";
            var expiration = TimeSpan.FromMinutes(5);

            // Act
            await _cacheService.SetAsync(key, value, expiration);
            await _cacheService.RemoveAsync(key);
            var result = await _cacheService.GetAsync<string>(key);

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task RemoveAsync_NonExistentKey_DoesNotThrow()
        {
            // Arrange
            var nonExistentKey = "non-existent-key";

            // Act & Assert
            Assert.DoesNotThrowAsync(async () => await _cacheService.RemoveAsync(nonExistentKey));
        }

        [Test]
        public async Task SetAsync_ComplexObject_StoresAndRetrievesCorrectly()
        {
            // Arrange
            var key = "complex-object-key";
            var value = new TestComplexObject
            {
                Id = Guid.NewGuid(),
                Name = "Test Object",
                Value = 42.5m,
                CreatedDate = DateTime.UtcNow
            };
            var expiration = TimeSpan.FromMinutes(5);

            // Act
            await _cacheService.SetAsync(key, value, expiration);
            var result = await _cacheService.GetAsync<TestComplexObject>(key);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(value.Id));
            Assert.That(result.Name, Is.EqualTo(value.Name));
            Assert.That(result.Value, Is.EqualTo(value.Value));
            Assert.That(result.CreatedDate, Is.EqualTo(value.CreatedDate));
        }

        [Test]
        public async Task GetAsync_WrongType_ReturnsNull()
        {
            // Arrange
            var key = "type-mismatch-key";
            var value = "string-value";
            var expiration = TimeSpan.FromMinutes(5);

            // Act
            await _cacheService.SetAsync(key, value, expiration);
            var result = await _cacheService.GetAsync<TestComplexObject>(key);

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task SetAsync_NullValue_StoresNull()
        {
            // Arrange
            var key = "null-value-key";
            string value = null;
            var expiration = TimeSpan.FromMinutes(5);

            // Act
            await _cacheService.SetAsync(key, value, expiration);
            var result = await _cacheService.GetAsync<string>(key);

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task SetAsync_ZeroExpiration_ItemExpiresImmediately()
        {
            // Arrange
            var key = "zero-expiration-key";
            var value = "test-value";
            var expiration = TimeSpan.Zero;

            // Act
            await _cacheService.SetAsync(key, value, expiration);
            
            // Small delay to ensure the item has expired
            await Task.Delay(1);
            
            var result = await _cacheService.GetAsync<string>(key);

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task SetAsync_LongExpiration_ItemPersists()
        {
            // Arrange
            var key = "long-expiration-key";
            var value = "test-value";
            var expiration = TimeSpan.FromHours(1);

            // Act
            await _cacheService.SetAsync(key, value, expiration);
            var result = await _cacheService.GetAsync<string>(key);

            // Assert
            Assert.That(result, Is.EqualTo(value));
        }

        [Test]
        public async Task CacheService_ConcurrentAccess_HandlesCorrectly()
        {
            // Arrange
            var tasks = new Task[10];
            var baseKey = "concurrent-key-";

            // Act
            for (int i = 0; i < 10; i++)
            {
                var index = i;
                tasks[i] = Task.Run(async () =>
                {
                    var key = baseKey + index;
                    var value = "value-" + index;
                    await _cacheService.SetAsync(key, value, TimeSpan.FromMinutes(5));
                    
                    var retrieved = await _cacheService.GetAsync<string>(key);
                    Assert.That(retrieved, Is.EqualTo(value));
                });
            }

            // Assert
            Assert.DoesNotThrowAsync(async () => await Task.WhenAll(tasks));
        }

        [Test]
        public async Task GetAsync_AfterPartialExpiration_OnlyReturnsValidItems()
        {
            // Arrange
            var shortKey = "short-expiry-key";
            var longKey = "long-expiry-key";
            var value = "test-value";

            // Act
            await _cacheService.SetAsync(shortKey, value, TimeSpan.FromMilliseconds(50));
            await _cacheService.SetAsync(longKey, value, TimeSpan.FromMinutes(5));

            // Wait for short expiry
            await Task.Delay(100);

            var shortResult = await _cacheService.GetAsync<string>(shortKey);
            var longResult = await _cacheService.GetAsync<string>(longKey);

            // Assert
            Assert.That(shortResult, Is.Null);
            Assert.That(longResult, Is.EqualTo(value));
        }

        private class TestComplexObject
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
            public decimal Value { get; set; }
            public DateTime CreatedDate { get; set; }
        }
    }
}
