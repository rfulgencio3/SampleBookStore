# SampleBookStore
API construída com .NET 8 Minimal API, utilizando Entity Framework Core InMemory, seguindo princípios RESTful com HATEOAS e paginação.
Projeto criado com objetivo didático para demonstrar organização de solution, separação de contratos (Request/Response) e boas práticas básicas de API moderna.

## Estrutura da Solution
```
SampleBookStore/
│
├── SampleBookStore.sln
├── README.md
└── src/
    └── SampleBookStoreApi/
        ├── Data/
        ├── Models/
        ├── Dtos/
        │   ├── Requests/
        │   └── Responses/
        └── Program.cs
```

## Tecnologias Utilizadas

.NET 8
Minimal API
Entity Framework Core
EF Core InMemory Database
Swagger (Swashbuckle)
GUID como identificador

## Conceitos Aplicados
### Minimal API
Uso direto de:
MapGet
MapPost
MapPut
MapDelete
Sem utilização de Controllers.
EF Core InMemory Database

Configuração:
```csharp
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseInMemoryDatabase("SampleBookStoreDb"));
```

### Características:
Armazena dados apenas em memória
Dados são perdidos ao reiniciar a aplicação
Ideal para testes, demonstrações e POC
Não indicado para produção

### RESTful Design

| Verbo  | Endpoint              | Descrição            |
|--------|-----------------------|----------------------|
| GET    | /api/books            | Lista paginada       |
| GET    | /api/books/{id}       | Busca por Id         |
| POST   | /api/books            | Criação de livro     |
| PUT    | /api/books/{id}       | Atualização por Id   |
| DELETE | /api/books/{id}       | Remoção por Id       |

### HATEOAS
Cada recurso retorna links de navegação:
```json
{
  "id": "guid",
  "title": "Clean Architecture",
  "links": [
    { "rel": "self", "href": "...", "method": "GET" },
    { "rel": "update", "href": "...", "method": "PUT" },
    { "rel": "delete", "href": "...", "method": "DELETE" }
  ]
}
```
Paginação também retorna links self, prev, next e create.

## Como Executar
1. Restaurar dependências
dotnet restore
2. Executar aplicação
cd src/SampleBookStoreApi
dotnet run

 ## Swagger
A API abre automaticamente em:
https://localhost:{porta}/
ou
https://localhost:{porta}/swagger

## Paginação

Exemplo:
```
GET /api/books?page=1&pageSize=10
```

#### Parâmetros:
```
page → padrão: 1
pageSize → padrão: 10 (máximo 100)
```

#### Resposta inclui:
```
Items
Page
PageSize
TotalCount
TotalPages
Links de navegação
```

## Exemplo de Criação
```
POST /api/books
```

```json
Body:
{
  "title": "Clean Code",
  "author": "Robert C. Martin",
  "year": 2008
}
```

## Melhorias Futuras
* Versionamento de API (v1)
* FluentValidation
* Testes unitários
* Banco relacional (SQLite / PostgreSQL)
* Clean Architecture
* CQRS

## Licença

Projeto desenvolvido para fins educacionais.
