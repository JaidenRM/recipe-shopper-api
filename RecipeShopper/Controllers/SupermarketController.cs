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

        /// <summary>
        /// Searches a specific supermarket for the `searchTerm` provided
        /// </summary>
        /// <param name="supermarketId">The internal id used to represent a specific supermarket</param>
        /// <param name="searchTerm">The phrase you wish to search for at the specified supermarket</param>
        /// <returns>A list of products found at the supermarket based on the `searchTerm`</returns>
        /// <response code="200">Contains a list of products in the response</response>
        /// <response code="400">If the specified supermarket is not found</response>
        [HttpGet("{supermarketId:int}/{searchTerm}")]
        public async Task<ActionResult<List<SearchSupermarket.Model>>> SearchSupermarket(int supermarketId, string searchTerm)
        {
            if (!Enum.IsDefined(typeof(SupermarketType), supermarketId)) return BadRequest("Invalid supermarket id");

            var supermarketType = (SupermarketType)supermarketId;
            var query = new SearchSupermarket.Query(searchTerm, new[] { supermarketType });
            var resp = await _mediator.Send(query);

            return Ok(resp.ProductsBySupermarketDict[supermarketType]);
        }
    }
}
