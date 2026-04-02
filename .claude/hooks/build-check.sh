#!/usr/bin/env bash
# Hook: Auto-format + build check sau khi edit file .cs
# Output systemMessage JSON để Claude đọc được lỗi và tự fix

export PATH="$PATH:/c/Program Files/dotnet"

FILE=$(python3 -c "import sys,json; d=json.load(sys.stdin); print(d.get('tool_input',{}).get('file_path',''))" 2>/dev/null)

# Chỉ chạy khi edit file .cs hoặc .razor
if ! echo "$FILE" | grep -qiE '\.(cs|razor)$'; then exit 0; fi

# Step 1: Auto-format toàn solution (sửa thẳng file, không chỉ check)
dotnet format /c/Users/Admin/football --no-restore 2>/dev/null

# Step 2: Build và capture output
BUILD_OUTPUT=$(dotnet build /c/Users/Admin/football --no-restore -v q 2>&1)
BUILD_STATUS=$?

# Step 3: Ghi log ra file theo ngày
LOG_DIR="/c/Users/Admin/football/logs/build"
LOG_FILE="$LOG_DIR/build-$(date +%Y%m%d).log"
mkdir -p "$LOG_DIR"
echo "[$(date '+%Y-%m-%d %H:%M:%S')] FILE: $FILE" >> "$LOG_FILE"
echo "$BUILD_OUTPUT" >> "$LOG_FILE"
echo "---" >> "$LOG_FILE"

# Step 4: Nếu build fail → feed lỗi vào Claude context để tự fix
if [ $BUILD_STATUS -ne 0 ]; then
    ERRORS=$(echo "$BUILD_OUTPUT" | grep -E "error CS|Build FAILED" | head -10)
    printf '{"systemMessage":"BUILD FAILED after editing %s:\n%s\nFull log: %s"}' \
        "$FILE" "$ERRORS" "$LOG_FILE"
fi
