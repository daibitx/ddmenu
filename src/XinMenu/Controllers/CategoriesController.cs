using Daibitx.AspNetCore.Utils.Models;
using Daibitx.Identity.Core.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using XinMenu.DTOs;
using XinMenu.Services.Abstractions;

namespace XinMenu.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;
    private readonly ILogger<CategoriesController> _logger;

    public CategoriesController(ICategoryService categoryService, ILogger<CategoriesController> logger)
    {
        _categoryService = categoryService;
        _logger = logger;
    }

    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

    [HttpGet]
    [AllowAnonymous]
    public async Task<OperateResult<List<CategoryDto>>> GetList()
    {
        return await _categoryService.GetAllAsync();
    }

    [HttpGet("{id:int}")]
    [RAMAuthorize("Category", "ReadDetail")]
    public async Task<OperateResult<CategoryDto>> GetDetail(int id)
    {
        return await _categoryService.GetByIdAsync(id);
    }

    [HttpPost]
    [RAMAuthorize("Category", "Create")]
    public async Task<OperateResult<CategoryDto>> Create([FromBody] CreateCategoryRequest request)
    {
        var userId = CurrentUserId;
        return await _categoryService.CreateAsync(request, userId);
    }

    [HttpPut("{id:int}")]
    [RAMAuthorize("Category", "Update")]
    public async Task<OperateResult<CategoryDto>> Update(int id, [FromBody] UpdateCategoryRequest request)
    {
        return await _categoryService.UpdateAsync(id, request);
    }

    [HttpDelete("{id:int}")]
    [RAMAuthorize("Category", "Delete")]
    public async Task<OperateResult<bool>> Delete(int id)
    {
        return await _categoryService.DeleteAsync(id);
    }
}
