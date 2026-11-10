using Microsoft.EntityFrameworkCore;
using NexHub.Data;
using NexHub.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlite("Data Source=nexhub.db");
});

var app = builder.Build();

const int HoraAbertura = 8;
const int HoraFechamento = 22;
const double DuracaoMinimaHoras = 0.5;
const double DuracaoMaximaHoras = 8;
const int LimiteReservasAtivas = 3;

app.MapGet("/", () => "NexHub Coworking API");

app.MapGet("/usuario/listar", (AppDbContext context) =>
{
    return context.Usuarios.ToList();
});

app.MapGet("/usuario/buscar/{id}", (AppDbContext context, int id) =>
{
    var usuario = context.Usuarios.Find(id);

    if (usuario == null)
        return Results.NotFound(new { erro = "Usuario nao encontrado." });

    return Results.Ok(usuario);
});

app.MapPost("/usuario/cadastrar", (AppDbContext context, Usuario usuario) =>
{
    string erro = usuario.Validar();
    if (erro != null)
        return Results.BadRequest(new { erro });

    if (context.Usuarios.Any(usuarioAtual => usuarioAtual.Cpf == usuario.Cpf))
        return Results.Conflict(new { erro = "Ja existe um usuario com esse CPF." });

    if (context.Usuarios.Any(usuarioAtual => usuarioAtual.Email == usuario.Email))
        return Results.Conflict(new { erro = "Ja existe um usuario com esse email." });

    context.Usuarios.Add(usuario);
    context.SaveChanges();

    return Results.Created($"/usuario/buscar/{usuario.Id}", usuario);
});

app.MapPut("/usuario/atualizar/{id}", (AppDbContext context, int id, Usuario dados) =>
{
    var usuario = context.Usuarios.Find(id);

    if (usuario == null)
        return Results.NotFound(new { erro = "Usuario nao encontrado." });

    dados.Cpf = usuario.Cpf;

    string erro = dados.Validar();
    if (erro != null)
        return Results.BadRequest(new { erro });

    if (context.Usuarios.Any(usuarioAtual => usuarioAtual.Email == dados.Email && usuarioAtual.Id != id))
        return Results.Conflict(new { erro = "Ja existe um usuario com esse email." });

    usuario.Nome = dados.Nome;
    usuario.Email = dados.Email;
    usuario.Telefone = dados.Telefone;
    context.SaveChanges();

    return Results.Ok(usuario);
});

app.MapDelete("/usuario/deletar/{id}", (AppDbContext context, int id) =>
{
    var usuario = context.Usuarios.Find(id);

    if (usuario == null)
        return Results.NotFound(new { erro = "Usuario nao encontrado." });

    if (context.Reservas.Any(reservaAtual => reservaAtual.UsuarioId == id))
        return Results.Conflict(new { erro = "Usuario possui reservas e nao pode ser excluido." });

    context.Usuarios.Remove(usuario);
    context.SaveChanges();

    return Results.Ok();
});

app.MapGet("/usuario/reservas/{id}", (AppDbContext context, int id) =>
{
    if (!context.Usuarios.Any(usuarioAtual => usuarioAtual.Id == id))
        return Results.NotFound(new { erro = "Usuario nao encontrado." });

    var reservas = context.Reservas
        .Include(reservaAtual => reservaAtual.Usuario)
        .Include(reservaAtual => reservaAtual.Espaco)
        .Where(reservaAtual => reservaAtual.UsuarioId == id)
        .ToList();

    return Results.Ok(reservas);
});

app.MapGet("/usuario/faturamento/{id}", (AppDbContext context, int id) =>
{
    var usuario = context.Usuarios.Find(id);

    if (usuario == null)
        return Results.NotFound(new { erro = "Usuario nao encontrado." });

    var reservas = context.Reservas
        .Include(reservaAtual => reservaAtual.Usuario)
        .Include(reservaAtual => reservaAtual.Espaco)
        .Where(reservaAtual => reservaAtual.UsuarioId == id && reservaAtual.Status == StatusReserva.Confirmada)
        .ToList();

    decimal totalGasto = reservas.Sum(reservaAtual => reservaAtual.ValorTotal);

    return Results.Ok(new
    {
        Usuario = usuario.Nome,
        QuantidadeReservas = reservas.Count,
        TotalGasto = totalGasto,
        Reservas = reservas
    });
});


app.MapGet("/espaco/listar", (AppDbContext context) =>
{
    return context.Espacos.ToList();
});

app.MapGet("/espaco/disponiveis", (AppDbContext context) =>
{
    return context.Espacos.Where(espacoAtual => espacoAtual.Disponivel).ToList();
});

app.MapGet("/espaco/buscar/{id}", (AppDbContext context, int id) =>
{
    var espaco = context.Espacos.Find(id);

    if (espaco == null)
        return Results.NotFound(new { erro = "Espaco nao encontrado." });

    return Results.Ok(espaco);
});

app.MapPost("/espaco/cadastrar", (AppDbContext context, EspacoDeTrabalho espaco) =>
{
    string erro = espaco.Validar();
    if (erro != null)
        return Results.BadRequest(new { erro });

    context.Espacos.Add(espaco);
    context.SaveChanges();

    return Results.Created($"/espaco/buscar/{espaco.Id}", espaco);
});

app.MapPut("/espaco/atualizar/{id}", (AppDbContext context, int id, EspacoDeTrabalho dados) =>
{
    var espaco = context.Espacos.Find(id);

    if (espaco == null)
        return Results.NotFound(new { erro = "Espaco nao encontrado." });

    string erro = dados.Validar();
    if (erro != null)
        return Results.BadRequest(new { erro });

    espaco.NomeSala = dados.NomeSala;
    espaco.Capacidade = dados.Capacidade;
    espaco.ValorPorHora = dados.ValorPorHora;
    espaco.Disponivel = dados.Disponivel;
    espaco.Tipo = dados.Tipo;
    context.SaveChanges();

    return Results.Ok(espaco);
});

app.MapDelete("/espaco/deletar/{id}", (AppDbContext context, int id) =>
{
    var espaco = context.Espacos.Find(id);

    if (espaco == null)
        return Results.NotFound(new { erro = "Espaco nao encontrado." });

    if (context.Reservas.Any(reservaAtual => reservaAtual.EspacoId == id))
        return Results.Conflict(new { erro = "Espaco possui reservas e nao pode ser excluido." });

    context.Espacos.Remove(espaco);
    context.SaveChanges();

    return Results.Ok();
});


app.MapGet("/reserva/listar", (AppDbContext context) =>
{
    return context.Reservas
        .Include(reservaAtual => reservaAtual.Usuario)
        .Include(reservaAtual => reservaAtual.Espaco)
        .ToList();
});

app.MapGet("/reserva/buscar/{id}", (AppDbContext context, int id) =>
{
    var reserva = context.Reservas
        .Include(reservaAtual => reservaAtual.Usuario)
        .Include(reservaAtual => reservaAtual.Espaco)
        .FirstOrDefault(reservaAtual => reservaAtual.Id == id);

    if (reserva == null)
        return Results.NotFound(new { erro = "Reserva nao encontrada." });

    return Results.Ok(reserva);
});

app.MapPost("/reserva/cadastrar", (AppDbContext context, Reserva reserva) =>
{
    var usuario = context.Usuarios.Find(reserva.UsuarioId);
    if (usuario == null)
        return Results.NotFound(new { erro = "Usuario nao encontrado." });

    var espaco = context.Espacos.Find(reserva.EspacoId);
    if (espaco == null)
        return Results.NotFound(new { erro = "Espaco nao encontrado." });

    if (!espaco.Disponivel)
        return Results.Conflict(new { erro = "Espaco indisponivel no momento." });

    DateTime inicio = reserva.DataHoraInicio;
    DateTime fim = reserva.DataHoraFim;
    DateTime agora = DateTime.Now;

    if (inicio < agora)
        return Results.BadRequest(new { erro = "Nao e possivel reservar um horario que ja passou." });

    if (fim <= inicio)
        return Results.BadRequest(new { erro = "O horario de fim deve ser maior que o horario de inicio." });

    if (inicio.Date != fim.Date)
        return Results.BadRequest(new { erro = "A reserva deve comecar e terminar no mesmo dia." });

    if (inicio < inicio.Date.AddHours(HoraAbertura) || fim > inicio.Date.AddHours(HoraFechamento))
        return Results.BadRequest(new { erro = $"O coworking funciona das {HoraAbertura}h as {HoraFechamento}h." });

    double horas = (fim - inicio).TotalHours;

    if (horas < DuracaoMinimaHoras)
        return Results.BadRequest(new { erro = "A reserva deve ter no minimo 30 minutos." });

    if (horas > DuracaoMaximaHoras)
        return Results.BadRequest(new { erro = $"A reserva deve ter no maximo {DuracaoMaximaHoras} horas." });

    // 3. quantidade de pessoas
    if (reserva.QuantidadePessoas <= 0)
        return Results.BadRequest(new { erro = "A quantidade de pessoas deve ser maior que zero." });

    if (reserva.QuantidadePessoas > espaco.Capacidade)
        return Results.BadRequest(new { erro = $"O espaco comporta no maximo {espaco.Capacidade} pessoas." });

    int reservasAtivas = context.Reservas.Count(reservaAtual =>
        reservaAtual.UsuarioId == usuario.Id &&
        reservaAtual.Status == StatusReserva.Confirmada &&
        reservaAtual.DataHoraFim > agora);

    if (reservasAtivas >= LimiteReservasAtivas)
        return Results.Conflict(new { erro = $"Limite de {LimiteReservasAtivas} reservas ativas por usuario atingido." });

    bool conflito = context.Reservas.Any(reservaAtual =>
        reservaAtual.EspacoId == espaco.Id &&
        reservaAtual.Status == StatusReserva.Confirmada &&
        inicio < reservaAtual.DataHoraFim &&
        fim > reservaAtual.DataHoraInicio);

    if (conflito)
        return Results.Conflict(new { erro = "Esse espaco ja esta reservado nesse horario." });

    reserva.Usuario = usuario;
    reserva.Espaco = espaco;
    reserva.Status = StatusReserva.Confirmada;
    reserva.CalcularValorTotal();

    context.Reservas.Add(reserva);
    context.SaveChanges();

    return Results.Created($"/reserva/buscar/{reserva.Id}", reserva);
});

app.MapPut("/reserva/cancelar/{id}", (AppDbContext context, int id) =>
{
    var reserva = context.Reservas
        .Include(reservaAtual => reservaAtual.Usuario)
        .Include(reservaAtual => reservaAtual.Espaco)
        .FirstOrDefault(reservaAtual => reservaAtual.Id == id);

    if (reserva == null)
        return Results.NotFound(new { erro = "Reserva nao encontrada." });

    if (reserva.Status == StatusReserva.Cancelada)
        return Results.Conflict(new { erro = "Essa reserva ja esta cancelada." });

    if (reserva.DataHoraInicio <= DateTime.Now)
        return Results.Conflict(new { erro = "Nao e possivel cancelar uma reserva que ja comecou." });

    reserva.Status = StatusReserva.Cancelada;
    context.SaveChanges();

    return Results.Ok(reserva);
});

app.Run();
