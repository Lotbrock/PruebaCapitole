# Runtime base: ASP.NET runtime only (no SDK), so the final image stays small.
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080


# Build stage (discarded: it is not part of the final image)
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# 1) Copy only the .csproj files: the restore layer stays cached until dependencies change.
COPY src/PruebaCapitole.Notifications.Worker/PruebaCapitole.Notifications.Worker.csproj src/PruebaCapitole.Notifications.Worker/
COPY src/PruebaCapitole.Shared/PruebaCapitole.Shared.csproj                             src/PruebaCapitole.Shared/

# The NuGet cache lives outside the image layers, so it is not duplicated on every build.
RUN --mount=type=cache,target=/root/.nuget/packages,sharing=locked \
    dotnet restore "src/PruebaCapitole.Notifications.Worker/PruebaCapitole.Notifications.Worker.csproj"

# 2) Copy the source code: code changes no longer trigger a new restore.
COPY src/PruebaCapitole.Notifications.Worker/ src/PruebaCapitole.Notifications.Worker/
COPY src/PruebaCapitole.Shared/               src/PruebaCapitole.Shared/

# 3) A single compile + publish step.
RUN --mount=type=cache,target=/root/.nuget/packages,sharing=locked \
    dotnet publish "src/PruebaCapitole.Notifications.Worker/PruebaCapitole.Notifications.Worker.csproj" \
        -c $BUILD_CONFIGURATION -o /app/publish --no-restore /p:UseAppHost=false

# Final image: published files only.
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "PruebaCapitole.Notifications.Worker.dll"]
