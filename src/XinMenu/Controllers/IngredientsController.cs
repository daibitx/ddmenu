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
public class IngredientsController : ControllerBase
{
    private readonly IIngredientService _ingredientService;
    private readonly ILogger<IngredientsController> _logger;

    public IngredientsController(IIngredientService ingredientService, ILogger<IngredientsController> logger)
    {
        _ingredientService = ingredientService;
        _logger = logger;
    }

    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

    [HttpGet]
    [RAMAuthorize("Ingredient", "Read")]
    public async Task<OperateResult<List<IngredientGroupDto>>> GetList([FromQuery] IngredientQueryRequest request)
    {
        return await _ingredientService.GetGroupedAsync(request);
    }

    [HttpGet("{id:int}")]
    [RAMAuthorize("Ingredient", "ReadDetail")]
    public async Task<OperateResult<IngredientDto>> GetDetail(int id)
    {
        return await _ingredientService.GetByIdAsync(id);
    }

    [HttpPost]
    [RAMAuthorize("Ingredient", "Create")]
    public async Task<OperateResult<IngredientDto>> Create([FromBody] CreateIngredientRequest request)
    {
        var userId = CurrentUserId;
        return await _ingredientService.CreateAsync(request, userId);
    }

    [HttpPut("{id:int}")]
    [RAMAuthorize("Ingredient", "Update")]
    public async Task<OperateResult<IngredientDto>> Update(int id, [FromBody] UpdateIngredientRequest request)
    {
        return await _ingredientService.UpdateAsync(id, request);
    }

    [HttpDelete("{id:int}")]
    [RAMAuthorize("Ingredient", "Delete")]
    public async Task<OperateResult<bool>> Delete(int id)
    {
        return await _ingredientService.DeleteAsync(id);
    }
}
