# Contoso Bike Store AI Assistant

An end-to-end sample that uses Microsoft Agent Framework to demonstrate how to build Human In the loop (HITL) AI assistant.
The agent uses GitHub Models for inference and exposes an AG-UI endpoint. The frontend integrates via CopilotKit to provide a chat experience for Contoso Bike Store.

## Tech Stack

- Backend: .NET (ASP.NET Core), Microsoft Agent Framework, AG-UI, `OpenAIClient` (GitHub Models)
- Frontend: Next.js (App Router), TypeScript
- Tools: Product inventory tools + a payment approval tool (server function with approval)

## Prerequisites

- .NET 9/10 SDK (match your local environment)
- Node.js 20+ and npm
- GitHub account and Personal Access Token (PAT) for GitHub Models access

## GitHub Models Setup

1. Create a GitHub Personal Access Token:
   - Go to GitHub Settings → Developer settings → Personal access tokens → Tokens (classic)
   - Generate a new token; no special scopes are required for models
   - Copy the token value

2. Set environment variables (choose your shell):

   Windows (PowerShell):
   ```powershell
   $env:GITHUB_TOKEN="<your_github_token>"
   $env:GITHUB_MODEL_ID="gpt-4o"       # optional, defaults to gpt-4o
   $env:GITHUB_MODELS_BASE_URL="https://models.inference.ai.azure.com" # optional
   ```

   Windows (Command Prompt):
   ```cmd
   set GITHUB_TOKEN=<your_github_token>
   set GITHUB_MODEL_ID=gpt-4o
   set GITHUB_MODELS_BASE_URL=https://models.inference.ai.azure.com
   ```

   macOS/Linux:
   ```bash
   export GITHUB_TOKEN="<your_github_token>"
   export GITHUB_MODEL_ID="gpt-4o"
   export GITHUB_MODELS_BASE_URL="https://models.inference.ai.azure.com"
   ```

You can also use the command `dotnet user-secrets` to set the `GITHUB_TOKEN` for the backend project.

## How to Run

1. Clone and open the repo
   ```bash
   git clone <this-repository-url>
   cd maf-apporval-workflow
   ```

2. Start the backend (agent host)
   ```bash
   cd src/backend/ContosoBikestore.Agent.Host
   dotnet restore
   dotnet run
   ```
   - Agent Dev UI: http://localhost:5001/devui
   - AG-UI endpoint: http://localhost:5001/agent/customer_service_assistant

3. Start the frontend (Next.js)
   ```bash
   cd src/frontend
   npm install
   # optional: echo backend base URL to .env.local
   # Windows PowerShell
   "$env:BACKEND_AGENT_BASE_URL=http://localhost:5001" | Out-File -Encoding utf8 .env.local
   # macOS/Linux
   # echo "BACKEND_AGENT_BASE_URL=http://localhost:5001" > .env.local
   npm run dev
   ```
   - App: http://localhost:3000

## Resources

- Microsoft Agent Framework Documentation: https://learn.microsoft.com/en-us/agent-framework/overview/agent-framework-overview
- CopilotKit: https://www.copilotkit.ai/
- GitHub Models: https://docs.github.com/en/github-models