using Xunit;

namespace LogoDesignPortal.API.IntegrationTests;

/// <summary>
/// One shared API host + in-memory database per test run. Prevents parallel cross-talk and static TestDataIds races.
/// </summary>
[CollectionDefinition("Integration")]
public class IntegrationCollection : ICollectionFixture<TestWebApplicationFactory>
{
}
