USE [master];
GO

IF DB_ID(N'TechMoveDB') IS NULL
BEGIN
    CREATE DATABASE [TechMoveDB];
END
GO

USE [TechMoveDB];
GO

IF OBJECT_ID(N'[dbo].[Clients]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Clients] (
        [ClientId] int IDENTITY(1,1) NOT NULL,
        [Name] nvarchar(120) NOT NULL,
        [ContactDetails] nvarchar(200) NOT NULL,
        [Region] nvarchar(80) NOT NULL,
        CONSTRAINT [PK_Clients] PRIMARY KEY ([ClientId])
    );

    CREATE INDEX [IX_Clients_Name] ON [dbo].[Clients] ([Name]);
END
GO

IF OBJECT_ID(N'[dbo].[ApplicationUsers]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[ApplicationUsers] (
        [ApplicationUserId] int IDENTITY(1,1) NOT NULL,
        [FullName] nvarchar(80) NOT NULL,
        [Email] nvarchar(160) NOT NULL,
        [PasswordHash] nvarchar(max) NOT NULL,
        [Role] nvarchar(40) NOT NULL,
        [CreatedAt] datetime2 NOT NULL CONSTRAINT [DF_ApplicationUsers_CreatedAt] DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT [PK_ApplicationUsers] PRIMARY KEY ([ApplicationUserId])
    );

    CREATE UNIQUE INDEX [IX_ApplicationUsers_Email] ON [dbo].[ApplicationUsers] ([Email]);
END
GO

IF OBJECT_ID(N'[dbo].[Contracts]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Contracts] (
        [ContractId] int IDENTITY(1,1) NOT NULL,
        [ClientId] int NOT NULL,
        [StartDate] datetime2 NOT NULL,
        [EndDate] datetime2 NOT NULL,
        [Status] int NOT NULL,
        [ServiceLevel] nvarchar(80) NOT NULL,
        [SignedAgreementFileName] nvarchar(180) NULL,
        [CreatedAt] datetime2 NOT NULL CONSTRAINT [DF_Contracts_CreatedAt] DEFAULT (SYSUTCDATETIME()),
        [ContractDiscriminator] nvarchar(40) NOT NULL,
        [CurrencyCode] nvarchar(3) NULL,
        [ExchangeRule] nvarchar(120) NULL,
        [PriorityLevel] int NULL,
        CONSTRAINT [PK_Contracts] PRIMARY KEY ([ContractId]),
        CONSTRAINT [FK_Contracts_Clients_ClientId] FOREIGN KEY ([ClientId])
            REFERENCES [dbo].[Clients] ([ClientId]) ON DELETE CASCADE,
        CONSTRAINT [CK_Contracts_DateRange] CHECK ([StartDate] <= [EndDate]),
        CONSTRAINT [CK_Contracts_PriorityLevel] CHECK ([PriorityLevel] IS NULL OR ([PriorityLevel] >= 1 AND [PriorityLevel] <= 5)),
        CONSTRAINT [CK_Contracts_Status] CHECK ([Status] IN (0, 1, 2, 3)),
        CONSTRAINT [CK_Contracts_Discriminator] CHECK ([ContractDiscriminator] IN (N'Standard', N'International', N'Premium'))
    );

    CREATE INDEX [IX_Contracts_ClientId] ON [dbo].[Contracts] ([ClientId]);
    CREATE INDEX [IX_Contracts_Status_StartDate_EndDate] ON [dbo].[Contracts] ([Status], [StartDate], [EndDate]);
END
GO

IF OBJECT_ID(N'[dbo].[ServiceRequests]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[ServiceRequests] (
        [ServiceRequestId] int IDENTITY(1,1) NOT NULL,
        [ContractId] int NOT NULL,
        [RequestType] nvarchar(100) NOT NULL,
        [Description] nvarchar(500) NOT NULL,
        [RequestedAmountUsd] decimal(18,2) NOT NULL,
        [CurrencyCode] nvarchar(3) NOT NULL,
        [ExchangeRate] decimal(18,4) NOT NULL,
        [Cost] decimal(18,2) NOT NULL,
        [Status] int NOT NULL,
        [CreatedAt] datetime2 NOT NULL CONSTRAINT [DF_ServiceRequests_CreatedAt] DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT [PK_ServiceRequests] PRIMARY KEY ([ServiceRequestId]),
        CONSTRAINT [FK_ServiceRequests_Contracts_ContractId] FOREIGN KEY ([ContractId])
            REFERENCES [dbo].[Contracts] ([ContractId]) ON DELETE NO ACTION,
        CONSTRAINT [CK_ServiceRequests_RequestedAmountUsd] CHECK ([RequestedAmountUsd] > 0),
        CONSTRAINT [CK_ServiceRequests_Status] CHECK ([Status] IN (0, 1, 2, 3))
    );

    CREATE INDEX [IX_ServiceRequests_ContractId_Status] ON [dbo].[ServiceRequests] ([ContractId], [Status]);
    CREATE INDEX [IX_ServiceRequests_CreatedAt] ON [dbo].[ServiceRequests] ([CreatedAt]);
END
GO

IF OBJECT_ID(N'[dbo].[Invoices]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Invoices] (
        [InvoiceId] int IDENTITY(1,1) NOT NULL,
        [ServiceRequestId] int NOT NULL,
        [AmountZar] decimal(18,2) NOT NULL,
        [Status] int NOT NULL,
        [IssuedAt] datetime2 NOT NULL CONSTRAINT [DF_Invoices_IssuedAt] DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT [PK_Invoices] PRIMARY KEY ([InvoiceId]),
        CONSTRAINT [FK_Invoices_ServiceRequests_ServiceRequestId] FOREIGN KEY ([ServiceRequestId])
            REFERENCES [dbo].[ServiceRequests] ([ServiceRequestId]) ON DELETE CASCADE,
        CONSTRAINT [CK_Invoices_AmountZar] CHECK ([AmountZar] >= 0),
        CONSTRAINT [CK_Invoices_Status] CHECK ([Status] IN (0, 1, 2))
    );

    CREATE UNIQUE INDEX [IX_Invoices_ServiceRequestId] ON [dbo].[Invoices] ([ServiceRequestId]);
    CREATE INDEX [IX_Invoices_IssuedAt] ON [dbo].[Invoices] ([IssuedAt]);
END
GO

IF OBJECT_ID(N'[dbo].[AuditLogs]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[AuditLogs] (
        [AuditLogId] int IDENTITY(1,1) NOT NULL,
        [EventType] nvarchar(80) NOT NULL,
        [Message] nvarchar(600) NOT NULL,
        [ContractId] int NULL,
        [ServiceRequestId] int NULL,
        [CreatedAt] datetime2 NOT NULL CONSTRAINT [DF_AuditLogs_CreatedAt] DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT [PK_AuditLogs] PRIMARY KEY ([AuditLogId])
    );

    CREATE INDEX [IX_AuditLogs_CreatedAt] ON [dbo].[AuditLogs] ([CreatedAt]);
    CREATE INDEX [IX_AuditLogs_ContractId] ON [dbo].[AuditLogs] ([ContractId]);
    CREATE INDEX [IX_AuditLogs_ServiceRequestId] ON [dbo].[AuditLogs] ([ServiceRequestId]);
END
GO

PRINT 'TechMoveDB schema setup completed.';
GO
