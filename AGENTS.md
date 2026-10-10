<!--VITE PLUS START-->

# Using Vite+, the Unified Toolchain for the Web

This project is using Vite+, a unified toolchain built on top of Vite, Rolldown, Vitest, tsdown, Oxlint, Oxfmt, and Vite Task. Vite+ wraps runtime management, package management, and frontend tooling in a single global CLI called `vp`. Vite+ is distinct from Vite, and it invokes Vite through `vp dev` and `vp build`. Run `vp help` to print a list of commands and `vp <command> --help` for information about a specific command.

Docs are local at `node_modules/vite-plus/docs` or online at <https://viteplus.dev/guide/>.

## Built-in Commands vs Scripts

`vp <name>` runs a built-in command. `vp run <name>` runs a `package.json` script or a `vite.config.ts` task. Scripts cannot overwrite built-ins, so `vp dev` and `vp run dev` may do different things. Check `package.json` and `vite.config.ts` first, and run `vp run <name>` when the project defines a script or task with that name.

## Tool Versions

Run `vp toolchain` to show versions and relationships in the active Vite+
release. Add a tool name to select part of the graph. For example, run
`vp toolchain vite`. Use `--global` to ignore the local `vite-plus` package. Use
`vp why <package>` to show the package-manager dependency graph.

## Review Checklist

- [ ] Run `vp install` after pulling remote changes and before getting started.
- [ ] Run `vp check` and `vp test` to format, lint, type check and test changes.
- [ ] Check if there are `vite.config.ts` tasks or `package.json` scripts necessary for validation, run via `vp run <script>`.
- [ ] If setup, runtime, or package-manager behavior looks wrong, run `vp env doctor` and include its output when asking for help.

<!--VITE PLUS END-->

## C# Style

- Always use full, brace-delimited bodies for C# methods, constructors, operators, and property accessors. Do not use expression-bodied (`=>`) class members.
- Lambda expressions remain allowed for LINQ predicates and delegate callbacks; they are not a substitute for named class methods.
- Add a blank line before and after C# control-flow block constructs, including `if`/`else`, `for`, `foreach`, and `while`. Do not add a leading blank line when the construct is the sole statement in its enclosing block.
- Add a blank line before C# `return` statements, except when the `return` is the sole statement in its enclosing block; do not add a leading blank line in that case.
- Prefix C# member variables (fields) with an underscore and use camelCase, for example `_members`. Keep local variables and parameters unprefixed.
- Treat warnings as errors throughout the repository. Keep `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>` enabled in the root `Directory.Build.props`; fix warnings instead of suppressing or downgrading them.
- Put each C# class, record, enum, or interface in its own file, named after the type. Do not bundle unrelated top-level types in one file.
- Organize related types into focused folders, such as `Identifiers`, `Values`, `Calendar`, `Members`, `Contributions`, `Savings`, `Shop`, `Wallets`, `Policies`, and `Events`. Partial-class files may contain only that partial type.

## TypeScript Style

- Use named function declarations for top-level methods and reusable logic.
- Use arrow functions for inline callbacks, including multi-line callbacks. Do not use anonymous `function` expressions for callbacks; named functions may be used as callbacks.
- Always use braces for control-flow bodies, including single-statement bodies.
- Add a blank line before and after TypeScript control-flow block constructs, including `if`/`else`, `for`, `foreach` (`for...of`/`for...in`), `while`, and `try`/`catch`/`finally`. Do not add a leading blank line when the construct is the sole statement in its enclosing block.
- Add a blank line before TypeScript `return` statements, except when the `return` is the sole statement in its enclosing block; do not add a leading blank line in that case.
