# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Ámbito

`KntRedmineApi` es una API independiente de integración con Redmine. Reutiliza la capa `Service`/`Repository`
compartida del resto de KNote (ver el `CLAUDE.md` de la raíz para el patrón Repository intercambiable
Dapper/EF), pero no depende de ninguno de los dos frontends (`Client` ni `ClientWin`).

## Archivo de solución

`KntRedmine.slnx` (en la raíz de `Src/KNote`) agrupa justo lo que necesita este proyecto, sin arrastrar los
frontends:

```
KntRedmine.slnx
  ├─ Model
  ├─ Service
  ├─ Repository
  ├─ Repository.EntityFramework
  ├─ Repository.Dapper
  ├─ MessageBroker
  ├─ MessageBroker.RabbitMQ
  └─ KntRedmineApi
```

```powershell
dotnet build KntRedmine.slnx
dotnet run --project KntRedmineApi/KntRedmineApi.csproj
```

## Dependencias del proyecto

`KntRedmineApi/KntRedmineApi.csproj` → `Service` (y transitivamente `Model`, `Repository`,
`Repository.Dapper`, `Repository.EntityFramework`, `MessageBroker`, `MessageBroker.RabbitMQ`).

La selección de ORM (Dapper vs EntityFramework) y de motor de BD (Sqlite vs SQL Server) sigue el mismo
mecanismo basado en `RepositoryRef.Orm`/`RepositoryRef.Provider` descrito en el `CLAUDE.md` raíz.
