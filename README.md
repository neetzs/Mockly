# Mockly — Plataforma SaaS de Evaluación Técnica Automatizada con IA

Mockly es una plataforma SaaS integral diseñada para optimizar, estandarizar y acelerar los procesos de reclutamiento y nivelación técnica. Combina un entorno de codificación interactivo en vivo con evaluación automatizada mediante Inteligencia Artificial, emitiendo métricas objetivas y análisis de competencias en tiempo real.

> Desarrollado como proyecto integrador para el bootcamp de **Devlights**.

---

## Funcionalidades Clave del MVP (En Desarrollo)

- **Landing Page SaaS (`/`):** Presentación del producto, propuesta de valor y comparativa de tiers de suscripción simulados.
- **Autenticación Basada en Roles (RBAC):** Flujo seguro de login y registro segregado para `Candidate` y `Recruiter` con JWT.
- **Sala de Entrevista Interactiva:** Entorno de coding en navegador con Monaco Editor configurado para **C#**, evaluación de código en tiempo real asistida por IA y emisión automática de certificados de rendimiento en formato PDF.
- **Motor de Evaluación Estructurado:** Integración mediante `Microsoft.Extensions.AI` (`IChatClient`) con garantía de formato JSON estricto para calificar legibilidad, complejidad algorítmica y patrones de diseño.
- **Dashboard en Tiempo Real para Reclutadores:** Sincronización instantánea mediante WebSockets con ASP.NET Core SignalR, ranking dinámico de postulantes y visualización de matrices de habilidades con Recharts.
- **Seed Data de Inicialización:** Precarga automática de perfiles, retos técnicos en C# y métricas para pruebas inmediatas de demostración técnica.

---

## Stack Tecnológico

- **Backend:** .NET 10 (C# 14), Clean Architecture, Entity Framework Core 10, PostgreSQL
- **Tiempo Real & Reportes:** ASP.NET Core SignalR, QuestPDF
- **Motor de IA:** `Microsoft.Extensions.AI` (`IChatClient` con JSON estricto)
- **Frontend:** React 19, TypeScript, Vite, Tailwind CSS, shadcn/ui
- **Herramientas de Cliente:** Monaco Editor (`@monaco-editor/react`), Recharts, `@microsoft/signalr`
- **Infraestructura:** Docker Compose, PostgreSQL 17 Alpine
- **Seguridad & Validación:** JWT Bearer, BCrypt.Net, FluentValidation

## Estructura del Monorepo

```text
Mockly/
├── backend/
│   ├── src/
│   │   ├── Domain/          # Entidades puras, enums y contratos (sin dependencias externas)
│   │   ├── Application/     # Casos de uso, DTOs, validaciones FluentValidation y servicios
│   │   ├── Infrastructure/  # EF Core, PostgreSQL, persistencia, seguridad (JWT/BCrypt)
│   │   └── WebApi/          # Minimal APIs, middlewares, CORS y endpoints
│   ├── Dockerfile
│   └── Mockly.slnx
├── frontend/                # React 19 SPA con Vite, Tailwind CSS y componentes shadcn/ui (Dockerfile + nginx)
├── docker-compose.yml       # Demo completa: PostgreSQL + API + frontend
├── .editorconfig
├── .env.example
├── .gitignore
└── README.md

## Cómo levantarlo

### Demo completa con Docker / Podman

```bash
podman compose up -d --build      # o: docker compose up -d --build
```

| Servicio | URL |
|---|---|
| Frontend | http://localhost:5173 |
| API (OpenAPI en `/openapi/v1.json`) | http://localhost:5067 |
| PostgreSQL | `localhost:5432` |

Al arrancar, la API aplica las migraciones y carga datos de demostración automáticamente.

**Usuarios demo** (contraseña `Demo123!`):
- `candidate@mockly.dev` — candidato con 3 retos en C# para resolver
- `recruiter@mockly.dev` — reclutador con dashboard y ranking en tiempo real

**IA real (opcional):** sin API key se usa un evaluador simulado. Para usar OpenAI:

```bash
OPENAI_API_KEY=sk-... podman compose up -d --build
```

### Desarrollo local

```bash
podman compose up -d postgres                          # solo la base
cd backend && dotnet run --project src/WebApi          # API en http://localhost:5067 (Scalar en /scalar)
cd frontend && npm install && npm run dev              # frontend en http://localhost:5173
```

En Development la API también migra y siembra al arrancar. La API key de OpenAI se carga con:

```bash
dotnet user-secrets set "OpenAI:ApiKey" "sk-..." --project backend/src/WebApi
```
