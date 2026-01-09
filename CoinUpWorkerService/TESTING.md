# CoinUpWorkerService — Tests & Qualité

Ce document décrit une base **pratique** (unit + intégration + couverture + SonarQube + perf + sécurité) pour le projet Worker.

## 1) Tests unitaires + intégration

Depuis la racine du repo:

- Lancer les tests Worker:

`dotnet test .\CoinUpWorkerService\CoinUpSollution.sln`

Les tests ajoutés couvrent:
- **Collecte** (HTTP CoinGecko): parsing + header d’API key.
- **Stockage**: persistance de `ChartsJson` (SQLite in-memory).
- **Orchestration** (job): logique de remplacement des données et écriture d’une fenêtre `{days}` (EF InMemory pour éviter les contraintes provider).

## 2) Couverture de tests (Coverlet)

OpenCover (utile pour SonarQube):

`dotnet test .\CoinUpWorkerService\CoinUpSollution.sln /p:CollectCoverage=true /p:CoverletOutputFormat=opencover /p:CoverletOutput=TestResults\coverage.opencover.xml`

## 3) SonarQube (analyse qualité)

Pré-requis:
- Un serveur SonarQube (local ou distant)
- Le scanner .NET:

`dotnet tool install --global dotnet-sonarscanner`

Exemple (PowerShell/CMD) — adaptez `sonar.host.url`, `sonar.login`, `sonar.projectKey`:

`dotnet sonarscanner begin /k:"CoinUpWorkerService" /d:sonar.host.url="http://localhost:9000" /d:sonar.login="<TOKEN>" /d:sonar.cs.opencover.reportsPaths="CoinUpWorkerService\\TestResults\\coverage.opencover.xml"`

`dotnet build .\CoinUpWorkerService\CoinUpSollution.sln`

`dotnet test .\CoinUpWorkerService\CoinUpSollution.sln /p:CollectCoverage=true /p:CoverletOutputFormat=opencover /p:CoverletOutput=TestResults\\coverage.opencover.xml`

`dotnet sonarscanner end /d:sonar.login="<TOKEN>"`

## 4) Tests de performance (k6 / Locust)

Le Worker n’expose pas d’API HTTP; la performance “système” se mesure typiquement sur l’API (ex: lecture paginée) et/ou sur la durée d’exécution des jobs.

### Option A — k6 (HTTP API)

Fichier: `CoinUpWorkerService/tests/perf/k6/coins_read.js`

Exemple:

`set BASE_URL=http://localhost:5269`

`set EMAIL=test@example.com`

`set PASSWORD=Passw0rd!`

`k6 run CoinUpWorkerService/tests/perf/k6/coins_read.js`

### Option B — Locust (HTTP API)

Fichier: `CoinUpWorkerService/tests/perf/locust/locustfile.py`

Exemple:

`pip install locust`

`set BASE_URL=http://localhost:5269`

`set EMAIL=test@example.com`

`set PASSWORD=Passw0rd!`

`locust -f CoinUpWorkerService/tests/perf/locust/locustfile.py --host %BASE_URL%`

## 5) Tests de sécurité automatisés (OWASP ZAP / Snyk)

### OWASP ZAP (baseline scan)

Exemple (scan de l’API locale):

`docker run --rm -t owasp/zap2docker-stable zap-baseline.py -t http://host.docker.internal:5269 -r zap_report.html`

### Snyk (dépendances)

Pré-requis: Snyk CLI + authentification

Exemples:
- `snyk test --file=CoinUpWorkerService/CoinUpWorkerService.csproj`
- `snyk test --all-projects`
