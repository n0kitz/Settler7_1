#!/bin/bash
# Hook: Warn when a file exceeds 300 lines after editing (PostToolUse)
# Prints a warning but does not block — the edit has already occurred.

INPUT=$(cat /dev/stdin)
FILE_PATH=$(echo "$INPUT" | python3 -c "import sys,json; data=json.load(sys.stdin); print(data.get('tool_input',{}).get('file_path',''))" 2>/dev/null)

if [ -z "$FILE_PATH" ] || [ ! -f "$FILE_PATH" ]; then
    exit 0
fi

# Only check C# source files
case "$FILE_PATH" in
    *.cs)
        LINE_COUNT=$(wc -l < "$FILE_PATH")
        if [ "$LINE_COUNT" -gt 300 ]; then
            echo "WARNING: $FILE_PATH has $LINE_COUNT lines (architecture limit: 300)."
            echo "Consider splitting into partial classes or extracting a helper."
        fi
        ;;
esac

exit 0
