# DevOps — Kiến thức & Thực hành

> Tổng hợp từ roadmap.sh/devops + thực tế deploy .NET 8 trên Linux VPS. Cập nhật 2026-05-14.

---

## DevOps là gì?

**DevOps** = văn hóa kết hợp giữa **Dev** (viết code) và **Ops** (vận hành server). Mục tiêu là giao phần mềm liên tục, nhanh, và ổn định qua **tự động hóa**.

Không phải một chức danh cố định — mà là tập hợp kỹ năng và mindset. Developer học DevOps = tự deploy được app, không phụ thuộc team Ops.

---

## Roadmap tổng quan (roadmap.sh/devops)

```
1. Nền tảng
   ├── OS & Linux basics
   ├── Networking & Security
   └── Version Control (Git)

2. Infrastructure
   ├── Cloud (AWS / Azure / GCP / Oracle)
   ├── IaC — Infrastructure as Code (Terraform, Ansible)
   └── Containers (Docker, Kubernetes)

3. CI/CD
   ├── GitHub Actions / GitLab CI / Jenkins
   └── Artifact management

4. Observability
   ├── Logging (Loki, ELK, Serilog)
   ├── Metrics (Prometheus, Grafana)
   └── Uptime monitoring (Uptime Kuma)

5. Security
   ├── SSH hardening
   ├── Firewall (UFW, iptables)
   ├── Secrets management
   └── Vulnerability scanning
```

---

## Skills theo cấp độ

### Entry-level
- Git cơ bản: commit, branch, push, pull, PR
- Bash scripting cơ bản: navigate, process, pipes
- Docker: build image, chạy container, đọc log
- CI/CD cơ bản: hiểu pipeline, viết workflow đơn giản
- Linux CLI: file system, process, permissions, package manager
- Một cloud platform ở mức dùng được (tạo VM, mở port, SSH vào)
- Reverse proxy: Caddy hoặc Nginx cơ bản

### Mid-level
- Docker Compose + multi-container, healthcheck, volumes, networks
- CI/CD nâng cao: secrets, cache, artifacts, matrix builds, deploy tự động
- Cloud VPC/networking: security groups, firewall rules, DNS
- Monitoring cơ bản: Uptime Kuma, Grafana dashboard
- IaC cơ bản: Terraform (provision VM bằng code)
- Security: SSH hardening, fail2ban, UFW, secrets management

### Senior-level
- Kubernetes: deployment, service, ingress, HPA, RBAC
- Terraform + Ansible kết hợp (provision + configure tự động)
- Multi-cloud / hybrid cloud architecture
- Observability đầy đủ: Prometheus + Grafana + alerting + tracing
- SRE mindset: SLO, SLI, error budget, incident response, runbooks
- Security: zero-trust, secret rotation, SAST/DAST trong pipeline
- Cost optimization: right-sizing, spot instances, reserved capacity

---

## IDE & Tools Setup

### VS Code — Extensions cần thiết cho DevOps

| Extension | ID | Dùng để làm gì |
|-----------|-----|----------------|
| **Remote - SSH** | `ms-vscode-remote.remote-ssh` | SSH vào server, edit code trên server như local |
| **Docker** | `ms-azuretools.vscode-docker` | Manage container, syntax highlight Dockerfile/Compose |
| **GitHub Actions** | `github.vscode-github-actions` | Xem workflow status, validate YAML Actions ngay trong editor |
| **GitLens** | `eamodio.gitlens` | Xem git blame từng dòng, history, compare branch |
| **YAML** | `redhat.vscode-yaml` | Validate + autocomplete YAML (Compose, Actions, Kubernetes) |
| **HashiCorp Terraform** | `hashicorp.terraform` | Syntax, format, validate `.tf` files |
| **ShellCheck** | `timonwong.shellcheck` | Lint bash script, phát hiện lỗi trước khi chạy |
| **Shell Format** | `foxundermoon.shell-format` | Format bash script |
| **DotENV** | `mikestead.dotenv` | Syntax highlight file `.env` |

### Remote SSH — SSH vào server từ VS Code

1. Cài extension **Remote - SSH**
2. `Ctrl+Shift+P` → `Remote-SSH: Add New SSH Host`
3. Nhập: `ssh ubuntu@<SERVER_IP> -i ~/.ssh/id_ed25519`
4. `Ctrl+Shift+P` → `Remote-SSH: Connect to Host` → chọn server
5. VS Code mở terminal và file explorer trực tiếp trên server

**Lợi ích**: Edit Caddyfile, `.env.prod`, docker-compose.prod.yml ngay trên server — không cần copy file qua lại.

### Docker Extension trong VS Code

Sau khi cài extension Docker:
- Sidebar Docker icon → xem tất cả container đang chạy
- Click chuột phải container → View Logs / Open in Terminal / Stop / Restart
- Dockerfile: hover để xem docs, autocomplete instruction

---

## 1. Git — Version Control

### Khái niệm

| Thuật ngữ | Nghĩa |
|-----------|-------|
| **Repository (repo)** | Thư mục project được Git theo dõi. Có thư mục `.git/` ẩn bên trong |
| **Working tree** | Files đang chỉnh sửa trên máy local |
| **Staging area (index)** | Khu vực "chờ commit" — `git add` đưa file vào đây |
| **Commit** | Snapshot trạng thái project tại 1 thời điểm — có hash ID duy nhất |
| **Branch** | Nhánh phát triển song song. `master`/`main` là nhánh chính |
| **Remote** | Repo trên server (GitHub, GitLab). Mặc định tên là `origin` |
| **Push / Pull** | Upload commit lên remote / Download commit về local |
| **Merge** | Gộp 2 branch lại |
| **PR (Pull Request)** | Yêu cầu merge branch vào main — review trước khi merge |

### Lệnh cơ bản

```bash
# Khởi tạo
git init                              # tạo repo mới từ thư mục hiện tại
git clone https://github.com/user/repo.git          # clone repo về
git clone https://github.com/user/repo.git my-folder # clone vào thư mục tên khác

# Trạng thái
git status                            # xem file nào thay đổi / staged / untracked
git status --short                    # compact output: M=modified, A=added, ?=untracked

# Stage & Commit
git add file.txt                      # stage 1 file
git add .                             # stage tất cả thay đổi
git add *.cs                          # stage tất cả file .cs
git commit -m "Fix login bug"         # commit với message
git commit -am "Update config"        # stage tracked files + commit cùng lúc

# Push / Pull
git push                              # push lên remote branch mặc định
git push origin master                # push lên branch cụ thể
git push -u origin feature/login      # set upstream và push
git pull                              # fetch + merge từ remote
git pull origin master
git pull --rebase                     # fetch + rebase thay vì merge
```

### Branch

```bash
git branch                            # list local branches
git branch -a                         # list tất cả (local + remote)
git branch feature/login              # tạo branch mới
git branch -d feature/old             # xóa branch đã merge
git branch -D feature/old             # xóa branch dù chưa merge (cẩn thận)
git branch -m old-name new-name       # đổi tên branch

git checkout main                     # switch sang branch main
git checkout -b feature/dashboard     # tạo và switch sang branch mới
git switch main                       # cách mới (Git 2.23+)
git switch -c feature/dashboard       # tạo và switch (cách mới)
```

### Merge & Rebase

```bash
# Merge: tạo 1 commit merge — giữ nguyên lịch sử
git merge feature/login               # merge vào branch hiện tại
git merge --no-ff feature/login       # tạo merge commit kể cả fast-forward được
git merge --squash feature/login      # gộp tất cả commit của branch thành 1

# Rebase: viết lại lịch sử — commits xuất hiện "trên đỉnh" branch target
git rebase main                       # rebase branch hiện tại lên main
git rebase -i HEAD~3                  # interactive rebase 3 commit cuối (squash, reword, drop)
git rebase --continue                 # tiếp tục sau khi resolve conflict
git rebase --abort                    # hủy rebase
```

### Stash — lưu tạm thay đổi

```bash
git stash                             # lưu tất cả thay đổi vào stack tạm
git stash push -m "WIP: login form"   # lưu kèm tên mô tả
git stash -u                          # bao gồm cả untracked files
git stash list                        # xem danh sách stash
git stash pop                         # lấy stash mới nhất ra và xóa khỏi stack
git stash apply stash@{0}             # lấy ra nhưng giữ lại trong stack
git stash drop stash@{0}              # xóa 1 stash cụ thể
git stash clear                       # xóa tất cả stash
```

### Undo / Reset

```bash
git reset file.txt                    # unstage file (bỏ ra khỏi staging area)
git restore file.txt                  # discard thay đổi trong working tree (cách mới)
git checkout -- file.txt              # discard thay đổi (cách cũ)

git reset HEAD~1                      # undo commit cuối, giữ nguyên thay đổi (unstaged)
git reset --soft HEAD~1               # undo commit cuối, giữ nguyên thay đổi (staged)
git reset --hard HEAD~1               # undo commit cuối, XÓA thay đổi — không recover được
git revert HEAD                       # tạo commit mới đảo ngược commit cuối (an toàn hơn reset)
```

### Log & Diff

```bash
git log                               # full history
git log --oneline                     # compact: 1 commit 1 dòng
git log --graph --all --oneline       # visualize branch graph
git log -p                            # xem diff từng commit
git log --author="hungpv"             # filter theo tác giả
git log --since="1 week ago"          # filter theo thời gian

git diff                              # thay đổi chưa staged
git diff --staged                     # thay đổi đã staged (sắp commit)
git diff main..feature/login          # so sánh 2 branch
git diff --name-only HEAD~1           # chỉ tên file thay đổi trong commit cuối
```

### Tag — đánh dấu release

```bash
git tag v1.0.0                        # lightweight tag
git tag -a v1.0.0 -m "Release 1.0"   # annotated tag (có message)
git tag -l                            # list tất cả tag
git push origin v1.0.0                # push tag lên remote
git push origin --tags                # push tất cả tag
```

### .gitignore — file không commit

```gitignore
# .NET
bin/
obj/
*.user
*.suo
.vs/

# Secrets — QUAN TRỌNG: không bao giờ commit
.env
.env.prod
appsettings.Production.json
*.pem
*.key

# Logs
logs/
*.log

# OS
.DS_Store
Thumbs.db

# IDE
.vscode/settings.json
.idea/
```

---

## 2. OS & Linux Basics

### Cấu trúc thư mục Linux

```
/                   # root — gốc của toàn bộ hệ thống
├── etc/            # config files: sshd_config, crontab, hosts, fstab
├── var/
│   ├── log/        # log files: syslog, auth.log, nginx/, docker/
│   └── lib/        # app data: docker images, postgresql data
├── home/
│   └── ubuntu/     # home directory của user ubuntu
├── usr/
│   └── bin/        # executable programs: docker, git, caddy
├── tmp/            # temp files — xóa khi reboot
└── proc/           # virtual FS: thông tin process, memory, CPU
```

### Lệnh điều hướng & file

```bash
pwd                                   # thư mục đang đứng
ls -la                                # list files + hidden + permissions + size
ls -lh                                # size dạng human-readable (KB, MB)
cd /home/ubuntu/football              # di chuyển đến thư mục
cd ~                                  # về home directory
cd -                                  # về thư mục trước đó

mkdir -p /home/ubuntu/app/logs        # tạo thư mục kể cả parent
rm file.txt                           # xóa file
rm -rf /path/to/dir                   # xóa thư mục đệ quy — CẨN THẬN
cp source.txt dest.txt                # copy file
cp -r src/ dest/                      # copy thư mục
mv old.txt new.txt                    # đổi tên / di chuyển

cat file.txt                          # xem toàn bộ file
less file.txt                         # xem từng trang (q để thoát)
head -20 file.txt                     # 20 dòng đầu
tail -50 file.txt                     # 50 dòng cuối
tail -f /var/log/syslog               # follow log real-time
grep "ERROR" app.log                  # tìm dòng chứa "ERROR"
grep -r "pattern" /var/log/           # tìm đệ quy trong thư mục
grep -n "ERROR" app.log               # kèm số dòng

find /home -name "*.log"              # tìm file theo tên
find /var/log -mtime +7               # file không sửa quá 7 ngày
```

### Package management (apt)

```bash
sudo apt update                       # cập nhật danh sách package (luôn làm trước khi install)
sudo apt upgrade                      # nâng cấp tất cả package đã cài
sudo apt install docker.io git htop   # cài package
sudo apt remove package-name          # gỡ package
sudo apt autoremove                   # xóa dependency không còn dùng
apt search keyword                    # tìm package
apt show package-name                 # thông tin chi tiết package
apt list --installed                  # list package đã cài
```

### Process management

```bash
ps aux                                # tất cả process đang chạy
ps aux | grep dotnet                  # tìm process theo tên
kill PID                              # gửi SIGTERM (yêu cầu tắt nhẹ nhàng)
kill -9 PID                           # gửi SIGKILL (tắt ngay lập tức)
pkill -f "dotnet"                     # kill process theo tên

htop                                  # interactive monitor (cài: apt install htop)
top                                   # monitor mặc định (kém hơn htop)

# Chạy process ngầm
nohup ./script.sh &                   # chạy background, không bị kill khi logout
screen -S mysession                   # tạo session tách biệt
tmux new -s deploy                    # tmux: mạnh hơn screen
```

### Disk & Memory

```bash
df -h                                 # disk usage tất cả partition
du -sh /var/log/                      # size của thư mục cụ thể
du -sh /var/log/* | sort -hr          # sort theo size giảm dần
free -h                               # RAM + swap usage
vmstat 1 5                            # memory + CPU stats mỗi 1s, 5 lần
```

### Systemd — quản lý service

```bash
sudo systemctl start caddy            # khởi động service
sudo systemctl stop caddy             # dừng
sudo systemctl restart caddy          # restart
sudo systemctl reload caddy           # reload config (không restart process)
sudo systemctl enable caddy           # auto-start khi reboot
sudo systemctl disable caddy          # bỏ auto-start
sudo systemctl status caddy           # xem trạng thái + log gần nhất
sudo systemctl is-active caddy        # active / inactive

journalctl -u caddy                   # toàn bộ log của service
journalctl -u caddy -f                # follow log real-time
journalctl -u caddy --since "1 hour ago"
journalctl -u caddy -n 100            # 100 dòng cuối
```

### Crontab — lập lịch tự động

Cú pháp: `minute hour day-of-month month day-of-week command`

```bash
crontab -e                            # edit crontab của user hiện tại
crontab -l                            # xem crontab hiện tại
sudo crontab -e                       # edit crontab của root

# Ví dụ:
0 2 * * *    /home/ubuntu/backup.sh   # 2:00 AM mỗi ngày
*/5 * * * *  curl https://yourdomain.com/health  # mỗi 5 phút
0 0 * * 0    docker image prune -f    # Chủ nhật nửa đêm dọn image
30 1 * * *   certbot renew --quiet    # 1:30 AM mỗi ngày check cert
```

```
Ký tự đặc biệt:
*     = mọi giá trị
*/5   = mỗi 5 đơn vị
1-5   = từ 1 đến 5
1,3,5 = các giá trị cụ thể
```

### Log management — Logrotate

Logrotate tự động xoay vòng + nén + xóa log cũ. Config: `/etc/logrotate.conf`

```bash
# Tạo config cho app: /etc/logrotate.d/footballblog
sudo nano /etc/logrotate.d/footballblog
```

```
/home/ubuntu/football/logs/*.log {
    daily               # xoay vòng mỗi ngày
    rotate 14           # giữ 14 file cũ
    compress            # nén bằng gzip
    delaycompress       # không nén file vừa xoay (app có thể đang ghi)
    missingok           # không lỗi nếu file không tồn tại
    notifempty          # không xoay nếu file rỗng
    create 0644 ubuntu ubuntu  # tạo file log mới với permission này
}
```

```bash
logrotate -f /etc/logrotate.conf      # force rotate ngay (test)
logrotate -d /etc/logrotate.conf      # dry-run: xem sẽ làm gì, không thực hiện
```

---

## 3. Networking

### Khái niệm cơ bản

| Thuật ngữ | Nghĩa |
|-----------|-------|
| **IP** | Địa chỉ máy tính trên mạng. Public IP = ra internet. Private IP = nội bộ |
| **Port** | "Cổng" của service. HTTP=80, HTTPS=443, SSH=22, PostgreSQL=5432, Redis=6379 |
| **DNS** | Chuyển domain → IP. `nslookup yourdomain.com` / `dig yourdomain.com` |
| **Firewall** | Bộ lọc traffic: ai được vào/ra port nào |
| **Reverse Proxy** | Đứng trước app, nhận request → forward đến container |
| **Load Balancer** | Phân phối request đến nhiều server/container |
| **TLS/SSL** | Mã hóa HTTPS. Let's Encrypt cấp cert miễn phí tự động |
| **CDN** | Mạng phân phối nội dung tĩnh (Cloudflare, CloudFront) — cache gần user |
| **VPC/VCN** | Mạng ảo riêng trong cloud |
| **Subnet** | Chia nhỏ VPC: public subnet (có internet), private subnet (nội bộ) |
| **NAT Gateway** | Cho private subnet ra internet mà không expose trực tiếp |

### Ports thường dùng

| Port | Service |
|------|---------|
| 22 | SSH |
| 80 | HTTP |
| 443 | HTTPS |
| 3000 | Grafana, Node.js dev |
| 3001 | Uptime Kuma |
| 5432 | PostgreSQL |
| 6379 | Redis |
| 8080 | App HTTP (dev/prod container) |
| 9090 | Prometheus |

### Network commands

```bash
ss -tuln                              # port nào đang listen
netstat -tuln                         # tương tự ss (cài net-tools)
lsof -i :8080                         # process nào đang dùng port 8080
curl -I https://yourdomain.com        # xem HTTP headers + status code
curl -v https://yourdomain.com        # verbose: xem toàn bộ handshake
wget -q -O- https://yourdomain.com/health  # test endpoint

ping yourdomain.com                   # kiểm tra connectivity
traceroute yourdomain.com             # trace route đến server
nslookup yourdomain.com               # DNS lookup
dig yourdomain.com                    # DNS lookup chi tiết hơn
dig +short yourdomain.com             # chỉ xem IP
```

### Tại sao cần Reverse Proxy?

```
Internet
    │
    ▼ port 443 (HTTPS)
  Caddy  ──────────── tự xử lý TLS, rate limit, compression
    │
    ├── api.yourdomain.com → localhost:8080  (API container)
    └── yourdomain.com     → localhost:8081  (Web container)
```

- 1 IP / 1 server chạy nhiều app qua domain khác nhau
- App container chỉ bind `127.0.0.1` — không expose trực tiếp
- Caddy xử lý HTTPS, app chỉ nói HTTP

### DNS Setup

Tạo A record tại DNS provider (Cloudflare, Namecheap):
```
yourdomain.com      A    <Oracle VM public IP>
api.yourdomain.com  A    <Oracle VM public IP>
status.yourdomain.com A  <Oracle VM public IP>
```
DNS propagation mất 1–48 giờ. Kiểm tra: `dig yourdomain.com +short`

---

## 4. Docker

### Khái niệm

| Thuật ngữ | Nghĩa |
|-----------|-------|
| **Image** | Bản đóng gói app — code + runtime + dependencies, read-only |
| **Container** | Image đang chạy, có thể write vào layer trên cùng |
| **Dockerfile** | Script để build image từng bước |
| **Docker Compose** | Chạy nhiều container từ 1 file YAML, quản lý network/volume |
| **Volume** | Lưu data bên ngoài container lifecycle — restart không mất data |
| **Network** | Mạng ảo cho container giao tiếp nhau |
| **Registry** | Kho image. Docker Hub (public), GHCR (GitHub), ECR (AWS) |
| **Layer** | Mỗi Dockerfile instruction tạo 1 layer — cached khi build lại |

### Lệnh Docker đầy đủ

```bash
# ── IMAGE ──────────────────────────────────────────────────
docker images                         # list images local
docker pull nginx:alpine              # download image từ registry
docker rmi nginx:alpine               # xóa image
docker image prune -f                 # xóa image dangling (không có tag)
docker image prune -af                # xóa tất cả image không dùng

# ── BUILD ──────────────────────────────────────────────────
docker build -t myapp:1.0 .           # build từ Dockerfile trong thư mục hiện tại
docker build -t myapp -f Dockerfile.api .  # chỉ định Dockerfile khác
docker build --no-cache -t myapp .    # build không dùng cache

# ── CONTAINER ──────────────────────────────────────────────
docker run -d -p 8080:8080 --name api myapp   # chạy background
docker run -it ubuntu bash            # chạy interactive (có terminal)
docker run --rm myapp                 # tự xóa container sau khi stop
docker run -e KEY=value myapp         # truyền env var
docker run --env-file .env myapp      # truyền env từ file

docker ps                             # container đang chạy
docker ps -a                          # tất cả (kể cả stopped)
docker start api                      # start container đã tạo
docker stop api                       # stop graceful (SIGTERM)
docker kill api                       # stop ngay (SIGKILL)
docker restart api                    # restart
docker rm api                         # xóa container
docker rm -f api                      # xóa container đang chạy

# ── DEBUG ──────────────────────────────────────────────────
docker logs api                       # xem log
docker logs api -f                    # follow real-time
docker logs api --tail 100            # 100 dòng cuối
docker logs api --since 1h            # log 1 giờ gần nhất

docker exec -it api bash              # vào shell trong container
docker exec api cat /app/appsettings.json  # chạy lệnh không cần shell

docker inspect api                    # toàn bộ config: network, env, mounts
docker stats                          # CPU/RAM real-time tất cả container
docker stats api                      # chỉ 1 container

# ── VOLUME ──────────────────────────────────────────────────
docker volume ls                      # list volumes
docker volume create mydata           # tạo volume
docker volume inspect mydata          # xem chi tiết
docker volume rm mydata               # xóa volume
docker volume prune                   # xóa volumes không dùng

# ── NETWORK ──────────────────────────────────────────────────
docker network ls                     # list networks
docker network create mynet           # tạo network
docker network inspect mynet          # xem chi tiết
docker network connect mynet api      # kết nối container vào network
```

### Multi-stage build (.NET 8)

```dockerfile
# Stage 1: Build — dùng SDK image lớn (~800MB)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy .csproj trước → cache layer khi chỉ thay đổi code (không thay đổi dependencies)
COPY FootballBlog.Core/FootballBlog.Core.csproj FootballBlog.Core/
COPY FootballBlog.API/FootballBlog.API.csproj FootballBlog.API/
RUN dotnet restore FootballBlog.API/FootballBlog.API.csproj

COPY FootballBlog.Core/ FootballBlog.Core/
COPY FootballBlog.API/ FootballBlog.API/
RUN dotnet publish FootballBlog.API/FootballBlog.API.csproj -c Release -o /app

# Stage 2: Runtime — chỉ dùng runtime image nhỏ (~200MB)
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "FootballBlog.API.dll"]
```

### .dockerignore — không copy vào image

```dockerignore
# Build artifacts
**/bin/
**/obj/

# Git
.git/
.gitignore

# IDE
.vs/
.vscode/
**/*.user

# Secrets
.env
.env.prod
*.pem

# Docs
*.md
docs/

# Logs
logs/
*.log
```

### Docker Compose nâng cao

```yaml
services:
  api:
    build:
      context: .
      dockerfile: Dockerfile.api
    ports:
      - "127.0.0.1:8080:8080"       # chỉ bind localhost — không expose ra internet
    env_file: .env.prod
    restart: unless-stopped          # luôn restart trừ khi tắt thủ công
    networks:
      - internal
    volumes:
      - logs:/app/logs               # named volume cho logs
    healthcheck:
      test: ["CMD-SHELL", "curl -f http://localhost:8080/health || exit 1"]
      interval: 30s
      timeout: 10s
      retries: 5
      start_period: 60s              # grace period khi lần đầu start

  web:
    build:
      context: .
      dockerfile: Dockerfile.web
    ports:
      - "127.0.0.1:8081:8080"
    env_file: .env.prod
    restart: unless-stopped
    networks:
      - internal
    depends_on:
      api:
        condition: service_healthy   # chờ API healthy mới start Web

networks:
  internal:                          # mạng nội bộ giữa các container

volumes:
  logs:                              # named volume
```

### Docker Compose commands

```bash
docker compose up -d --build          # build + start background
docker compose down                   # stop + xóa container (giữ volume)
docker compose down -v                # stop + xóa container + volume
docker compose restart                # restart tất cả service
docker compose restart api            # restart 1 service

docker compose ps                     # trạng thái các service
docker compose logs -f                # follow log tất cả service
docker compose logs -f api            # follow log 1 service
docker compose logs --tail=100 api    # 100 dòng cuối
docker compose exec api bash          # vào shell trong container
docker compose exec api dotnet --version

docker compose config                 # validate + xem config đã merge
docker compose build --no-cache       # build lại không dùng cache
docker compose pull                   # pull image mới nhất

# Scale (nhiều instance)
docker compose up -d --scale web=3    # chạy 3 instance của web
```

### ARM64 (Oracle Cloud A1)

Build trực tiếp trên server — Docker tự detect ARM64, dùng đúng base image:
```bash
# Trên Oracle ARM64 server:
git pull origin master
docker compose -f docker-compose.prod.yml up -d --build
# Không cần --platform flag khi build native
```

Nếu build trên máy x86 local rồi push lên registry:
```bash
docker buildx create --use
docker buildx build --platform linux/arm64 -t ghcr.io/user/myapp:latest --push .
```

---

## 5. CI/CD với GitHub Actions

### Khái niệm

| Thuật ngữ | Nghĩa |
|-----------|-------|
| **Pipeline** | Chuỗi bước tự động: test → build → deploy |
| **Workflow** | File YAML trong `.github/workflows/` định nghĩa pipeline |
| **Job** | Nhóm steps chạy trên 1 runner (máy ảo) |
| **Step** | 1 bước: chạy lệnh shell hoặc dùng pre-built action |
| **Action** | Module tái sử dụng từ Marketplace (`uses: actions/checkout@v4`) |
| **Runner** | Máy chạy job. `ubuntu-latest` = GitHub-hosted, miễn phí |
| **Secret** | Biến nhạy cảm lưu trong GitHub Settings — không lộ trong log |
| **Environment** | Môi trường deploy (staging, production) — có secrets riêng |
| **Artifact** | File output của job — truyền giữa các job hoặc download sau |

### Triggers — điều kiện kích hoạt

```yaml
on:
  push:
    branches: [master, main]          # push lên branch
    paths: ['src/**', 'Dockerfile*']  # chỉ khi file trong paths thay đổi

  pull_request:
    branches: [master]                # khi mở PR vào master

  schedule:
    - cron: '0 2 * * *'               # cron: 2AM UTC mỗi ngày
    - cron: '0 0 * * 0'               # 12AM UTC mỗi Chủ nhật

  workflow_dispatch:                  # trigger thủ công từ GitHub UI
    inputs:
      environment:
        description: 'Deploy to'
        required: true
        default: 'staging'
        type: choice
        options: [staging, production]
```

### Workflow SSH deploy — dự án này

```yaml
name: Deploy to Oracle Cloud

on:
  push:
    branches: [master]

jobs:
  deploy:
    name: Deploy via SSH
    runs-on: ubuntu-latest
    steps:
      - name: Deploy via SSH
        uses: appleboy/ssh-action@v1.0.3
        with:
          host: ${{ secrets.OCI_HOST }}
          username: ubuntu
          key: ${{ secrets.OCI_SSH_KEY }}
          timeout: 300s
          script: |
            set -e
            cd /home/ubuntu/football
            git pull origin master
            docker compose -f docker-compose.prod.yml up -d --build
            docker image prune -f
```

### Workflow với Test trước khi Deploy

```yaml
name: Test and Deploy

on:
  push:
    branches: [master]

jobs:
  test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4

      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '8.0.x'

      - name: Cache NuGet
        uses: actions/cache@v4
        with:
          path: ~/.nuget/packages
          key: ${{ runner.os }}-nuget-${{ hashFiles('**/packages.lock.json') }}
          restore-keys: ${{ runner.os }}-nuget-

      - name: Restore
        run: dotnet restore

      - name: Build
        run: dotnet build --no-restore

      - name: Test
        run: dotnet test --no-build --verbosity normal

  deploy:
    needs: test                       # chỉ deploy khi test pass
    runs-on: ubuntu-latest
    steps:
      - name: Deploy via SSH
        uses: appleboy/ssh-action@v1.0.3
        with:
          host: ${{ secrets.OCI_HOST }}
          username: ubuntu
          key: ${{ secrets.OCI_SSH_KEY }}
          timeout: 300s
          script: |
            set -e
            cd /home/ubuntu/football
            git pull origin master
            docker compose -f docker-compose.prod.yml up -d --build
            docker image prune -f
```

### Secrets

**Repository secrets** (Settings → Secrets → Actions):
- Dùng được trong tất cả workflow
- `${{ secrets.SECRET_NAME }}`

**Environment secrets** (Settings → Environments → chọn env):
- Chỉ dùng được khi job có `environment: production`
- Có thể yêu cầu approval trước khi deploy

```yaml
jobs:
  deploy:
    environment: production           # reference environment
    steps:
      - run: echo ${{ secrets.PROD_KEY }}  # chỉ available ở env này
```

### GitHub Secrets cần thiết — dự án này

Vào repo → **Settings → Secrets and variables → Actions → New repository secret**:
- `OCI_HOST` = IP public Oracle VM
- `OCI_SSH_KEY` = toàn bộ nội dung file `.pem` (kể cả `-----BEGIN RSA PRIVATE KEY-----`)

---

## 6. Reverse Proxy — Caddy

### Caddy vs Nginx

| | Caddy | Nginx |
|--|-------|-------|
| **HTTPS** | Tự động Let's Encrypt | Cần cài thêm Certbot |
| **Config** | Đơn giản, khai báo | Phức tạp hơn, chi tiết hơn |
| **Reload** | Tự reload khi thay config | `nginx -s reload` |
| **Cert renewal** | Tự động | Cần cron job |
| **Performance** | Tốt | Tốt hơn ở scale lớn |
| **Phổ biến** | Đang tăng nhanh | Vẫn phổ biến nhất (cần biết) |
| **Học** | Dễ bắt đầu | Quan trọng hơn cho job |

### Caddyfile đầy đủ — dự án này

```
# Thay yourdomain.com bằng domain thật
# Caddy tự cấp HTTPS Let's Encrypt

api.yourdomain.com {
    reverse_proxy localhost:8080

    # WebSocket support (SignalR)
    @websockets {
        header Connection *Upgrade*
        header Upgrade websocket
    }
    reverse_proxy @websockets localhost:8080
}

yourdomain.com {
    reverse_proxy localhost:8081
}

status.yourdomain.com {
    reverse_proxy localhost:3001      # Uptime Kuma
}
```

### Cài Caddy trên Ubuntu

```bash
sudo apt install -y debian-keyring debian-archive-keyring apt-transport-https curl
curl -1sLf 'https://dl.cloudsmith.io/public/caddy/stable/gpg.key' \
  | sudo gpg --dearmor -o /usr/share/keyrings/caddy-stable-archive-keyring.gpg
curl -1sLf 'https://dl.cloudsmith.io/public/caddy/stable/debian.deb.txt' \
  | sudo tee /etc/apt/sources.list.d/caddy-stable.list
sudo apt update && sudo apt install caddy

# Copy config
sudo cp /home/ubuntu/football/Caddyfile /etc/caddy/Caddyfile
sudo nano /etc/caddy/Caddyfile        # sửa yourdomain.com

# Validate trước khi reload
sudo caddy validate --config /etc/caddy/Caddyfile
sudo systemctl reload caddy

# Xem log
sudo journalctl -u caddy -f
```

### Nginx cơ bản (cần biết)

```nginx
# /etc/nginx/sites-available/footballblog
server {
    listen 80;
    server_name yourdomain.com;
    return 301 https://$host$request_uri;  # redirect HTTP → HTTPS
}

server {
    listen 443 ssl;
    server_name yourdomain.com;

    ssl_certificate /etc/letsencrypt/live/yourdomain.com/fullchain.pem;
    ssl_certificate_key /etc/letsencrypt/live/yourdomain.com/privkey.pem;

    location / {
        proxy_pass http://localhost:8081;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;

        # WebSocket support
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection "upgrade";
    }
}
```

```bash
sudo nginx -t                         # validate config
sudo nginx -s reload                  # reload
sudo certbot --nginx -d yourdomain.com  # cấp cert Let's Encrypt
```

---

## 7. Cloud Platforms

> Xem so sánh chi tiết trong `TODO_DEPLOY.md`.

| Platform | Free | Specs | Phù hợp |
|----------|------|-------|---------|
| **Oracle Cloud A1** | Mãi mãi (nâng PAYG) | 4 OCPU ARM64 + 24GB RAM | Production miễn phí |
| **Hetzner CPX21** | Không | 3 vCPU + 4GB RAM, SG | Paid budget tốt nhất (~€9/tháng) |
| **AWS EC2** | $200 credit / 6 tháng | t3.small: 2 vCPU + 2GB | Học cloud, job market |
| **Azure** | $100/năm (sinh viên) | B1s: 1 vCPU + 1GB | Microsoft stack |
| **Fly.io** | Không | 1 vCPU + 1GB | ~$14/tháng, config sẵn |

### Oracle Cloud A1 — lưu ý quan trọng

1. **Upgrade PAYG ngay** sau khi tạo account — PAYG không bị reclaim VM idle
2. VM reclaim khi đồng thời 7 ngày: CPU p95 < 20% + RAM < 10% + network < 10%
3. Region Singapore (`ap-singapore-1`) ít "Out of Capacity" hơn US
4. Firewall Oracle có **3 lớp**: VCN Security List + OS iptables + UFW (chọn 1 trong 2 cái sau)

---

## 8. IaC — Infrastructure as Code

### Terraform là gì?

Thay vì click trên cloud console để tạo VM → viết code `.tf` → `terraform apply` → cloud tạo tự động.

**Lợi ích:**
- Reproducible: chạy lại cho ra y hệt
- Version control: infra changes đi cùng code PR
- Rollback: `terraform destroy` xóa tất cả resource

### Cài đặt

```bash
wget -O- https://apt.releases.hashicorp.com/gpg | sudo gpg --dearmor -o /usr/share/keyrings/hashicorp-archive-keyring.gpg
echo "deb [signed-by=/usr/share/keyrings/hashicorp-archive-keyring.gpg] https://apt.releases.hashicorp.com $(lsb_release -cs) main" | sudo tee /etc/apt/sources.list.d/hashicorp.list
sudo apt update && sudo apt install terraform
terraform --version
```

### Workflow cơ bản

```bash
terraform init          # download providers, setup backend
terraform plan          # preview: sẽ tạo/sửa/xóa gì
terraform apply         # thực hiện (hỏi confirm)
terraform apply -auto-approve  # không hỏi confirm
terraform destroy       # xóa tất cả resource
terraform state list    # xem resource đang manage
```

### HCL syntax cơ bản

```hcl
# provider.tf — khai báo cloud provider
terraform {
  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = "~> 5.0"
    }
  }
}

provider "aws" {
  region = "ap-southeast-1"  # Singapore
}

# variables.tf — input variables
variable "instance_type" {
  type        = string
  default     = "t3.small"
  description = "EC2 instance type"
}

# main.tf — resources
resource "aws_instance" "app_server" {
  ami           = "ami-0df7a207adb9748c7"  # Ubuntu 22.04 Singapore
  instance_type = var.instance_type

  key_name = "my-key"

  tags = {
    Name = "footballblog-server"
  }
}

# outputs.tf — output sau khi apply
output "public_ip" {
  value = aws_instance.app_server.public_ip
}
```

### State file

- `terraform.tfstate` — map config ↔ resource thật trên cloud
- **Không commit** vào git (chứa sensitive data)
- Team nhiều người: lưu remote state (S3, Terraform Cloud)

```gitignore
# thêm vào .gitignore
*.tfstate
*.tfstate.backup
.terraform/
```

---

## 9. Security — Bảo mật VPS

### SSH Hardening

```bash
# Tạo SSH key pair Ed25519 (trên máy LOCAL)
ssh-keygen -t ed25519 -C "youremail@gmail.com"
# → ~/.ssh/id_ed25519 (private — KHÔNG share)
# → ~/.ssh/id_ed25519.pub (public — ok để share)

# Copy public key lên server
ssh-copy-id -i ~/.ssh/id_ed25519.pub ubuntu@SERVER_IP
# Hoặc thủ công:
cat ~/.ssh/id_ed25519.pub | ssh ubuntu@SERVER_IP "mkdir -p ~/.ssh && cat >> ~/.ssh/authorized_keys"

# Tắt password login (sau khi đã test SSH key hoạt động)
sudo nano /etc/ssh/sshd_config
# Sửa các dòng:
#   PasswordAuthentication no
#   PermitRootLogin no
#   PubkeyAuthentication yes
sudo systemctl restart sshd

# Kiểm tra SSH permissions
chmod 700 ~/.ssh
chmod 600 ~/.ssh/authorized_keys
chmod 400 ~/key.pem                   # Oracle private key
```

### UFW Firewall

```bash
sudo ufw default deny incoming        # chặn tất cả inbound
sudo ufw default allow outgoing       # cho phép tất cả outbound
sudo ufw allow 22/tcp                 # SSH
sudo ufw allow 80/tcp                 # HTTP (Caddy ACME challenge)
sudo ufw allow 443/tcp                # HTTPS

sudo ufw enable
sudo ufw status numbered              # xem rules đang có
sudo ufw delete 3                     # xóa rule số 3
sudo ufw disable                      # tắt UFW tạm (KHÔNG nên trên prod)
```

**⚠️ Docker bypass UFW**: Docker thêm iptables rules trực tiếp, bỏ qua UFW.
Fix duy nhất: bind port container vào `127.0.0.1`:
```yaml
ports:
  - "127.0.0.1:8080:8080"   # ✅ only localhost reaches this
  # KHÔNG dùng:
  - "8080:8080"              # ❌ 0.0.0.0:8080 — Docker bypass UFW
```

### Fail2ban — chặn brute force

```bash
sudo apt install fail2ban
sudo systemctl enable --now fail2ban

# Config mặc định: SSH — 5 fail trong 10 phút → ban 10 phút
# Xem bị ban ai:
sudo fail2ban-client status sshd
sudo fail2ban-client status           # tất cả jails

# Unban IP:
sudo fail2ban-client set sshd unbanip 1.2.3.4

# Config custom: /etc/fail2ban/jail.local
[sshd]
enabled = true
maxretry = 3
findtime = 600
bantime = 3600                        # ban 1 giờ
```

### Secrets management

```bash
# .env.prod trên server — KHÔNG commit vào git
chmod 600 /home/ubuntu/football/.env.prod  # chỉ owner đọc được

# Kiểm tra .gitignore có .env.prod không:
grep ".env.prod" .gitignore

# Xem secret đang có trong git history (nguy hiểm):
git log --all --full-history -- .env

# Nếu lỡ commit secret → xóa khỏi history:
git filter-branch --force --index-filter \
  'git rm --cached --ignore-unmatch .env.prod' --prune-empty --tag-name-filter cat -- --all
```

---

## 10. Monitoring

### Uptime Kuma — đơn giản, free

```bash
docker run -d \
  --restart unless-stopped \
  -p 127.0.0.1:3001:3001 \
  -v uptime-kuma:/app/data \
  --name uptime-kuma \
  louislam/uptime-kuma:1
```

Thêm vào Caddyfile:
```
status.yourdomain.com {
    reverse_proxy localhost:3001
}
```

Tạo monitors:
- `https://yourdomain.com` — HTTP(s)
- `https://api.yourdomain.com/health` — HTTP(s)
- `localhost:5432` — TCP (PostgreSQL nếu local)

### Prometheus + Grafana — nâng cao

```yaml
# docker-compose.monitoring.yml
services:
  prometheus:
    image: prom/prometheus:latest
    ports: ["127.0.0.1:9090:9090"]
    volumes:
      - ./prometheus.yml:/etc/prometheus/prometheus.yml:ro
      - prometheus_data:/prometheus
    restart: unless-stopped

  grafana:
    image: grafana/grafana:latest
    ports: ["127.0.0.1:3000:3000"]
    environment:
      - GF_SECURITY_ADMIN_PASSWORD=changeme
    volumes:
      - grafana_data:/var/lib/grafana
    restart: unless-stopped

volumes:
  prometheus_data:
  grafana_data:
```

```yaml
# prometheus.yml
global:
  scrape_interval: 15s

scrape_configs:
  - job_name: 'uptime-kuma'
    static_configs:
      - targets: ['localhost:3001']
    metrics_path: /metrics
```

### Cần monitor gì?

| Metric | Ngưỡng alert | Tool |
|--------|-------------|------|
| Uptime | < 99.9% | Uptime Kuma |
| CPU | > 80% liên tục 5 phút | htop / Grafana |
| RAM | > 85% | docker stats |
| Disk | > 80% | `df -h` / Grafana |
| SSL cert expiry | < 14 ngày | Uptime Kuma |
| Response time | > 2s | Uptime Kuma |
| Container restart count | > 3 lần/giờ | Docker logs |

### Lệnh debug server nhanh

```bash
# Tổng quan nhanh
df -h && free -h && docker ps

# Log gần nhất của app
docker compose -f docker-compose.prod.yml logs --tail=50 api

# Process nào ăn CPU/RAM nhiều nhất
htop   # sort bằng F6

# Disk ai chiếm nhiều nhất
du -sh /var/lib/docker/     # Docker images/containers/volumes
du -sh /home/ubuntu/football/logs/

# Dọn dẹp Docker
docker system df            # xem Docker dùng bao nhiêu disk
docker system prune -f      # xóa stopped containers + dangling images
docker volume prune -f      # xóa volumes không dùng
```

---

## 11. Vấn đề hay gặp (Common Issues)

### Docker

| Vấn đề | Nguyên nhân | Fix |
|--------|-------------|-----|
| Container exit sau khi start | Thiếu env var, crash app | `docker logs <name>` xem lỗi cụ thể |
| Port already in use | Port bị process khác chiếm | `ss -tuln \| grep 8080` → `kill PID` |
| OOMKilled (exit code 137) | Hết RAM | `docker stats` → tối ưu hoặc thêm RAM |
| "exec format error" | Image x86 chạy trên ARM | Build native trên server ARM64 |
| Image cũ không update | Build cache | `docker compose up --build --force-recreate` |
| `permission denied` khi chạy docker | User chưa trong docker group | `sudo usermod -aG docker ubuntu` → logout/login |
| Container restart liên tục | App crash, thiếu config | `docker logs --tail 50` → tìm exception |

### CI/CD GitHub Actions

| Vấn đề | Fix |
|--------|-----|
| SSH timeout | Kiểm tra firewall Oracle port 22 + IP đúng chưa |
| `git pull` conflict | Trên server: `git reset --hard HEAD && git pull` |
| Permission denied (publickey) | Secret phải có cả header `-----BEGIN RSA PRIVATE KEY-----` |
| Deploy xong nhưng app không update | Thêm `--force-recreate` vào compose command |
| `set -e` làm job fail | Đọc log SSH action dòng nào fail |
| Workflow không trigger | Kiểm tra branch name trong `on.push.branches` khớp không |

### Caddy / HTTPS

| Vấn đề | Fix |
|--------|-----|
| Cert không được cấp | DNS chưa trỏ đúng → `dig yourdomain.com +short` kiểm tra |
| Too many redirects | Cloudflare + Caddy đều redirect HTTPS → Cloudflare set "Full (strict)" |
| 502 Bad Gateway | Container chưa chạy hoặc port sai → `docker ps` |
| Caddy không nhận config mới | `sudo caddy validate --config /etc/caddy/Caddyfile` rồi `sudo systemctl reload caddy` |
| Rate limit Let's Encrypt | Test với staging: thêm `acme_ca https://acme-staging-v02.api.letsencrypt.org/directory` vào Caddyfile |

### Oracle Cloud

| Vấn đề | Fix |
|--------|-----|
| Out of Capacity | Script retry `github.com/hitrov/oci-arm-host-capacity` hoặc region Japan/UAE |
| App chạy nhưng không access từ ngoài | Kiểm tra 3 lớp: VCN Security List + `iptables -L INPUT` + `ufw status` |
| VM suspend sau vài ngày | Upgrade PAYG + Hangfire polling giữ CPU active |
| SSH không vào được sau reboot | Mở console OCI → restart instance từ console |

### .NET trên Linux

| Vấn đề | Fix |
|--------|-----|
| SignalR ngắt kết nối | Timeout Caddy: thêm `transport http { read_timeout 0 }` trong reverse_proxy block |
| Hangfire jobs không fire | Xem `/hangfire` dashboard → check job queue + server list |
| Neon connection lỗi SSL | Connection string cần `?sslmode=require` — code normalize `postgresql://` URI |
| App trả 502 | Container chưa healthy → `docker compose ps` + `docker logs api` |
| Timezone sai trong Hangfire | Set timezone trong code: `TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time")` |

---

## 12. Checklist Deploy Production

```
□ PROVISION
  □ Oracle VM tạo xong: Ubuntu 22.04, Ampere A1, 4 OCPU 24GB
  □ Upgrade lên PAYG ngay
  □ SSH key download (.pem)
  □ SSH vào được: ssh -i key.pem ubuntu@IP

□ FIREWALL
  □ Oracle VCN Security List: mở inbound 22, 80, 443
  □ OS firewall:
      sudo iptables -I INPUT -p tcp --dport 80 -j ACCEPT
      sudo iptables -I INPUT -p tcp --dport 443 -j ACCEPT
      sudo netfilter-persistent save
  □ UFW bật (nếu dùng thay iptables): ufw allow 22,80,443/tcp && ufw enable

□ SERVER SETUP
  □ apt update && apt upgrade
  □ Docker cài: curl -fsSL https://get.docker.com | sh
  □ User trong docker group: sudo usermod -aG docker ubuntu
  □ fail2ban cài và chạy
  □ Repo clone: git clone <url> /home/ubuntu/football

□ CONFIG
  □ .env.prod tạo từ .env.prod-example, điền đầy đủ secrets
  □ .env.prod permissions: chmod 600 .env.prod
  □ Caddyfile: thay yourdomain.com bằng domain thật
  □ DNS A record trỏ đúng IP server
  □ Kiểm tra DNS propagate: dig yourdomain.com +short

□ DEPLOY
  □ docker compose -f docker-compose.prod.yml up -d --build
  □ docker compose ps → tất cả running + healthy
  □ sudo cp Caddyfile /etc/caddy/Caddyfile && sudo systemctl reload caddy
  □ HTTPS hoạt động: https://yourdomain.com
  □ API health: https://api.yourdomain.com/health → 200
  □ Hangfire Dashboard: https://api.yourdomain.com/hangfire

□ CI/CD
  □ GitHub Secrets thêm: OCI_HOST, OCI_SSH_KEY
  □ Push test commit → Actions tab → workflow chạy thành công

□ MONITORING
  □ Uptime Kuma chạy: docker run ... louislam/uptime-kuma
  □ Monitor 2 endpoints: yourdomain.com + api.yourdomain.com/health
  □ Notification setup (email / Telegram)
```

---

## Nguồn học thêm

| Resource | Mô tả |
|----------|-------|
| [roadmap.sh/devops](https://roadmap.sh/devops) | Roadmap visual đầy đủ — biết cần học gì tiếp |
| [roadmap.sh/docker](https://roadmap.sh/docker) | Docker deep dive |
| [docs.docker.com](https://docs.docker.com) | Docker official docs |
| [caddyserver.com/docs](https://caddyserver.com/docs) | Caddy documentation |
| [docs.github.com/actions](https://docs.github.com/en/actions) | GitHub Actions reference |
| [developer.hashicorp.com/terraform](https://developer.hashicorp.com/terraform) | Terraform official docs |
| [digitalocean.com/community](https://www.digitalocean.com/community/tutorials) | Tutorials Linux/VPS thực tế — rất tốt cho beginner |
| [learn.microsoft.com — Host .NET on Linux](https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/linux-nginx) | .NET trên Linux chính thức |
| [github.com/louislam/uptime-kuma](https://github.com/louislam/uptime-kuma) | Uptime Kuma |
| [github.com/hitrov/oci-arm-host-capacity](https://github.com/hitrov/oci-arm-host-capacity) | Script retry Oracle Out of Capacity |
