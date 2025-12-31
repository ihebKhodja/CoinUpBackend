# Tests (CoinUpAPI)

Technique de tests couverte:
- Tests unitaires + tests d’intégration sur les fonctionnalités critiques (**collecte**, **stockage**, **visualisation**)
- Usage de mocks pour simuler les dépendances externes (SMTP / Auth)
- Couverture + intégration SonarQube (commandes)
- Tests de performance (k6)
- Tests de sécurité (OWASP ZAP / Snyk)

## 1) Unit + Integration (xUnit)

Projet: `CoinUpAPI/CoinUpAPI.Tests/CoinUpAPI.Tests.csproj`

- Unit (stockage/alertes): `AlertsServiceTests`, `AlertEvaluationHostedServiceTests`
- Unit (visualisation): `CoinsServiceTests` (MarketChartDetails)
- Integration (HTTP): `CoinsControllerIntegrationTests` via `WebApplicationFactory`

Run:
- `dotnet test CoinUpAPI/CoinUpAPI.Tests/CoinUpAPI.Tests.csproj`

## 2) Mocks (dépendances externes)

- SMTP: mock `IEmailSender` avec `FakeEmailSender`
- Auth: handler de test `TestAuthHandler` (pas besoin de JWT réel)

## 3) Couverture + SonarQube (exemples)

Couverture (Coverlet collector):
- `dotnet test CoinUpAPI/CoinUpAPI.Tests/CoinUpAPI.Tests.csproj --collect:"XPlat Code Coverage"`

SonarQube (exemple, à adapter):
- `dotnet tool install --global dotnet-sonarscanner`
- `dotnet-sonarscanner begin /k:"CoinUp" /d:sonar.host.url="http://localhost:9000" /d:sonar.token="<TOKEN>"`
- `dotnet test CoinUpAPI/CoinUpAPI.Tests/CoinUpAPI.Tests.csproj --collect:"XPlat Code Coverage"`
- `dotnet-sonarscanner end /d:sonar.token="<TOKEN>"`

## 4) Performance (k6)

Script: `CoinUpAPI/tests/performance/k6-smoke.js`

Exemple:
- `k6 run -e BASE_URL=http://localhost:5000 -e TOKEN="Bearer <jwt>" CoinUpAPI/tests/performance/k6-smoke.js`

## 5) Sécurité (ZAP / Snyk)

- OWASP ZAP baseline:
  - `docker run --rm -t owasp/zap2docker-stable zap-baseline.py -t http://host.docker.internal:5000 -r zap-report.html`
- Snyk (exemple):
  - `snyk test`

(À brancher dans la CI selon votre environnement.)
