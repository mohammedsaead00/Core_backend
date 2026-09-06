using CoreGym.Domain.Entities;
using CoreGym.Domain.ReadModels;
using Microsoft.EntityFrameworkCore;

namespace CoreGym.Infrastructure;

public class CoreGymDbContext : DbContext
{
    public CoreGymDbContext(DbContextOptions<CoreGymDbContext> options) : base(options)
    {
    }

    public DbSet<Profile> Profiles => Set<Profile>();
    public DbSet<Food> Foods => Set<Food>();
    public DbSet<Exercise> Exercises => Set<Exercise>();
    public DbSet<TrainingProgram> TrainingPrograms => Set<TrainingProgram>();
    public DbSet<ProgramDay> ProgramDays => Set<ProgramDay>();
    public DbSet<ProgramDayExercise> ProgramDayExercises => Set<ProgramDayExercise>();
    public DbSet<Onboarding> Onboardings => Set<Onboarding>();
    public DbSet<UserGoal> UserGoals => Set<UserGoal>();
    public DbSet<UserStreak> UserStreaks => Set<UserStreak>();
    public DbSet<BodyMeasurement> BodyMeasurements => Set<BodyMeasurement>();
    public DbSet<NutritionLog> NutritionLogs => Set<NutritionLog>();
    public DbSet<DailySummary> DailySummaries => Set<DailySummary>();
    public DbSet<DailyActivity> DailyActivities => Set<DailyActivity>();
    public DbSet<StreakActivityLog> StreakActivityLogs => Set<StreakActivityLog>();
    public DbSet<WorkoutSession> WorkoutSessions => Set<WorkoutSession>();
    public DbSet<WorkoutSet> WorkoutSets => Set<WorkoutSet>();
    public DbSet<ExerciseProgress> ExerciseProgress => Set<ExerciseProgress>();
    public DbSet<UserActiveProgram> UserActivePrograms => Set<UserActiveProgram>();
    public DbSet<UserProgram> UserPrograms => Set<UserProgram>();
    public DbSet<BarcodeProduct> BarcodeProducts => Set<BarcodeProduct>();
    public DbSet<BarcodeScanHistory> BarcodeScanHistories => Set<BarcodeScanHistory>();
    public DbSet<FoodScan> FoodScans => Set<FoodScan>();
    public DbSet<FoodScanItem> FoodScanItems => Set<FoodScanItem>();
    public DbSet<VoiceFoodLog> VoiceFoodLogs => Set<VoiceFoodLog>();
    public DbSet<VoiceFoodLogItem> VoiceFoodLogItems => Set<VoiceFoodLogItem>();
    public DbSet<WeeklyActivity> WeeklyActivities => Set<WeeklyActivity>();
    public DbSet<Coach> Coaches => Set<Coach>();
    public DbSet<CoachProfile> CoachProfiles => Set<CoachProfile>();
    public DbSet<CoachOnboarding> CoachOnboardings => Set<CoachOnboarding>();
    public DbSet<CoachContent> CoachContents => Set<CoachContent>();
    public DbSet<ClientAssignment> ClientAssignments => Set<ClientAssignment>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<Subscription> Subscriptions => Set<Subscription>();
    public DbSet<SubscriptionPlan> SubscriptionPlans => Set<SubscriptionPlan>();
    public DbSet<SubscriptionPhase> SubscriptionPhases => Set<SubscriptionPhase>();
    public DbSet<PaymentIntent> PaymentIntents => Set<PaymentIntent>();
    public DbSet<StripeCustomer> StripeCustomers => Set<StripeCustomer>();
    public DbSet<Conversation> Conversations => Set<Conversation>();
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<NotificationPreference> NotificationPreferences => Set<NotificationPreference>();
    public DbSet<NotificationLog> NotificationLogs => Set<NotificationLog>();

    // Keyless read models over the SQL Server views.
    public DbSet<PersonalRecord> PersonalRecords => Set<PersonalRecord>();
    public DbSet<WeeklyProgress> WeeklyProgress => Set<WeeklyProgress>();
    public DbSet<WeightProgress> WeightProgress => Set<WeightProgress>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
        => modelBuilder.ApplyConfigurationsFromAssembly(typeof(CoreGymDbContext).Assembly);
}
