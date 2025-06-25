#!/bin/bash
# Usage: ./make_mpak.sh path/to/sensor-config.json path/to/info.json

SENSOR_CONFIG="sensor-config.json"
INFO_JSON="./info.json"
OUTPUT_ZIP="sensor-config.mpak"

# Create a temporary directory
TMPDIR=$(mktemp -d)

# Create app directory inside temp
mkdir -p "$TMPDIR/app"

# Copy files to the appropriate locations
cp "$INFO_JSON" "$TMPDIR/info.json"
cp "$SENSOR_CONFIG" "$TMPDIR/app/sensor-config.json"

# Create the zip archive
(cd "$TMPDIR" && zip -r "../$OUTPUT_ZIP" info.json app)

# Move the zip to current directory and clean up
mv "$TMPDIR"/../"$OUTPUT_ZIP" .
rm -rf "$TMPDIR"

echo "Created $OUTPUT_ZIP"