# Free-first deployment plan

## Chosen public names
- Frontend: `marketboard.pages.dev`
- Backend: `marketboard-api.onrender.com`

## Recommended stack
- Frontend: Cloudflare Pages
- Backend: Render Web Service (.NET 8)
- Repo: GitHub (`marketboard`)

## Frontend
Project path:
- `frontend/coffee-dashboard-ui`

Build command:
- `cd frontend/coffee-dashboard-ui && npm install && npm run build`

Output directory:
- `frontend/coffee-dashboard-ui/dist/coffee-dashboard-ui/browser`

Production API config:
- file: `src/environments/environment.production.ts`
- current value:
  - `https://marketboard-api.onrender.com/api`

## Backend
Project path:
- `backend/src/CoffeeDashboard.Api`

Recommended Render settings:
- Service name: `marketboard-api`
- Root directory: `backend/src/CoffeeDashboard.Api`
- Build command:
  - `dotnet publish CoffeeDashboard.Api.csproj -c Release -o out`
- Start command:
  - `dotnet out/CoffeeDashboard.Api.dll`

## CORS
Current config lives in:
- `backend/src/CoffeeDashboard.Api/appsettings.json`
- `backend/src/CoffeeDashboard.Api/appsettings.Development.json`

Current production-ready allowed origin includes:
- `https://marketboard.pages.dev`

## Local dev
Frontend local API:
- `src/environments/environment.ts`
- current local value:
  - `http://localhost:5099/api`

## Deploy order
1. Push repo `marketboard` to GitHub
2. Deploy backend first on Render
3. Verify backend URL is `https://marketboard-api.onrender.com`
4. Deploy frontend on Cloudflare Pages
5. Verify frontend URL is `https://marketboard.pages.dev`
6. Test login + dashboard + API calls

## Current live data coverage
Already suitable for public demo with live feeds for:
- USD/VND
- Gold
- Silver (USD live; VND domestic parsing still needs polish)
- Oil
- VN-Index
- Crypto
- VNExpress RSS

Still pending:
- Coffee domestic price
- London coffee

## Final note before push
Codebase is prepared for free-first deploy. After push, the next step is platform setup only.
