# Showroom Backend Agent Instructions

Before making any change anywhere inside `Backend`, read and follow `Plan.md`.

`Plan.md` is the source of truth for the local coding workflow. It defines the required discovery, requirements, design, task breakdown, implementation, verification, and final reporting rules for the showroom backend.

The backend includes `ApiGateway`, `AuthService`, `CatalogService`, `OrderService`, `PaymentService`, and `database`. Keep work scoped to the service, module, route, or database object required by the user's request.

If a user request conflicts with `Plan.md`, follow the latest explicit user instruction when it is clear and safe. If the conflict could cause destructive changes, broad refactors, public API changes, database changes, configuration changes, package changes, or cross-service behavior changes, ask for clarification before editing.
