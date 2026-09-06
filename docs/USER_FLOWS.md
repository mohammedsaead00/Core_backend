# User flows

How users move through the CoreGym backend. Each step maps to a real endpoint
(see [API.md](API.md)); everything below is enforced by code covered by the
105 integration tests. You can watch these flows execute live by running
`verify.ps1` in the repo root.

> The diagrams are Mermaid — GitHub renders them inline.

## 1. Sign-up & onboarding (new client)

```mermaid
flowchart TD
    A[User signs up with the auth provider] --> B["JWT issued - sub claim = user id"]
    B --> C["POST /api/me - profile provisioned, role = client"]
    C --> D["PUT /api/me/onboarding - age, weight, goal, activity level"]
    D --> E["PUT /api/me/goals - daily calorie/macro/water/step targets"]
    E --> F[Ready to track]
```

## 2. Daily nutrition logging (4 input paths → one summary)

Every food item lands in `nutrition_logs`; the **daily summary re-syncs
automatically** after each write (the `sync_nutrition_to_summary` replacement),
and the user's streak is fed by the nutrition activity.

```mermaid
flowchart TD
    U[User opens the food logger] --> P{Input method}

    P -->|Photo| A1["POST /api/ai/food/image<br/>Gemini vision -> food_scans + items<br/>image stored in food-scans bucket"]
    P -->|Voice| A2["POST /api/ai/food/voice<br/>Gemini audio -> transcript + voice_food_logs + items"]
    P -->|Text| A3["POST /api/ai/food/text<br/>Gemini extraction - stateless"]
    P -->|Barcode| B1["GET /api/ai/barcode/{code}<br/>1. cache 2. Open Food Facts 3. Gemini estimate"]

    A1 --> R[User reviews the detected items]
    A2 --> R
    A3 --> R
    B1 --> R

    R --> L["POST /api/nutrition/logs - one call per item<br/>summary re-synced: calories + macros"]
    L --> K["POST /api/me/streak/activity - source: nutrition"]

    K --> S{"Streak rule"}
    S -->|consecutive day| I["current streak +1"]
    S -->|exactly 1 missed day| Z["monthly freeze consumed - streak +1"]
    S -->|bigger gap| X["streak resets to 1"]
    I --> D[GET /api/me/summary/date - daily dashboard]
    Z --> D
    X --> D
```

## 3. Workout logging

```mermaid
flowchart TD
    A[User finishes a workout] --> B["POST /api/workouts/sessions<br/>session + ordered sets in one call"]
    B --> C["summary re-synced: workout_done + total duration"]
    B --> D["POST /api/me/streak/activity - source: workout"]
    D --> E["GET /api/me/summary/date<br/>GET /api/me/streak - current / longest / at-risk"]
    B --> F["Sets feed the personal_records view<br/>best weight, best reps, best volume, est. 1RM"]
```

## 4. Coaching: subscribe → chat → monitor → review

```mermaid
flowchart TD
    A[Visitor browses the directory] --> B["GET /api/coaches - anonymous, rating order"]
    B --> C[Client picks a coach and pays via Stripe Checkout]
    C --> D["Stripe calls POST /api/webhooks/stripe<br/>signature verified - checkout.session.completed"]
    D --> E["Subscription -> active<br/>conversation created - coach client count +1<br/>payment_intents + stripe_customers recorded"]
    E --> F["Chat: POST /api/chat/conversations/id/messages<br/>preview + unread counter + in-app notification"]
    F --> G["OneSignal push to the recipient - best effort"]
    G --> H["Coach monitors the client<br/>GET /api/coach/clients/{id}/summary, nutrition, workouts<br/>guarded by the OwnDataOrActiveCoach policy"]
    H --> I{"Subscription ends?"}
    I -->|cancelled / expired via Stripe event| J["status transition - client count -1<br/>coach loses access to the client's data"]
    I -->|still active| K["Client posts a review<br/>POST /api/coaches/{id}/reviews - ratings recalculated"]
```

## 5. Notifications & reminders (background jobs)

```mermaid
flowchart TD
    subgraph Jobs[Hosted background jobs]
        J1[MealReminderJob - probes every 10 min]
        J2[StreakFreezeResetJob - probes hourly]
    end

    J1 --> W{"UTC window 06 / 12 / 18 reached?"}
    W -->|yes| U["For every profile:"]
    U --> Q{"Quiet hours in Cairo time?"}
    Q -->|yes| S1[skip]
    Q -->|no| L{"Logged food today, Cairo date?"}
    L -->|yes| S2[skip]
    L -->|no| R{"Already reminded in this window? notification_log"}
    R -->|yes| S3[skip]
    R -->|no| P["OneSignal push + notification_log row<br/>push failure = retried next probe"]
    P --> N[User sees reminder in-app and on their phone]

    J2 --> M["On the 1st of each month:<br/>freeze_available reset to 1 for everyone below it"]
```

## 6. Barcode scan detail

```mermaid
flowchart LR
    A[User scans a barcode] --> B{"Tier 1: barcode_products cache?"}
    B -->|hit| H[Return cached product - lookup_count +1]
    B -->|miss| C{"Tier 2: Open Food Facts?"}
    C -->|hit| D[Return + cache as openfoodfacts - confidence high]
    C -->|miss| E{"Tier 3: Gemini estimate?"}
    E -->|hit| F[Return + cache as gemini_estimate - confidence low]
    E -->|miss| G[404 - user logs the food manually]
    H --> Z[User confirms and logs via POST /api/nutrition/logs]
    D --> Z
    F --> Z
    G --> Z
```

## What runs where

| Layer | Pieces |
|---|---|
| Client (Flutter) | calls the REST API with a Bearer JWT; owns all confirmations |
| API controllers | thin: resolve the user from the JWT, delegate, map errors to problem details |
| Application services | streaks, summary sync, messaging, notifications, subscription lifecycle, coach ratings, AI, barcode, reminders |
| SQL Server | schema (42 tables), `updated_at` triggers, CHECK constraints, 3 read views |
| External | Gemini, OneSignal, Open Food Facts, Stripe (webhook only) |
