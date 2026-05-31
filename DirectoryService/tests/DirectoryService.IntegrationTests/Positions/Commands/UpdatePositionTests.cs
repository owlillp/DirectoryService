using System.Net.Http.Json;
using DirectoryService.Contracts.Positions.Requests;
using DirectoryService.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Shared.Failures;
using Shared.HttpCommunication;

namespace DirectoryService.IntegrationTests.Positions.Commands;

public class UpdatePositionTests(IntegrationTestsWebFactory factory) : DirectoryServiceTestsBase(factory)
{
    [Fact]
    public async Task UpdatePosition_name_should_succeed()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var location = await CreateLocationAsync();
        var department = await CreateDepartmentAsync([location.Id.Value]);
        var position = await CreatePositionAsync([department.Id.Value], name: "old_name");

        string newNameValue = "new_name";
        var request = new UpdatePositionRequest(newNameValue);

        // act
        var updateResponse = await AppHttpClient.PatchAsJsonAsync($"/api/Positions/{position.Id.Value}", request, cancellationToken);
        var updateResult = await updateResponse.HandleResponseAsync(cancellationToken);

        // assert
        Assert.True(updateResult.IsSuccess);

        await ExecuteInDb(async dbContext =>
        {
            var updatedPosition = await dbContext.Positions
                .FirstOrDefaultAsync(p => p.Id == position.Id, cancellationToken);

            Assert.NotNull(updatedPosition);
            Assert.Equal(newNameValue, updatedPosition.Name.Value);
        });
    }

    [Fact]
    public async Task UpdatePosition_with_same_name_should_succeed()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var location = await CreateLocationAsync();
        var department = await CreateDepartmentAsync([location.Id.Value]);
        var position = await CreatePositionAsync([department.Id.Value], name: "same_name");

        var request = new UpdatePositionRequest("same_name");

        // act
        var updateResponse = await AppHttpClient.PatchAsJsonAsync($"/api/Positions/{position.Id.Value}", request, cancellationToken);
        var updateResult = await updateResponse.HandleResponseAsync(cancellationToken);

        // assert
        Assert.True(updateResult.IsSuccess);

        await ExecuteInDb(async dbContext =>
        {
            var updatedPosition = await dbContext.Positions
                .FirstOrDefaultAsync(p => p.Id == position.Id, cancellationToken);

            Assert.NotNull(updatedPosition);
            Assert.Equal("same_name", updatedPosition.Name.Value);
        });
    }

    [Fact]
    public async Task UpdatePosition_with_empty_name_should_fail()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var location = await CreateLocationAsync();
        var department = await CreateDepartmentAsync([location.Id.Value]);
        var position = await CreatePositionAsync([department.Id.Value]);

        var request = new UpdatePositionRequest(string.Empty);

        // act
        var updateResponse = await AppHttpClient.PatchAsJsonAsync($"/api/Positions/{position.Id.Value}", request, cancellationToken);
        var updateResult = await updateResponse.HandleResponseAsync(cancellationToken);

        // assert
        Assert.True(updateResult.IsFailure);
        Assert.NotNull(updateResult.Error);
        Assert.Contains(updateResult.Error, e => e.Type == ErrorType.VALIDATION);

        await ExecuteInDb(async dbContext =>
        {
            var positionAfterAttempt = await dbContext.Positions
                .FirstOrDefaultAsync(p => p.Id == position.Id, cancellationToken);

            Assert.NotNull(positionAfterAttempt);
            Assert.Equal(position.Name.Value, positionAfterAttempt.Name.Value);
        });
    }

    [Fact]
    public async Task UpdatePosition_with_invalid_name_length_should_fail()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var location = await CreateLocationAsync();
        var department = await CreateDepartmentAsync([location.Id.Value]);
        var position = await CreatePositionAsync([department.Id.Value]);

        var request = new UpdatePositionRequest("sh");

        // act
        var updateResponse = await AppHttpClient.PatchAsJsonAsync($"/api/Positions/{position.Id.Value}", request, cancellationToken);
        var updateResult = await updateResponse.HandleResponseAsync(cancellationToken);

        // assert
        Assert.True(updateResult.IsFailure);
        Assert.NotNull(updateResult.Error);
        Assert.Contains(updateResult.Error, e => e.Type == ErrorType.VALIDATION);
    }

    [Fact]
    public async Task UpdatePosition_non_existent_position_should_fail()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var request = new UpdatePositionRequest("new_name");

        // act
        var updateResponse = await AppHttpClient.PatchAsJsonAsync($"/api/Positions/{Guid.NewGuid()}", request, cancellationToken);
        var updateResult = await updateResponse.HandleResponseAsync(cancellationToken);

        // assert
        Assert.True(updateResult.IsFailure);
        Assert.NotNull(updateResult.Error);
        Assert.Contains(updateResult.Error, e => e.Type == ErrorType.NOT_FOUND);
    }

    [Fact]
    public async Task UpdatePosition_deleted_position_should_fail()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var location = await CreateLocationAsync();
        var department = await CreateDepartmentAsync([location.Id.Value]);
        var position = await CreatePositionAsync([department.Id.Value]);

        // soft delete position
        var deleteResponse = await AppHttpClient.DeleteAsync($"/api/Positions/{position.Id.Value}", cancellationToken);
        var deleteResult = await deleteResponse.HandleResponseAsync(cancellationToken);
        Assert.True(deleteResult.IsSuccess);

        var request = new UpdatePositionRequest("new_name");

        // act
        var updateResponse = await AppHttpClient.PatchAsJsonAsync($"/api/Positions/{position.Id.Value}", request, cancellationToken);
        var updateResult = await updateResponse.HandleResponseAsync(cancellationToken);

        // assert
        Assert.True(updateResult.IsFailure);
        Assert.NotNull(updateResult.Error);
        Assert.Contains(updateResult.Error, e => e.Type == ErrorType.NOT_FOUND);
    }

    [Fact]
    public async Task UpdatePosition_with_invalid_guid_should_fail()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var request = new UpdatePositionRequest("new_name");

        // act
        var updateResponse = await AppHttpClient.PatchAsJsonAsync($"/api/Positions/{Guid.Empty}", request, cancellationToken);
        var updateResult = await updateResponse.HandleResponseAsync(cancellationToken);

        // assert
        Assert.True(updateResult.IsFailure);
        Assert.NotNull(updateResult.Error);
        Assert.Contains(updateResult.Error, e => e.Type == ErrorType.VALIDATION);
    }
}