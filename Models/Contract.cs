using System.ComponentModel.DataAnnotations;

namespace TechMoveLogisticsApplication.Models;

public abstract class Contract
{
    public int ContractId { get; set; }

    [Required]
    public int ClientId { get; set; }

    public Client? Client { get; set; }

    [Required, Display(Name = "Start Date"), DataType(DataType.Date)]
    public DateTime StartDate { get; set; }

    [Required, Display(Name = "End Date"), DataType(DataType.Date)]
    public DateTime EndDate { get; set; }

    [Required]
    public ContractStatus Status { get; set; } = ContractStatus.Draft;

    [Required, Display(Name = "Service Level"), StringLength(80)]
    public string ServiceLevel { get; set; } = string.Empty;

    [Display(Name = "Signed Agreement")]
    public string? SignedAgreementFileName { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<ServiceRequest> ServiceRequests { get; set; } = new List<ServiceRequest>();

    public abstract ContractType ContractType { get; }

    public virtual bool Validate()
    {
        return ClientId > 0 && StartDate.Date <= EndDate.Date && !string.IsNullOrWhiteSpace(ServiceLevel);
    }
}

public class StandardContract : Contract
{
    public override ContractType ContractType => ContractType.Standard;

    public override bool Validate()
    {
        return base.Validate();
    }
}

public class InternationalContract : Contract
{
    [Required, Display(Name = "Currency Code"), StringLength(3, MinimumLength = 3)]
    public string CurrencyCode { get; set; } = "USD";

    [Required, Display(Name = "Exchange Rule"), StringLength(120)]
    public string ExchangeRule { get; set; } = "External API conversion to ZAR";

    public override ContractType ContractType => ContractType.International;

    public override bool Validate()
    {
        return base.Validate() && CurrencyCode.Length == 3;
    }
}

public class PremiumContract : Contract
{
    [Range(1, 5), Display(Name = "Priority Level")]
    public int PriorityLevel { get; set; } = 1;

    public override ContractType ContractType => ContractType.Premium;

    public override bool Validate()
    {
        return base.Validate() && PriorityLevel is >= 1 and <= 5;
    }
}
