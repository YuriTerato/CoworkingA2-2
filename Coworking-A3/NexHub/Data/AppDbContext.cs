using Microsoft.EntityFrameworkCore;
using NexHub.Models;

namespace NexHub.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Usuario> Usuarios { get; set; }
    public DbSet<EspacoDeTrabalho> Espacos { get; set; }
    public DbSet<Reserva> Reservas { get; set; }
}
