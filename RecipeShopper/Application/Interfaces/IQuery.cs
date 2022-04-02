using MediatR;

namespace RecipeShopper.Application.Interfaces
{
    public interface IQuery<out TResponse> : IRequest<TResponse>
    {
    }
}
