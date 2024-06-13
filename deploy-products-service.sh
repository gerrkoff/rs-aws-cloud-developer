#!/bin/bash
dotnet build
cd "products-service/ProductsService" || exit
dotnet lambda deploy-serverless --resolve-s3 true
