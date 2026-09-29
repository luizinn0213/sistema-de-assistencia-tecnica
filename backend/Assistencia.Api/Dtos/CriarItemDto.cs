using System.ComponentModel.DataAnnotations;
using Assistencia.Api.Enums;

namespace Assistencia.Api.Dtos;

public class CriarItemDto
{
    [Required(ErrorMessage = "A descrição é obrigatória.")]
    public string Descricao { get; set; }
        = string.Empty;

    [EnumDataType(typeof(TipoItemOrcamento))]
    public TipoItemOrcamento Tipo { get; set; }

    [Range(
        1,
        int.MaxValue,
        ErrorMessage = "A quantidade deve ser maior que zero.")]
    public int Quantidade { get; set; }

    [Range(
        0.01,
        999999999.0,
        ErrorMessage = "O valor unitário deve ser maior que zero.")]
    public decimal ValorUnitario { get; set; }
}