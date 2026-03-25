---
paths:
  - "**/*.Tests/**"
  - "**/*Tests.cs"
  - "**/*Test.cs"
---

# Testing Rules

## Thư viện
- Unit test: xUnit + FluentAssertions + NSubstitute (mock)
- Integration test: xUnit + WebApplicationFactory + Testcontainers (real PostgreSQL)

## Cấu trúc
```
FootballBlog.Tests/
├── Unit/
│   ├── Services/PostServiceTests.cs
│   └── Validators/CreatePostValidatorTests.cs
└── Integration/
    ├── Api/PostsEndpointTests.cs
    └── Fixtures/DatabaseFixture.cs
```

## Naming Convention
```csharp
// Method_Scenario_ExpectedResult
[Fact]
public async Task GetPostBySlug_WhenSlugExists_ReturnsPost() { }

[Fact]
public async Task GetPostBySlug_WhenSlugNotFound_ReturnsNull() { }
```

## Unit Test Pattern (Arrange-Act-Assert)
```csharp
[Fact]
public async Task CreatePost_ValidData_ReturnsCreatedPost()
{
    // Arrange
    var mockRepo = Substitute.For<IPostRepository>();
    var service = new PostService(mockRepo, _logger);
    var command = new CreatePostCommand { Title = "Test", Slug = "test" };

    // Act
    var result = await service.CreatePostAsync(command);

    // Assert
    result.Should().NotBeNull();
    result.Slug.Should().Be("test");
    await mockRepo.Received(1).AddAsync(Arg.Any<Post>());
}
```

## Integration Test — dùng real DB
- KHÔNG mock DbContext trong integration test
- Dùng Testcontainers để spin up PostgreSQL container
- Reset DB state giữa các test (transactions hoặc truncate)

## Nguyên tắc
- Mỗi test chỉ assert 1 behavior
- Test phải độc lập, không phụ thuộc thứ tự chạy
- Không test EF Core internals — test behavior của service
