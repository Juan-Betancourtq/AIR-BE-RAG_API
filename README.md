# RAG API (AIR-BE-RAG_API)

Backend API for RAG queries that exposes HTTP endpoints and SignalR for chat. It uses Azure OpenAI and Azure AI Search to retrieve context and generate responses.

## What it does
- Receives user questions and orchestrates search + generation.
- Provides REST endpoints and a SignalR hub for real-time interaction.
- Defines chat and document models for the frontend.

## Key components
- `Controllers/`: API endpoints.
- `Hubs/`: SignalR hub for real-time chat.
- `Models/`: request/response models.

## Tech
- .NET (C#)
- Azure OpenAI
- Azure AI Search
- SignalR
- Serilog
