using MediatR;

namespace RecipeShopper.Application.Interfaces
{
    public interface ICommand<out TResponse> : IRequest<TResponse> { }
}
