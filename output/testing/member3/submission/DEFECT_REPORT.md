# Member 3 Defect Report

Dinuwara K.D.S | IT24103033 | 8 October 2026

| Defect ID | Description | Severity / priority | Reproduction | Expected | Actual before fix | Evidence | Fix / status | Retest |
|---|---|---|---|---|---|---|---|---|
| M3-D01 | Booking tool reports unverified booking as success | Medium / P2 | Valid slot, no conflict; booking provider returns nonempty text without ID | ProviderError | Success with no appointment ID | agents-before-fix/agents-before-fix.trx | CreateTentativeBookingTool now returns ProviderError when ID is missing; Fixed | Missing-ID case passed in action-agent/action-agent.trx and final suite |
| M3-D02 | Patient case response omits persisted doctor review history | Medium / P2 | Submit, book, approve as doctor, retrieve case as patient | Approved review-history entry | Empty review history | workflow-before-fix/workflow-before-fix.trx | GetById now includes ReviewHistories; Fixed | Complete workflow passed in workflow/member3-workflow.trx |

Initial expectation of HTTP 400 for premature confirmation was a test mistake, corrected to the implemented HTTP 409 Conflict. It is not a product defect.

Open observation: live urgency changed from Medium to Low with injection text; causation is unverified. No clinical correctness conclusion is claimed. Live requests had no available slots. Appointment notifications remain unsupported.
