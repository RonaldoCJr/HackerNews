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