# JsonECore Enhancement Plan (Kent Beck Style)

## Overview
Refactoring and enhancement plan following Kent Beck's principles:
- Remove duplication (DRY)
- Simplify design
- Improve test coverage
- Add CI/CD pipeline

## Tasks

### Phase 1: Remove Duplication (DRY)
- [x] Create `JsonElementHelper.cs` with shared utilities
- [x] Refactor operators to use shared helpers
- [x] Refactor functions to use shared helpers
- [x] Refactor AST expressions to use shared helpers

### Phase 2: Add CI/CD Pipeline
- [x] Create GitHub Actions workflow
- [x] Add build, test, and coverage steps

### Phase 3: Add Benchmarks
- [x] Create benchmark project
- [x] Add benchmarks for core operations

### Phase 4: Improve Test Names (Behavior-Driven)
- [ ] Rename tests to describe behavior
- [ ] Add more edge case tests

### Phase 5: Import Specification Tests (Future)
- [ ] Parse specification.yml
- [ ] Generate theory tests from spec
- [ ] Achieve full spec compliance

## Progress Tracking

| Phase | Status | Files Changed |
|-------|--------|---------------|
| 1 | ✅ Complete | JsonElementHelper.cs, 10 operators, 4 function files, 8 AST files |
| 2 | ✅ Complete | .github/workflows/ci.yml |
| 3 | ✅ Complete | benchmarks/JsonECore.Benchmarks project |
| 4 | 🔲 Pending | - |
| 5 | 🔲 Future | - |

## Commands

```bash
# Build
dotnet build

# Test
dotnet test

# Run benchmarks
dotnet run --project benchmarks/JsonECore.Benchmarks -c Release

# Check coverage
dotnet test --collect:"XPlat Code Coverage"
```
