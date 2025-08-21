# Sistema de SuperUser - CompanyManager

## 🔐 **Visão Geral**

O sistema de **SuperUser** é um nível especial de acesso que permite ao usuário realizar **qualquer ação** no sistema, **sem limitações hierárquicas** ou de permissões.

---

## 🎯 **Características do SuperUser**

### **1. Nível Hierárquico Especial**
- **Valor**: `999` (muito superior aos outros níveis)
- **Nome**: `SuperUser`
- **Descrição**: "SuperUser"

### **2. Permissões Totais**
O SuperUser tem **todas as permissões** do sistema, incluindo:
- ✅ **Funcionários**: read, write, delete, admin
- ✅ **Perfis**: read, update, admin
- ✅ **Projetos**: read, write, delete, admin
- ✅ **Mentoria**: read, write, delete, admin
- ✅ **Departamentos**: read, write, delete, admin
- ✅ **Roles**: read, write, delete, admin
- ✅ **Usuários**: read, write, delete, admin
- ✅ **Sistema**: admin, config, audit

### **3. Regras de Negócio Ignoradas**
- **Criação de Roles**: Pode criar qualquer role, incluindo outros SuperUsers
- **Validação Hierárquica**: Não está limitado pela hierarquia normal
- **Permissões**: Tem acesso a todas as funcionalidades do sistema

---

## 🏗️ **Implementação Técnica**

### **1. Enum HierarchicalRole**
```csharp
public enum HierarchicalRole
{
    Junior = 1,
    Pleno = 2,
    Senior = 3,
    Manager = 4,
    Director = 5,
    SuperUser = 999 // Nível especial com permissões totais
}
```

### **2. Extensões do Enum**
```csharp
public static bool CanCreateRole(this HierarchicalRole creatorRole, HierarchicalRole targetRole)
{
    // SuperUser pode criar qualquer role, incluindo outros SuperUsers
    if (creatorRole == HierarchicalRole.SuperUser)
        return true;
        
    // Usuário só pode criar funcionários com nível igual ou inferior
    return (int)targetRole <= (int)creatorRole;
}

public static bool IsSuperUser(this HierarchicalRole role)
{
    return role == HierarchicalRole.SuperUser;
}
```

### **3. Entidade Role**
```csharp
public bool IsSuperUser()
{
    return Level.IsSuperUser();
}

public bool HasPermission(string permission)
{
    if (IsSuperUser())
        return true; // SuperUser tem todas as permissões
        
    return _permissions.Contains(permission, StringComparer.OrdinalIgnoreCase);
}
```

### **4. Entidade UserAccount**
```csharp
public bool IsSuperUser()
{
    return _roles.Any(r => r.IsSuperUser());
}

public bool HasPermission(string permission)
{
    if (IsSuperUser())
        return true; // SuperUser tem todas as permissões
        
    return _roles.Any(r => r.HasPermission(permission));
}
```

---

## 🔄 **Fluxo de Validação**

### **1. Criação de Funcionários**
```csharp
// VALIDAÇÃO HIERÁRQUICA CRÍTICA
// SuperUser pode criar qualquer role, incluindo outros SuperUsers
if (!currentUser.IsSuperUser() && !currentUser.CanCreateRole(targetRoleLevel))
{
    throw new UnauthorizedAccessException($"You cannot create employees with role level '{command.RoleLevel}'. Your highest role level is '{currentUser.GetHighestRoleLevel().GetDescription()}'.");
}
```

### **2. Verificação de Permissões**
```csharp
// SuperUser sempre tem todas as permissões
if (user.IsSuperUser())
    return true; // Acesso total garantido
```

---

## 👤 **Usuário SuperUser Padrão**

### **Credenciais de Acesso**
- **Email**: `admin@companymanager.com`
- **Senha**: `Admin123!`
- **Role**: `SuperUser`
- **Permissões**: Todas

### **Criação Automática**
O usuário SuperUser é criado automaticamente durante a inicialização do banco de dados através do `DatabaseInitializer`.

---

## 🛡️ **Considerações de Segurança**

### **1. Uso Responsável**
- **SuperUser deve ser usado apenas para administração do sistema**
- **Não deve ser usado para operações diárias**
- **Credenciais devem ser mantidas seguras**

### **2. Auditoria**
- **Todas as ações do SuperUser são logadas**
- **Histórico completo de operações mantido**
- **Rastreabilidade total das ações**

### **3. Limitações**
- **SuperUser não pode ser criado por usuários normais**
- **Apenas outro SuperUser pode criar SuperUsers**
- **Não pode ser downgradeado para role inferior**

---

## 📊 **Exemplos de Uso**

### **Cenário 1: SuperUser criando Director**
```csharp
// Usuário atual: SuperUser (nível 999)
// Tentando criar: Director (nível 5)
// Resultado: ✅ PERMITIDO (SuperUser pode criar qualquer role)
```

### **Cenário 2: SuperUser criando outro SuperUser**
```csharp
// Usuário atual: SuperUser (nível 999)
// Tentando criar: SuperUser (nível 999)
// Resultado: ✅ PERMITIDO (SuperUser pode criar outros SuperUsers)
```

### **Cenário 3: Director tentando criar SuperUser**
```csharp
// Usuário atual: Director (nível 5)
// Tentando criar: SuperUser (nível 999)
// Resultado: ❌ NEGADO (Director não pode criar SuperUser)
// Erro: "You cannot create employees with role level 'SuperUser'. Your highest role level is 'Director'."
```

---

## 🔧 **Configuração**

### **1. Banco de Dados**
O role SuperUser é criado automaticamente com:
- **Nome**: "SuperUser"
- **Nível**: 999
- **Permissões**: Todas as permissões do sistema

### **2. Validação**
- **FluentValidation**: Aceita "SuperUser" como role válido
- **Mensagem**: Inclui SuperUser na lista de roles válidos

### **3. Swagger**
- **Documentação**: Inclui exemplos com SuperUser
- **Testes**: Permite testar funcionalidades com SuperUser

---

## 🎯 **Casos de Uso**

### **1. Administração do Sistema**
- **Criação de usuários administrativos**
- **Configuração de roles e permissões**
- **Manutenção do sistema**

### **2. Emergências**
- **Recuperação de acesso**
- **Correção de problemas críticos**
- **Auditoria de segurança**

### **3. Desenvolvimento**
- **Testes de funcionalidades**
- **Debug de permissões**
- **Validação de regras de negócio**

---

## ⚠️ **Avisos Importantes**

### **1. Uso Limitado**
- **SuperUser deve ser usado apenas quando necessário**
- **Não deve ser o role padrão para usuários**
- **Credenciais devem ser compartilhadas com cuidado**

### **2. Monitoramento**
- **Todas as ações do SuperUser devem ser monitoradas**
- **Alertas para uso excessivo**
- **Relatórios de auditoria regulares**

### **3. Backup de Segurança**
- **Manter sempre pelo menos um SuperUser ativo**
- **Credenciais de emergência seguras**
- **Procedimentos de recuperação documentados**

---

## 🚀 **Conclusão**

O sistema de **SuperUser** fornece acesso total ao sistema, permitindo:
- **Administração completa** sem limitações
- **Flexibilidade total** para operações críticas
- **Controle absoluto** sobre o sistema

**Use com responsabilidade e mantenha as credenciais seguras!** 🔐
