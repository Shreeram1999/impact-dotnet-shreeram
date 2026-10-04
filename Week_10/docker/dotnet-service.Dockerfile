# One multi-stage Dockerfile for all four .NET services (Identity,
# Academics, Reporting, Gateway). docker-compose.yml passes which one:
#   SERVICE = folder name (e.g. Identity), PROJECT = project name (e.g. Identity.Api)
# Build context is Week_10/, so the shared library can be copied in.

ARG SERVICE
ARG PROJECT

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG SERVICE
ARG PROJECT
WORKDIR /src

# Restore first, from the project files only, so the NuGet layer is cached
# until a .csproj actually changes.
COPY Shared/StudentPortal.Shared/StudentPortal.Shared.csproj Shared/StudentPortal.Shared/
COPY ${SERVICE}/${PROJECT}/${PROJECT}.csproj ${SERVICE}/${PROJECT}/
RUN dotnet restore ${SERVICE}/${PROJECT}/${PROJECT}.csproj

COPY Shared/ Shared/
COPY ${SERVICE}/${PROJECT}/ ${SERVICE}/${PROJECT}/
RUN dotnet publish ${SERVICE}/${PROJECT}/${PROJECT}.csproj -c Release -o /app --no-restore /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
ARG PROJECT
ENV ASPNETCORE_URLS=http://+:8080 \
    APP_DLL=${PROJECT}.dll
WORKDIR /app
COPY --from=build /app .
EXPOSE 8080
# The aspnet image ships a non-root "app" user; don't run as root.
USER $APP_UID
ENTRYPOINT ["sh", "-c", "exec dotnet \"$APP_DLL\""]
