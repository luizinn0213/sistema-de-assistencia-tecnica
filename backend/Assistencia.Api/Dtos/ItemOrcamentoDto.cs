using System.ComponentModel.DataAnnotations;
using Assistencia.Api.Enums;

namespace Assistencia.Api.Dtos;

public class ItemOrcamentoDto
{
    [Required(
        ErrorMessage =
            "A descrição do item é obrigatória.")]
    [StringLength(
        500,
        ErrorMessage =
            "A descrição deve possuir no máximo 500 caracteres.")]
    public string Descricao { get; set; }
        = string.Empty;

    [EnumDataType(
        typeof(TipoItemOrcamento),
        ErrorMessage = "O tipo do item é inválido.")]
    public TipoItemOrcamento Tipo { get; set; }

    [Range(
        1,
        int.MaxValue,
        ErrorMessage =
            "A quantidade deve ser maior que zero.")]
    public int Quantidade { get; set; }

    [Range(
        typeof(decimal),
        "0",
        "9999999999999999.99",
        ErrorMessage =
            "O valor unitário não pode ser negativo.")]
    public decimal ValorUnitario { get; set; }
}