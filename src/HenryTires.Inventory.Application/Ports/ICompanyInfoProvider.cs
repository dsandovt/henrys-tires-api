using HenryTires.Inventory.Application.DTOs;

namespace HenryTires.Inventory.Application.Ports;

public interface ICompanyInfoProvider
{
    InvoiceCompanyInfoDto GetCompanyInfo();
    InvoiceCompanyInfoDto GetCompanyInfo(string? branchAddress, string? branchPhone);
}
