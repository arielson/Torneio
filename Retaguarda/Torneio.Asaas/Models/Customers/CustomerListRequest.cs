namespace Torneio.Asaas.Models.Customers;

public class CustomerListRequest
{
    public string Name { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string CpfCnpj { get; set; } = null!;
    public string GroupName { get; set; } = null!;
    public string ExternalReference { get; set; } = null!;
    public int? Offset { get; set; }
    public int? Limit { get; set; }
}
