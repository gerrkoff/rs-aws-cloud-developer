FROM mcr.microsoft.com/dotnet/sdk:8.0.302 AS builder

COPY cart-service/CartService/CartService.csproj /app/CartService.csproj

RUN dotnet restore app/CartService.csproj

COPY cart-service/CartService/ /app/

WORKDIR /app

RUN dotnet publish -c Release -o /build

FROM mcr.microsoft.com/dotnet/aspnet:8.0

COPY --from=builder /build /app/

WORKDIR /app

ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "CartService.dll"]
