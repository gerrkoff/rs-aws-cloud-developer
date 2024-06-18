#!/bin/bash
dotnet publish products-service/ProductService/ProductService.csproj -o dist/product-service -r linux-x64
cd "deployment" || exit
cdk deploy
