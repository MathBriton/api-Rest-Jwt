# 🚀 API REST com JWT - .NET 8

API REST completa e profissional com autenticação JWT, refresh tokens, roles, CRUD, tratamento de erros, logging e rate limiting.

## ⚡ Como Rodar o Projeto

### 1. Clonar o repositório
```bash
git clone https://github.com/MathBriton/api-Rest-Jwt.git
cd api-Rest-Jwt
```

### 2. Restaurar dependências
```bash
dotnet restore
```

### 3. Rodar o projeto
```bash
dotnet run
```

✅ **Pronto!** A API está rodando em: `https://localhost:7xxx`

Acesse o Swagger em: `https://localhost:7xxx/swagger`

## 📊 Recursos Implementados

### ✅ Autenticação & Segurança
- JWT com Access Token (15 min)
- Refresh Token (7 dias) com rotation
- Sistema de Roles (Admin, User, Manager)
- BCrypt para hash de senhas
- Rate Limiting (proteção DDoS e brute force)

### ✅ CRUD Completo
- Produtos com paginação
- Filtros: busca, categoria, preço, status
- Ordenação: nome, preço, estoque, data
- Validações de entrada

### ✅ Qualidade & Produção
- Tratamento global de erros
- Logging estruturado (Serilog)
- Respostas padronizadas
- Swagger/OpenAPI documentado

### ✅ Banco de Dados
- SQLite com Entity Framework Core
- 3 tabelas: Users, Products, RefreshTokens
- Seed de dados automático

## 🔒 Rate Limiting

Proteção automática contra abuso:

- **Geral**: 60 requisições/minuto
- **Login**: 5 tentativas/minuto
- **Registro**: 10 cadastros/hora

Quando atingir o limite, receberá: `429 Too Many Requests`

## ª Tecnologias

- .NET 8
- ASP.NET Core Web API
- Entity Framework Core
- SQLite
- JWT Bearer Authentication
- Serilog
- AspNetCoreRateLimit
- BCrypt.Net
- Swagger/OpenAPI
