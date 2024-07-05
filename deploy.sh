#!/bin/bash
dotnet publish product-service/ProductService/ProductService.csproj -o dist/product-service -r linux-x64 || exit
dotnet publish import-service/ImportService/ImportService.csproj -o dist/import-service -r linux-x64 || exit
cd "deployment" || exit
cdk deploy
