namespace NexHub.Models;

public class EspacoDeTrabalho
{
    public int Id { get; set; }
    public string NomeSala { get; set; }
    public int Capacidade { get; set; }
    public decimal ValorPorHora { get; set; }
    public bool Disponivel { get; set; } = true;
    public TipoEspaco Tipo { get; set; }

    // devolve a mensagem de erro, ou null quando esta tudo certo
    public string Validar()
    {
        if (string.IsNullOrWhiteSpace(NomeSala))
            return "Nome do espaco nao pode ser vazio.";

        if (Capacidade <= 0)
            return "Capacidade deve ser maior que zero.";

        if (ValorPorHora <= 0)
            return "Valor por hora deve ser maior que zero.";

        if (!Enum.IsDefined(typeof(TipoEspaco), Tipo))
            return "Tipo de espaco invalido.";

        return null;
    }
}
