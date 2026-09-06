namespace CoreGym.Domain.Enums;

/// <summary>
/// Mirror of the Postgres enum <c>user_role</c>.
/// NOTE: the full value list of the original enum is unconfirmed — no SQL CHECK
/// constraint is enforced yet (deferred until values are read from the live DB).
/// Known values: 'client' (default), 'coach' (referenced by RLS policies).
/// </summary>
public enum UserRole
{
    Client = 0,
    Coach = 1,
}
