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

        /// <summary>
        /// Fetches a list of recipes for the current user filtered by the desired ids passed
        /// </summary>
        /// <param name="ids">The ids of the recipes wanted. Use an empty array to get all the recipes for the user.</param>
        /// <returns>A list of recipes</returns>
        /// <response code="200">Contains a list of recipes filtered as per the parameters</response>
        /// <response code="400">If the `ids` parameter is null</response>
        [HttpGet]
        public async Task<ActionResult<List<GetRecipes.Model>>> GetRecipes([FromQuery] int[] ids)
        {
            if (ids == null) return BadRequest("Invalid 'ids' value");

            var query = new GetRecipes.Query(ids);
            var resp = await _mediator.Send(query);

            return Ok(resp.Results);
        }

        /// <summary>Creates a new recipe for the current user</summary>
        /// <param name="recipe">The command that has all the required information to create this new recipe</param>
        /// <returns>No data is returned</returns>
        /// <response code="201">Location of the new recipe</response>
        [HttpPost]
        public async Task<ActionResult> CreateRecipe([FromBody] CreateRecipe.Command recipe)
        {
            var resp = await _mediator.Send(recipe);

            return CreatedAtAction(nameof(GetRecipes), new { ids = new[] { resp } }, null);
        }


        /// <summary>
        ///     Used to update a specific recipe with new values. 
        ///     Existing values must be repassed to the command to be kept otherwise they will be overwritten by the command's values.
        /// </summary>
        /// <param name="id">Id of the recipe to update</param>
        /// <param name="recipe">Contains new values to update an existing recipe with</param>
        /// <returns>No data is returned</returns>
        [HttpPut("{id:int}")]
        public async Task<ActionResult> UpdateRecipe(int id, UpdateRecipe.Command recipe)
        {
            await _mediator.Send(recipe);

            return Ok();
        }

        /// <summary>Deletes a specific recipe</summary>
        /// <param name="id">Id of the recipe to delete</param>
        /// <returns>No data is returned</returns>
        [HttpDelete]
        public async Task<ActionResult> DeleteRecipe(int id)
        {
            var cmd = new DeleteRecipe.Command(id);
            await _mediator.Send(cmd);

            return Ok();
        }
    }
}
