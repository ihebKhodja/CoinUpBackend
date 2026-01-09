# CoinUp – User Stories (export Notion)

Ce document est une version lisible (review) du CSV Notion.
Source des comportements: Controllers côté API + WorkerService.

## Epic: AUTH

### CU-US-001 — S'inscrire (register)
En tant que visiteur, je veux créer un compte afin de pouvoir accéder à l'application.

Critères d’acceptation
- POST /api/auth/register avec body valide retourne 200 OK.
- Si erreur métier/validation survient, l’API retourne 400 BadRequest avec { message }.

### CU-US-002 — Se connecter (login)
En tant qu'utilisateur, je veux me connecter afin d'obtenir un jeton et accéder aux endpoints protégés.

Critères d’acceptation
- POST /api/auth/login avec identifiants valides retourne 200 OK.
- Si identifiants invalides / erreur, l’API retourne 400 BadRequest avec { message }.

## Epic: COINS

### CU-US-003 — Lister les coins (paginé + recherche)
En tant qu'utilisateur connecté, je veux voir la liste des coins (avec pagination et recherche) afin de naviguer dans le marché.

Critères d’acceptation
- GET /api/coins retourne 200 OK et un PaginatedCoinsResponse.
- La requête accepte query (optionnel), page (défaut 1), pageSize (défaut 20).
- Sans token, l’API retourne 401 Unauthorized (controller protégé).

### CU-US-004 — Consulter un coin par id
En tant qu'utilisateur connecté, je veux consulter le détail d'un coin afin d'avoir ses informations de marché.

Critères d’acceptation
- GET /api/coins/{id} retourne 200 OK si le coin existe.
- Si le coin n’existe pas, l’API retourne 404 NotFound avec un message.
- Sans token, l’API retourne 401.

### CU-US-005 — Consulter le market chart (90 jours)
En tant qu'utilisateur connecté, je veux voir le market chart d'un coin sur 90 jours afin d'analyser la tendance.

Critères d’acceptation
- GET /api/coins/{id}/market-chart retourne 200 OK et MarketChartDetailsDto si trouvé.
- Si non trouvé, 404 NotFound.
- La fenêtre est 90 jours (valeur forcée côté controller).

## Epic: WALLET

### CU-US-006 — Consulter mon wallet
En tant qu'utilisateur connecté, je veux consulter mon wallet afin de connaître mon solde et mes holdings.

Critères d’acceptation
- GET /api/wallet retourne 200 OK si userId est présent dans le token.
- Si userId est manquant/invalid dans le token, retourner 401 avec message.
- Si wallet introuvable, retourner 404.
- Si erreur inattendue, retourner 500 avec message (et détails).

### CU-US-007 — Acheter un coin
En tant qu'utilisateur connecté, je veux acheter un coin afin d'investir.

Critères d’acceptation
- POST /api/wallet/buy retourne 200 OK.
- Si userId manquant dans token, 401.

### CU-US-008 — Vendre un coin
En tant qu'utilisateur connecté, je veux vendre un coin afin de récupérer des fonds.

Critères d’acceptation
- POST /api/wallet/sell retourne 200 OK.
- Si userId manquant dans token, 401.

### CU-US-009 — Consulter mes transactions
En tant qu'utilisateur connecté, je veux consulter l'historique de mes transactions afin de suivre mon activité.

Critères d’acceptation
- GET /api/wallet/transactions retourne 200 OK.
- Si userId manquant, 401.

### CU-US-010 — Déposer des fonds
En tant qu'utilisateur connecté, je veux déposer des fonds afin d'augmenter mon solde.

Critères d’acceptation
- POST /api/wallet/deposit avec Amount > 0 retourne 200 OK avec message et amount.
- Si Amount <= 0, retourner 400.
- Si userId manquant, retourner 401.
- Si dépôt échoue, retourner 400.
- Si exception serveur, retourner 500.

## Epic: WATCHLIST

### CU-US-011 — Ajouter un coin à ma watchlist
En tant qu'utilisateur connecté, je veux ajouter un coin à ma watchlist afin de suivre mes favoris.

Critères d’acceptation
- POST /api/watchlist/{coinId} retourne 201 Created.
- Si userId manquant, 401.
- Si coin/user introuvable, 404.
- Si erreur inattendue, 500.

### CU-US-012 — Supprimer un coin de ma watchlist
En tant qu'utilisateur connecté, je veux retirer un coin de ma watchlist afin de la maintenir à jour.

Critères d’acceptation
- DELETE /api/watchlist/{coinId} retourne 200 OK avec message.
- Si userId manquant, 401.
- Si introuvable, 404.
- Si erreur inattendue, 500.

### CU-US-013 — Consulter ma watchlist
En tant qu'utilisateur connecté, je veux consulter ma watchlist afin de voir mes coins favoris.

Critères d’acceptation
- GET /api/watchlist retourne 200 OK.
- Si userId manquant, 401.
- Si introuvable, 404.
- Si erreur inattendue, 500.

## Epic: ALERTS

### CU-US-014 — Lister mes alertes
En tant qu'utilisateur connecté, je veux lister mes alertes afin de gérer mes notifications de prix.

Critères d’acceptation
- GET /api/alerts retourne 200 OK.
- Si userId manquant, 401.

### CU-US-015 — Créer une alerte de prix
En tant qu'utilisateur connecté, je veux créer une alerte de prix afin d'être notifié quand un seuil est atteint.

Critères d’acceptation
- POST /api/alerts retourne 200 OK.
- Si validation/erreur métier (ArgumentException), retourner 400.
- Si userId manquant, 401.

### CU-US-016 — Modifier une alerte de prix
En tant qu'utilisateur connecté, je veux modifier une alerte afin d'ajuster mes seuils.

Critères d’acceptation
- PUT /api/alerts/{id} retourne 200 OK.
- Si alerte introuvable, 404.
- Si validation (ArgumentException), 400.
- Si userId manquant, 401.

### CU-US-017 — Supprimer une alerte
En tant qu'utilisateur connecté, je veux supprimer une alerte afin de ne plus recevoir de notifications.

Critères d’acceptation
- DELETE /api/alerts/{id} retourne 200 OK.
- Si introuvable, 404.
- Si userId manquant, 401.

## Epic: ADMIN

### CU-US-018 — Lister les utilisateurs (admin)
En tant qu'admin, je veux lister les utilisateurs afin de gérer la plateforme.

Critères d’acceptation
- GET /api/users retourne 200 OK.
- Si non admin ou non authentifié, accès refusé (401/403 selon config).

### CU-US-019 — Activer/désactiver un utilisateur (admin)
En tant qu'admin, je veux activer/désactiver un utilisateur afin de contrôler l'accès.

Critères d’acceptation
- PUT /api/users/{userId}/is-active retourne 204 NoContent.
- Si user introuvable, 404.
- Si non admin/non authentifié, 401/403.

## Epic: SYSTEM (WorkerService)

### CU-US-020 — Collecter les données du marché (scheduler)
En tant que système, je veux collecter périodiquement les données de marché afin de garder la base à jour.

Critères d’acceptation
- Au démarrage, le Worker lance le job Market une fois.
- Puis il relance à intervalle MarketIntervalMinutes (défaut 1440).
- Si une exécution est déjà en cours, la suivante est ignorée (lock).

### CU-US-021 — Collecter l'historique multi-fenêtres
En tant que système, je veux collecter périodiquement l'historique des charts afin d'afficher des courbes fiables.

Critères d’acceptation
- Au démarrage, le Worker lance le job History une fois.
- Puis il relance à intervalle HistoryIntervalMinutes (défaut 60).
- Le job récupère plusieurs fenêtres (HistoryDaysOptions; défaut [90]).
- En cas d’échec, retry jusqu’au succès (ou limites configurées).

### CU-US-022 — Gérer le rate-limit externe
En tant que système, je veux gérer le rate-limit de l'API externe afin d'éviter des échecs en boucle.

Critères d’acceptation
- Si une requête chart reçoit 429, attendre RateLimitMs puis retry.
- RateLimitMs est configurable.
