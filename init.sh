#!/bin/bash
set -e

echo "Starting SSH ..."
service ssh start

dotnet Conductor.Web.dll