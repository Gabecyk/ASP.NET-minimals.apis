using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MinimalApi.Dominio.Interfaces;
using MinimalApi.Dominio.ModelViews;
using MinimalApi.Dominio.Servicos;
using MinimalApi.DTOs;
using MinimalApi.Infraestrutura.Db;
using MinimalApi.Dominio.Entidades;

# region Builder

var builder = WebApplication.CreateBuilder(args);

//ADD Serviços
builder.Services.AddScoped<IAdministradorSevico, AdministradorServico>();
builder.Services.AddScoped<IVeiculoSevico, VeiculoServico>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<DbContexto>(options => {
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

#endregion

# region Veiculos

app.MapPost("/veiculos", ([FromBody] VeiculoDTO veiculoDTO, IVeiculoSevico veiculoSevico) =>
{
    var veiculo = new Veiculo
    {
        Nome = veiculoDTO.Nome,
        Marca = veiculoDTO.Marca,
        Ano = veiculoDTO.Ano
    };
    veiculoSevico.Incluir(veiculo);

    return Results.Created($"/veiculo/{veiculo.Id}", veiculo);
}).WithTags("Veiculos");

app.MapGet("/veiculos", ([FromQuery]int? pagina, IVeiculoSevico veiculoSevico) =>
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

app.MapPut("/veiculos/{id}", ([FromRoute]int id, VeiculoDTO veiculoDTO, IVeiculoSevico veiculoSevico) =>
{
    var veiculo = veiculoSevico.BuscaPorId(id);

    if (veiculo == null)
        return Results.NotFound();
    
    veiculo.Nome = veiculoDTO.Nome;
    veiculo.Marca = veiculoDTO.Marca;
    veiculo.Ano = veiculoDTO.Ano;

    veiculoSevico.Atualizar(veiculo);

    return Results.Ok(veiculo);
}).WithTags("Veiculos");

app.MapDelete("/veiculos/{id}", ([FromRoute]int id, IVeiculoSevico veiculoSevico) =>
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