using MediatR;
using Microsoft.AspNetCore.Mvc;
using RecipeShopper.Features.Recipes;

namespace RecipeShopper.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RecipeController : ControllerBase
    {
        private readonly IMediator _mediator;

        public RecipeController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<ActionResult<List<GetRecipes.Model>>> GetRecipes([FromQuery] int[] ids)
        {
            if (ids == null) return BadRequest("Invalid 'ids' value");

            var query = new GetRecipes.Query(ids);
            var resp = await _mediator.Send(query);

            return Ok(resp.Results);
        }

        [HttpPost]
        public async Task<ActionResult> CreateRecipe([FromBody]CreateRecipe.Command recipe)
        {
            var resp = await _mediator.Send(recipe);

            return CreatedAtAction(nameof(GetRecipes), new { ids = new[] { resp } }, null);
        }

        [HttpPut]
        public async Task<ActionResult> UpdateRecipe(UpdateRecipe.Command recipe)
        {
            await _mediator.Send(recipe);

            return Ok();
        }

        [HttpDelete]
        public async Task<ActionResult> DeleteRecipe(int id)
        {
            var cmd = new DeleteRecipe.Command(id);
            await _mediator.Send(cmd);

            return Ok();
        }
    }
}
