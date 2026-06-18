#!/usr/bin/env bash
set -e

echo "📱 Buscando dispositivo Android..."
DEVICES=$(adb devices | grep -w "device" | awk '{print $1}')

if [ -z "$DEVICES" ]; then
  echo "❌ No se detectó ningún dispositivo. Conecta tu celular por USB con depuración USB activada."
  exit 1
fi

echo "✅ Dispositivo encontrado: $DEVICES"
echo "🚀 Ejecutando app en el dispositivo..."

dotnet run --project Identificador -f net10.0-android
