---
paths:
  - "**/*.Tests/**"
  - "**/*Tests.cs"
  - "**/*Test.cs"
---

# Testing

- Unit: xUnit + FluentAssertions + NSubstitute | Integration: xUnit + WebApplicationFactory + Testcontainers (real PostgreSQL)
- Method naming: `MethodName_Scenario_ExpectedResult` (e.g., `GetPostBySlug_WhenNotFound_ReturnsNull`)
- Pattern: Arrange / Act / Assert | 1 behavior per test | test độc lập, không phụ thuộc thứ tự

## Unit test
```csharp
var mockUow = Substitute.For<IUnitOfWork>();
var service = new PostService(mockUow, NullLogger<PostService>.Instance);
var result = await service.GetBySlugAsync("test");
result.Should().BeNull();
await mockUow.Posts.Received(1).GetBySlugAsync("test");
```

## Integration test
- KHÔNG mock DbContext | Testcontainers spin up PostgreSQL | reset DB state giữa tests
