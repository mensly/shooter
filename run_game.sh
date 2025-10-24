#!/bin/bash

# Determine build configuration (default to Debug)
CONFIG="${1:-Debug}"

# Build content first
echo "Building content..."
mgcb Content/Content.mgcb /clean /build > /dev/null 2>&1

# Build the game
echo "Building game (configuration: $CONFIG)..."
dotnet build --configuration "$CONFIG"

# Check if build was successful
if [ $? -ne 0 ]; then
    echo "Build failed!"
    exit 1
fi

# Copy .xnb files to output directory
echo "Copying content files..."
mkdir -p "bin/$CONFIG/net8.0/Content"
cp build/Content/bin/DesktopGL/Content/*.xnb "bin/$CONFIG/net8.0/Content/" 2>/dev/null || true

# Determine the output directory
OUTPUT_DIR="bin/$CONFIG/net8.0"

# Get the full path to the output directory
PROJECT_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
OUTPUT_PATH="$PROJECT_ROOT/$OUTPUT_DIR"

# Change to output directory and run the game
echo "Running game from: $OUTPUT_PATH"
cd "$OUTPUT_PATH"
dotnet ./Shooter.dll

