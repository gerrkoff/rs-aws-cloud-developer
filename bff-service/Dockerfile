FROM mcr.microsoft.com/dotnet/sdk:8.0.302 AS builder

COPY bff-service/BffService/BffService.csproj /app/BffService.csproj

RUN dotnet restore app/BffService.csproj

COPY bff-service/BffService/ /app/

WORKDIR /app

RUN dotnet publish -c Release -o /build

FROM mcr.microsoft.com/dotnet/aspnet:8.0

COPY --from=builder /build /app/

WORKDIR /app

ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 8080

ENTRYPOINT ["dotnet", "BffService.dll"]
