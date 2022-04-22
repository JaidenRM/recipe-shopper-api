using MediatR;
using Microsoft.AspNetCore.Mvc;
using RecipeShopper.Domain.Enums;
using RecipeShopper.Features.Supermarket;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace RecipeShopper.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SupermarketController : ControllerBase
    {
        private readonly IMediator _mediator;

        public SupermarketController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("{supermarketId:int}/{searchTerm}")]
        public async Task<ActionResult<List<SearchSupermarket.Model>>> SearchSupermarket(int supermarketId, string searchTerm)
        {
            if (!Enum.IsDefined(typeof(SupermarketType), supermarketId)) return BadRequest("Invalid supermarket id");

            var supermarketType = (SupermarketType)supermarketId;
            var query = new SearchSupermarket.Query(searchTerm, new[] { supermarketType });
            var resp = await _mediator.Send(query);

            return resp.ProductsBySupermarketDict[supermarketType];
        }
    }
}
