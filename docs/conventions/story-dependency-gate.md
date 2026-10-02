# Story dependency gate

**Status: mandatory. An AI agent must run this check before it writes any code for a story.**

## The rule

A story may only be developed once **every story in its `dependsOn` list has already been developed**.

| Dependency status | Developed? | What the agent does |
|---|---|---|
| `done` | yes | Continue. |
| `review` | yes (code exists, review pending) | Continue, and mention in the report that the dependency is still in review. |
| `ready-for-dev`, `in-progress`, `drafted`, `backlog`, or any other value | **no** | **Stop.** Report and don't develop. |
| Story file not found, or `dependsOn` can't be read | unknown | **Stop.** Report and don't develop. |

## How to check

1. Open the story file under `_bmad-output/implementation-artifacts/stories/epic-NN/`. Read `dependsOn` in its frontmatter (for example `dependsOn: ["1.8", "2.1"]`).
2. For each dependency, open that story's file and read the `status` in its frontmatter. That value is the source of truth.
3. Compare it with the status column in `_bmad-output/implementation-artifacts/stories/README.md`. If the two disagree, stop and report the mismatch. Don't guess which one is right.
4. Apply the table above.

## When the gate blocks

- Don't create, edit or delete any source, test, migration or config file. Don't change the story's status.
- Report to the user in this shape, then stop and wait:

  ```
  ⛔ Story 2.2 is blocked: dependencies not developed yet.

  | Dependency | Title                         | Status        |
  |------------|-------------------------------|---------------|
  | 1.8        | Walking skeleton ...          | ready-for-dev |
  | 2.1        | Staff register                | review   ✓    |

  Develop 1.8 first (suggested order in stories/README.md), or tell me explicitly to override the gate.
  ```

- Only an explicit instruction from the user in the current conversation to override the gate for that story lets the agent continue. Record the override in the story's Change Log.
