using FootballBlog.API.Common;
using FootballBlog.Core.DTOs;
using FootballBlog.Core.Interfaces;
using FootballBlog.Core.Models;
using FootballBlog.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FootballBlog.API.Controllers;

[ApiController]
[Route("api/admin/api-keys")]
[Authorize(Roles = "Admin")]
public class ApiKeysController(
    ApplicationDbContext dbContext,
    IApiKeyRotator keyRotator,
    ILogger<ApiKeysController> logger) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<ApiKeyDto>>>> GetAll()
    {
        var keys = await dbContext.ApiKeyConfigs
            .AsNoTracking()
            .OrderBy(k => k.Provider)
            .ThenBy(k => k.Priority)
            .Select(k => MapToDto(k))
            .ToListAsync();

        return Ok(ApiResponse<IEnumerable<ApiKeyDto>>.Ok(keys));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<ApiKeyDto>>> Create([FromBody] CreateApiKeyDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Provider) || string.IsNullOrWhiteSpace(dto.KeyValue))
        {
            return BadRequest(ApiResponse<ApiKeyDto>.Fail("Provider và KeyValue không được rỗng"));
        }

        var key = new ApiKeyConfig
        {
            Provider = dto.Provider,
            KeyValue = dto.KeyValue,
            Priority = dto.Priority,
            IsActive = true,
            DailyLimit = dto.DailyLimit,
            Note = dto.Note,
            CreatedAt = DateTime.UtcNow,
        };

        dbContext.ApiKeyConfigs.Add(key);
        await dbContext.SaveChangesAsync();
        await keyRotator.InvalidateCacheAsync(dto.Provider);

        logger.LogInformation("API key added for provider {Provider}", dto.Provider);
        return Ok(ApiResponse<ApiKeyDto>.Ok(MapToDto(key)));
    }

    [HttpPatch("{id:int}/toggle")]
    public async Task<ActionResult<ApiResponse<ApiKeyDto>>> Toggle(int id)
    {
        var key = await dbContext.ApiKeyConfigs.FindAsync(id);
        if (key is null)
        {
            return NotFound(ApiResponse<ApiKeyDto>.Fail("Key không tồn tại"));
        }

        key.IsActive = !key.IsActive;
        await dbContext.SaveChangesAsync();
        await keyRotator.InvalidateCacheAsync(key.Provider);

        logger.LogInformation("API key {Id} for {Provider} toggled to {State}", id, key.Provider, key.IsActive);
        return Ok(ApiResponse<ApiKeyDto>.Ok(MapToDto(key)));
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(int id)
    {
        var key = await dbContext.ApiKeyConfigs.FindAsync(id);
        if (key is null)
        {
            return NotFound(ApiResponse<bool>.Fail("Key không tồn tại"));
        }

        string provider = key.Provider;
        dbContext.ApiKeyConfigs.Remove(key);
        await dbContext.SaveChangesAsync();
        await keyRotator.InvalidateCacheAsync(provider);

        logger.LogInformation("API key {Id} for {Provider} deleted", id, provider);
        return Ok(ApiResponse<bool>.Ok(true));
    }

    private static ApiKeyDto MapToDto(ApiKeyConfig k)
    {
        string masked = k.KeyValue.Length > 4
            ? "****" + k.KeyValue[^4..]
            : "****";

        return new ApiKeyDto(k.Id, k.Provider, masked, k.Priority, k.IsActive, k.DailyLimit, k.Note, k.CreatedAt);
    }
}
