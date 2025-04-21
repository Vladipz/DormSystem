# Prompt for Cursor AI: Mini‑OLX Reverse Marketplace Microservice

## 1. Role
You are a **senior back‑end engineer** specializing in **.NET 8**, and **Vertical Slice Architecture**.

## 2. Goal
Generate a **stand‑alone microservice** named **StoreService** that implements a *reverse marketplace* for dormitory students:
- **Покупці** (buyers) create **Ticket** requests describing an item they want (e.g., “холодильник”, desk lamp) with price range, condition, dorm block, and contact info.
- **Продавці** (sellers) post **Offer** listings.
- Whenever a new Offer matches one or more active Tickets, *all relevant buyers* receive real‑time notifications.

## 3. Functional Requirements
1. **CRUD APIs** for Tickets and Offers.
2. **Automatic matching engine** (background worker or domain event) that checks:
   - Same category OR fuzzy item‑name match.
   - Buyer’s max‑price ≥ Offer price.
4. **Authentication stub**
   - JWT Bearer (OIDC compatible), 
5. Buyers can *mark Ticket as fulfilled*; sellers can *mark Offer as sold*.

## 4. Non‑Functional & Architecture
- Language/Runtime: **.NET 8, C# 13**.
- **Vertical Slice Architecture**
  - Features live under `/Features/{Area}/{Action}` containing `Endpoint`, `Request/Response DTO`, `Validator`, `Handler`.
- **Persistence**: **SQLite** via **EF Core 8** (code‑first). Provide initial migration & seed demo data.
- **OpenAPI**: Swashbuckle (v3) with XML comments.
- **CI‑ready** Dockerfile (Alpine image) and optional docker‑compose with a volume‑mounted SQLite db.

## 5. Deliverables
1. Solution & project: `StoreService.sln`, `StoreService.csproj`.
2. Folder structure:
   ```
Domain/
Infrastructure/
Features/
  Tickets/
    Create/
    Update/
    Delete/
    List/
    MarkFulfilled/
  Offers/
    Create/
    Update/
    Delete/
    List/
    MarkSold/
Shared/
   ```