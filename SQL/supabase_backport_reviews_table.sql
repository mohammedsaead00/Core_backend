-- CoreGym — Supabase backport for the `reviews` table
-- (decision 2026-09-06: `reviews` is the canonical review table; `coach_reviews` is NOT ported to .NET).
--
-- `reviews` was created manually in prod and is missing from every local
-- migration file, so staging/fresh environments lack it. This script is a
-- no-op where the table already exists.
--
-- NOTE: FK constraints are deliberately omitted because prod's manual DDL is
-- unverified — if your environment needs them, add them explicitly after
-- confirming prod's definition. Run a SELECT DISTINCT check as well if you
-- plan to constrain `rating` or reuse this elsewhere.

CREATE TABLE IF NOT EXISTS public.reviews (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    client_id uuid NOT NULL,
    coach_id uuid NOT NULL,
    rating integer NOT NULL,
    comment text,
    created_at timestamptz DEFAULT now()
);

-- The app queries reviews by coach (dashboard rating stats, coach detail page).
CREATE INDEX IF NOT EXISTS idx_reviews_coach_id ON public.reviews (coach_id);
CREATE INDEX IF NOT EXISTS idx_reviews_client_id ON public.reviews (client_id);
