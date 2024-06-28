#!/bin/bash
TABLE_PRODUCTS='AwsShopBE-AwsShopProducts76BDA372-8ZCSC80K3CVW'
TABLE_STOCKS='AwsShopBE-AwsShopStocks6A960CF9-1IP3OQPLOCCUV'

aws dynamodb put-item \
    --table-name $TABLE_PRODUCTS \
    --item '{"id": {"S": "7567ec4b-b10c-48c5-9345-fc73c48a80a0"}, "title": {"S": "Guitar"}, "description": {"S": "Top guitar"}, "price": {"N": "100"}}'
aws dynamodb put-item \
    --table-name $TABLE_PRODUCTS \
    --item '{"id": {"S": "7567ec4b-b10c-48c5-9345-fc73c48a80a1"}, "title": {"S": "Drums"}, "description": {"S": "Top drums"}, "price": {"N": "200"}}'
aws dynamodb put-item \
    --table-name $TABLE_PRODUCTS \
    --item '{"id": {"S": "7567ec4b-b10c-48c5-9345-fc73c48a80a3"}, "title": {"S": "Mic"}, "description": {"S": "Top mic"}, "price": {"N": "300"}}'
aws dynamodb put-item \
    --table-name $TABLE_PRODUCTS \
    --item '{"id": {"S": "7567ec4b-b10c-48c5-9345-fc73348a80a2"}, "title": {"S": "Bass"}, "description": {"S": "Top bass"}, "price": {"N": "300"}}'        
        
aws dynamodb put-item \
    --table-name $TABLE_STOCKS \
    --item '{"productId": {"S": "7567ec4b-b10c-48c5-9345-fc73c48a80a0"}, "count": {"N": "3"}}'
aws dynamodb put-item \
    --table-name $TABLE_STOCKS \
    --item '{"productId": {"S": "7567ec4b-b10c-48c5-9345-fc73c48a80a1"}, "count": {"N": "5"}}'
aws dynamodb put-item \
    --table-name $TABLE_STOCKS \
    --item '{"productId": {"S": "7567ec4b-b10c-48c5-9345-fc73c48a80a3"}, "count": {"N": "2"}}'
aws dynamodb put-item \
    --table-name $TABLE_STOCKS \
    --item '{"productId": {"S": "7567ec4b-b10c-48c5-9345-fc73348a80a2"}, "count": {"N": "1"}}'
