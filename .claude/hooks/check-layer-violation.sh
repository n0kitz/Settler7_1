#!/bin/bash
# Hook: Warn when adding UnityEngine imports to Simulation layer
# Called as PreToolUse hook for Edit|Write tools
# Input: JSON on stdin with tool_input

INPUT=$(cat /dev/stdin)

FILE_PATH=$(echo "$INPUT" | python3 -c "import sys,json; data=json.load(sys.stdin); print(data.get('tool_input',{}).get('file_path',''))" 2>/dev/null)
CONTENT=$(echo "$INPUT" | python3 -c "import sys,json; data=json.load(sys.stdin); print(data.get('tool_input',{}).get('new_string','') or data.get('tool_input',{}).get('content',''))" 2>/dev/null)

if [ -z "$FILE_PATH" ]; then
    exit 0
fi

# Only check Simulation layer files
case "$FILE_PATH" in
    */Scripts/Simulation/*)
        # Check if the new content adds a UnityEngine import
        if echo "$CONTENT" | grep -q "using UnityEngine" 2>/dev/null; then
            echo "WARNING: Adding 'using UnityEngine' to Simulation layer violates architecture rules."
            echo "Simulation/ must be pure C# (no Unity dependencies)."
            echo "Use Vector3/Mathf via System.Numerics or custom types instead."
            exit 2
        fi
        ;;
esac

exit 0
