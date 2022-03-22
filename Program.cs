using Microsoft.EntityFrameworkCore;
using MinimalAPI.Data;
using MinimalAPI.Models;
using MiniValidation;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<MinimalContextDb>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/fornecedor", async (
    MinimalContextDb context) =>
    await context.Fornecedores.ToListAsync())
    .WithName("GetFornecedor")
    .WithTags("Fornecedor");

app.MapGet("/fornecedor/{id}", async (
        Guid id,
        MinimalContextDb context) =>
    await context.Fornecedores.FindAsync(id)
        is Fornecedor fornecedor
            ? Results.Ok(fornecedor)
            : Results.NotFound())
    .Produces<Fornecedor>(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status404NotFound)
    .WithName("GetFornecedorPorId")
    .WithTags("Fornecedor");

app.MapPost("/fornecedor", async (
    MinimalContextDb context,
    Fornecedor fornecedor) =>
    {
        if (!MiniValidator.TryValidate(fornecedor, out var errors))
            return Results.ValidationProblem(errors);

        context.Fornecedores.Add(fornecedor);
        var result = await context.SaveChangesAsync();

        return result > 0
            ? Results.CreatedAtRoute("GetFornecedorPorId", new { id = fornecedor.Id }, fornecedor)
            : Results.BadRequest("Houve um problema ao salvar o registro");
    })
  .ProducesValidationProblem()
  .Produces<Fornecedor>(StatusCodes.Status201Created)
  .Produces(StatusCodes.Status400BadRequest)
  .WithName("PostFornecedor")
  .WithTags("Fornecedor");

app.MapPut("/fornecedor/{id}", async (
        Guid id,
        MinimalContextDb context,
        Fornecedor fornecedor) =>
    {
        var fornecedorBanco = await context.Fornecedores
            .AsNoTracking<Fornecedor>()
            .FirstOrDefaultAsync(f => f.Id == id);
        if (fornecedorBanco == null) return Results.BadRequest();

        if (!MiniValidator.TryValidate(fornecedor, out var errors))
            return Results.ValidationProblem(errors);

        context.Fornecedores.Update(fornecedor);
        var result = await context.SaveChangesAsync();
        return result > 0
            ? Results.NoContent()
            : Results.BadRequest("Houve um problema ao salvar o registro");
    })
    .ProducesValidationProblem()
    .Produces(StatusCodes.Status204NoContent)
    .Produces(StatusCodes.Status400BadRequest)
    .WithName("PutFornecedor")
    .WithTags("Fornecedor");

app.Run();
