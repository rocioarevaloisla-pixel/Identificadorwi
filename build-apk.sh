#!/bin/bash
set -e

dotnet publish Identificador/Identificador.csproj \
  -f net10.0-android -c Release

mkdir -p Identificador/APKs
cp Identificador/bin/Release/net10.0-android/*-Signed.apk Identificador/APKs/

echo ""
echo "APK generado en: Identificador/APKs/"
ls -lh Identificador/APKs/*.apk
