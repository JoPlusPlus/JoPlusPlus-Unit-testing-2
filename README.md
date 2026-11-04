# Simulation Unit Testing Assignment

Mini Checkout Engine starter codebase for practicing unit testing in .NET.

This repository contains a small, production-ready checkout flow. **Your job is to write the tests** — not to build new features.

## Overview

`CheckoutService` processes a customer order by:

1. Validating the order
2. Checking inventory
3. Calculating subtotal, discount, and total
4. Saving the order
5. Sending a confirmation notification
6. Returning a `CheckoutReceipt`

There is no web API, database, authentication, or UI. External dependencies are represented by interfaces so they can be mocked in tests.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- An IDE or editor of your choice (Visual Studio, Rider, or VS Code)

## Solution structure

```text
Simulation.Checkout.slnx
├── src
│   ├── Simulation.Checkout.Domain          # Entities and exceptions
│   ├── Simulation.Checkout.Application     # CheckoutService + interfaces
│   └── Simulation.Checkout.Infrastructure  # In-memory repository
└── tests
    └── Simulation.Checkout.Tests           # Write your tests here
```

### Project responsibilities

| Project | Responsibility |
|---|---|
| **Domain** | Core models (`Order`, `OrderItem`, `CheckoutReceipt`) and checkout exceptions |
| **Application** | Business logic (`CheckoutService`) and abstractions (`IOrderRepository`, `IInventoryService`, `INotificationService`) |
| **Infrastructure** | Technical implementations used outside tests (`InMemoryOrderRepository`) |
| **Tests** | Your unit, snapshot, architecture, and coverage work (starter contains packages only, no test solutions) |

### Dependency direction

```text
Infrastructure ──► Application ──► Domain
      ▲                 ▲
      └────── Tests ────┘
```

- Domain does **not** depend on Application or Infrastructure
- Application does **not** depend on Infrastructure
- Infrastructure may depend on Application and Domain
- Tests may reference all projects

## Build

```bash
dotnet restore
dotnet build
```

The solution must build successfully before you start writing tests.

## Run tests

After you implement tests in `Simulation.Checkout.Tests`:

```bash
dotnet test
```

With detailed output:

```bash
dotnet test --logger "console;verbosity=detailed"
```

## Code coverage

Collect coverage with coverlet (already referenced in the test project):

```bash
dotnet test --collect:"XPlat Code Coverage"
```

Coverage files are written under `tests/Simulation.Checkout.Tests/TestResults/`.

### HTML report with ReportGenerator

Install the tool once:

```bash
dotnet tool install -g dotnet-reportgenerator-globaltool
```

Generate an HTML report (adjust the coverage file path if needed):

```bash
reportgenerator \
  -reports:tests/Simulation.Checkout.Tests/TestResults/**/coverage.cobertura.xml \
  -targetdir:coverage-report \
  -reporttypes:Html
```

Open `coverage-report/index.html` in a browser.

## Assignment details

Read [`ASSIGNMENT.md`](ASSIGNMENT.md) for:

- Business rules
- Required testing categories
- Libraries to practice
- Architecture rules to verify
- Acceptance criteria

## Important

- Do **not** change production business rules unless an instructor asks you to
- Do **not** add a database, API, or authentication layer
- Focus on testing the existing `CheckoutService` behavior and architecture boundaries
