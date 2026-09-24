using System;
using System.Collections.Generic;

namespace Assistencia.Api.Models;

public class Especialidade
{
    public int Id { get; private set; }
    public string Nome { get; private set; } = string.Empty;
    public string Descricao { get; private set; } = string.Empty;
    public bool Ativa { get; private set; }

    public ICollection<TecnicoEspecialidade> TecnicoEspecialidades { get; private set; }
        = new List<TecnicoEspecialidade>();

    // Usado pelo Entity Framework
    public Especialidade()
    {
    }

    public Especialidade(string nome, string descricao)
    {
        AtualizarDados(nome, descricao);
        Ativa = true;
    }

    public void AtualizarDados(string nome, string descricao)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException(
                "O nome da especialidade é obrigatório.");

        Nome = nome;
        Descricao = descricao;
    }

    public void Ativar()
    {
        Ativa = true;
    }

    public void Inativar()
    {
        Ativa = false;
    }
}