namespace CoreGym.Api.Contracts;

// --- profile / onboarding / goals ---
public record ProvisionProfileRequest(string? Email, string? Name);

public record UpdateProfileRequest(
    string? Name,
    string? FullName,
    string? Gender,
    int? Age,
    decimal? WeightKg,
    decimal? HeightCm,
    string? FitnessGoal,
    string? AvatarUrl);

public record UpsertOnboardingRequest(
    int? Age,
    string? Gender,
    decimal? HeightCm,
    decimal? WeightKg,
    string? Goal,
    string? ActivityLevel,
    decimal? TargetWeight,
    int? WeeklyWorkouts,
    bool Completed = false);

public record UpsertGoalsRequest(
    int? DailyCalories,
    int? DailyProteinG,
    int? DailyCarbsG,
    int? DailyFatG,
    int? DailyWaterMl,
    int? DailySteps,
    decimal? DailySleepHours,
    int? WeeklyWorkouts,
    decimal? TargetWeightKg);

// --- nutrition / measurements / workouts ---
public record CreateNutritionLogRequest(
    Guid? FoodId,
    string FoodName,
    string? MealType,
    decimal? Quantity,
    string? ServingUnit,
    decimal Calories,
    decimal ProteinG,
    decimal CarbsG,
    decimal FatG,
    DateTime? Date);

public record CreateMeasurementRequest(
    decimal? WeightKg,
    decimal? BodyFatPct,
    decimal? MuscleMass,
    decimal? ChestCm,
    decimal? WaistCm,
    decimal? HipsCm,
    decimal? ArmsCm,
    decimal? ThighsCm,
    DateTime? MeasuredDate,
    string? Notes);

public record CreateWorkoutSessionRequest(
    string MuscleGroup,
    string? SessionName,
    int DurationMin = 0,
    string? Notes = null,
    DateTime? Date = null,
    DateTimeOffset? StartedAt = null,
    DateTimeOffset? EndedAt = null,
    IReadOnlyList<CreateWorkoutSetRequest>? Sets = null);

public record CreateWorkoutSetRequest(
    string ExerciseName,
    int SetNumber,
    int? Reps,
    decimal? WeightKg,
    int? DurationSec,
    int? RestSec,
    bool IsWarmup);

// --- chat ---
public record SendChatMessageRequest(string Content, string Type = "text", string? FileUrl = null);

public record ChatConversationResponse(
    Guid Id,
    Guid ClientId,
    Guid CoachId,
    Guid? SubscriptionId,
    string? LastMessage,
    DateTimeOffset? LastMessageAt,
    int ClientUnread,
    int CoachUnread,
    bool? IsActive);

public record MarkedReadResponse(int Marked);

public record UnreadCountResponse(int Count);

// --- notifications ---
public record UpdateNotificationPreferencesRequest(
    bool? MealRemindersEnabled,
    bool? WaterRemindersEnabled,
    bool? CalorieAlertsEnabled,
    bool? ChatNotificationsEnabled,
    TimeSpan? QuietHoursStart,
    TimeSpan? QuietHoursEnd);

// --- coaches / programs / streaks ---
public record CreateReviewRequest(int Rating, string? Comment);

public record CoachListItemResponse(
    Guid Id,
    string? Name,
    string? AvatarUrl,
    decimal Rating,
    List<string>? Specialization,
    decimal PriceMonthly);

public record CoachDetailResponse(
    Guid Id,
    string? Name,
    string? AvatarUrl,
    string? Bio,
    decimal Rating,
    decimal PriceMonthly,
    List<string>? Specialization,
    bool? IsActive);

public record CoachSubscriptionResponse(
    Guid Id,
    Guid ClientId,
    string? Status,
    string? Tier,
    DateTime? StartDate,
    DateTime? EndDate,
    string? PaymentStatus);

public record SetActiveProgramRequest(Guid ProgramId, int? CurrentWeek, int? CurrentDay, string? Notes);

public record StreakActivityRequest(string Source);

// --- AI ---
public record AnalyzeFoodImageRequest(string ImageBase64, string? MimeType, string? Notes);

public record AnalyzeFoodVoiceRequest(string AudioBase64, string? MimeType, string? Notes);

public record AnalyzeFoodTextRequest(string Text);

// --- daily/weekly activity, exercise progress, user programs, barcode scans ---
public record CreateDailyActivityRequest(
    DateTime? Date,
    int? Steps,
    decimal? ActiveCaloriesBurned,
    decimal? HeartRateAvg,
    decimal? ExerciseMinutes,
    string? Source,
    DateTimeOffset? SyncedAt);

public record UpdateDailyActivityRequest(
    int? Steps,
    decimal? ActiveCaloriesBurned,
    decimal? HeartRateAvg,
    decimal? ExerciseMinutes,
    string? Source,
    DateTimeOffset? SyncedAt);

public record UpsertWeeklyActivityRequest(DateTime WeekStart, int DayIndex, int? ActualPct, int? GoalPct);

public record CreateExerciseProgressRequest(
    Guid? ExerciseId,
    Guid? SessionId,
    DateTime? SessionDate,
    decimal? BestSetWeight,
    int? BestSetReps,
    decimal? TotalVolume,
    decimal? OneRmEstimate);

public record CreateUpdateUserProgramRequest(string ProgramName, string MuscleGroup, bool? IsActive, DateTimeOffset? StartedAt);

public record RecordBarcodeScanRequest(string? Barcode, decimal? QuantityG, Guid? NutritionLogId);

// --- coach side ---
public record CoachClientResponse(
    Guid SubscriptionId,
    Guid ClientId,
    string? ClientName,
    string? AvatarUrl,
    string? Status,
    string? Tier,
    DateTime? StartDate,
    DateTime? EndDate,
    DateTimeOffset? ExpiresAt);

public record CreateCoachContentRequest(
    string Title,
    string Type,
    string FileUrl,
    string? Description,
    bool IsPublic,
    string? ThumbnailUrl,
    int? FileSizeKb,
    int SortOrder = 0);

public record UpdateCoachContentRequest(
    string? Title,
    string? Type,
    string? FileUrl,
    string? Description,
    bool? IsPublic,
    string? ThumbnailUrl,
    int? FileSizeKb,
    int? SortOrder);

public record CoachContentResponse(
    Guid Id,
    Guid CoachId,
    string Title,
    string? Description,
    string Type,
    string FileUrl,
    bool IsPublic,
    string? ThumbnailUrl,
    int? FileSizeKb,
    int SortOrder,
    DateTimeOffset? CreatedAt);

public record CreateAssignmentRequest(Guid ClientId, Guid ContentId, string? Note);

public record AssignmentResponse(
    Guid Id,
    Guid CoachId,
    Guid ClientId,
    Guid ContentId,
    string? Note,
    DateTimeOffset? AssignedAt,
    string? ContentTitle,
    string? ContentType,
    string? ContentFileUrl);

public record CreatePhaseRequest(int PhaseNumber, string Title, string? Type, string? Description, int? DurationWeeks, string? Status);

public record UpdatePhaseRequest(
    string? Title,
    string? Type,
    string? Description,
    int? DurationWeeks,
    string? Status,
    DateTimeOffset? StartedAt,
    DateTimeOffset? CompletedAt);

public record PhaseResponse(
    Guid Id,
    Guid SubscriptionId,
    int PhaseNumber,
    string Title,
    string? Type,
    string? Description,
    int? DurationWeeks,
    string? Status,
    DateTimeOffset? StartedAt,
    DateTimeOffset? CompletedAt);

public record CreateSubscriptionRequest(
    Guid ClientId,
    string Tier,
    Guid? PlanId,
    DateTime? EndDate,
    DateTimeOffset? ExpiresAt,
    string? Goals,
    string? Notes);

public record UpdateSubscriptionStatusRequest(string Status);

public record UpsertCoachOnboardingRequest(
    string? DisplayName,
    int? YearsExperience,
    List<string>? Certifications,
    List<string>? Specialization,
    string? Bio,
    decimal? PriceMonthly,
    decimal? PricePremium,
    List<string>? Languages,
    int? MaxClients,
    string? ProfileImageUrl,
    string? IntroVideoUrl,
    bool? IsCompleted,
    string? PhoneNumber,
    string? City,
    string? Gender,
    List<string>? GalleryImages,
    List<string>? PdfUrls,
    List<string>? CertificateFiles,
    List<string>? TransformationImages);
