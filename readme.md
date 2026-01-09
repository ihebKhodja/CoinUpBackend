CoinUp — Market Tracking & E-Wallet Platform

CoinUp is a modular system composed of:

CoinUp API (.NET 8 Web API) — exposes endpoints for coins, users, portfolios & market data

CoinUp Worker Service (.NET Worker) — fetches market data from external providers, pushes updates to API via RabbitMQ

RabbitMQ — message broker between Worker and API

(External) Angular Front-End — hosted in a separate repository

📁 Repository Structure (This Repo)
/CoinUpApi
    Controllers/
    Models/
    DTOs/
    Services/
    Infrastructure/
    appsettings.json

/CoinUpWorkerService
    Jobs/
    Services/
    Models/
    consumers/
    producers/

/shared
    DTOs/
    Messages/

/docs
    UML/
    Architecture/
/README.md


Note:
The Angular project is NOT included in this repository and is maintained separately.

🏗️ System Architecture

The system follows a modular, message-driven architecture:

[ Worker Service ] --> (RabbitMQ Queue: market_updates) --> [ API ] --> DB
                                     ^
                                     | REST
                                     |
                               [ Angular UI ] (separate repo)

Components
Component	Description
.NET API	Manages users, portfolios, purchased coins, and exposes coin data.
Worker Service	Periodically fetches market data and publishes messages to RabbitMQ.
RabbitMQ	Decouples real-time updates from the API.
Angular Frontend	Displays charts, coins list, and user portfolio (stored in another repo).
🧩 Key Features
🔐 Authentication & User Management

JWT-based authentication

E-wallet per user

Ability to buy/sell coins

Profit/loss tracking

📈 Market Features

Market data ingestion (Worker → RabbitMQ → API)

Market chart details stored in DB

Ranking, trending, and 24h changes

💸 Portfolio Management

Track purchased coins

Average price per coin

Real-time or historical profit calculation

📦 Technologies
Layer	Technology
Backend	.NET 8, EF Core, REST
Worker	Hosted Service, HttpClient, RabbitMQ producer
Messaging	RabbitMQ
Frontend	Angular (separate repository)
Database	SQL Server / PostgreSQL
📚 Documentation

Inside the /docs folder:

/docs
   /UML
      class-diagram.mmd
      sequence-diagram.mmd
      erd.mmd
   /architecture
      architecture-diagram.png

🚀 How to Run
1️⃣ Start RabbitMQ

Docker example:

docker run -d --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:3-management

2️⃣ Start the API
cd CoinUpApi
dotnet run

3️⃣ Start Worker Service
cd CoinUpWorkerService
dotnet run

4️⃣ Run Angular (in its separate repo)
ng serve

---

## DevOps & CI/CD (local-first, no online deployment required)

This repository includes:

- **CI pipeline (GitHub Actions)**: build + unit/integration tests + Docker build + **Kubernetes deployment smoke test on Kind**.
- **Docker Compose**: SQL Server + API + Worker (and optional frontend).
- **Kubernetes manifests (Minikube/Kind)**: SQL Server + API + Worker + automated DB backups (CronJob).
- **Observability (Prometheus + Grafana + Alertmanager)** for local monitoring.

### 1) CI/CD (GitHub Actions)

Workflow file:

- [.github/workflows/ci.yml](.github/workflows/ci.yml)

What it does:

- `dotnet restore/build/test`
- Builds Docker images for API and Worker
- Creates a **Kind** cluster and applies Kubernetes manifests from [k8s/overlays/ci](k8s/overlays/ci)
- Smoke-tests Swagger JSON (`/swagger/v1/swagger.json`)

> This is a full CI/CD demonstration even if you don’t deploy online.

### 2) Dockerisation complète (local)

Backend (API + Worker + SQL Server):

1) Create a `.env` file at repo root with:

    `MSSQL_SA_PASSWORD=YourStrong!Passw0rd`

2) Start:

    `docker compose up -d --build`

Swagger:

- `http://localhost:5000/swagger`

Frontend (Angular) is not inside this repo.

- Optional compose: [docker-compose.frontend.yml](docker-compose.frontend.yml)
- Example:

  `FRONTEND_CONTEXT=../CoinUpFront docker compose -f docker-compose.yml -f docker-compose.frontend.yml up -d --build`

### 3) Kubernetes (Minikube/Kind)

Manifests:

- Base: [k8s/base](k8s/base)
- CI overlay: [k8s/overlays/ci](k8s/overlays/ci)

Local deploy example (Kind/Minikube):

1) Build images:

    `docker build -f CoinUpAPI/Dockerfile -t coinupapi:local .`
    `docker build -f CoinUpWorkerService/Dockerfile -t coinupworker:local .`

2) Apply:

    `kubectl apply -k k8s/base`

3) Access Swagger:

    `kubectl -n coinup port-forward svc/coinupapi 8080:8080`
    then open `http://localhost:8080/swagger`

> Note: for real usage, update the secret in [k8s/base/sqlserver.yaml](k8s/base/sqlserver.yaml) (password + connection string).

### 4) Monitoring & observabilité (Prometheus + Grafana)

Local stack:

- [docker-compose.observability.yml](docker-compose.observability.yml)

Run:

`docker compose -f docker-compose.yml -f docker-compose.observability.yml up -d`

Endpoints:

- Prometheus: `http://localhost:9090`
- Grafana: `http://localhost:3000` (default `admin/admin` unless overridden)
- cAdvisor: `http://localhost:8081`

Alerting:

- Basic rule in [ops/observability/prometheus/alerts.yml](ops/observability/prometheus/alerts.yml)

### 5) Sauvegardes automatiques (DB)

Docker (local loop backup container):

- [docker-compose.backup.yml](docker-compose.backup.yml)

Run:

`docker compose -f docker-compose.yml -f docker-compose.backup.yml up -d`

Kubernetes (CronJob):

- Included in base deploy: [k8s/base/backup-cronjob.yaml](k8s/base/backup-cronjob.yaml)
- Runs daily at 02:00 and stores `.bak` files on a PVC.

🔗 Related Repositories

Since Angular is NOT inside this repo:

Frontend (Angular):  https://github.com/<your-username>/<angular-repo-name>
Worker + API:        This repository
