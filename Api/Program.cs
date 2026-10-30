using Api.Servicos;
using Application.Configuration;
using Application.Interfaces;
using Application.Services;
using Domain.Interfaces;
using Infrastructure.Context;
using Infrastructure.Email;
using Infrastructure.Pdf;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Seeders;
using Infrastructure.Repositories;
using Infrastructure.Security;
using Infrastructure.Services;
using Infrastructure.Storage;
using Infrastructure.Time;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

var origensPermitidas = builder.Configuration.GetSection("Cors:Origens").Get<string[]>() ?? Array.Empty<string>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("PoliticaDoFront", policy =>
    {
        if (origensPermitidas.Length > 0)
            policy.WithOrigins(origensPermitidas).AllowAnyHeader().AllowAnyMethod();
        else
            policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<ConfiguracoesSmtp>(builder.Configuration.GetSection("Smtp"));
builder.Services.AddScoped<IEmailService, EmailService>();

builder.Services.AddScoped<ICodigoRecuperacaoSenhaRepository, CodigoRecuperacaoSenhaRepository>();
builder.Services.AddScoped<IRecuperacaoSenhaService, RecuperacaoSenhaService>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();

builder.Services.AddScoped<ISecurityService, BCryptoSecurityService>();
builder.Services.AddScoped<IProfessorService, ProfessorService>();
builder.Services.AddScoped<IProfessorRepository, ProfessorRepository>();
builder.Services.AddScoped<ITurmaService, TurmaService>();
builder.Services.AddScoped<ITurmaRepository, TurmaRepository>();

builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITokenService, JwtTokenService>();

builder.Services.AddScoped<IAlunoRepository, AlunoRepository>();
builder.Services.AddScoped<IAlunoService, AlunoService>();
builder.Services.AddScoped<IArmazenamentoArquivoService, ArmazenamentoLocalService>();

builder.Services.AddScoped<ICategoriaRepository, CategoriaRepository>();
builder.Services.AddScoped<ICategoriaService, CategoriaService>();

builder.Services.AddScoped<ICoordenadorRepository, CoordenadorRepository>();
builder.Services.AddScoped<ICoordenadorService, CoordenadorService>();
builder.Services.AddScoped<ICampeonatoRepository, CampeonatoRepository>();
builder.Services.AddScoped<ICampeonatoService, CampeonatoService>();
builder.Services.AddScoped<ICampeonatoCompeticaoRepository, CampeonatoCompeticaoRepository>();
builder.Services.AddScoped<ICampeonatoCompeticaoService, CampeonatoCompeticaoService>();

builder.Services.AddScoped<IPresencaRepository, PresencaRepository>();
builder.Services.AddScoped<IPresencaService, PresencaService>();

builder.Services.AddScoped<IFinanceiroService, FinanceiroService>();
builder.Services.AddSingleton(builder.Configuration.GetSection("CobrancaAutomatica").Get<ConfiguracoesCobrancaAutomatica>()
    ?? new ConfiguracoesCobrancaAutomatica());
builder.Services.AddScoped<IRotinaFinanceiraService, RotinaFinanceiraService>();
builder.Services.AddHostedService<RotinaFinanceiraEmSegundoPlano>();
builder.Services.AddScoped<IAvisoCobrancaRepository, AvisoCobrancaRepository>();
builder.Services.AddScoped<IWhatsAppService, WhatsAppService>();

builder.Services.AddSingleton<IRelogio, RelogioBrasil>();
builder.Services.AddSingleton(builder.Configuration.GetSection("Presenca").Get<ConfiguracoesPresenca>()
    ?? new ConfiguracoesPresenca());

builder.Services.AddScoped<IReposicaoRepository, ReposicaoRepository>();
builder.Services.AddScoped<IReposicaoService, ReposicaoService>();

builder.Services.AddSingleton(builder.Configuration.GetSection("Contrato").Get<ConfiguracoesContrato>()
    ?? throw new InvalidOperationException("Configuração 'Contrato' não encontrada em appsettings."));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IContratoRepository, ContratoRepository>();
builder.Services.AddScoped<IParcelaRepository, ParcelaRepository>();
builder.Services.AddScoped<IContratoService, ContratoService>();
builder.Services.AddScoped<IContratoPdfService, ContratoPdfService>();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);

var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrWhiteSpace(jwtKey) || jwtKey.Length < 32)
{
    throw new InvalidOperationException(
        "Configure Jwt:Key com pelo menos 32 caracteres (user-secrets ou variável de ambiente Jwt__Key).");
}

builder.Services.AddRateLimiter(opcoes =>
{
    opcoes.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    opcoes.AddPolicy("autenticacao", contextoHttp =>
        RateLimitPartition.GetFixedWindowLimiter(
            contextoHttp.Connection.RemoteIpAddress?.ToString() ?? "desconhecido",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }));
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

if (builder.Configuration.GetValue<bool>("Banco:MigrarAoIniciar"))
{
    using var escopoDaMigration = app.Services.CreateScope();
    escopoDaMigration.ServiceProvider.GetRequiredService<AppDbContext>().Database.Migrate();
}

try
{
    using var escopoDaNormalizacao = app.Services.CreateScope();
    var contextoDoBanco = escopoDaNormalizacao.ServiceProvider.GetRequiredService<AppDbContext>();
    var turmasSemConversao = await TurmaDiasSemanaNormalizador.NormalizarAsync(contextoDoBanco);
    foreach (var descricaoDaTurma in turmasSemConversao)
    {
        app.Logger.LogWarning("Turma com dias da semana não reconhecidos, corrigir manualmente: {Turma}", descricaoDaTurma);
    }
}
catch (Exception excecao)
{
    app.Logger.LogWarning(excecao, "Não foi possível padronizar os dias da semana das turmas. Aplique as migrations e reinicie.");
}

if (app.Environment.IsDevelopment() || builder.Configuration.GetValue<bool>("Swagger:Habilitado"))
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (app.Environment.IsDevelopment() || builder.Configuration.GetValue<bool>("Seed:Habilitado"))
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var securityService = scope.ServiceProvider.GetRequiredService<ISecurityService>();

    var emailDoCoordenador = builder.Configuration["UsuariosIniciais:EmailDoCoordenador"]
        ?? throw new InvalidOperationException("Configure UsuariosIniciais:EmailDoCoordenador.");
    var emailDoProfessorDeExemplo = app.Environment.IsDevelopment()
        ? builder.Configuration["UsuariosIniciais:EmailDoProfessor"]
        : null;
    var senhaInicial = builder.Configuration["UsuariosIniciais:SenhaInicial"]
        ?? (app.Environment.IsDevelopment()
            ? "user123"
            : throw new InvalidOperationException("Configure UsuariosIniciais:SenhaInicial."));

    UsuarioSeeder.Seed(dbContext, securityService, emailDoCoordenador, emailDoProfessorDeExemplo, senhaInicial);
    await CategoriaSeeder.SeedAsync(dbContext);

    if (builder.Configuration.GetValue<bool>("Seed:DadosDeDemonstracao"))
    {
        var relogio = scope.ServiceProvider.GetRequiredService<IRelogio>();
        var senhaDosProfessores = builder.Configuration["Seed:SenhaDosProfessoresDeDemonstracao"] ?? "professor123";
        var resumo = await DadosDeDemonstracaoSeeder.SeedAsync(dbContext, securityService, relogio.HojeNoBrasil(), senhaDosProfessores);
        app.Logger.LogInformation("Dados de demonstração: {Resumo}", resumo);
    }
}

app.UseStaticFiles();
app.UseCors("PoliticaDoFront");

app.UseExceptionHandler(aplicacaoDeErro => aplicacaoDeErro.Run(async contexto =>
{
    contexto.Response.StatusCode = StatusCodes.Status500InternalServerError;
    await contexto.Response.WriteAsJsonAsync(new
    {
        Message = "Erro interno ao processar a requisição. Tente novamente.",
        Status = false,
        Data = (object?)null
    });
}));
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();