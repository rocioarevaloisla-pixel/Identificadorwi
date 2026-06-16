#!/usr/bin/env bash
set -e

echo "🚀 Iniciando emulador..."
export ANDROID_AVD_HOME="$HOME/.config/.android/avd"
export QT_QPA_PLATFORM=xcb
/opt/android-sdk/emulator/emulator -avd Pixel_6 &
EMU_PID=$!

echo "⏳ Esperando a que arranque..."
adb wait-for-device
echo "✅ Emulador listo"

echo "📱 Ejecutando app..."
dotnet run -f net10.0-android
