# Runtime base: ASP.NET runtime only (no SDK), so the final image stays small.
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# Build stage (discarded: it is not part of the final image)
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# 1) Copy only the .csproj files: the restore layer stays cached until dependencies change.
COPY src/PruebaCapitole.Api/PruebaCapitole.Api.csproj                      src/PruebaCapitole.Api/
COPY src/PruebaCapitole.Service/PruebaCapitole.Service.csproj              src/PruebaCapitole.Service/
COPY src/PruebaCapitole.Domain/PruebaCapitole.Domain.csproj                src/PruebaCapitole.Domain/
COPY src/PruebaCapitole.Shared/PruebaCapitole.Shared.csproj                src/PruebaCapitole.Shared/
COPY src/PruebaCapitole.Infraestructure/PruebaCapitole.Infrastructure.csproj src/PruebaCapitole.Infraestructure/

# The NuGet cache lives outside the image layers, so it is not duplicated on every build.
RUN --mount=type=cache,target=/root/.nuget/packages \
    dotnet restore "src/PruebaCapitole.Api/PruebaCapitole.Api.csproj"

# 2) Now copy the source code: code changes no longer trigger a new restore.
COPY src/ src/

# 3) A single compile + publish step (no separate build step, so nothing is compiled twice).
RUN --mount=type=cache,target=/root/.nuget/packages \
    dotnet publish "src/PruebaCapitole.Api/PruebaCapitole.Api.csproj" \
        -c $BUILD_CONFIGURATION -o /app/publish --no-restore /p:UseAppHost=false

# Final image: published files only.
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "PruebaCapitole.Api.dll"]
