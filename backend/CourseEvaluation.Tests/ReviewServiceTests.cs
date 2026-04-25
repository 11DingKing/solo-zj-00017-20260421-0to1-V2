using CourseEvaluation.Core.DTOs;
using CourseEvaluation.Core.Entities;
using CourseEvaluation.Core.Interfaces;
using CourseEvaluation.Core.Services;
using FluentAssertions;
using Moq;
using Xunit;

namespace CourseEvaluation.Tests;

public class ReviewServiceTests
{
    private readonly Mock<IReviewRepository> _reviewRepositoryMock;
    private readonly Mock<ICourseRepository> _courseRepositoryMock;
    private readonly ReviewService _reviewService;

    public ReviewServiceTests()
    {
        _reviewRepositoryMock = new Mock<IReviewRepository>();
        _courseRepositoryMock = new Mock<ICourseRepository>();
        _reviewService = new ReviewService(
            _reviewRepositoryMock.Object,
            _courseRepositoryMock.Object);
    }

    #region Rating Validation Tests

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    [InlineData(-1)]
    public async Task CreateOrUpdateReviewAsync_InvalidRating_ThrowsArgumentException(int invalidRating)
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new CreateReviewRequest(Guid.NewGuid(), invalidRating, "Test");

        // Act
        var act = async () => await _reviewService.CreateOrUpdateReviewAsync(userId, request);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("评分必须在1到5星之间");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(5)]
    public async Task CreateOrUpdateReviewAsync_ValidRating_DoesNotThrow(int validRating)
    {
        // Arrange
        var userId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var request = new CreateReviewRequest(courseId, validRating, "Test");

        _courseRepositoryMock
            .Setup(r => r.GetByIdAsync(courseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Course { Id = courseId, Name = "Test Course" });

        _reviewRepositoryMock
            .Setup(r => r.GetByUserAndCourseAsync(userId, courseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Review?)null);

        _reviewRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Review>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Review r) => r);

        // Act
        var act = async () => await _reviewService.CreateOrUpdateReviewAsync(userId, request);

        // Assert
        await act.Should().NotThrowAsync<ArgumentException>();
    }

    #endregion

    #region Duplicate Review Detection Tests

    [Fact]
    public async Task CreateOrUpdateReviewAsync_ExistingReview_UpdatesInsteadOfCreates()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var existingReviewId = Guid.NewGuid();
        
        var existingReview = new Review
        {
            Id = existingReviewId,
            UserId = userId,
            CourseId = courseId,
            Rating = 3,
            Content = "Old content",
            CreatedAt = DateTime.UtcNow.AddDays(-5)
        };

        var request = new CreateReviewRequest(courseId, 5, "New updated content");

        _courseRepositoryMock
            .Setup(r => r.GetByIdAsync(courseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Course { Id = courseId, Name = "Test Course" });

        _reviewRepositoryMock
            .Setup(r => r.GetByUserAndCourseAsync(userId, courseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingReview);

        _reviewRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Review>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _reviewRepositoryMock
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _reviewService.CreateOrUpdateReviewAsync(userId, request);

        // Assert
        result.Rating.Should().Be(5);
        result.Content.Should().Be("New updated content");
        result.Id.Should().Be(existingReviewId);
        
        _reviewRepositoryMock.Verify(
            r => r.UpdateAsync(It.Is<Review>(rev => 
                rev.Rating == 5 && 
                rev.Content == "New updated content"),
                It.IsAny<CancellationToken>()),
            Times.Once);
        
        _reviewRepositoryMock.Verify(
            r => r.AddAsync(It.IsAny<Review>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateOrUpdateReviewAsync_NoExistingReview_CreatesNewReview()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var request = new CreateReviewRequest(courseId, 5, "Test content");

        _courseRepositoryMock
            .Setup(r => r.GetByIdAsync(courseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Course { Id = courseId, Name = "Test Course" });

        _reviewRepositoryMock
            .Setup(r => r.GetByUserAndCourseAsync(userId, courseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Review?)null);

        _reviewRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Review>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Review r) => r);

        _reviewRepositoryMock
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _reviewService.CreateOrUpdateReviewAsync(userId, request);

        // Assert
        result.Rating.Should().Be(5);
        result.Content.Should().Be("Test content");
        
        _reviewRepositoryMock.Verify(
            r => r.AddAsync(It.IsAny<Review>(), It.IsAny<CancellationToken>()),
            Times.Once);
        
        _reviewRepositoryMock.Verify(
            r => r.UpdateAsync(It.IsAny<Review>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    #endregion

    #region Rating Distribution Tests

    [Fact]
    public void GetRatingDistribution_EmptyReviews_ReturnsAllZeros()
    {
        // Arrange
        var reviews = new List<Review>();

        // Act
        var result = _reviewService.GetRatingDistribution(reviews);

        // Assert
        result.Star1.Should().Be(0);
        result.Star2.Should().Be(0);
        result.Star3.Should().Be(0);
        result.Star4.Should().Be(0);
        result.Star5.Should().Be(0);
    }

    [Fact]
    public void GetRatingDistribution_MultipleReviews_ReturnsCorrectCounts()
    {
        // Arrange
        var reviews = new List<Review>
        {
            new() { Rating = 1 },
            new() { Rating = 1 },
            new() { Rating = 2 },
            new() { Rating = 3 },
            new() { Rating = 3 },
            new() { Rating = 3 },
            new() { Rating = 4 },
            new() { Rating = 5 },
            new() { Rating = 5 },
            new() { Rating = 5 }
        };

        // Act
        var result = _reviewService.GetRatingDistribution(reviews);

        // Assert
        result.Star1.Should().Be(2);
        result.Star2.Should().Be(1);
        result.Star3.Should().Be(3);
        result.Star4.Should().Be(1);
        result.Star5.Should().Be(4);
    }

    #endregion

    #region Permission Tests

    [Fact]
    public async Task CanDeleteReviewAsync_Admin_ReturnsTrue()
    {
        // Arrange
        var reviewId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        
        _reviewRepositoryMock
            .Setup(r => r.GetByIdAsync(reviewId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Review { Id = reviewId, UserId = Guid.NewGuid() });

        // Act
        var result = await _reviewService.CanDeleteReviewAsync(reviewId, userId, isAdmin: true);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task CanDeleteReviewAsync_OwnerButNotAdmin_ReturnsTrue()
    {
        // Arrange
        var reviewId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        
        _reviewRepositoryMock
            .Setup(r => r.GetByIdAsync(reviewId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Review { Id = reviewId, UserId = userId });

        // Act
        var result = await _reviewService.CanDeleteReviewAsync(reviewId, userId, isAdmin: false);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task CanDeleteReviewAsync_NotOwnerNotAdmin_ReturnsFalse()
    {
        // Arrange
        var reviewId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        
        _reviewRepositoryMock
            .Setup(r => r.GetByIdAsync(reviewId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Review { Id = reviewId, UserId = otherUserId });

        // Act
        var result = await _reviewService.CanDeleteReviewAsync(reviewId, userId, isAdmin: false);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task CanDeleteReviewAsync_NonExistentReview_ReturnsFalse()
    {
        // Arrange
        var reviewId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        
        _reviewRepositoryMock
            .Setup(r => r.GetByIdAsync(reviewId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Review?)null);

        // Act
        var result = await _reviewService.CanDeleteReviewAsync(reviewId, userId, isAdmin: true);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region Course Exists Tests

    [Fact]
    public async Task CreateOrUpdateReviewAsync_NonExistentCourse_ThrowsKeyNotFoundException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var request = new CreateReviewRequest(courseId, 5, "Test");

        _courseRepositoryMock
            .Setup(r => r.GetByIdAsync(courseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Course?)null);

        // Act
        var act = async () => await _reviewService.CreateOrUpdateReviewAsync(userId, request);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("课程不存在");
    }

    #endregion
}
