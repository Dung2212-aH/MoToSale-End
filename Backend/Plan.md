# Showroom Backend Coding Plan Contract

## Mission

This file is the workflow contract for all agent-assisted coding inside the showroom backend.

The backend contains:

- `ApiGateway`
- `AuthService`
- `CatalogService`
- `OrderService`
- `PaymentService`
- `database`

Agents must protect the existing codebase first:

- Do not break existing behavior unless the user explicitly asks for that change.
- Do not refactor outside the requested task.
- Do not revert, delete, or overwrite changes made by the user or another agent.
- Prefer the current project patterns over new abstractions.
- Keep changes scoped to the service, module, route, or database object required by the task.
- Do not change public APIs, database schema, stored procedures, migrations, package references, or configuration contracts unless the task requires it.
- When a change touches a contract between services, inspect both the caller and callee.
- When a change affects routing through the gateway, inspect and update `ApiGateway/ocelot.json` as needed.

## Required Workflow

Every coding task must follow this sequence.

### 1. Discovery

Before editing files, the agent must:

- Check the current git state with `git status --short`.
- Read the files directly related to the requested change.
- Identify the current flow, entry points, data model, and affected contracts.
- Look for existing tests or test projects before inventing a new testing approach.
- Report important existing user changes if they affect the task.

No repo-tracked file should be edited during discovery.

### 2. Requirements

The agent must restate the task as acceptance criteria before implementation.

Include:

- What must work after the change.
- What is explicitly in scope.
- What is explicitly out of scope.
- Any risky ambiguity that needs user confirmation.

If the request is unclear in a way that could cause a wrong implementation, ask before coding.

### 3. Design

Before implementation, the agent must provide a short design plan.

Include:

- The implementation approach.
- The services, files, modules, routes, or database objects expected to change.
- Any API, schema, configuration, gateway, or behavior impact.
- Risks and how the implementation will avoid regressions.
- The tests or verification commands that will be run.

### 4. Task Breakdown

The agent must break the work into small implementation tasks.

Each task must be:

- Specific.
- Checkable.
- Limited to the requested behavior.

### 5. Plan Gate

The agent must not implement code until the plan is accepted by the user.

Exceptions:

- The user explicitly says to implement immediately.
- The user gives a direct follow-up such as "apply this plan", "please implement this plan", "lam luon", or "sua luon".
- The task is documentation-only or instruction-only and the user already provided a complete implementation plan.
- The task is small, low-risk, and clearly bounded.

When an exception applies, the agent may implement but must still preserve the discovery, safety, and verification requirements.

### 6. Implementation

During implementation, the agent must:

- Keep the diff as small as practical.
- Follow existing naming, folder, dependency, DTO, entity, repository, service, and controller patterns.
- Avoid unrelated formatting or whitespace churn.
- Avoid broad rewrites unless the accepted plan requires them.
- Add comments only where they clarify non-obvious behavior.
- Leave unrelated files unchanged.
- Work with existing dirty worktree changes instead of reverting them.

Files such as `Program.cs`, `Data`, `Entities`, controllers, project files, configuration, gateway routes, database scripts, and package references may be changed only when the task requires it. The agent must state the impact before making those changes.

For database changes:

- Do not alter schema, stored procedures, triggers, seed data, or scripts unless required.
- Explain migration and compatibility risk before editing.
- Preserve existing naming and SQL style.

For cross-service changes:

- Inspect all affected request and response DTOs, entities, controllers, services, repositories, and gateway routes.
- Keep wire contracts backward compatible unless the user explicitly asks for a breaking change.
- Build every affected project.

### 7. Verification

After implementation, the agent must verify the change.

For backend code changes, build every affected project:

```powershell
dotnet build .\AuthService\AuthService.csproj --no-restore -p:UseAppHost=false
dotnet build .\CatalogService\CatalogService.csproj --no-restore -p:UseAppHost=false
dotnet build .\OrderService\OrderService.csproj --no-restore -p:UseAppHost=false
dotnet build .\PaymentService\PaymentService.csproj --no-restore -p:UseAppHost=false
dotnet build .\ApiGateway\BaseCore.ApiGateway.csproj --no-restore -p:UseAppHost=false
```

Run only the commands relevant to the services changed. If a change spans services, build all affected services. If a test project exists or is added later, also run the relevant tests.

If build fails because `bin` or `obj` files are locked or access is denied, retry in a way that does not change source behavior or explain the residual risk.

For documentation-only or instruction-only changes, at minimum:

- Read back the changed files.
- Run `git status --short`.

If verification cannot be run, the agent must explain why and list the residual risk.

### 8. Completion Reply

The final response should be concise. It should include only the information needed to understand the result, such as:

- What changed.
- What checks passed or could not be run.
- Any important risk or next step.

The agent must not create a separate report file after finishing unless the user explicitly asks for one.

## Standard Response Template

```markdown
## Requirements

Acceptance criteria:
- [ ] ...

In scope:
- ...

Out of scope:
- ...

## Design

Approach:
- ...

Expected services/modules:
- ...

Impacts:
- ...

Risks:
- ...

## Task Plan

- [ ] ...
- [ ] ...

## Test Plan

- ...
```

## Conflict Handling

If this contract conflicts with a direct user instruction:

- Follow the latest explicit user instruction when it is clear and safe.
- Ask for clarification when the conflict could cause destructive changes or broad scope expansion.
- Never use this contract as permission to modify unrelated code.
