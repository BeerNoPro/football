
# DEPLOY / HOSTING — Quyết định server

> Cập nhật 2026-05-11. Đã thêm Azure, cập nhật AWS free tier thay đổi 15/7/2025, bổ sung job market demand.

## Tình trạng Fly.io — KHÔNG DÙNG ĐƯỢC NỮA

- **Fly.io đã bỏ free tier vĩnh viễn từ 2024.** Chỉ còn trial với giới hạn machine hours.
- Trial đã dùng gần hết → sắp bị suspend toàn bộ app.
- Muốn tiếp tục dùng Fly.io **bắt buộc add credit card** → chuyển sang Pay-as-You-Go.
- Config `fly.api.toml` + `fly.web.toml` + CI/CD `.github/workflows/deploy.yml` đã đúng — nếu add card chỉ cần scale xuống là dùng được ngay.

**Các platform bị loại (không phù hợp với stack này):**
- **Render** — spin down sau 15p idle, cold start 50–60s, Hangfire không fire được, Blazor IS ngắt kết nối
- **Railway** — không có free tier thực sự ($1 credit/tháng, hết sau vài giờ)
- **Google Cloud Run** — serverless, CPU throttled khi idle → Hangfire recurring jobs không fire; Blazor InteractiveServer lỗi sticky session với multi-instance

---

## So sánh 3 Platform: Oracle Cloud vs AWS vs Azure

> **Nguồn:** Oracle Docs (always-free), AWS Free Tier 2025 announcement, Azure pricing, Stack Overflow Developer Survey 2025.
> **Đã verify lại 2026-05-14** qua web search — xem ghi chú bên dưới mỗi section.

#### Specs miễn phí

| | Oracle Cloud A1 | AWS EC2 | Azure |
|--|----------------|---------|-------|
| **Free tier type** | **Always Free** (vĩnh viễn) ⚠️ nâng PAYG để tránh idle reclaim | 6 tháng, **$200 credit** (tài khoản mới từ 15/7/2025) | 30 ngày $200 + 12 tháng giới hạn |
| **vCPU** | **4 OCPUs** (ARM64 Ampere) | 1 vCPU (t3.micro) | 1 vCPU (B1s) |
| **RAM** | **24 GB** | 1 GB | 1 GB |
| **Storage** | 200 GB block | — | — |
| **Egress/tháng** | **10 TB** | 100 GB | 15 GB |
| **Credit card** | Bắt buộc (không charge nếu trong quota) | Bắt buộc | Bắt buộc |
| **Docker Compose** | ✅ Excellent (RAM thừa) | ⚠️ Tight (1GB cho 2 containers) | ⚠️ Tight (1GB cho 2 containers) |
| **Hangfire** | ✅ Persistent process | ✅ | ✅ |
| **Blazor IS** | ✅ | ✅ | ✅ |
| **ARM64 .NET 8** | ✅ Official images support | N/A (x86) | N/A (x86) |

#### Chi phí sau free tier

| | Oracle Cloud A1 | AWS EC2 | Azure |
|--|----------------|---------|-------|
| **Sau free** | $0 mãi mãi (trong quota) | t3.small ~$15/tháng + Elastic IP $3.6/tháng | B1s ~$7.6/tháng sau 12 tháng |
| **Nếu muốn 2GB RAM** | Vẫn $0 (dùng 2 OCPU + 2GB trong quota 24GB) | t3.small ~$15/tháng | B2s ~$38/tháng |

#### Job Market Demand (Stack Overflow Developer Survey 2025)

| | Oracle Cloud | AWS | Azure |
|--|-------------|-----|-------|
| **Usage rate** | ~5% | **43%** | ~26% |
| **Cloud market share** | ~2% | ~30% | ~20% |
| **Phổ biến nhất cho** | Cá nhân / startup | Startup + tech company | Enterprise + .NET shop |
| **Giá trị học** | Thấp (ít job) | **Cao nhất** (nhiều job nhất) | Cao (đặc biệt .NET/Microsoft stack) |

**Nhận xét job market:**
- **AWS:** Số lượng job nhiều nhất, giá trị kỹ năng cao nhất ở hầu hết thị trường
- **Azure:** Rất phổ biến ở enterprise dùng Microsoft stack (.NET + Azure DevOps + AAD) — phù hợp nếu muốn làm trong công ty dùng Windows/Office 365
- **Oracle Cloud:** Ít được tuyển dụng, chủ yếu dùng bởi database admin và enterprise Oracle DB

#### Networking complexity

| | Oracle Cloud A1 | AWS EC2 | Azure |
|--|----------------|---------|-------|
| **Setup** | VCN + Security List + iptables (3 lớp firewall) | VPC + Security Group (2 lớp, đơn giản hơn) | VNet + NSG (tương tự AWS) |
| **Độ khó** | Cao nhất | Trung bình | Trung bình |
| **"Out of Capacity"** | ⚠️ Thực tế xảy ra ở region phổ biến | Không có | Không có |

#### CI/CD từ GitHub Actions

Tất cả 3 platform đều support SSH deploy từ GitHub Actions — chỉ cần thay `flyctl deploy` bằng:

```yaml
- name: Deploy via SSH
  uses: appleboy/ssh-action@master
  with:
    host: ${{ secrets.SERVER_HOST }}
    username: ubuntu
    key: ${{ secrets.SSH_KEY }}
    script: |
      cd /home/ubuntu/football
      git pull origin master
      docker compose -f docker-compose.prod.yml up -d --build
```

---

## Option A — Oracle Cloud Always Free ⭐ $0 vĩnh viễn

**Phù hợp khi:** Muốn miễn phí lâu dài, không cần học cloud ecosystem

**Specs miễn phí mãi mãi:**
- **4 OCPUs (ARM64 Ampere A1) + 24 GB RAM** — đủ chạy API + Web + Hangfire + Caddy thoải mái
- 200 GB block storage, 10 TB egress/tháng

**Steps chi tiết:**

1. Đăng ký tại cloud.oracle.com → add credit card (bắt buộc để unlock PAYG, không bị charge nếu trong quota)
2. OCI Console → Networking → VCN → Create VCN (wizard "VCN with Internet Connectivity")
3. Compute → Instances → Create:
   - Image: Ubuntu 22.04
   - Shape: Ampere A1 Flex → **4 OCPU, 24 GB RAM**
   - Region ít phổ biến: `ap-osaka-1` (Japan), `me-dubai-1` (UAE), `sa-santiago-1` (Chile)
   - Generate SSH key → download `.pem`
4. Nếu lỗi **"Out of Capacity"**: dùng script retry tự động
   ```bash
   git clone https://github.com/hitrov/oci-arm-host-capacity
   # Xem README để config + chạy liên tục mỗi 30s
   ```
5. Mở firewall OCI Security List: port 80, 443, 22 inbound
6. Mở firewall OS:
   ```bash
   sudo iptables -I INPUT -p tcp --dport 80 -j ACCEPT
   sudo iptables -I INPUT -p tcp --dport 443 -j ACCEPT
   sudo netfilter-persistent save
   ```
7. Cài Docker: `curl -fsSL https://get.docker.com | sh && sudo usermod -aG docker ubuntu`
8. Tạo `docker-compose.prod.yml` (API port 8080 + Web port 8081, dùng Neon + Upstash external)
9. Cài Caddy, tạo `/etc/caddy/Caddyfile`:
   ```
   api.yourdomain.com { reverse_proxy localhost:8080 }
   yourdomain.com { reverse_proxy localhost:8081 }
   ```
10. Update `.github/workflows/deploy.yml` → SSH deploy (secrets: `OCI_HOST`, `OCI_SSH_KEY`)

**⚠️ Rủi ro:**
| Rủi ro | Xử lý |
|--------|-------|
| "Out of Capacity" khi provision | Script retry hoặc region khác (Japan, UAE) |
| Oracle thu hồi VM nếu idle 7 ngày liên tục (CPU p95 < 20%, RAM < 10%, network < 10%) | **Upgrade lên PAYG ngay sau khi tạo account** — PAYG miễn phí trong quota nhưng không bị reclaim. Hangfire polling cũng giữ CPU active. |
| Build ARM64 image chậm | Build trực tiếp trên server (git pull → docker compose build) — không cần `--platform` flag |

---

## Option B — AWS EC2 ⭐ Học AWS ecosystem, giá trị job cao nhất

**Phù hợp khi:** Muốn học AWS (IAM, S3, CloudWatch, VPC) — kỹ năng có giá trị nhất trên thị trường

> **Thay đổi 15/7/2025:** Account mới **không còn 12-month free EC2**. Chỉ nhận **6 tháng credit-based** với t3.micro/t3.small/t4g instances. Sau đó phải trả tiền.

**Specs:**
- t3.micro: 2 vCPU, **1 GB RAM** — không đủ cho API + Web + Hangfire
- t3.small: 2 vCPU, **2 GB RAM** — đủ tối thiểu (~$15/tháng sau free tier)

**Steps:**
1. AWS Console → EC2 → Launch Instance:
   - AMI: Ubuntu Server 22.04 LTS
   - Instance type: `t3.small` (2GB)
   - Security Group: port 22, 80, 443 inbound
2. Allocate Elastic IP → Associate (IP cố định khi restart)
3. SSH vào, cài Docker + Docker Compose
4. Clone repo, tạo `docker-compose.prod.yml`, cài Caddy (giống Oracle Cloud)
5. Update CI/CD: thay `OCI_HOST` → `ELASTIC_IP` trong GitHub Actions secrets

**Chi phí thực:**
| Tài khoản | 0–6 tháng | Sau đó |
|-----------|-----------|--------|
| Mới (sau 15/7/2025) | Credit-based, t3.small miễn phí | ~$15/tháng + $3.6/tháng Elastic IP |
| Cũ (trước 15/7/2025) | t2.micro free 12 tháng (không đủ RAM) | Nâng t3.small ~$15/tháng |

---

## Option C — Azure ⭐ Phù hợp nếu là sinh viên / dùng Microsoft stack

**Phù hợp khi:** Là sinh viên (Azure for Students $100/năm), hoặc muốn học Azure DevOps + AAD cho công ty .NET

**Free tier thực tế:**
- Standard: $200 credit 30 ngày + 12 tháng giới hạn (B1s = 1 vCPU, 1GB RAM, **không đủ**)
- **Azure for Students:** $100 credit/năm (renewable, không cần credit card)

**Vấn đề:** B1s 1GB RAM không đủ cho API + Web + Hangfire. Cần B2s (2 vCPU, 4GB) = ~$38/tháng sau free tier.

**Steps (Azure VM + Docker):**
1. portal.azure.com → Virtual Machines → Create
   - Image: Ubuntu 22.04
   - Size: B1s (free) hoặc B2s (trả tiền nếu cần đủ RAM)
   - SSH public key
2. Network Security Group: open port 80, 443, 22
3. SSH vào → cài Docker + Caddy (giống Oracle Cloud)
4. GitHub Actions → Azure SSH deploy (secrets: `AZURE_HOST`, `AZURE_SSH_KEY`)

**⚠️ Lưu ý Azure for Students:**
- Cần email trường đại học để đăng ký
- Renew thủ công trước khi hết 12 tháng
- $100 credit không đủ nếu dùng VM lớn (B2s ~$38/tháng = 2.6 tháng/credit)

---

## Khuyến nghị cuối

| Mục tiêu | Lựa chọn | Lý do |
|----------|----------|-------|
| **$0 mãi mãi, deploy nhanh** | Oracle Cloud A1 (PAYG) | 24GB RAM, always-free, Docker Compose thoải mái — nâng PAYG để tránh idle reclaim |
| **Học cloud để xin việc** | AWS | 43% job market demand, ecosystem IAM/S3/CloudWatch giá trị nhất |
| **Sinh viên, học Microsoft stack** | Azure for Students | $100/năm không cần credit card, Azure DevOps + AAD |
| **Có budget nhỏ, gần VN (~€9/tháng)** | Hetzner CPX21 Singapore | 3 vCPU, 4GB RAM, NVMe, không BS, giá rẻ nhất paid option |
| **Có budget nhỏ (~$15/tháng)** | AWS t3.small | Kỹ năng + infrastructure production-grade |

> **Recommendation cho dự án này:** Dùng **Oracle Cloud A1 (PAYG)** để deploy production (free, specs dư thừa) — nhớ upgrade PAYG ngay sau khi tạo account để tránh bị reclaim. Song song **học AWS** qua $200 credit để build kỹ năng job market. Sau khi Oracle provision xong (có thể mất vài ngày nếu bị "Out of Capacity"), toàn bộ flow hiện tại chạy được ngay vì Neon + Upstash đã external.

---

## docker-compose.prod.yml (dùng cho cả 3 platform)

```yaml
services:
  api:
    build:
      context: .
      dockerfile: Dockerfile.api
    ports: ["8080:8080"]
    environment:
      - ConnectionStrings__DefaultConnection=<neon-conn-str>
      - ConnectionStrings__Redis=<upstash-conn-str>
      - Jwt__Key=<jwt-key>
      - WebBaseUrl=https://yourdomain.com
      - ASPNETCORE_ENVIRONMENT=Production
    restart: unless-stopped

  web:
    build:
      context: .
      dockerfile: Dockerfile.web
    ports: ["8081:8080"]
    environment:
      - ApiBaseUrl=https://api.yourdomain.com
      - ASPNETCORE_ENVIRONMENT=Production
    restart: unless-stopped
```

---
