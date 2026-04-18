using FootballBlog.API.Common;
using FootballBlog.Core.DTOs;
using FootballBlog.Core.Interfaces;
using FootballBlog.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FootballBlog.API.Controllers;

[ApiController]
[Route("api/admin/prompts")]
[Authorize(Roles = "Admin")]
public class AdminPromptsController(
    IUnitOfWork uow,
    ILogger<AdminPromptsController> logger) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<PromptTemplateDto>>>> GetAll()
    {
        var items = await uow.PromptTemplates.GetAllAsync();
        var dtos = items
            .OrderByDescending(t => t.UpdatedAt)
            .Select(MapToDto);
        return Ok(ApiResponse<IEnumerable<PromptTemplateDto>>.Ok(dtos));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<PromptTemplateDto>>> GetById(int id)
    {
        var item = await uow.PromptTemplates.GetByIdAsync(id);
        if (item is null)
        {
            return NotFound(ApiResponse<PromptTemplateDto>.Fail("Không tìm thấy template"));
        }

        return Ok(ApiResponse<PromptTemplateDto>.Ok(MapToDto(item)));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<PromptTemplateDto>>> Create([FromBody] CreatePromptTemplateDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name) || string.IsNullOrWhiteSpace(dto.Content))
        {
            return BadRequest(ApiResponse<PromptTemplateDto>.Fail("Name và Content không được rỗng"));
        }

        var now = DateTime.UtcNow;
        var template = new PromptTemplate
        {
            Name = dto.Name,
            Provider = dto.Provider,
            Content = dto.Content,
            IsActive = dto.IsActive,
            CreatedAt = now,
            UpdatedAt = now
        };

        await uow.PromptTemplates.AddAsync(template);
        await uow.CommitAsync();

        logger.LogInformation("Prompt template '{Name}' created", template.Name);
        return Ok(ApiResponse<PromptTemplateDto>.Ok(MapToDto(template)));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse<PromptTemplateDto>>> Update(int id, [FromBody] CreatePromptTemplateDto dto)
    {
        var template = await uow.PromptTemplates.GetByIdAsync(id);
        if (template is null)
        {
            return NotFound(ApiResponse<PromptTemplateDto>.Fail("Không tìm thấy template"));
        }

        template.Name = dto.Name;
        template.Provider = dto.Provider;
        template.Content = dto.Content;
        template.IsActive = dto.IsActive;
        template.UpdatedAt = DateTime.UtcNow;

        await uow.PromptTemplates.UpdateAsync(template);
        await uow.CommitAsync();

        logger.LogInformation("Prompt template {Id} updated", id);
        return Ok(ApiResponse<PromptTemplateDto>.Ok(MapToDto(template)));
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(int id)
    {
        var template = await uow.PromptTemplates.GetByIdAsync(id);
        if (template is null)
        {
            return NotFound(ApiResponse<bool>.Fail("Không tìm thấy template"));
        }

        await uow.PromptTemplates.DeleteAsync(template);
        await uow.CommitAsync();

        logger.LogInformation("Prompt template {Id} deleted", id);
        return Ok(ApiResponse<bool>.Ok(true));
    }

    private static PromptTemplateDto MapToDto(PromptTemplate t) =>
        new(t.Id, t.Name, t.Provider, t.Content, t.IsActive, t.CreatedAt, t.UpdatedAt);
}
