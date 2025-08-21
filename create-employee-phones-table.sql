-- Script para criar apenas a tabela EmployeePhones
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='EmployeePhones' AND xtype='U')
BEGIN
    CREATE TABLE [EmployeePhones] (
        [Id] uniqueidentifier NOT NULL,
        [EmployeeId] uniqueidentifier NOT NULL,
        [PhoneNumber] nvarchar(50) NOT NULL,
        [Type] nvarchar(20) NOT NULL DEFAULT 'Mobile',
        [IsPrimary] bit NOT NULL DEFAULT 0,
        [CreatedAt] datetime2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_EmployeePhones] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_EmployeePhones_Employees_EmployeeId] FOREIGN KEY ([EmployeeId]) REFERENCES [Employees] ([Id]) ON DELETE CASCADE
    );

    -- Criar índices
    CREATE INDEX [IX_EmployeePhones_EmployeeId] ON [EmployeePhones] ([EmployeeId]);
    
    CREATE UNIQUE INDEX [IX_EmployeePhones_EmployeeId_IsPrimary] ON [EmployeePhones] ([EmployeeId], [IsPrimary]) 
    WHERE [IsPrimary] = 1;

    -- Adicionar comentários nas colunas
    EXEC sp_addextendedproperty 'MS_Description', N'ID do funcionário proprietário do telefone', 'SCHEMA', dbo, 'TABLE', EmployeePhones, 'COLUMN', EmployeeId;
    EXEC sp_addextendedproperty 'MS_Description', N'Número do telefone no formato original', 'SCHEMA', dbo, 'TABLE', EmployeePhones, 'COLUMN', PhoneNumber;
    EXEC sp_addextendedproperty 'MS_Description', N'Tipo do telefone (Mobile, Work, Home, Other)', 'SCHEMA', dbo, 'TABLE', EmployeePhones, 'COLUMN', Type;
    EXEC sp_addextendedproperty 'MS_Description', N'Indica se é o telefone principal do funcionário', 'SCHEMA', dbo, 'TABLE', EmployeePhones, 'COLUMN', IsPrimary;

    PRINT 'Tabela EmployeePhones criada com sucesso!';
END
ELSE
BEGIN
    PRINT 'Tabela EmployeePhones já existe.';
END
