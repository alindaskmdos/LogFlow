FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 5000

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY LogFlow.Api/LogFlow.Api.csproj ./LogFlow.Api/
RUN dotnet restore ./LogFlow.Api/LogFlow.Api.csproj

COPY . .

WORKDIR /src/LogFlow.Api
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "LogFlow.Api.dll"]