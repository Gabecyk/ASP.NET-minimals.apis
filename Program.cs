using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MinimalApi.Dominio.Interfaces;
using MinimalApi.Dominio.ModelViews;
using MinimalApi.Dominio.Servicos;
using MinimalApi.DTOs;
using MinimalApi.Infraestrutura.Db;
using MinimalApi.Dominio.Entidades;
using MinimalApi.Dominio.Enuns;

# region Builder

var builder = WebApplication.CreateBuilder(args);

//ADD Serviços
builder.Services.AddScoped<IAdministradorSevico, AdministradorServico>();
builder.Services.AddScoped<IVeiculoSevico, VeiculoServico>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<DbContexto>(options =>
{
    options.UseMySql(
        builder.Configuration.GetConnectionString("mysql"), ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("mysql"))
    );
});

var app = builder.Build();

#endregion

# region Home

app.MapGet("/", () => Results.Json(new Home())).WithTags("Home");

#endregion

# region Administradores

app.MapPost("/administradores/login", ([FromBody] LoginDTO loginDTO, IAdministradorSevico administradorSevico) =>
{
    if (administradorSevico.Login(loginDTO) != null)
    {
        return Results.Ok("Login com sucesso!");
    }
    else
    {
        return Results.Unauthorized();
    }
}).WithTags("Administradores");

app.MapGet("/administradores", ([FromQuery] int? pagina, IAdministradorSevico administradorSevico) =>
{
    return Results.Ok(administradorSevico.Todos(pagina));
}).WithTags("Administradores");

app.MapGet("/administradores/{id}", ([FromRoute] int id, IAdministradorSevico administradorSevico) =>
{
    var administrador = administradorSevico.BuscaPorId(id);

    if (administrador == null)
        return Results.NotFound();


    return Results.Ok(administrador);
}).WithTags("Administradores");

app.MapPost("/administradores", ([FromBody] AdministradorDTO administradorDTO, IAdministradorSevico administradorSevico) =>
{
    var validacao = new ErrosDeValidacao
    {
        Mensagens = new List<string>()
    };

    if (string.IsNullOrEmpty(administradorDTO.Email))
        validacao.Mensagens.Add("Email não pode ser vazio");

    if (string.IsNullOrEmpty(administradorDTO.Senha))
        validacao.Mensagens.Add("Senha não pode ser vazia");

    if (administradorDTO.Perfil == null)
        validacao.Mensagens.Add("Perfil não pode ser vazio");

    if (validacao.Mensagens.Count > 0)
        return Results.BadRequest(validacao);

    var adm = new Administrador
    {
        Email = administradorDTO.Email,
        Senha = administradorDTO.Senha,
        Perfil = administradorDTO.Perfil.ToString() ?? Perfil.editor.ToString()
    };
    administradorSevico.Incluir(adm);

    return Results.Created($"/administrador/{adm.Id}", adm);

}).WithTags("Administradores");

#endregion

# region Veiculos
ErrosDeValidacao validaDTO(VeiculoDTO veiculoDTO)
{
    var validacao = new ErrosDeValidacao
    {
        Mensagens = new List<string>()
    };

    if (string.IsNullOrEmpty(veiculoDTO.Nome))
        validacao.Mensagens.Add("O nome não pode ser vazio");

    if (string.IsNullOrEmpty(veiculoDTO.Marca))
        validacao.Mensagens.Add("A marca não pode ser vazio");

    if (veiculoDTO.Ano < 1950)
        validacao.Mensagens.Add("O ano não pode ser menor que 1950");

    return validacao;
}

app.MapPost("/veiculos", ([FromBody] VeiculoDTO veiculoDTO, IVeiculoSevico veiculoSevico) =>
{

    var validacao = validaDTO(veiculoDTO);
    if (validacao.Mensagens.Count > 0)
        return Results.BadRequest(validacao);

    var veiculo = new Veiculo
    {
        Nome = veiculoDTO.Nome,
        Marca = veiculoDTO.Marca,
        Ano = veiculoDTO.Ano
    };
    veiculoSevico.Incluir(veiculo);

    return Results.Created($"/veiculo/{veiculo.Id}", veiculo);
}).WithTags("Veiculos");

app.MapGet("/veiculos", ([FromQuery] int? pagina, IVeiculoSevico veiculoSevico) =>
{
    var veiculos = veiculoSevico.Todos(pagina);

    return Results.Ok(veiculos);
}).WithTags("Veiculos");

app.MapGet("/veiculos/{id}", ([FromRoute] int id, IVeiculoSevico veiculoSevico) =>
{
    var veiculo = veiculoSevico.BuscaPorId(id);

    if (veiculo == null)
        return Results.NotFound();


    return Results.Ok(veiculo);
}).WithTags("Veiculos");

app.MapPut("/veiculos/{id}", ([FromRoute] int id, VeiculoDTO veiculoDTO, IVeiculoSevico veiculoSevico) =>
{
    var veiculo = veiculoSevico.BuscaPorId(id);

    if (veiculo == null)
        return Results.NotFound();

    var validacao = validaDTO(veiculoDTO);
    if (validacao.Mensagens.Count > 0)
        return Results.BadRequest(validacao);

    veiculo.Nome = veiculoDTO.Nome;
    veiculo.Marca = veiculoDTO.Marca;
    veiculo.Ano = veiculoDTO.Ano;

    veiculoSevico.Atualizar(veiculo);

    return Results.Ok(veiculo);
}).WithTags("Veiculos");

app.MapDelete("/veiculos/{id}", ([FromRoute] int id, IVeiculoSevico veiculoSevico) =>
{
    var veiculo = veiculoSevico.BuscaPorId(id);

    if (veiculo == null)
        return Results.NotFound();

    veiculoSevico.Apagar(veiculo);

    return Results.NoContent();
}).WithTags("Veiculos");

#endregion

# region App
// Swagger
app.UseSwagger();
app.UseSwaggerUI();

app.Run();

#endregion