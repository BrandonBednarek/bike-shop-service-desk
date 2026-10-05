# One image for the whole app: the API serves the built front end from the same origin.
# Build and run with: docker compose up --build

# ---- Stage 1: build the SvelteKit front end into static files -------------------------
FROM node:24-slim AS web-build
WORKDIR /src/web

# Same pnpm version as package.json's "packageManager", so the lockfile is read identically.
RUN npm install --global pnpm@12.9.1

# Dependencies first, in their own layer, so source edits don't trigger a reinstall.
COPY web/package.json web/pnpm-lock.yaml web/pnpm-workspace.yaml web/.npmrc ./
RUN pnpm install --frozen-lockfile

COPY web/ ./
RUN pnpm build

# ---- Stage 2: publish the ASP.NET Core API ------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS api-build
WORKDIR /src

COPY Directory.Build.props .editorconfig ./
COPY api/BikeShop.Api.csproj api/
RUN dotnet restore api/BikeShop.Api.csproj

COPY api/ api/
RUN dotnet publish api/BikeShop.Api.csproj --configuration Release --no-restore --output /app/publish

# ---- Stage 3: runtime image ---------------------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=api-build /app/publish ./
COPY --from=web-build /src/web/build ./wwwroot

# The app runs as the image's non-root user, which can't create folders under /app.
# Creating the data folder here, owned by that user, means the named volume mounted
# over it starts with the same ownership and the app can write its database there.
RUN mkdir -p /app/data && chown "$APP_UID" /app/data
USER $APP_UID
ENV Storage__DataDirectory=/app/data

EXPOSE 8080
ENTRYPOINT ["dotnet", "BikeShop.Api.dll"]
