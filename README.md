# Bike Shop Service Desk

[![CI](https://github.com/BrandonBednarek/bike-shop-service-desk/actions/workflows/ci.yml/badge.svg)](https://github.com/BrandonBednarek/bike-shop-service-desk/actions/workflows/ci.yml)

An internal app for a small bike shop's repair desk: check bikes in, track each job through to pickup, log labour and parts against the estimate, and see what's late. SvelteKit SPA, ASP.NET Core 10 Minimal API and SQLite, in one Docker image.

Assumptions, scope, trade-offs and next steps are in [docs/DECISIONS.md](docs/DECISIONS.md).

## Run

```bash
docker compose up --build
```

Open <http://localhost:8080>, or set `PORT=9090` if 8080 is taken. Demo mode seeds 10 jobs (#1001 to #1010) into an empty database. Reset with `docker compose down -v`.

Sign in from the demo panel on the sign-in page. Every account's password is `demo-password`.

| Username | Role |
| --- | --- |
| `owner` | Owner: can also edit and reopen closed jobs |
| `lebis`, `harry` | Staff |
| `mack` | Staff, deactivated |

## Worth a look

- **#1007:** on hold for the customer's approval, because a part would have taken the bill over the estimate.
- **#1010:** checked in today. Add a part to see the over-estimate prompt, then take the job through to collected.
- **Dashboard:** overdue jobs grouped by status, and bikes that are ready but not collected.
- **#1002 and #1003:** closed jobs, at `/work-orders/1002`. They're read-only for staff and editable by the owner.

## Development

You'll need the .NET 10 SDK, Node 24 and pnpm 12.

```bash
dotnet run --project api              # API on :5080, database in api/.data
cd web && pnpm install && pnpm dev    # UI on :5173, proxies /api to the API
dotnet test                           # domain and API integration tests
cd web && pnpm check                  # svelte-check
```

CI runs the tests, `pnpm check`, the front-end build and `docker build` on every pull request.
