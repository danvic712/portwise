FROM mcr.microsoft.com/dotnet/sdk:10.0.100-alpine3.22 AS contract-build

WORKDIR /workspace
COPY . .
RUN dotnet restore Portwise.slnx
RUN dotnet build src/Portwise/Portwise.csproj -c Release --no-restore

FROM node:24-alpine AS frontend-build

WORKDIR /workspace/src/Portwise.Web
COPY src/Portwise.Web/package.json src/Portwise.Web/pnpm-lock.yaml ./
RUN corepack enable && pnpm install --frozen-lockfile
COPY src/Portwise.Web/ ./
COPY locales/ /workspace/locales/
COPY --from=contract-build /workspace/src/Portwise.Web/openapi/portwise_v1.json ./openapi/portwise_v1.json
RUN pnpm api:generate && pnpm api:check && pnpm build

FROM contract-build AS backend-build

COPY --from=frontend-build /workspace/src/Portwise/wwwroot ./src/Portwise/wwwroot
RUN dotnet publish src/Portwise/Portwise.csproj -c Release --no-restore -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine3.22 AS runtime

WORKDIR /app
ENV ASPNETCORE_HTTP_PORTS=8080
ENV DOTNET_EnableDiagnostics=0
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
RUN apk add --no-cache icu-libs krb5-libs tzdata
RUN mkdir -p /app/logs
COPY --from=backend-build /app/publish ./
VOLUME ["/app/logs"]
EXPOSE 8080
ENTRYPOINT ["dotnet", "Portwise.dll"]
