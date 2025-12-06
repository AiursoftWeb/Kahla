#!/bin/bash

# Configuration matches .gitlab-ci.yml
export DOTNET_CLI_TELEMETRY_OPTOUT=1
export PATH=$PATH:$HOME/.dotnet/tools

RED='\033[0;31m'
GREEN='\033[0;32m'
NC='\033[0m' # No Color

# Cleanup only if not in CI (GitLab CI sets the CI variable)
if [ -z "$CI" ]; then
    trap 'rm -f analyze_output.xml' EXIT
fi

echo "Checking for JetBrains ReSharper Global Tools..."
if ! command -v jb &> /dev/null; then
    echo "jb not found, installing..."
    dotnet tool install JetBrains.ReSharper.GlobalTools --global --add-source https://nuget.aiursoft.com/v3/index.json --configfile ./nuget.config -v d
else
    echo "jb is already installed."
fi

echo "Restoring dependencies..."
dotnet restore --no-cache --configfile nuget.config || \
(echo "Restore failed. Retrying in 10 seconds..." && sleep 10 && dotnet restore --no-cache --configfile nuget.config)

echo "Running ReSharper Code Inspection..."
# 3 times retry because sometimes the first time will fail (copied from CI)
if ! jb inspectcode ./*.sln --output=analyze_output.xml --build -f=xml &> /dev/null; then
    echo "First attempt failed, retrying..."
    if ! jb inspectcode ./*.sln --output=analyze_output.xml --build -f=xml &> /dev/null; then
        echo "Second attempt failed, retrying..."
        jb inspectcode ./*.sln --output=analyze_output.xml --build -f=xml &> /dev/null
    fi
fi

echo "Filtering known false positives..."
# Current filters from .gitlab-ci.yml
sed -i '/InconsistentNaming/d' analyze_output.xml
sed -i '/AssignNullToNotNullAttribute/d' analyze_output.xml
sed -i '/UnusedAutoPropertyAccessor/d' analyze_output.xml
sed -i '/DuplicateResource/d' analyze_output.xml

# Check for warnings
# Mimic .gitlab-ci.yml logic: Fail if 'WARNING' string is found.
if grep -q 'WARNING' analyze_output.xml; then
    echo -e "${RED}Linting FAILED!${NC}"
    echo "Issues found:"

    # Filter issues to only show those with Severity="WARNING" or "ERROR"
    # Identify IssueTypes that are Warnings or Errors
    # We use grep and cut to extract the IDs purely from lines with Severity="WARNING" or "ERROR"
    WARNING_IDS=$(grep -E 'Severity="(WARNING|ERROR)"' analyze_output.xml | grep -o 'Id="[^"]*"' | cut -d'"' -f2)

    # Check if we found any IDs (to avoid syntax errors in loop if empty, though unlikely if grep passed)
    if [ ! -z "$WARNING_IDS" ]; then
        for ID in $WARNING_IDS; do
            # Find issues matching this TypeId
            # We assume one issue per line
            grep "TypeId=\"$ID\"" analyze_output.xml | while read -r line; do
                # Extract attributes using grep -o (lazy parsing)
                FILE=$(echo "$line" | grep -o 'File="[^"]*"' | cut -d'"' -f2 | sed 's/\\/\//g')
                LINE=$(echo "$line" | grep -o 'Line="[^"]*"' | cut -d'"' -f2)
                if [ -z "$LINE" ]; then
                     OFFSET=$(echo "$line" | grep -o 'Offset="[^"]*"' | cut -d'"' -f2)
                     if [ ! -z "$OFFSET" ]; then
                          LINE="Offset $OFFSET"
                     else
                          LINE="Unknown"
                     fi
                fi
                MSG=$(echo "$line" | grep -o 'Message="[^"]*"' | cut -d'"' -f2)

                echo "File: $FILE | Line: $LINE | Reason: $MSG"
            done
        done
    else
        echo "Warning severity found in header, but no specific IssueType IDs extracted. Check XML format."
    fi

    exit 1
else
    echo -e "${GREEN}Linting PASSED! No warnings found.${NC}"
fi
