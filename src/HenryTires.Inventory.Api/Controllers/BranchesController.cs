using HenryTires.Inventory.Application.Common;
using HenryTires.Inventory.Application.DTOs;
using HenryTires.Inventory.Application.Ports;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HenryTires.Inventory.Api.Controllers;

[ApiController]
[Route("api/branches")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class BranchesController : ControllerBase
{
    private readonly IBranchRepository _branchRepository;

    public BranchesController(IBranchRepository branchRepository)
    {
        _branchRepository = branchRepository;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<BranchDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<BranchDto>>>> GetAll()
    {
        var branches = await _branchRepository.GetAllAsync();
        var dtos = branches.Select(b => new BranchDto
        {
            Id = b.Id,
            Code = b.Code,
            Name = b.Name,
        });

        return Ok(ApiResponse<IEnumerable<BranchDto>>.SuccessResponse(dtos));
    }
}
