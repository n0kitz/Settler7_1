#!/bin/bash
# Hook: Block edits to Unity-managed files
# Called as PreToolUse hook for Edit|Write tools
# Input: JSON on stdin with tool_input.file_path

FILE_PATH=$(cat /dev/stdin | python3 -c "import sys,json; data=json.load(sys.stdin); print(data.get('tool_input',{}).get('file_path',''))" 2>/dev/null)

if [ -z "$FILE_PATH" ]; then
    exit 0
fi

# Block edits to protected paths
case "$FILE_PATH" in
    */ProjectSettings/*)
        echo "BLOCKED: Do not edit ProjectSettings/ — use Unity Editor instead."
        exit 2
        ;;
    */Library/*)
        echo "BLOCKED: Do not edit Library/ — Unity-managed directory."
        exit 2
        ;;
    */.git/*)
        echo "BLOCKED: Do not edit .git/ — use git commands instead."
        exit 2
        ;;
    *.meta)
        echo "BLOCKED: Do not edit .meta files — Unity manages these automatically."
        exit 2
        ;;
esac

exit 0
