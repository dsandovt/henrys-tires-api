using HenryTires.Inventory.Application.DTOs;

namespace HenryTires.Inventory.Application.Ports;

public interface ICompanyInfoProvider
{
    InvoiceCompanyInfoDto GetCompanyInfo();
}
