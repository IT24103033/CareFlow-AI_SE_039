# Member 3 live cross-platform integration

This test links one real Flutter patient submission and booking to React doctor approval and then Flutter appointment verification. It uses the running API, its configured PostgreSQL database, and live Gemini. It creates synthetic test records; use dedicated test accounts.

## Prerequisites

- Android emulator visible to `adb devices`.
- Local API running on port 5241 with a working database and Gemini configuration.
- React running on port 5173 with `VITE_API_BASE_URL=http://localhost:5241`.
- Flutter SDK and project dependencies (`flutter pub get` in mobile_app).
- Node.js and Playwright available (`npm install --no-save playwright` if needed), with Google Chrome installed. Alternatively set PLAYWRIGHT_MODULE to an installed Playwright package directory.
- Copy cross-platform.example.json to cross-platform.local.json and fill synthetic patient and active General Practitioner doctor credentials. This private file is ignored by Git. Do not include it in submissions.

## Execute from repository root

```bash
node scripts/testing/run-cross-platform.cjs
```

The runner:
1. Logs in with the doctor account and creates/reuses tomorrow's availability.
2. Runs Flutter integration_test on the Android emulator. The test logs in through the production login screen, mounts the production submission screen, submits symptoms to the real API, selects a returned slot and checks the booking is Tentative/Pending.
3. Carries the exact case ID, appointment ID and run marker into Playwright. The test logs in through React, searches that marker, verifies the exact case ID and approves it through the review form.
4. Runs the Flutter verification stage against the same IDs, checking Approved case and Confirmed appointment through the real API and the production appointment screen.

Authentication and feature screens are exercised, but full home-screen navigation between features is not covered; the Flutter test mounts the relevant production screen after login. No responses are mocked. Appointment notification delivery is not asserted.

## Evidence and failures

Each run has its own output/testing/member3/cross-platform/M3-... directory with result.json, Flutter logs and React screenshots. Only a final result.json status of Passed establishes a completed run. A partial Flutter success does not establish cross-platform success. IDs in the handoff and review evidence tie the stages to the same records.

The initial report predates this work. Do not claim a live pass from the earlier isolated xUnit test or include private config/credentials in an evidence archive.
