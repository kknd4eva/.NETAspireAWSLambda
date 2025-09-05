using Amazon.DynamoDBv2.DataModel;
using FluentAssertions;
using System.Reflection;
using static AspireLambda.Functions;

namespace AspireLambda.UnitTests.Models;

public class AccountsTests
{
    [Fact]
    public void Accounts_ShouldHaveCorrectTableAttribute()
    {
        // Arrange
        var accountsType = typeof(Accounts);

        // Act
        var tableAttribute = accountsType.GetCustomAttribute<DynamoDBTableAttribute>();

        // Assert
        tableAttribute.Should().NotBeNull();
        tableAttribute!.TableName.Should().Be("Accounts");
    }

    [Fact]
    public void Accounts_Id_ShouldHaveCorrectHashKeyAttribute()
    {
        // Arrange
        var idProperty = typeof(Accounts).GetProperty("Id");

        // Act
        var hashKeyAttribute = idProperty?.GetCustomAttribute<DynamoDBHashKeyAttribute>();

        // Assert
        hashKeyAttribute.Should().NotBeNull();
        hashKeyAttribute!.AttributeName.Should().Be("Id");
    }

    [Fact]
    public void Accounts_ShouldHaveRequiredProperties()
    {
        // Arrange
        var accountsType = typeof(Accounts);

        // Act
        var idProperty = accountsType.GetProperty("Id");
        var nameProperty = accountsType.GetProperty("Name");
        var addressProperty = accountsType.GetProperty("Address");

        // Assert
        idProperty.Should().NotBeNull();
        nameProperty.Should().NotBeNull();
        addressProperty.Should().NotBeNull();

        idProperty!.PropertyType.Should().Be(typeof(string));
        nameProperty!.PropertyType.Should().Be(typeof(string));
        addressProperty!.PropertyType.Should().Be(typeof(string));
    }

    [Fact]
    public void Accounts_ShouldAllowSettingProperties()
    {
        // Arrange
        var account = new Accounts();

        // Act
        account.Id = "test-id";
        account.Name = "Test Account";
        account.Address = "123 Test Street";

        // Assert
        account.Id.Should().Be("test-id");
        account.Name.Should().Be("Test Account");
        account.Address.Should().Be("123 Test Street");
    }
}