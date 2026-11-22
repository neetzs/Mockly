# Mockly

Plataforma SaaS de entrevistas técnicas con evaluación por IA (proyecto integrador del bootcamp Devlights). Monorepo: `backend/` (.NET 10 / C# 14) y `frontend/` (React 19 + TS + Vite). Trabajo en equipo, fullstack.

## Cómo levantarlo
```bash
podman compose up -d                     # PostgreSQL 17 en localhost:5432 (mockly_db)
cd backend && dotnet build Mockly.slnx
dotnet run --project src/WebApi          # API en http://localhost:5067 (https 7161), Scalar en /scalar
cd frontend && npm install && npm run dev
```
- Migraciones EF Core (viven en Infrastructure):
  `dotnet ef migrations add <Nombre> --project src/Infrastructure --startup-project src/WebApi`
  `dotnet ef database update --project src/Infrastructure --startup-project src/WebApi`
- Verificar antes de terminar: `dotnet build Mockly.slnx` en backend y `npm run lint && npm run build` en frontend.
- Probar endpoints con `src/WebApi/WebApi.http` o Scalar.

## Arquitectura backend (Clean Architecture)
- `Domain`: entidades, enums, interfaces. Sin dependencias externas.
- `Application`: casos de uso por feature en `Features/<Feature>/` (DTOs, validadores FluentValidation, servicios). Depende solo de Domain. Usa la abstracción del DbContext, no EF directo.
- `Infrastructure`: EF Core + PostgreSQL, migraciones, seguridad (JWT, BCrypt).
- `WebApi`: Minimal APIs en `Endpoints/<Feature>Endpoints.cs`, CORS, OpenAPI/Scalar.
- Cada capa registra sus servicios en su `DependencyInjection.cs`.
- Respetar la dirección de dependencias: nunca referenciar Infrastructure desde Application/Domain.
- Estilo (.editorconfig): file-scoped namespaces, llaves en línea nueva, pattern matching / switch expressions, 4 espacios.

## Frontend
- Organización por feature en `src/features/<feature>/` (context, services, types, páginas). Ejemplo de referencia: `features/auth`.
- Componentes UI de shadcn en `src/components/ui` — reutilizarlos antes de crear nuevos (`npx shadcn add <comp>`).
- Tailwind 3, 2 espacios, npm (hay package-lock.json).
- Librerías planificadas: Monaco Editor (editor C#), Recharts, `@microsoft/signalr`.

## Git (equipo)
- Ramas: `main` (releases), `develop` (integración), `feat/<modulo>` para trabajo nuevo.
- Nunca commitear ni pushear directo a `main`; crear rama desde `develop` y mergear vía PR.
- Commits en inglés con Conventional Commits: `feat(auth): ...`, `fix(...)`, `docs: ...`.
- No commitear `.env`, `bin/`, `obj/`, `node_modules/`.

## Estado
- Hecho: auth JWT (Candidate/Recruiter), sala de entrevista con Monaco, motor de IA (`IChatClient`, JSON estricto) con rate limit, dashboard en tiempo real con SignalR, certificados PDF (QuestPDF), modo práctica, seed de datos demo, búsquedas del reclutador (crear con retos de la biblioteca, invitar candidatos, revisar sesiones, cerrar búsquedas).
- Seed demo (solo Development): `recruiter@mockly.dev` / `candidate@mockly.dev`, password `Demo123!`.
- `../mockly.ai/` es un borrador viejo: ignorarlo.
