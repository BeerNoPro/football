#!/usr/bin/env bash
# Hook: Thông báo khi Claude dừng

printf '{"systemMessage":"Claude stopped. Verify with: dotnet build --no-restore -v q"}'
