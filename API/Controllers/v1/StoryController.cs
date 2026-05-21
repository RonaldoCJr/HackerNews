using Asp.Versioning;
using HackerNews.Application.Interfaces;
using HackerNews.Application.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace HackerNews.API.Controllers.v1;

[ApiController]
[ApiVersion("1.0")]
[Route("v{version:apiVersion}/story")]
public class StoryController : ControllerBase
{
    private readonly IStoryService _storyService;


    public StoryController(IStoryService storyService)
    {
        _storyService = storyService;
    }

    ///// <summary>
    ///// Retrieves the top 'n' stories from Hacker News, ordered by their score in descending order.
    ///// </summary>
    ///// <remarks>
    ///// This endpoint fetches all current best stories, retrieves their individual details in parallel, 
    ///// and sorts them by score to ensure accuracy. Results are cached to optimize performance.
    ///// </remarks>
    ///// <param name="n">The number of top stories to retrieve. Must be greater than zero.</param>
    ///// <returns>A list of the best stories with titles, scores, and comment counts.</returns>
    ///// <response code="200">Returns the list of best stories successfully.</response>
    ///// <response code="400">If the provided 'n' is less than or equal to zero.</response>
    ///// <response code="502">If there is an error communicating with upstream Hacker News API or Circuit Breaker triggered.</response>
    ///// <response code="504">If the request to the upstream Hacker News API times out.</response>
    ///// <response code="500">If an unhandled error occurs internally within the server.</response>
    //[HttpGet("bests/limit={n}")]
    //[OutputCache]
    //public async Task<IActionResult> GetBests(int n)
    //{
    //    if (n <= 0)
    //        return BadRequest("n must be > 0");

    //    return Ok(await _storyService.GetBestsAsync(n));
    //}

    /// <summary>
    /// Retrieves the top 'n' stories from Hacker News, ordered by score descending.
    /// </summary>
    /// <param name="n">The number of top stories to retrieve. Must be greater than zero.</param>
    [HttpGet("bests/limit={n}")]
    [OutputCache]
    [ProducesResponseType(typeof(IEnumerable<StoryDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status502BadGateway)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status504GatewayTimeout)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetBests(int n)
    {
        if (n <= 0)
            return BadRequest("n must be > 0");

        return Ok(await _storyService.GetBestsAsync(n));
    }
}