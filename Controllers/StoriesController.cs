using Microsoft.AspNetCore.Mvc;
using santander_hr_api.Interfaces;
using santander_hr_api.Models;

namespace santander_hr_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StoriesController : ControllerBase
    {
        private readonly IHackerNewsService _hackerNewsService;
        private readonly ILogger<StoriesController> _logger;

        public StoriesController(
            IHackerNewsService hackerNewsService,
            ILogger<StoriesController> logger
        )
        {
            _hackerNewsService = hackerNewsService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<StoryDto>>> Get([FromQuery] short count = 10)
        {
            if (count <= 0)
            {
                return BadRequest("Count must be greater than 0");
            }

            try
            {
                var stories = await _hackerNewsService.GetBestStoriesAsync(count);
                return Ok(stories);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving best stories");
                var problemDetails = new ProblemDetails
                {
                    Title = "An error occurred while retrieving best stories",
                    Status = (int)ex.StatusCode,
                    Detail = ex.Message,
                    Type = "HackerNewsClientException"
                };

                return StatusCode((int)ex.StatusCode, problemDetails);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving best stories");
                var problemDetails = new ProblemDetails
                {
                    Title = "An error occurred while retrieving best stories",
                    Status = StatusCodes.Status500InternalServerError,
                    Detail = ex.Message,
                    Type = "GlobalExceptionHandler"
                };

                return StatusCode(StatusCodes.Status500InternalServerError, problemDetails);
            }
        }
    }
}
