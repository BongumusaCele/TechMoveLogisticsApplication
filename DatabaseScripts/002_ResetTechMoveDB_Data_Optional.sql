
USE [TechMoveDB];
GO

DELETE FROM [dbo].[Invoices];
DELETE FROM [dbo].[ServiceRequests];
DELETE FROM [dbo].[Contracts];
DELETE FROM [dbo].[Clients];
DELETE FROM [dbo].[AuditLogs];
DELETE FROM [dbo].[ApplicationUsers];
GO

DBCC CHECKIDENT ('[dbo].[Invoices]', RESEED, 0);
DBCC CHECKIDENT ('[dbo].[ServiceRequests]', RESEED, 0);
DBCC CHECKIDENT ('[dbo].[Contracts]', RESEED, 0);
DBCC CHECKIDENT ('[dbo].[Clients]', RESEED, 0);
DBCC CHECKIDENT ('[dbo].[AuditLogs]', RESEED, 0);
DBCC CHECKIDENT ('[dbo].[ApplicationUsers]', RESEED, 0);
GO

PRINT 'TechMoveDB prototype data reset completed. Start the app to seed default records.';
GO
