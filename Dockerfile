FROM node:24-alpine AS frontend-build

WORKDIR /workspace/src/Portwise.Web
COPY src/Portwise.Web/package.json src/Portwise.Web/pnpm-lock.yaml ./
RUN corepack enable && pnpm install --frozen-lockfile
COPY src/Portwise.Web/ ./
COPY locales/ /workspace/locales/
RUN pnpm build

FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS backend-build

WORKDIR /workspace
COPY . .
COPY --from=frontend-build /workspace/src/Portwise/wwwroot ./src/Portwise/wwwroot
RUN dotnet restore Portwise.slnx
RUN dotnet publish src/Portwise/Portwise.csproj -c Release --no-restore -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS runtime

WORKDIR /app
ENV ASPNETCORE_HTTP_PORTS=8080
ENV DOTNET_EnableDiagnostics=0
RUN mkdir -p /app/data /app/logs
COPY --from=backend-build /app/publish ./
VOLUME ["/app/data", "/app/logs"]
EXPOSE 8080
ENTRYPOINT ["dotnet", "Portwise.dll"]
