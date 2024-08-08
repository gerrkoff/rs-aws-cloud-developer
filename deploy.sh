#!/bin/bash
dotnet publish product-service/ProductService/ProductService.csproj -o dist/product-service -r linux-x64 || exit
dotnet publish import-service/ImportService/ImportService.csproj -o dist/import-service -r linux-x64 || exit
dotnet publish authorization-service/AuthorizationService/AuthorizationService.csproj -o dist/authorization-service -r linux-x64 || exit
#dotnet publish cart-service/CartService/CartService.csproj -o dist/cart-service -r linux-x64 || exit
cd "deployment" || exit
cdk deploy
