using HenryTires.Inventory.Domain.Entities;
using HenryTires.Inventory.Domain.ValueObjects;
using HenryTires.Inventory.Infrastructure.Adapters.Persistence.MongoDB.Documents;

namespace HenryTires.Inventory.Infrastructure.Adapters.Persistence.MongoDB.Mappings;

public static class SaleDocumentMapper
{
    public static Sale ToEntity(SaleDocument document)
    {
        return new Sale
        {
            Id = document.Id,
            Number = document.Number,
            BranchReference = document.BranchReference,
            BranchCode = document.BranchCode,
            SaleDateUtc = document.SaleDateUtc,
            Lines = document.Lines.Select(ToLineEntity).ToList(),
            CustomerName = document.CustomerName,
            CustomerPhone = document.CustomerPhone,
            Notes = document.Notes,
            PaymentMethod = document.PaymentMethod,
            PaymentDetails = document.PaymentDetails?.Select(pd => new PaymentDetail
            {
                Method = pd.Method,
                Amount = pd.Amount,
                CheckNumber = pd.CheckNumber
            }).ToList(),
            Status = document.Status,
            StatusHistory = document.StatusHistory.Select(StatusHistoryMapper.ToEntity).ToList(),
            CreatedAtUtc = document.CreatedAtUtc,
            CreatedBy = document.CreatedBy,
            ModifiedAtUtc = document.ModifiedAtUtc,
            ModifiedBy = document.ModifiedBy
        };
    }

    public static SaleDocument ToDocument(Sale entity)
    {
        return new SaleDocument
        {
            Id = entity.Id,
            Number = entity.Number,
            BranchReference = entity.BranchReference,
            BranchCode = entity.BranchCode,
            SaleDateUtc = entity.SaleDateUtc,
            Lines = entity.Lines.Select(ToLineDocument).ToList(),
            CustomerName = entity.CustomerName,
            CustomerPhone = entity.CustomerPhone,
            Notes = entity.Notes,
            PaymentMethod = entity.PaymentMethod,
            PaymentDetails = entity.PaymentDetails?.Select(pd => new PaymentDetailDocument
            {
                Method = pd.Method,
                Amount = pd.Amount,
                CheckNumber = pd.CheckNumber
            }).ToList(),
            Status = entity.Status,
            StatusHistory = entity.StatusHistory.Select(StatusHistoryMapper.ToDocument).ToList(),
            CreatedAtUtc = entity.CreatedAtUtc,
            CreatedBy = entity.CreatedBy,
            ModifiedAtUtc = entity.ModifiedAtUtc,
            ModifiedBy = entity.ModifiedBy
        };
    }

    private static SaleLine ToLineEntity(SaleLineDocument document)
    {
        return new SaleLine
        {
            LineId = document.LineId,
            ItemReference = document.ItemReference,
            ItemCode = document.ItemCode,
            Description = document.Description,
            Classification = document.Classification,
            Condition = document.Condition,
            Quantity = document.Quantity,
            UnitPrice = document.UnitPrice,
            Currency = document.Currency,
            IsTaxable = document.IsTaxable,
            AppliesShopFee = document.AppliesShopFee,
        };
    }

    private static SaleLineDocument ToLineDocument(SaleLine entity)
    {
        return new SaleLineDocument
        {
            LineId = entity.LineId,
            ItemReference = entity.ItemReference,
            ItemCode = entity.ItemCode,
            Description = entity.Description,
            Classification = entity.Classification,
            Condition = entity.Condition,
            Quantity = entity.Quantity,
            UnitPrice = entity.UnitPrice,
            Currency = entity.Currency,
            IsTaxable = entity.IsTaxable,
            AppliesShopFee = entity.AppliesShopFee,
        };
    }
}
