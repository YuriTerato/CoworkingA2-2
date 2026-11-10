namespace NexHub.Models;

public class Reserva
{
    public int Id { get; set; }

    public int UsuarioId { get; set; }
    public Usuario Usuario { get; set; }

    public int EspacoId { get; set; }
    public EspacoDeTrabalho Espaco { get; set; }

    public DateTime DataHoraInicio { get; set; }
    public DateTime DataHoraFim { get; set; }
    public int QuantidadePessoas { get; set; }
    public decimal ValorTotal { get; set; }
    public StatusReserva Status { get; set; }

    public void CalcularValorTotal()
    {
        double horas = (DataHoraFim - DataHoraInicio).TotalHours;
        ValorTotal = Math.Round(Espaco.ValorPorHora * (decimal)horas, 2);
    }
}
