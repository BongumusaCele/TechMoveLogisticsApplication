USE [TechMoveDB];
GO

DECLARE @Now datetime2 = SYSUTCDATETIME();

DECLARE @Clients TABLE (
    Name nvarchar(120) NOT NULL,
    ContactDetails nvarchar(200) NOT NULL,
    Region nvarchar(80) NOT NULL
);

INSERT INTO @Clients (Name, ContactDetails, Region)
VALUES
    (N'CapeLink Automotive', N'logistics@capelinkauto.example | +27 21 555 0134', N'Africa'),
    (N'BlueHarbor Retail Group', N'supplychain@blueharbor.example | +44 20 5555 0198', N'Europe'),
    (N'Karoo Mining Supplies', N'ops@karoomining.example | +27 53 555 0177', N'Africa'),
    (N'Pacific MedTech Imports', N'importdesk@pacificmedtech.example | +65 555 0149', N'Asia-Pacific'),
    (N'Solaris Energy Components', N'freight@solarisenergy.example | +971 4 555 0182', N'Middle East');

INSERT INTO Clients (Name, ContactDetails, Region)
SELECT source.Name, source.ContactDetails, source.Region
FROM @Clients AS source
WHERE NOT EXISTS (
    SELECT 1 FROM Clients AS existing WHERE existing.Name = source.Name
);

DECLARE
    @CapeLink int = (SELECT ClientId FROM Clients WHERE Name = N'CapeLink Automotive'),
    @BlueHarbor int = (SELECT ClientId FROM Clients WHERE Name = N'BlueHarbor Retail Group'),
    @Karoo int = (SELECT ClientId FROM Clients WHERE Name = N'Karoo Mining Supplies'),
    @Pacific int = (SELECT ClientId FROM Clients WHERE Name = N'Pacific MedTech Imports'),
    @Solaris int = (SELECT ClientId FROM Clients WHERE Name = N'Solaris Energy Components');

DECLARE @Contracts TABLE (
    ClientId int NOT NULL,
    StartDate datetime2 NOT NULL,
    EndDate datetime2 NOT NULL,
    Status int NOT NULL,
    ServiceLevel nvarchar(80) NOT NULL,
    ContractDiscriminator nvarchar(40) NOT NULL,
    CurrencyCode nvarchar(3) NULL,
    ExchangeRule nvarchar(120) NULL,
    PriorityLevel int NULL
);

INSERT INTO @Contracts (ClientId, StartDate, EndDate, Status, ServiceLevel, ContractDiscriminator, CurrencyCode, ExchangeRule, PriorityLevel)
VALUES
    (@CapeLink, DATEADD(month, -4, @Now), DATEADD(month, 8, @Now), 1, N'Automotive parts standard freight', N'Standard', NULL, NULL, NULL),
    (@BlueHarbor, DATEADD(month, -2, @Now), DATEADD(month, 16, @Now), 1, N'European retail priority SLA', N'Premium', NULL, NULL, 4),
    (@Karoo, DATEADD(month, -7, @Now), DATEADD(month, 5, @Now), 3, N'Mining equipment heavy freight', N'Premium', NULL, NULL, 5),
    (@Pacific, DATEADD(month, -1, @Now), DATEADD(month, 23, @Now), 1, N'MedTech international cold-chain', N'International', N'USD', N'Use latest USD to ZAR exchange rate', NULL),
    (@Solaris, DATEADD(month, -3, @Now), DATEADD(month, 21, @Now), 1, N'Solar components import SLA', N'International', N'EUR', N'Use latest EUR to ZAR exchange rate', NULL);

INSERT INTO Contracts (
    ClientId,
    StartDate,
    EndDate,
    Status,
    ServiceLevel,
    SignedAgreementFileName,
    CreatedAt,
    ContractDiscriminator,
    CurrencyCode,
    ExchangeRule,
    PriorityLevel
)
SELECT
    source.ClientId,
    source.StartDate,
    source.EndDate,
    source.Status,
    source.ServiceLevel,
    NULL,
    DATEADD(day, -10, @Now),
    source.ContractDiscriminator,
    source.CurrencyCode,
    source.ExchangeRule,
    source.PriorityLevel
FROM @Contracts AS source
WHERE NOT EXISTS (
    SELECT 1
    FROM Contracts AS existing
    WHERE existing.ClientId = source.ClientId
      AND existing.ServiceLevel = source.ServiceLevel
);

DECLARE
    @CapeContract int = (SELECT ContractId FROM Contracts WHERE ClientId = @CapeLink AND ServiceLevel = N'Automotive parts standard freight'),
    @BlueContract int = (SELECT ContractId FROM Contracts WHERE ClientId = @BlueHarbor AND ServiceLevel = N'European retail priority SLA'),
    @KarooContract int = (SELECT ContractId FROM Contracts WHERE ClientId = @Karoo AND ServiceLevel = N'Mining equipment heavy freight'),
    @PacificContract int = (SELECT ContractId FROM Contracts WHERE ClientId = @Pacific AND ServiceLevel = N'MedTech international cold-chain'),
    @SolarisContract int = (SELECT ContractId FROM Contracts WHERE ClientId = @Solaris AND ServiceLevel = N'Solar components import SLA');

DECLARE @Requests TABLE (
    ContractId int NOT NULL,
    RequestType nvarchar(100) NOT NULL,
    Description nvarchar(500) NOT NULL,
    RequestedAmountUsd decimal(18,2) NOT NULL,
    CurrencyCode nvarchar(3) NOT NULL,
    ExchangeRate decimal(18,4) NOT NULL,
    Cost decimal(18,2) NOT NULL,
    Status int NOT NULL
);

INSERT INTO @Requests (ContractId, RequestType, Description, RequestedAmountUsd, CurrencyCode, ExchangeRate, Cost, Status)
VALUES
    (@CapeContract, N'Inbound container shipment', N'Two 40ft containers of automotive service parts from Durban to Cape Town distribution hub.', 12500.00, N'USD', 18.7500, 234375.00, 1),
    (@CapeContract, N'Warehouse cross-dock', N'Cross-dock handling for urgent dealership replenishment orders.', 3200.00, N'USD', 18.7500, 60000.00, 0),
    (@BlueContract, N'Priority air freight', N'High-priority retail launch stock routed through Johannesburg and London.', 18400.00, N'USD', 18.6200, 342608.00, 3),
    (@KarooContract, N'Heavy machinery transport', N'Low-bed transport for drilling components from port to Northern Cape site.', 27500.00, N'USD', 18.9000, 519750.00, 1),
    (@PacificContract, N'Temperature-controlled shipment', N'Cold-chain shipment for diagnostic equipment with temperature logs required.', 22150.00, N'USD', 18.8100, 416641.50, 0),
    (@PacificContract, N'Customs clearance', N'SAHPRA documentation support and customs clearance for medical import batch.', 4300.00, N'USD', 18.8100, 80883.00, 3),
    (@SolarisContract, N'Solar battery import', N'Lithium battery import documentation and container monitoring.', 19800.00, N'EUR', 20.3000, 401940.00, 1);

INSERT INTO ServiceRequests (
    ContractId,
    RequestType,
    Description,
    RequestedAmountUsd,
    CurrencyCode,
    ExchangeRate,
    Cost,
    Status,
    CreatedAt
)
SELECT
    source.ContractId,
    source.RequestType,
    source.Description,
    source.RequestedAmountUsd,
    source.CurrencyCode,
    source.ExchangeRate,
    source.Cost,
    source.Status,
    DATEADD(day, -5, @Now)
FROM @Requests AS source
WHERE NOT EXISTS (
    SELECT 1
    FROM ServiceRequests AS existing
    WHERE existing.ContractId = source.ContractId
      AND existing.RequestType = source.RequestType
);

INSERT INTO Invoices (ServiceRequestId, AmountZar, Status, IssuedAt)
SELECT
    request.ServiceRequestId,
    request.Cost,
    CASE WHEN request.Status = 3 THEN 2 ELSE 1 END,
    DATEADD(day, -2, @Now)
FROM ServiceRequests AS request
WHERE request.Status IN (1, 3)
  AND request.RequestType IN (
      N'Inbound container shipment',
      N'Priority air freight',
      N'Heavy machinery transport',
      N'Customs clearance',
      N'Solar battery import'
  )
  AND NOT EXISTS (
      SELECT 1 FROM Invoices AS invoice WHERE invoice.ServiceRequestId = request.ServiceRequestId
  );

INSERT INTO AuditLogs (EventType, Message, ContractId, ServiceRequestId, CreatedAt)
SELECT
    N'TestData',
    CONCAT(N'Testing data loaded for contract ', contract.ContractId, N' (', contract.ServiceLevel, N').'),
    contract.ContractId,
    NULL,
    DATEADD(day, -1, @Now)
FROM Contracts AS contract
WHERE contract.ServiceLevel IN (
    N'Automotive parts standard freight',
    N'European retail priority SLA',
    N'Mining equipment heavy freight',
    N'MedTech international cold-chain',
    N'Solar components import SLA'
)
AND NOT EXISTS (
    SELECT 1
    FROM AuditLogs AS log
    WHERE log.EventType = N'TestData'
      AND log.ContractId = contract.ContractId
);

SELECT 'Clients' AS TableName, COUNT(*) AS TotalRows FROM Clients
UNION ALL SELECT 'Contracts', COUNT(*) FROM Contracts
UNION ALL SELECT 'ServiceRequests', COUNT(*) FROM ServiceRequests
UNION ALL SELECT 'Invoices', COUNT(*) FROM Invoices
UNION ALL SELECT 'AuditLogs', COUNT(*) FROM AuditLogs
UNION ALL SELECT 'ApplicationUsers', COUNT(*) FROM ApplicationUsers;
GO
