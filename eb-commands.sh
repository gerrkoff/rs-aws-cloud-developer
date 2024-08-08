#!/bin/bash
eb init
eb create prod --single --cname gerrkoff-cart-api-prod
eb logs -cw enable prod
eb deploy prod