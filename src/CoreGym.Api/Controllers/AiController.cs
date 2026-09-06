using System.Diagnostics.CodeAnalysis;
using CoreGym.Api.Contracts;
using CoreGym.Domain.Authorization;
using CoreGym.Domain.Services;
using CoreGym.Infrastructure.AI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace CoreGym.Api.Controllers;

[ApiController]
[Route("api/ai")]
[Authorize]
public class AiController : CoreGymControllerBase
{
    private readonly IFoodAnalysisService _foodAnalysis;
    private readonly IBarcodeLookupService _barcodeLookup;
    private readonly GeminiOptions _geminiOptions;

    public AiController(
        IFoodAnalysisService foodAnalysis,
        IBarcodeLookupService barcodeLookup,
        IOptions<GeminiOptions> geminiOptions,
        ICurrentUserService currentUser)
        : base(currentUser)
    {
        _foodAnalysis = foodAnalysis;
        _barcodeLookup = barcodeLookup;
        _geminiOptions = geminiOptions.Value;
    }

    /// <summary>analyze-food replacement: image → Gemini → persisted scan + items.</summary>
    [HttpPost("food/image")]
    public async Task<IActionResult> AnalyzeImage([FromBody] AnalyzeFoodImageRequest request, CancellationToken cancellationToken)
    {
        if (!IsConfigured(out IActionResult? problem))
        {
            return problem!;
        }

        var result = await _foodAnalysis.AnalyzeImageAsync(UserId, request.ImageBase64, request.MimeType, request.Notes, cancellationToken);
        return Ok(result);
    }

    /// <summary>log-food-voice replacement: audio → Gemini → persisted voice log + items.</summary>
    [HttpPost("food/voice")]
    public async Task<IActionResult> AnalyzeVoice([FromBody] AnalyzeFoodVoiceRequest request, CancellationToken cancellationToken)
    {
        if (!IsConfigured(out IActionResult? problem))
        {
            return problem!;
        }

        var result = await _foodAnalysis.AnalyzeVoiceAsync(UserId, request.AudioBase64, request.MimeType, request.Notes, cancellationToken);
        return Ok(result);
    }

    /// <summary>log-food-text replacement: stateless extraction — the client confirms and logs normally.</summary>
    [HttpPost("food/text")]
    public async Task<IActionResult> ExtractFromText([FromBody] AnalyzeFoodTextRequest request, CancellationToken cancellationToken)
    {
        if (!IsConfigured(out IActionResult? problem))
        {
            return problem!;
        }

        var result = await _foodAnalysis.ExtractFromTextAsync(request.Text, cancellationToken);
        return Ok(result);
    }

    /// <summary>lookup-barcode replacement: cache → Open Food Facts → Gemini estimate.</summary>
    [HttpGet("barcode/{barcode}")]
    public async Task<IActionResult> LookupBarcode(string barcode, CancellationToken cancellationToken)
    {
        var result = await _barcodeLookup.LookupAsync(barcode, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    private bool IsConfigured([NotNullWhen(false)] out IActionResult? problem)
    {
        if (string.IsNullOrWhiteSpace(_geminiOptions.ApiKey))
        {
            problem = Problem(statusCode: StatusCodes.Status503ServiceUnavailable, title: "Not configured",
                detail: "Gemini:ApiKey is not configured.");
            return false;
        }

        problem = null;
        return true;
    }
}
