#!/bin/bash

# Build content first
echo "Building content..."
mgcb Content/Content.mgcb /clean /build > /dev/null 2>&1

# Copy .xnb files to output directory
echo "Copying content files..."
mkdir -p bin/Debug/net8.0/Content
cp Content/bin/DesktopGL/Content/*.xnb bin/Debug/net8.0/Content/ 2>/dev/null || true

# Build the game
echo "Building game..."
dotnet build

# Check if build was successful
if [ $? -ne 0 ]; then
    echo "Build failed!"
    exit 1
fi

# Determine the output directory
OUTPUT_DIR="bin/Debug/net8.0"

# Get the full path to the output directory
PROJECT_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
OUTPUT_PATH="$PROJECT_ROOT/$OUTPUT_DIR"

# Change to output directory and run the game
echo "Running game from: $OUTPUT_PATH"
cd "$OUTPUT_PATH"
dotnet ./Shooter.dll

