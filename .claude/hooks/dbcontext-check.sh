#!/usr/bin/env bash
# Hook: Nhắc nhở tạo EF migration khi sửa ApplicationDbContext

FILE=$(python3 -c "import sys,json; d=json.load(sys.stdin); print(d.get('tool_input',{}).get('file_path',''))" 2>/dev/null)

if echo "$FILE" | grep -qi "DbContext"; then
    printf '{"systemMessage":"DbContext changed -- remember to add EF migration:\n  dotnet ef migrations add <Name> --project FootballBlog.Infrastructure --startup-project FootballBlog.API"}'
fi
