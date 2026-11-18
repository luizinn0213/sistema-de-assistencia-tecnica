Api/
├── Api.csproj
├── Program.cs
├── Models.cs
├── DTOs.cs
├── AppDbContext.cs
├── appsettings.json
├── appsettings.Development.json
├── Properties/
│   └── launchSettings.json
├── Security/
│   ├── JwtOptions.cs
│   ├── Politicas.cs
│   ├── TokenService.cs
│   ├── AutenticacaoService.cs
│   └── UsuarioAtual.cs
├── Infra/
│   ├── TratamentoDeErrosMiddleware.cs
│   └── DbSeeder.cs
└── Controllers/
    ├── AuthController.cs
    ├── ClientesController.cs
    ├── EquipamentosController.cs
    └── UsuariosController.cs

Arquivos auxiliares: README.md, Api.http, .gitignore
