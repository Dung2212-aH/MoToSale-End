# Showroom Frontend Coding Plan Contract

## Mission

This file is the workflow contract for all agent-assisted coding inside the showroom frontend.

The frontend is a React + Vite application. Current project areas include:

- `src/pages`
- `src/components`
- `src/components/product`
- `src/components/store`
- `src/contexts`
- `src/hooks`
- `src/services`
- `src/utils`
- `src/assets`
- `src/index.css`

Agents must protect the existing user experience first:

- Do not break existing behavior unless the user explicitly asks for that change.
- Do not refactor outside the requested task.
- Do not revert, delete, or overwrite changes made by the user or another agent.
- Prefer the current component, styling, routing, API, and state patterns over new abstractions.
- Keep changes scoped to the page, component, hook, context, service, utility, route, or stylesheet required by the task.
- Do not change route structure, API contracts, local storage keys, environment variables, package dependencies, or global styling contracts unless the task requires it.
- When a change touches data from the backend, inspect both the UI caller and `src/services` or mapping utilities that shape the data.
- When a change affects shared layout, cart, auth, order status, notifications, or product rendering, inspect all affected pages and components before editing.

## Required Workflow

Every coding task must follow this sequence.

### 1. Discovery

Before editing files, the agent must:

- Check the current git state with `git status --short`.
- Read the files directly related to the requested change.
- Identify the current route, component tree, state source, API call, mapper, utility, and style rules involved.
- Look for existing tests, scripts, screenshots, or manual verification notes before inventing a new testing approach.
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
- The pages, components, hooks, contexts, services, utilities, routes, or styles expected to change.
- Any API, routing, environment, package, storage, or visual impact.
- Risks and how the implementation will avoid regressions.
- The test, build, smoke, visual, and retest steps that will be run.

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
- The user gives a direct follow-up such as "apply this plan", "please implement this plan", "lam luon", "sua luon", or "code luon".
- The task is documentation-only or instruction-only and the user already provided a complete implementation plan.
- The task is small, low-risk, and clearly bounded.

When an exception applies, the agent may implement but must still preserve the discovery, safety, and verification requirements.

### 6. Implementation

During implementation, the agent must:

- Keep the diff as small as practical.
- Follow existing naming, folder, component, hook, context, service, utility, and CSS patterns.
- Avoid unrelated formatting or whitespace churn.
- Avoid broad rewrites unless the accepted plan requires them.
- Add comments only where they clarify non-obvious behavior.
- Leave unrelated files unchanged.
- Work with existing dirty worktree changes instead of reverting them.

Frontend-specific rules:

- Reuse existing components before adding new ones.
- Keep shared UI changes backward compatible with every page that imports the shared component.
- Keep data formatting in existing formatter or mapper utilities when such utilities already exist.
- Keep API calls in `src/services` unless the surrounding code already uses a different pattern.
- Keep auth, cart, favorite, notification, and storage behavior compatible with the existing contexts and utilities.
- Avoid changing `package.json`, `package-lock.json`, `.env.example`, `vite.config.js`, or global CSS unless required by the task.
- Do not introduce a new dependency for behavior that can be implemented cleanly with the existing stack.

### 7. UI Quality Checklist

For any user-facing UI change, the agent must check:

- Loading, empty, success, and error states still make sense.
- Text fits in buttons, cards, tables, forms, and mobile layouts.
- Layout does not overlap at common desktop and mobile widths.
- Controls remain keyboard and pointer usable.
- Links, forms, and buttons still trigger the intended action.
- Currency, dates, order statuses, product names, quantities, and totals are formatted consistently.
- Images keep stable dimensions and do not cause layout shift.
- Shared header, footer, cart, auth, and navigation behavior are not accidentally changed.

### 8. Verification And Retest

After implementation, the agent must verify the change.

For frontend code changes, run:

```powershell
npm run build
```

If a test runner is added later, also run the relevant test command and update this file with the exact command.

For browser verification, use the smallest sufficient retest set:

- Start the app with `npm run dev` when interactive browser verification is needed.
- Open the affected route or workflow.
- Retest the exact behavior changed.
- Retest one adjacent regression path that uses the same shared component, context, service, or utility.
- Check the browser console for runtime errors when a browser is used.
- For responsive UI changes, retest at one desktop width and one mobile width.

Recommended smoke paths by feature:

- Auth: login page, register page, protected route redirect, account state after refresh.
- Product listing: filter/search/sort path, product card rendering, product detail navigation.
- Product detail: image gallery, variant/quantity selection, add to cart, related products.
- Cart: quantity update, remove item, totals, checkout navigation.
- Checkout: address/contact validation, payment method, order submission, success page.
- Orders: order list, status label, order detail, empty/error states.
- Stores: filters, store list, map/display area, empty state.
- Shared API/service/mapping change: retest every page using the changed service or mapper.

For documentation-only or instruction-only changes, at minimum:

- Read back the changed files.
- Run `git status --short`.

If verification cannot be run, the agent must explain why and list the residual risk.

### 9. Completion Reply

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

Expected frontend modules:
- ...

Impacts:
- ...

Risks:
- ...

## Task Plan

- [ ] ...
- [ ] ...

## Test And Retest Plan

- Build: `npm run build`
- Direct retest:
- Regression retest:
- Responsive/browser check:
```

## Conflict Handling

If this contract conflicts with a direct user instruction:

- Follow the latest explicit user instruction when it is clear and safe.
- Ask for clarification when the conflict could cause destructive changes or broad scope expansion.
- Never use this contract as permission to modify unrelated code.
