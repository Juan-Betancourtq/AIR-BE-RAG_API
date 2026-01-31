# RAG API (AIR-BE-RAG_API)

API backend para consultas RAG que expone endpoints HTTP y SignalR para chat. Consume Azure OpenAI y Azure AI Search para recuperar contexto y generar respuestas.

## Qué hace
- Recibe preguntas de usuario y orquesta búsqueda + generación.
- Provee endpoints REST y un hub SignalR para interacción en tiempo real.
- Estructura modelos de chat y documentos para el front.

## Componentes clave
- `Controllers/`: endpoints de la API.
- `Hubs/`: SignalR para chat en tiempo real.
- `Models/`: modelos de solicitud/respuesta.

## Tecnologías
- .NET (C#)
- Azure OpenAI
- Azure AI Search
- SignalR
- Serilog
