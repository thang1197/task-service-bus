
# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

COPY . .

# Receive the token passed by az acr build
ARG GITHUB_TOKEN

# Verify that the token is available without exposing it
RUN test -n "$GITHUB_TOKEN" && echo "GitHub token is available" || (echo "GitHub token is missing" && exit 1)

RUN dotnet restore
RUN dotnet publish -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0

WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "TaskManagementServiceBusApi.dll"]
