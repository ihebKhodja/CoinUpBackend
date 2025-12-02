## Architecture Diagram

flowchart TB

  subgraph Frontend
    ANG[Angular Web App]
  end

  subgraph Backend
    API[.NET 8 Web API]
    WORKER[.NET 8 WorkerService]
  end

  subgraph Messaging
    MQ[(RabbitMQ)]
  end

  subgraph Database
    SQL[(SQL Server)]
  end

  subgraph Monitoring
    PROM[Prometheus]
    GRAF[Grafana]
  end

  ANG -->|REST / WebSocket| API
  API -->|Queries| SQL
  API -->|Consume events| MQ
  WORKER -->|Insert market data| SQL
  WORKER -->|Publish events| MQ

  API --> PROM
  WORKER --> PROM
  PROM --> GRAF
