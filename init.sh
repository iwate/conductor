#!/bin/bash
set -e

echo "Starting SSH ..."
service ssh start

cd /app

dotnet Conductor.Web.dll