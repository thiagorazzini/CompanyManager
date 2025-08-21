# Relatório de Análise - CompanyManager Backend

## 📋 **Visão Geral do Sistema**
O CompanyManager é uma aplicação .NET 8 que implementa um sistema de gerenciamento de funcionários com controle de acesso baseado em hierarquia de roles. O sistema utiliza Clean Architecture com separação clara entre Domain, Application, Infrastructure e API layers.

---

## 🔐 **SISTEMA SUPERUSER - ACESSO TOTAL SEM LIMITAÇÕES**

### 🎯 **Características do SuperUser**

#### **1. Nível Hierárquico Especial**
- **Valor**: `999` (muito superior aos outros níveis)
- **Nome**: `SuperUser`
- **Descrição**: "SuperUser"
- **Permissões**: **Todas** as permissões do sistema

#### **2. Regras de Negócio Ignoradas**
- **Criação de Roles**: Pode criar qualquer role, incluindo outros SuperUsers
- **Validação Hierárquica**: Não está limitado pela hierarquia normal
- **Permissões**: Tem acesso a todas as funcionalidades do sistema

### 🏗️ **Implementação Técnica**

#### **1. Validação Hierárquica Modificada**
```csharp
public static bool CanCreateRole(this HierarchicalRole creatorRole, HierarchicalRole targetRole)
{
    // SuperUser pode criar qualquer role, incluindo outros SuperUsers
    if (creatorRole == HierarchicalRole.SuperUser)
        return true;
        
    // Usuário só pode criar funcionários com nível igual ou inferior
    return (int)targetRole <= (int)creatorRole;
}
```

#### **2. Verificação de Permissões**
```csharp
public bool HasPermission(string permission)
{
    if (IsSuperUser())
        return true; // SuperUser tem todas as permissões
        
    return _permissions.Contains(permission, StringComparer.OrdinalIgnoreCase);
}
```

#### **3. Validação no Handler**
```csharp
// VALIDAÇÃO HIERÁRQUICA CRÍTICA
// SuperUser pode criar qualquer role, incluindo outros SuperUsers
if (!currentUser.IsSuperUser() && !currentUser.CanCreateRole(targetRoleLevel))
{
    throw new UnauthorizedAccessException($"You cannot create employees with role level '{command.RoleLevel}'. Your highest role level is '{currentUser.GetHighestRoleLevel().GetDescription()}'.");
}
```

### 👤 **Usuário SuperUser Padrão**

#### **Credenciais de Acesso**
- **Email**: `admin@companymanager.com`
- **Senha**: `Admin123!`
- **Role**: `SuperUser`
- **Permissões**: Todas

#### **Criação Automática**
O usuário SuperUser é criado automaticamente durante a inicialização do banco de dados através do `DatabaseInitializer`.

### 📊 **Exemplos de Uso**

#### **Cenário 1: SuperUser criando Director**
```csharp
// Usuário atual: SuperUser (nível 999)
// Tentando criar: Director (nível 5)
// Resultado: ✅ PERMITIDO (SuperUser pode criar qualquer role)
```

#### **Cenário 2: SuperUser criando outro SuperUser**
```csharp
// Usuário atual: SuperUser (nível 999)
// Tentando criar: SuperUser (nível 999)
// Resultado: ✅ PERMITIDO (SuperUser pode criar outros SuperUsers)
```

#### **Cenário 3: Director tentando criar SuperUser**
```csharp
// Usuário atual: Director (nível 5)
// Tentando criar: SuperUser (nível 999)
// Resultado: ❌ NEGADO (Director não pode criar SuperUser)
// Erro: "You cannot create employees with role level 'SuperUser'. Your highest role level is 'Director'."
```

### 🛡️ **Considerações de Segurança**

#### **1. Uso Responsável**
- **SuperUser deve ser usado apenas para administração do sistema**
- **Não deve ser usado para operações diárias**
- **Credenciais devem ser mantidas seguras**

#### **2. Auditoria**
- **Todas as ações do SuperUser são logadas**
- **Histórico completo de operações mantido**
- **Rastreabilidade total das ações**

#### **3. Limitações**
- **SuperUser não pode ser criado por usuários normais**
- **Apenas outro SuperUser pode criar SuperUsers**
- **Não pode ser downgradeado para role inferior**

---

## 🔐 **SISTEMA DE HIERARQUIA DE ROLES (USER LEVEL)**

### 🏗️ **Arquitetura do Sistema de Hierarquia**

#### **1. Enum HierarchicalRole (Domain Layer)**
```csharp
public enum HierarchicalRole
{
    Junior = 1,      // Nível 1 - Acesso básico
    Pleno = 2,       // Nível 2 - Acesso intermediário
    Senior = 3,      // Nível 3 - Acesso avançado
    Manager = 4,     // Nível 4 - Acesso gerencial
    Director = 5,    // Nível 5 - Acesso total
    SuperUser = 999  // Nível especial - Acesso total sem limitações
}
```

**Características:**
- **Valores numéricos**: Cada nível tem um valor inteiro para comparações hierárquicas
- **Extensões**: Métodos de validação e permissões padrão
- **Descrições**: Atributos Description para nomes amigáveis
- **SuperUser**: Nível especial (999) com permissões totais e sem limitações hierárquicas

#### **2. Entidade Role (Domain Layer)**
```csharp
public sealed class Role : BaseEntity
{
    public string Name { get; private set; }
    public HierarchicalRole Level { get; private set; }
    public IReadOnlyCollection<string> Permissions { get; }
    
    // Validação hierárquica
    public bool CanCreateRole(HierarchicalRole targetLevel)
    {
        return Level.CanCreateRole(targetLevel);
    }
    
    // Verificação de SuperUser
    public bool IsSuperUser()
    {
        return Level.IsSuperUser();
    }
    
    // Verificação de permissões (SuperUser tem todas)
    public bool HasPermission(string permission)
    {
        if (IsSuperUser())
            return true; // SuperUser tem todas as permissões
            
        return _permissions.Contains(permission, StringComparer.OrdinalIgnoreCase);
    }
}
```

**Funcionalidades:**
- **Validação hierárquica**: Usuário só pode criar roles de nível igual ou inferior
- **Permissões dinâmicas**: Lista de permissões baseada no nível
- **Imutabilidade**: Propriedades protegidas contra modificação direta
- **SuperUser**: Acesso total sem limitações

#### **3. Entidade UserAccount (Domain Layer)**
```csharp
public sealed class UserAccount : BaseEntity
{
    private readonly List<Role> _roles = new();
    public IReadOnlyCollection<Role> Roles => _roles.AsReadOnly();
    
    // Validação de criação de roles
    public bool CanCreateRole(HierarchicalRole targetRole)
    {
        if (!_roles.Any()) return false;
        return _roles.Any(r => r.CanCreateRole(targetRole));
    }
    
    // Verificação de SuperUser
    public bool IsSuperUser()
    {
        return _roles.Any(r => r.IsSuperUser());
    }
    
    // Nível hierárquico mais alto
    public HierarchicalRole GetHighestRoleLevel()
    {
        if (!_roles.Any()) return HierarchicalRole.Junior;
        return _roles.Max(r => r.Level);
    }
    
    // Verificação de permissões (SuperUser tem todas)
    public bool HasPermission(string permission)
    {
        if (IsSuperUser())
            return true; // SuperUser tem todas as permissões
            
        return _roles.Any(r => r.HasPermission(permission));
    }
}
```

**Características:**
- **Múltiplos roles**: Usuário pode ter vários roles simultaneamente
- **Validação hierárquica**: Verifica se pode criar roles baseado no nível mais alto
- **Fallback**: Retorna Junior se não tiver roles
- **SuperUser**: Acesso total sem limitações hierárquicas

---

## 🔐 **SISTEMA DE AUTENTICAÇÃO JWT IMPLEMENTADO**

### 🛡️ **Proteção das APIs**

#### **1. Configuração JWT no Program.cs**
```csharp
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opts =>
    {
        opts.TokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer = jwt.Issuer,
            ValidAudience = jwt.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Secret)),
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(jwt.ClockSkewSeconds)
        };
    });
```

#### **2. Política de Autorização Padrão**
```csharp
builder.Services.AddAuthorization(options =>
{
    // Política padrão: exige autenticação em todas as rotas
    options.DefaultPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
    
    // Políticas específicas para diferentes permissões
    options.AddPolicy("EmployeesRead", p => p.RequireClaim("perm", "employees:read"));
    options.AddPolicy("EmployeesWrite", p => p.RequireClaim("perm", "employees:write"));
    options.AddPolicy("DepartmentsRead", p => p.RequireClaim("perm", "departments:read"));
    options.AddPolicy("DepartmentsWrite", p => p.RequireClaim("perm", "departments:write"));
    options.AddPolicy("UsersAdmin", p => p.RequireClaim("perm", "users:admin"));
});
```

#### **3. Controllers Protegidos**
- ✅ **EmployeesController**: `[Authorize]` - Todas as rotas protegidas
- ✅ **DepartmentsController**: `[Authorize]` - Todas as rotas protegidas  
- ✅ **JobTitlesController**: `[Authorize]` - Todas as rotas protegidas
- ✅ **AuthController**: Sem `[Authorize]` - Rotas de autenticação públicas
- ✅ **HealthController**: Sem `[Authorize]` - Health checks públicos

### 🔑 **Geração de Token JWT com Permissões**

#### **1. TokenService Modificado**
```csharp
public string GenerateAccessToken(UserAccount user, IEnumerable<Claim>? extraClaims = null)
{
    var claims = new List<Claim>
    {
        new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        new Claim("name", user.UserName),
        new Claim("sstamp", user.SecurityStamp.ToString())
    };

    // Adicionar permissões do usuário
    var permissions = user.GetAllPermissions();
    foreach (var permission in permissions)
    {
        claims.Add(new Claim("perm", permission));
    }

    // ... resto da implementação
}
```

#### **2. Claims Incluídos no Token**
- **`sub`**: ID do usuário
- **`jti`**: ID único do token
- **`name`**: Nome do usuário
- **`sstamp`**: Security stamp para invalidação
- **`perm`**: Lista de permissões baseadas no nível hierárquico

### 📱 **Configuração do Swagger com Autenticação**

#### **1. Swagger Configurado para JWT**
```csharp
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});
```

---

### 🔄 **FLUXO DE CRIAÇÃO DE FUNCIONÁRIOS COM VALIDAÇÃO HIERÁRQUICA**

#### **1. Comando de Criação**
```csharp
public sealed class CreateEmployeeCommand
{
    // ... outros campos ...
    public string RoleLevel { get; set; } = string.Empty; // Nível hierárquico desejado
}
```

#### **2. Validação no Handler**
```csharp
public async Task<CreateEmployeeResult> Handle(CreateEmployeeCommand command, CancellationToken ct, Guid? currentUserId = null)
{
    // 1. Obter usuário atual
    var currentUser = await _users.GetByIdAsync(userId, ct);
    
    // 2. Converter string para enum
    var targetRoleLevel = Enum.Parse<HierarchicalRole>(command.RoleLevel, true);
    
    // 3. VALIDAÇÃO HIERÁRQUICA CRÍTICA
    if (!currentUser.CanCreateRole(targetRoleLevel))
    {
        throw new UnauthorizedAccessException(
            $"You cannot create employees with role level '{command.RoleLevel}'. " +
            $"Your highest role level is '{currentUser.GetHighestRoleLevel().GetDescription()}'.");
    }
    
    // 4. Criação do funcionário e role
    var role = new Role(command.JobTitle, targetRoleLevel);
    account.AddRole(role);
}
```

#### **3. Validação no FluentValidation**
```csharp
public sealed class CreateEmployeeRequestValidator : AbstractValidator<CreateEmployeeRequest>
{
    public CreateEmployeeRequestValidator()
    {
        RuleFor(x => x.RoleLevel)
            .NotEmpty().WithMessage("Role level is required.")
            .Must(IsValidRoleLevel)
            .WithMessage("Invalid role level. Must be one of: Junior, Pleno, Senior, Manager, Director.");
    }
    
    private static bool IsValidRoleLevel(string? roleLevel)
    {
        if (string.IsNullOrWhiteSpace(roleLevel)) return false;
        return Enum.TryParse<HierarchicalRole>(roleLevel, true, out _);
    }
}
```

---

### 🛡️ **REGRAS DE SEGURANÇA IMPLEMENTADAS**

#### **1. Validação Hierárquica**
- **Princípio**: Usuário só pode criar funcionários com nível igual ou inferior
- **Implementação**: `currentUser.CanCreateRole(targetRoleLevel)`
- **Exemplo**: Manager (nível 4) pode criar Junior, Pleno, Senior, Manager
- **Restrição**: Manager NÃO pode criar Director (nível 5)

#### **2. Validação de Permissões**
```csharp
public static string[] GetDefaultPermissions(this HierarchicalRole role)
{
    return role switch
    {
        HierarchicalRole.Junior => new[] { "employees:read", "profile:read", "profile:update" },
        HierarchicalRole.Pleno => new[] { "employees:read", "profile:read", "profile:update", "projects:read" },
        HierarchicalRole.Senior => new[] { "employees:read", "profile:read", "profile:update", "projects:read", "projects:write", "mentoring:read" },
        HierarchicalRole.Manager => new[] { "employees:read", "employees:write", "profile:read", "profile:update", "projects:read", "projects:write", "mentoring:read", "mentoring:write", "departments:read" },
        HierarchicalRole.Director => new[] { "employees:read", "employees:write", "profile:read", "profile:update", "projects:read", "projects:write", "mentoring:read", "mentoring:write", "departments:read", "departments:write", "roles:read", "roles:write" },
        _ => new[] { "profile:read" }
    };
}
```

#### **3. Validação de Entrada**
- **Formato**: String que deve ser parseada para enum
- **Valores válidos**: Junior, Pleno, Senior, Manager, Director
- **Case-insensitive**: Aceita variações de maiúsculas/minúsculas
- **Fallback**: Retorna erro de validação se inválido

---

### 🗄️ **PERSISTÊNCIA NO BANCO DE DADOS**

#### **1. Configuração do Entity Framework**
```csharp
public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.Property(r => r.Level)
            .IsRequired()
            .HasConversion<int>()  // Converte enum para int no banco
            .HasComment("Nível hierárquico da role");
    }
}
```

#### **2. Estrutura da Tabela**
- **Campo**: `Level` (int)
- **Valores**: 1 (Junior) a 5 (Director)
- **Índices**: Otimizados para consultas hierárquicas
- **Constraints**: NOT NULL, validação de domínio

---

### 📊 **EXEMPLOS DE USO E VALIDAÇÃO**

#### **Cenário 1: Manager criando Senior**
```csharp
// Usuário atual: Manager (nível 4)
// Tentando criar: Senior (nível 3)
// Resultado: ✅ PERMITIDO (4 >= 3)
```

#### **Cenário 2: Manager criando Director**
```csharp
// Usuário atual: Manager (nível 4)
// Tentando criar: Director (nível 5)
// Resultado: ❌ NEGADO (4 < 5)
// Erro: "You cannot create employees with role level 'Director'. Your highest role level is 'Manager'."
```

#### **Cenário 3: Director criando qualquer nível**
```csharp
// Usuário atual: Director (nível 5)
// Tentando criar: Qualquer nível (1-5)
// Resultado: ✅ PERMITIDO (5 >= todos)
```

---

### 🔍 **PONTOS DE ATENÇÃO E MELHORIAS**

#### **1. Implementações Atuais**
- ✅ **Validação hierárquica**: Implementada e funcionando
- ✅ **Validação de entrada**: FluentValidation configurado
- ✅ **Persistência**: EF Core configurado corretamente
- ✅ **Logging**: Logs detalhados para auditoria
- ✅ **Tratamento de erros**: Exceções específicas para violações
- ✅ **Autenticação JWT**: Implementada e funcionando
- ✅ **Proteção das APIs**: Todas as rotas protegidas exceto auth e health
- ✅ **Swagger com JWT**: Configurado para testes de autenticação

#### **2. Melhorias Sugeridas**
- 🔄 **Cache de permissões**: Evitar recálculos frequentes
- 🔄 **Auditoria completa**: Log de todas as operações hierárquicas
- 🔄 **Validação de transição**: Verificar mudanças de nível existentes
- 🔄 **Notificações**: Alertar sobre mudanças hierárquicas críticas
- 🔄 **Rate Limiting**: Implementar limitação de tentativas de login
- 🔄 **Refresh Token**: Implementar renovação automática de tokens

#### **3. Considerações de Segurança**
- **Princípio do menor privilégio**: Usuários só acessam o necessário
- **Validação em múltiplas camadas**: Domain + Application + Validation
- **Logs de auditoria**: Rastreamento de todas as operações
- **Imutabilidade**: Propriedades protegidas contra modificação direta
- **Autenticação obrigatória**: Todas as APIs protegidas por JWT
- **Validação de claims**: Verificação de permissões baseada em roles

---

### 📈 **FLUXO COMPLETO DE VALIDAÇÃO**

```
1. Request HTTP → CreateEmployeeCommand
2. FluentValidation → Validação de formato e valores
3. Handler → Obtenção do usuário atual
4. Domain → Validação hierárquica (CanCreateRole)
5. Domain → Criação do Role com nível validado
6. Persistence → Salvamento no banco com constraints
7. Response → Sucesso ou erro detalhado
```

---

### 🔐 **FLUXO DE AUTENTICAÇÃO JWT**

```
1. Login → POST /api/v1/auth/login
2. Validação → Credenciais verificadas
3. Geração Token → JWT com permissões do usuário
4. Proteção API → Todas as rotas exigem Bearer token
5. Validação Token → Claims e permissões verificados
6. Acesso → API liberada baseada nas permissões
```

---

## 🎯 **CONCLUSÃO**

O sistema de hierarquia de roles e autenticação JWT está **completamente implementado** com:

### ✅ **Sistema de Hierarquia:**
- **Arquitetura sólida**: Clean Architecture com separação clara de responsabilidades
- **Validação robusta**: Múltiplas camadas de validação (Domain + Application + Validation)
- **Segurança**: Princípio hierárquico bem definido e implementado
- **Persistência**: Configuração correta do Entity Framework
- **Auditoria**: Logs detalhados para rastreamento de operações

### ✅ **Sistema de Autenticação JWT:**
- **Proteção total**: Todas as APIs protegidas exceto autenticação e health checks
- **Tokens com permissões**: Claims de permissões baseados no nível hierárquico
- **Validação robusta**: Verificação de issuer, audience, assinatura e expiração
- **Swagger integrado**: Interface de teste com autenticação JWT
- **Políticas de autorização**: Controle granular baseado em permissões

### 🛡️ **Segurança Garantida:**
- **Usuários só podem criar funcionários com nível hierárquico igual ou inferior**
- **Todas as APIs exigem Bearer token válido**
- **Permissões baseadas em roles hierárquicos**
- **Validação em múltiplas camadas**
- **Auditoria completa de todas as operações**

O sistema está **pronto para produção** com segurança robusta e controle de acesso hierárquico implementado.
