using CoreGym.Api.Contracts;
using CoreGym.Domain.Authorization;
using CoreGym.Domain.Entities;
using CoreGym.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CoreGym.Api.Controllers;

/// <summary>Coach onboarding submission (own row, upserted — PK-style unique user_id per prod).</summary>
[ApiController]
[Route("api/coach/onboarding")]
[Authorize]
public class CoachOnboardingController : CoreGymControllerBase
{
    private readonly CoreGymDbContext _db;

    public CoachOnboardingController(CoreGymDbContext db, ICurrentUserService currentUser) : base(currentUser)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var onboarding = await _db.CoachOnboardings
            .AsNoTracking()
            .FirstOrDefaultAsync(co => co.UserId == UserId, cancellationToken);
        return Ok(onboarding);
    }

    [HttpPut]
    public async Task<IActionResult> Upsert([FromBody] UpsertCoachOnboardingRequest request, CancellationToken cancellationToken)
    {
        var onboarding = await _db.CoachOnboardings
            .FirstOrDefaultAsync(co => co.UserId == UserId, cancellationToken);
        if (onboarding is null)
        {
            onboarding = new CoachOnboarding { Id = Guid.NewGuid(), UserId = UserId };
            _db.CoachOnboardings.Add(onboarding);
        }

        if (request.DisplayName is not null) onboarding.DisplayName = request.DisplayName;
        if (request.YearsExperience is not null) onboarding.YearsExperience = request.YearsExperience.Value;
        if (request.Certifications is not null) onboarding.Certifications = request.Certifications;
        if (request.Specialization is not null) onboarding.Specialization = request.Specialization;
        if (request.Bio is not null) onboarding.Bio = request.Bio;
        if (request.PriceMonthly is not null) onboarding.PriceMonthly = request.PriceMonthly;
        if (request.PricePremium is not null) onboarding.PricePremium = request.PricePremium;
        if (request.Languages is not null) onboarding.Languages = request.Languages;
        if (request.MaxClients is not null) onboarding.MaxClients = request.MaxClients;
        if (request.ProfileImageUrl is not null) onboarding.ProfileImageUrl = request.ProfileImageUrl;
        if (request.IntroVideoUrl is not null) onboarding.IntroVideoUrl = request.IntroVideoUrl;
        if (request.IsCompleted is not null) onboarding.IsCompleted = request.IsCompleted.Value;
        if (request.PhoneNumber is not null) onboarding.PhoneNumber = request.PhoneNumber;
        if (request.City is not null) onboarding.City = request.City;
        if (request.Gender is not null) onboarding.Gender = request.Gender;
        if (request.GalleryImages is not null) onboarding.GalleryImages = request.GalleryImages;
        if (request.PdfUrls is not null) onboarding.PdfUrls = request.PdfUrls;
        if (request.CertificateFiles is not null) onboarding.CertificateFiles = request.CertificateFiles;
        if (request.TransformationImages is not null) onboarding.TransformationImages = request.TransformationImages;

        await _db.SaveChangesAsync(cancellationToken);
        return Ok(onboarding);
    }
}
