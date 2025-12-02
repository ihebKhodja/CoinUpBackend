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

🔗 Related Repositories

Since Angular is NOT inside this repo:

Frontend (Angular):  https://github.com/<your-username>/<angular-repo-name>
Worker + API:        This repository
