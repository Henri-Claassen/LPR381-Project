# LPR381 Project — LP/IP Solver & Sensitivity Analysis

A Windows Forms (.NET Framework 4.7.2) application built for the LPR381 (Linear
Programming) module. It solves Linear Programming and Integer Programming
models from a plain-text input file, displays every tableau iteration, and
performs sensitivity analysis on the optimal solution — all exported to a
text output file.

## Contents

- [Features](#features)
- [Getting started](#getting-started)
- [Project structure](#project-structure)
- [Input file format](#input-file-format)
- [Using the application](#using-the-application)
- [Sensitivity analysis](#sensitivity-analysis)
- [Output file](#output-file)
- [Known limitations](#known-limitations)
- [Contributors](#contributors)

## Features

| Requirement | Status |
|---|---|
| Primal Simplex Algorithm | ✅ Implemented, with full tableau iteration history |
| Revised Primal Simplex Algorithm | ❌ Not implemented (`Solver.SolveRevisedSimplex` is a stub) |
| Branch & Bound Simplex Algorithm | ✅ Implemented (backtracking, sub-problem tree, fathoming, best candidate) |
| Cutting Plane Algorithm | ✅ Implemented |
| Branch & Bound Knapsack Algorithm | ✅ Implemented (greedy fractional relaxation + branching) |
| Dual Simplex (internal) | ✅ Used automatically to recover from negative RHS instead of Big-M/Two-Phase |
| Sensitivity Analysis | ✅ Implemented — see [Sensitivity analysis](#sensitivity-analysis) |
| Duality (apply / solve / strong-weak check) | ✅ Implemented |
| Infeasible / unbounded detection | ✅ Detected and reported for every algorithm |
| Non-linear solver (bonus) | ✅ Steepest ascent/descent, golden section line search, Hessian-based convexity check |

The project deliberately does **not** use Two-Phase or Big-M. Infeasibility
introduced by `>=`/`=` constraints (negative RHS after building canonical
form) is resolved by running Dual Simplex immediately after canonical form is
built, before normal Primal Simplex pivoting starts. The same mechanism is
reused by Branch & Bound and Cutting Plane to re-optimize after a new
row is appended to an already-optimal tableau.

## Getting started

**Requirements:** Visual Studio 2022+ (or MSBuild) with the .NET Framework
4.7.2 targeting pack installed.

1. Open `LPR381/LPR381.csproj` (or the solution, if one is present) in Visual
   Studio.
2. Build and run — the entry point is `Program.cs`, which opens the main
   menu (`FormMain_Menu`).

Building from the command line:

```
MSBuild LPR381\LPR381.csproj /p:Configuration=Debug
```

The built executable is placed at `LPR381\bin\Debug\LPR381.exe`.

## Project structure

```
LPR381/
├── Form1.cs                     Main solver screen (Simplex, B&B, Knapsack, Cutting Plane)
├── Form2.cs                     Main menu (class name: FormMain_Menu)
├── FormNL.cs                    Non-linear programming screen (bonus)
├── FormSen.cs                   Sensitivity analysis screen
├── Input File Handler/          Parses the input text file into an LpModel
├── Output File Handler/         Writes canonical form + iterations to an output text file
├── Sensitivity Analysis/        AnalyzeSensitivity — ranging, apply-change, duality
├── Solving/                     Solver, Tableau, SolverResult, BranchNode, ExpressionParser, NonLinearSolver
├── Stored Info/                 LpModel, Constraints, NlpModel, Knapsack data structures
└── UserDisplay/                 DataGridView population helpers (Display)
```

## Input file format

### Linear / Integer programming models

```
max +2 +3 +3 +5 +2 +4
+11 +8 +6 +14 +10 +10 <=40
bin bin bin bin bin bin
```

- **Line 1:** `max` or `min`, then a signed coefficient for every decision
  variable.
- **Middle lines:** one per constraint — signed coefficients (same order as
  the objective), then a relation (`<=`, `>=`, or `=`) with the right-hand
  side, e.g. `<=40`. A space before the RHS (`<= 40`) is also accepted.
- **Last line:** one sign restriction per variable, space-separated: `+`,
  `-`, `urs`, `int`, or `bin`.

Enter the LP/IP model itself — not a canonical form, and not a relaxed
version of an IP model.

### Non-linear models (bonus)

```
MIN
x1^2+x2^2
2 2
```

- **Line 1:** `MAX` or `MIN` (case-insensitive).
- **Line 2:** the objective function as an infix expression (`+ - * / ^ ( )`
  supported). Multiplication must be explicit — `2*x1`, not `2x1`. Variables
  are `x1`, `x2`, … (1-based).
- **Line 3 (optional):** a space-separated initial point. Defaults to
  `{1, 1}` if omitted.

## Using the application

The main menu (`FormMain_Menu`) opens three screens:

- **Solver** (`Form1`) — load a file, view its Canonical Form, and solve with
  Simplex, Branch & Bound, Cutting Plane, or Knapsack Branch & Bound. Every
  tableau iteration is shown in the grid and written to the output file.
  **New Pivot** lets you hand-edit a displayed tableau's values and continue
  pivoting from that edited state.
- **Sensitivity Analysis** (`FormSen`) — load and solve a model, then explore
  and modify its optimal solution (see below).
- **Non-Linear Analysis** (`FormNL`) — load and solve an unconstrained
  non-linear objective.

## Sensitivity analysis

`FormSen` solves the model with Primal Simplex, then lets you interact
directly with the tableau grid instead of typing variable/constraint names:

- **Click a variable's column** (its header, or any coefficient under it) to
  select that variable.
- **Click a constraint's row label or its RHS cell** to select that
  constraint.
- The status bar at the top shows what's currently selected and what
  **Find Range** will report on.

Available operations, all backed by `AnalyzeSensitivity`:

| Button | What it does |
|---|---|
| Find Range | Shows the selected variable's range (objective-coefficient range, plus a column scale-factor range if it's non-basic) or the selected constraint's RHS range |
| Apply Variable Change | Re-solves with a new objective coefficient for the selected variable |
| Apply RHS Change | Re-solves with a new RHS for the selected constraint |
| Apply Column Change | Re-solves with new technological coefficients for the selected non-basic variable's column |
| Add Activity | Adds a new decision variable (column + objective coefficient) and re-solves |
| Add Constraint | Adds a new constraint and re-solves |
| Display Shadow Price | Shows the shadow price of every constraint |
| Show Dual Model | Displays the dual of the current model |
| Solve Dual Model | Solves and displays the dual model's optimal tableau |
| Duality Strength | Reports whether the model exhibits strong or weak duality |

**Every successful Apply/Add operation becomes the new working model** —
later operations build on top of it, so adding an activity and then adding a
constraint correctly accounts for the extra variable. A change that makes the
model infeasible or unbounded is rejected and rolled back, so a bad "what-if"
never corrupts the working session.

## Output file

Every solve and every applied sensitivity change is written to a text file
in the user's Downloads folder (`LP_Output_<timestamp>.txt`), containing the
canonical form, every tableau iteration, and the final result — all decimal
values rounded to three decimal places as required by the spec. Sensitivity
operations after the initial solve are **appended** to the same file rather
than overwriting it, so the file ends up as a full log of the session.

## Known limitations

- **Revised Primal Simplex is not implemented** — `Solver.SolveRevisedSimplex`
  is a stub and isn't wired to any button. The spec requires it alongside
  standard Primal Simplex.
- **`urs` (unrestricted) variables** are split into two columns internally
  (`x2'`, `x2''`). Sensitivity analysis operations that need a single
  basic/non-basic column for a variable will throw a clear error if that
  variable is `urs`, rather than silently picking the wrong column.
- Sensitivity analysis is built on Primal Simplex's tableau structure; it
  isn't applied to Branch & Bound or Knapsack solutions.

## Contributors

- Henri-Claassen
- Lebogang Masia
- Busi202
- CoffeeLover
- Nicholas005
