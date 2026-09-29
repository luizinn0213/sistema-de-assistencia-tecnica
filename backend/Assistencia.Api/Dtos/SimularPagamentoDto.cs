using System.ComponentModel.DataAnnotations;

namespace Assistencia.Api.Dtos;

public class SimularPagamentoDto
{
    [Required(ErrorMessage = "A forma de pagamento é obrigatória.")]
    public string FormaPagamento { get; set; }
        = "Pix";
}