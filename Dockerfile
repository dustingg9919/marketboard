# syntax=docker/dockerfile:1
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# copy csproj and restore
COPY backend/src/CoffeeDashboard.Domain/CoffeeDashboard.Domain.csproj backend/src/CoffeeDashboard.Domain/
COPY backend/src/CoffeeDashboard.Application/CoffeeDashboard.Application.csproj backend/src/CoffeeDashboard.Application/
COPY backend/src/CoffeeDashboard.Infrastructure/CoffeeDashboard.Infrastructure.csproj backend/src/CoffeeDashboard.Infrastructure/
COPY backend/src/CoffeeDashboard.Api/CoffeeDashboard.Api.csproj backend/src/CoffeeDashboard.Api/
RUN dotnet restore backend/src/CoffeeDashboard.Api/CoffeeDashboard.Api.csproj

# copy everything and publish
COPY . .
RUN dotnet publish backend/src/CoffeeDashboard.Api/CoffeeDashboard.Api.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://0.0.0.0:10000
EXPOSE 10000
ENTRYPOINT ["dotnet", "CoffeeDashboard.Api.dll"]
