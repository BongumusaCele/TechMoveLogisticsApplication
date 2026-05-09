namespace TechMoveLogisticsApplication.Models;

public enum ContractStatus
{
    Draft = 0,
    Active = 1,
    Expired = 2,
    OnHold = 3
}

public enum ContractType
{
    Standard = 0,
    International = 1,
    Premium = 2
}

public enum ServiceRequestStatus
{
    Submitted = 0,
    Approved = 1,
    Rejected = 2,
    Completed = 3
}

public enum InvoiceStatus
{
    Draft = 0,
    Issued = 1,
    Paid = 2
}
