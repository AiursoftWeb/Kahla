using Aiursoft.Kahla.Server;

namespace Aiursoft.Kahla.Tests.ServiceTests;

[TestClass]
public class ExtensionTests
{
    [TestMethod]
    public void SkipUntilEquals_ShouldSkipUntilTargetIsFound()
    {
        // Arrange
        var input = new List<int> { 1, 2, 3, 4, 5 };
        int target = 3;

        // Act
        var result = input.SkipUntilEquals(target).ToList();

        // Assert
        var expected = new List<int> { 4, 5 };
        CollectionAssert.AreEqual(expected, result);
    }

    [TestMethod]
    public void SkipUntilEquals_ShouldReturnAllIfTargetIsNotFound()
    {
        // Arrange
        var input = new List<int> { 1, 2, 3, 4, 5 };
        int target = 6;

        // Act
        var result = input.SkipUntilEquals(target).ToList();

        // Assert
        var expected = new List<int>
        {
            Capacity = 0
        };
        CollectionAssert.AreEqual(expected, result);
    }

    [TestMethod]
    public void SkipUntilEquals_ShouldReturnEmptyIfSourceIsEmpty()
    {
        var target = 3;

        // Act
        var result = new List<int>().SkipUntilEquals(target).ToList();

        // Assert
        var expected = new List<int>();
        CollectionAssert.AreEqual(expected, result);
    }

    [TestMethod]
    public void SkipUntilEquals_ShouldReturnEmptyIfTargetIsFirstElement()
    {
        // Arrange
        var input = new List<int> { 3, 4, 5 };
        int target = 3;

        // Act
        var result = input.SkipUntilEquals(target).ToList();

        // Assert
        var expected = new List<int> { 4, 5 };
        CollectionAssert.AreEqual(expected, result);
    }
    
    [TestMethod]
    public void SkipUntilEquals_ShouldReturnAllIfTargetIsNull()
    {
        // Arrange
        var input = new List<int> { 3, 4, 5 };
        int? target = null;

        // Act
        var result = input.SkipUntilEquals(target).ToList();

        // Assert
        var expected = new List<int> { 3, 4, 5 };
        CollectionAssert.AreEqual(expected, result);
    }
}