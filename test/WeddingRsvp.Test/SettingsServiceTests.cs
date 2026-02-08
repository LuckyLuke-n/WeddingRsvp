using System.Net;
using Microsoft.Extensions.Logging;
using Moq;
using WeddingRsvp.Api.Repository;
using WeddingRsvp.Api.Repository.Entities;
using WeddingRsvp.Api.Repository.Generic;
using WeddingRsvp.Api.Services;

namespace WeddingRsvp.Test;

public class SettingsServiceTests
{
    private Mock<ISettingsRepository> RepoMock { get; }
    private Mock<ILogger<SettingsService>> LoggerMock { get; }
    private SettingsService Service { get; }
    private Guid SettingsId => SettingsService.SettingsId;

    public SettingsServiceTests()
    {
        RepoMock = new Mock<ISettingsRepository>();
        LoggerMock = new Mock<ILogger<SettingsService>>();
        Service = new SettingsService(RepoMock.Object, LoggerMock.Object);
    }

    [Fact]
    public async Task GetAsync_WhenReadSuccess_ReturnsSettings()
    {
        var settings = new Settings();
        RepoMock.Setup(r => r.ReadAsync(SettingsId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(RepositoryResponse<Settings, RepositoryFailResponse>.CreateSuccess(settings));

        var result = await Service.GetAsync();

        Assert.True(result.IsSuccess);
        Assert.Same(settings, result.ValueSuccess);
        RepoMock.Verify(r => r.ReadAsync(SettingsId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAsync_WhenReadNotFound_CreatesSettings()
    {
        var readFail = new RepositoryFailResponse { StatusCode = HttpStatusCode.NotFound, Message = "missing" };
        RepoMock.Setup(r => r.ReadAsync(SettingsId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(RepositoryResponse<Settings, RepositoryFailResponse>.CreateFail(readFail));

        var created = new Settings();
        RepoMock.Setup(r => r.CreateAsync(It.IsAny<Settings>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(RepositoryResponse<Settings, RepositoryFailResponse>.CreateSuccess(created));

        var result = await Service.GetAsync();

        Assert.True(result.IsSuccess);
        Assert.Same(created, result.ValueSuccess);
        RepoMock.Verify(r => r.ReadAsync(SettingsId, It.IsAny<CancellationToken>()), Times.Once);
        RepoMock.Verify(r => r.CreateAsync(It.IsAny<Settings>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAsync_WhenCreateFails_ReturnsFail()
    {
        var readFail = new RepositoryFailResponse { StatusCode = HttpStatusCode.NotFound, Message = "missing" };
        RepoMock.Setup(r => r.ReadAsync(SettingsId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(RepositoryResponse<Settings, RepositoryFailResponse>.CreateFail(readFail));

        var createFail = new RepositoryFailResponse { StatusCode = HttpStatusCode.BadRequest, Message = "bad" };
        RepoMock.Setup(r => r.CreateAsync(It.IsAny<Settings>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(RepositoryResponse<Settings, RepositoryFailResponse>.CreateFail(createFail));

        var result = await Service.GetAsync();

        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        RepoMock.Verify(r => r.ReadAsync(SettingsId, It.IsAny<CancellationToken>()), Times.Once);
        RepoMock.Verify(r => r.CreateAsync(It.IsAny<Settings>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAsync_WhenReadFailsOther_ReturnsFail()
    {
        var readFail = new RepositoryFailResponse { StatusCode = HttpStatusCode.InternalServerError, Message = "boom" };
        RepoMock.Setup(r => r.ReadAsync(SettingsId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(RepositoryResponse<Settings, RepositoryFailResponse>.CreateFail(readFail));

        var result = await Service.GetAsync();

        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.InternalServerError, result.StatusCode);
        RepoMock.Verify(r => r.ReadAsync(SettingsId, It.IsAny<CancellationToken>()), Times.Once);
        RepoMock.Verify(r => r.CreateAsync(It.IsAny<Settings>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpsertAsync_SetsIdAndReturnsSuccess()
    {
        RepoMock.Setup(r => r.UpdateAsync(It.IsAny<Settings>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(RepositoryResponse<Settings, RepositoryFailResponse>.CreateSuccess(new Settings()));

        var input = new Settings();

        var result = await Service.UpsertAsync(input);

        Assert.True(result.IsSuccess);
        Assert.Equal(SettingsId.ToString(), input.Id);
        RepoMock.Verify(r => r.UpdateAsync(It.Is<Settings>(s => s.Id == SettingsId.ToString()), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpsertAsync_WhenUpdateFails_ReturnsFail()
    {
        var updateFail = new RepositoryFailResponse { StatusCode = HttpStatusCode.InternalServerError, Message = "boom" };
        RepoMock.Setup(r => r.UpdateAsync(It.IsAny<Settings>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(RepositoryResponse<Settings, RepositoryFailResponse>.CreateFail(updateFail));

        var input = new Settings();

        var result = await Service.UpsertAsync(input);

        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.InternalServerError, result.StatusCode);
        Assert.Equal(SettingsId.ToString(), input.Id);
        RepoMock.Verify(r => r.UpdateAsync(It.IsAny<Settings>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
