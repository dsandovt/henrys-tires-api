using HenryTires.Inventory.Application.DTOs;
using HenryTires.Inventory.Application.Ports;

namespace HenryTires.Inventory.Api.Services;

public class CompanyInfoProvider : ICompanyInfoProvider
{
    private readonly IConfiguration _configuration;

    public CompanyInfoProvider(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public InvoiceCompanyInfoDto GetCompanyInfo()
    {
        return new InvoiceCompanyInfoDto
        {
            LegalName = _configuration["CompanyInfo:LegalName"] ?? "Henry's Tires Inc.",
            TradeName = _configuration["CompanyInfo:TradeName"],
            AddressLine1 = _configuration["CompanyInfo:AddressLine1"] ?? "",
            CityStateZip = _configuration["CompanyInfo:CityStateZip"] ?? "",
            Phone = _configuration["CompanyInfo:Phone"] ?? ""
        };
    }
}
