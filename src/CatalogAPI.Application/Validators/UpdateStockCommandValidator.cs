using CatalogAPI.Application.Commands;
using FluentValidation;

namespace CatalogAPI.Application.Validators;

public class UpdateStockCommandValidator : AbstractValidator<UpdateStockCommand>
{
    public UpdateStockCommandValidator()
    {
        RuleFor(x => x.GameId)
            .NotEmpty().WithMessage("GameId é obrigatório");

        RuleFor(x => x.Quantity)
            .NotEqual(0).WithMessage("Quantidade não pode ser zero");
    }
}
