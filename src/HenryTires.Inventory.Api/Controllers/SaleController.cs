using HenryTires.Inventory.Application.Common;
using HenryTires.Inventory.Application.DTOs;
using HenryTires.Inventory.Application.Ports;
using HenryTires.Inventory.Application.Ports.Inbound;
using HenryTires.Inventory.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HenryTires.Inventory.Api.Controllers;

[ApiController]
[Route("api/v1/sale")]
[Authorize]
public class SaleController : ControllerBase
{
    private readonly ISaleService _saleService;
    private readonly ICurrentUserService _currentUser;

    public SaleController(ISaleService saleService, ICurrentUserService currentUser)
    {
        _saleService = saleService;
        _currentUser = currentUser;
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<SaleDto>>> CreateSale(
        [FromBody] CreateSaleRequest request
    )
    {
        // Non-admin users can only create sales for their assigned branches
        if (_currentUser.RoleCodes != null && !_currentUser.RoleCodes.Contains("ADMIN"))
        {
            var branchCodes = _currentUser.BranchCodes;
            if (branchCodes == null || branchCodes.Count == 0)
            {
                return StatusCode(
                    403,
                    ApiResponse<object>.ErrorResponse("User must have a branch assigned")
                );
            }
            if (
                !string.IsNullOrEmpty(request.BranchCode)
                && !branchCodes.Contains(request.BranchCode)
            )
            {
                return StatusCode(
                    403,
                    ApiResponse<object>.ErrorResponse(
                        "Access denied: you do not have access to the specified branch"
                    )
                );
            }
            if (string.IsNullOrEmpty(request.BranchCode))
            {
                request.BranchCode = branchCodes[0];
            }
        }

        Sale sale = await _saleService.CreateSaleAsync(request);
        SaleDto dto = MapToDto(sale);
        ApiResponse<SaleDto> response = ApiResponse<SaleDto>.SuccessResponse(dto);
        return CreatedAtAction(nameof(GetSaleById), new { id = sale.Id }, response);
    }

    [HttpPost("{id}/post")]
    public async Task<ActionResult<ApiResponse<SaleDto>>> PostSale(string id)
    {
        var sale = await _saleService.PostSaleAsync(id);
        var dto = MapToDto(sale);
        return Ok(ApiResponse<SaleDto>.SuccessResponse(dto));
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PaginatedResponse<SaleDto>>>> GetSales(
        [FromQuery] string? branchReference = null,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 100
    )
    {
        // Non-admin users can only view sales from their assigned branches
        if (_currentUser.RoleCodes != null && !_currentUser.RoleCodes.Contains("ADMIN"))
        {
            var branchRefs = _currentUser.BranchReferences;
            if (branchRefs == null || branchRefs.Count == 0)
            {
                return StatusCode(
                    403,
                    ApiResponse<object>.ErrorResponse("User must have a branch assigned")
                );
            }
            if (!string.IsNullOrEmpty(branchReference) && !branchRefs.Contains(branchReference))
            {
                return StatusCode(
                    403,
                    ApiResponse<object>.ErrorResponse(
                        "Access denied: you do not have access to the specified branch"
                    )
                );
            }
            if (string.IsNullOrEmpty(branchReference))
            {
                branchReference = branchRefs[0];
            }
        }

        IEnumerable<Sale> sales;

        if (!string.IsNullOrEmpty(branchReference) && from.HasValue && to.HasValue)
        {
            sales = await _saleService.GetSalesByBranchAndDateRangeAsync(
                branchReference,
                from.Value,
                to.Value
            );
        }
        else if (from.HasValue && to.HasValue)
        {
            sales = await _saleService.GetSalesByDateRangeAsync(from.Value, to.Value);
        }
        else
        {
            sales = await _saleService.SearchSalesAsync(branchReference, from, to, page, pageSize);
        }

        var totalCount = await _saleService.CountSalesAsync(branchReference, from, to);

        var response = new PaginatedResponse<SaleDto>
        {
            Items = sales.Select(MapToDto).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
        };

        return Ok(ApiResponse<PaginatedResponse<SaleDto>>.SuccessResponse(response));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<SaleDto>>> GetSaleById(string id)
    {
        var sale = await _saleService.GetSaleByIdAsync(id);
        if (sale == null)
        {
            return NotFound();
        }

        // Non-admin users can only view sales from their assigned branches
        if (_currentUser.RoleCodes != null && !_currentUser.RoleCodes.Contains("ADMIN"))
        {
            var branchRefs = _currentUser.BranchReferences;
            if (branchRefs == null || branchRefs.Count == 0)
            {
                return StatusCode(
                    403,
                    ApiResponse<object>.ErrorResponse("User must have a branch assigned")
                );
            }

            var branchCodes = _currentUser.BranchCodes;
            if (branchCodes == null || !branchCodes.Contains(sale.BranchCode))
            {
                return StatusCode(
                    403,
                    ApiResponse<object>.ErrorResponse(
                        "Access denied: Sale belongs to a different branch"
                    )
                );
            }
        }

        var dto = MapToDto(sale);
        return Ok(ApiResponse<SaleDto>.SuccessResponse(dto));
    }

    private static SaleDto MapToDto(Sale sale)
    {
        return new SaleDto
        {
            Id = sale.Id,
            Number = sale.Number,
            BranchReference = sale.BranchReference,
            BranchCode = sale.BranchCode,
            SaleDateUtc = sale.SaleDateUtc,
            Lines = sale
                .Lines.Select(l => new SaleLineDto
                {
                    LineId = l.LineId!,
                    ItemReference = l.ItemReference,
                    ItemCode = l.ItemCode,
                    Description = l.Description,
                    Classification = l.Classification,
                    Condition = l.Condition,
                    Quantity = l.Quantity,
                    UnitPrice = l.UnitPrice,
                    Currency = l.Currency,
                    IsTaxable = l.IsTaxable,
                    AppliesShopFee = l.AppliesShopFee,
                    LineTotal = l.LineTotal,
                })
                .ToList(),
            CustomerName = sale.CustomerName,
            CustomerPhone = sale.CustomerPhone,
            Notes = sale.Notes,
            PaymentMethod = sale.PaymentMethod,
            PaymentDetails = sale
                .PaymentDetails?.Select(pd => new PaymentDetailDto
                {
                    Method = pd.Method.ToString(),
                    Amount = pd.Amount,
                    CheckNumber = pd.CheckNumber,
                })
                .ToList(),
            Status = sale.Status,
            StatusHistory = sale.StatusHistory.Select(sh => new StatusHistoryEntryDto
            {
                Date = sh.Date,
                Status = sh.Status.ToString(),
                User = new UserLiteDto
                {
                    FirstName = sh.User.FirstName,
                    MiddleName = sh.User.MiddleName,
                    LastName = sh.User.LastName,
                    SecondLastName = sh.User.SecondLastName,
                    Username = sh.User.Username,
                    Email = sh.User.Email,
                },
                Comment = sh.Comment,
            }).ToList(),
            CreatedAtUtc = sale.CreatedAtUtc,
            CreatedBy = sale.CreatedBy,
            ModifiedAtUtc = sale.ModifiedAtUtc,
            ModifiedBy = sale.ModifiedBy,
        };
    }
}
