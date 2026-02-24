IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
CREATE TABLE [Guests] (
    [GuestId] int NOT NULL IDENTITY,
    [Name] nvarchar(max) NOT NULL,
    [Email] nvarchar(max) NOT NULL,
    [Phone] nvarchar(max) NOT NULL,
    [IsActive] bit NOT NULL,
    CONSTRAINT [PK_Guests] PRIMARY KEY ([GuestId])
);

CREATE TABLE [Rooms] (
    [RoomId] int NOT NULL IDENTITY,
    [RoomNumber] nvarchar(max) NOT NULL,
    [RoomType] nvarchar(max) NOT NULL,
    [PricePerNight] decimal(18,2) NOT NULL,
    [IsAvailable] bit NOT NULL,
    CONSTRAINT [PK_Rooms] PRIMARY KEY ([RoomId])
);

CREATE TABLE [Bookings] (
    [BookingId] int NOT NULL IDENTITY,
    [RoomId] int NOT NULL,
    [GuestId] int NOT NULL,
    [CheckInDate] datetime2 NOT NULL,
    [CheckOutDate] datetime2 NOT NULL,
    [TotalAmount] decimal(18,2) NOT NULL,
    CONSTRAINT [PK_Bookings] PRIMARY KEY ([BookingId]),
    CONSTRAINT [FK_Bookings_Guests_GuestId] FOREIGN KEY ([GuestId]) REFERENCES [Guests] ([GuestId]) ON DELETE CASCADE,
    CONSTRAINT [FK_Bookings_Rooms_RoomId] FOREIGN KEY ([RoomId]) REFERENCES [Rooms] ([RoomId]) ON DELETE CASCADE
);

CREATE INDEX [IX_Bookings_GuestId] ON [Bookings] ([GuestId]);

CREATE INDEX [IX_Bookings_RoomId] ON [Bookings] ([RoomId]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260210191930_InitialCreate', N'10.0.3');

COMMIT;
GO

BEGIN TRANSACTION;
DECLARE @var nvarchar(max);
SELECT @var = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Rooms]') AND [c].[name] = N'RoomType');
IF @var IS NOT NULL EXEC(N'ALTER TABLE [Rooms] DROP CONSTRAINT ' + @var + ';');
ALTER TABLE [Rooms] ALTER COLUMN [RoomType] nvarchar(50) NOT NULL;

DECLARE @var1 nvarchar(max);
SELECT @var1 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Rooms]') AND [c].[name] = N'RoomNumber');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [Rooms] DROP CONSTRAINT ' + @var1 + ';');
ALTER TABLE [Rooms] ALTER COLUMN [RoomNumber] nvarchar(50) NOT NULL;

DECLARE @var2 nvarchar(max);
SELECT @var2 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Guests]') AND [c].[name] = N'Phone');
IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [Guests] DROP CONSTRAINT ' + @var2 + ';');
ALTER TABLE [Guests] ALTER COLUMN [Phone] nvarchar(20) NOT NULL;

DECLARE @var3 nvarchar(max);
SELECT @var3 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Guests]') AND [c].[name] = N'Name');
IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [Guests] DROP CONSTRAINT ' + @var3 + ';');
ALTER TABLE [Guests] ALTER COLUMN [Name] nvarchar(100) NOT NULL;

DECLARE @var4 nvarchar(max);
SELECT @var4 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Guests]') AND [c].[name] = N'Email');
IF @var4 IS NOT NULL EXEC(N'ALTER TABLE [Guests] DROP CONSTRAINT ' + @var4 + ';');
ALTER TABLE [Guests] ALTER COLUMN [Email] nvarchar(100) NOT NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260210200305_UpdateModelsWithValidation', N'10.0.3');

COMMIT;
GO

