# CareFlow-AI_SE_039
SE3090 Assignment 1: CareFlow AI - An integrated full-stack healthcare and Agentic AI triage system (Group SE_039).

## Backend Configuration

To set up the backend AI capabilities, you must configure the Gemini AI agent. 
**Do not commit real secrets or API keys.**

Use `.NET user secrets` or environment variables for credentials:
- `Gemini__ApiKey`: Your Gemini API key or proxy token (e.g., Neon AI Gateway).

The model and other settings can be configured in `appsettings.Development.json`:
- `Gemini:Model`: The model name to use (e.g., `gemini-3.8-flash`).

### Important Note on Timeouts
- `Planning:AttemptTimeoutSeconds` is currently capped at 30 seconds.
- `Planning:WorkflowTimeoutSeconds` is currently capped at 90 seconds.
