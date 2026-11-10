namespace NexHub.Models;

public class Usuario
{
    public int Id { get; set; }
    public string Nome { get; set; }
    public string Cpf { get; set; }
    public string Email { get; set; }
    public string Telefone { get; set; }

    public string Validar()
    {
        if (string.IsNullOrWhiteSpace(Nome))
            return "Nome nao pode ser vazio.";

        if (string.IsNullOrWhiteSpace(Cpf) || Cpf.Length != 11 || !Cpf.All(char.IsDigit))
            return "CPF deve ter exatamente 11 numeros, sem pontos ou traco.";

        if (string.IsNullOrWhiteSpace(Email) || !Email.Contains("@") || !Email.Contains("."))
            return "Email invalido.";

        if (string.IsNullOrWhiteSpace(Telefone))
            return "Telefone nao pode ser vazio.";

        return null;
    }
}
