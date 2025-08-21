-- Script para corrigir roles do admin
-- 1. Verificar usuário admin
SELECT 'Admin User:' as Info, u.Id, u.UserName FROM UserAccounts u WHERE u.UserName = 'admin@companymanager.com';

-- 2. Verificar role SuperUser
SELECT 'SuperUser Role:' as Info, r.Id, r.Name, r.Level FROM Roles r WHERE r.Name = 'SuperUser';

-- 3. Verificar se admin tem role SuperUser
SELECT 'Admin Roles:' as Info, u.UserName, r.Name, r.Level 
FROM UserAccounts u 
LEFT JOIN UserAccountRoles ur ON u.Id = ur.UserAccountId 
LEFT JOIN Roles r ON ur.RolesId = r.Id 
WHERE u.UserName = 'admin@companymanager.com';

-- 4. Inserir role SuperUser se não existir
IF NOT EXISTS (SELECT 1 FROM Roles WHERE Name = 'SuperUser')
BEGIN
    INSERT INTO Roles (Id, Name, Level, CreatedAt) 
    VALUES (NEWID(), 'SuperUser', 999, GETUTCDATE());
    PRINT 'Role SuperUser criada';
END

-- 5. Associar admin com role SuperUser se não estiver associado
DECLARE @AdminId UNIQUEIDENTIFIER = (SELECT Id FROM UserAccounts WHERE UserName = 'admin@companymanager.com');
DECLARE @SuperUserRoleId UNIQUEIDENTIFIER = (SELECT Id FROM Roles WHERE Name = 'SuperUser');

IF @AdminId IS NOT NULL AND @SuperUserRoleId IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM UserAccountRoles WHERE UserAccountId = @AdminId AND RolesId = @SuperUserRoleId)
    BEGIN
        INSERT INTO UserAccountRoles (UserAccountId, RolesId) 
        VALUES (@AdminId, @SuperUserRoleId);
        PRINT 'Role SuperUser associada ao admin';
    END
    ELSE
    BEGIN
        PRINT 'Admin já possui role SuperUser';
    END
END

-- 6. Verificar resultado final
SELECT 'Final Result:' as Info, u.UserName, r.Name, r.Level 
FROM UserAccounts u 
JOIN UserAccountRoles ur ON u.Id = ur.UserAccountId 
JOIN Roles r ON ur.RolesId = r.Id 
WHERE u.UserName = 'admin@companymanager.com';
