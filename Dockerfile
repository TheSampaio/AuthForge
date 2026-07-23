# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY Backend/Domain/Domain.csproj Backend/Domain/
COPY Backend/Application/Application.csproj Backend/Application/
COPY Backend/Infrastructure/Infrastructure.csproj Backend/Infrastructure/
COPY Backend/Presentation/Presentation.csproj Backend/Presentation/
RUN dotnet restore Backend/Presentation/Presentation.csproj

COPY Backend/Domain/ Backend/Domain/
COPY Backend/Application/ Backend/Application/
COPY Backend/Infrastructure/ Backend/Infrastructure/
COPY Backend/Presentation/ Backend/Presentation/

RUN dotnet publish Backend/Presentation/Presentation.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .
RUN adduser --disabled-password --gecos "" appuser \
    && chown -R appuser:appuser /app
USER appuser

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "Presentation.dll"]
