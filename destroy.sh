#!/bin/bash
cd "deployment" || exit
cdk destroy -f
