# Design Document

## Overview

This design document outlines the approach for cleaning up deprecated, unused, and problematic code in the Shop_ProjForWeb application. The cleanup follows Clean Architecture principles and focuses on removing dead code while maintaining system integrity.

## Architecture

The cleanup targets all layers of the Clean Architecture:

```
┌─────────────────────────────────────────────────────────────┐
│                    Presentation Layer                        │
│  - Remove unnecessary using directives in Controllers        │
└─────────────────────────────────────────────────────────────┘
                              │
┌─────────────────────────────────────────────────────────────┐
│                    Application Layer                         │
│  - Remove unused service methods                             │
│  - Remove unused interfaces (IProductImageService)           │
│  - Remove unused classes (TransactionManager)                │
│  - Remove unused interface methods                           │
└─────────────────────────────────────────────────────────────┘
                              │
┌─────────────────────────────────────────────────────────────┐
│                      Domain Layer                            │
│  - Remove unused entity methods                              │
│  - Remove unused interface methods (IVipStatusCalculator)    │
│  - Simplify collection initializations                       │
└─────────────────────────────────────────────────────────────┘
                              │
┌─────────────────────────────────────────────────────────────┐
│                   Infrastructure Layer                       │
│  - Remove unused repository methods                          │
│  - Remove TransactionManager implementation                  │
└─────────────────────────────────────────────────────────────┘
```

## Components and Interfaces

### Domain Layer Changes

#### Product Entity
Remove the following unused methods:
- `ValidatePrice()` - Redundant with `ValidateEntity()`
- `ValidateDiscountPercent()` - Redundant with `ValidateEntity()`
- `UpdatePrice()` - Never called
- `UpdateDiscount()` - Never called
- `Activate()` - Never called
- `Deactivate()` - Never called

#### Order Entity
Remove the following unused methods:
- `Cancel()` - Handled by OrderCancellationService
- `UpdateTotalPrice()` - Never called

#### OrderItem Entity
Remove the following unused methods:
- `GetTotalPrice()` - Never called externally
- `GetDiscountAmount()` - Never called externally
- `GetFinalPrice()` - Never called
- `UpdateQuantity()` - Never called
- `UpdateUnitPrice()` - Never called

#### Inventory Entity
Remove the following unused methods:
- `ValidateStock()` - Redundant with `ValidateEntity()`
- `UpdateThreshold()` - Never called

#### User Entity
Remove the following unused methods:
- `UpdateTotalSpending()` - VipUpgradeService handles this
- `ShouldBeVip()` - VipStatusCalculator handles this
- `UpgradeToVip()` - VipUpgradeService handles this

#### IVipStatusCalculator Interface
Remove the following unused methods:
- `ShouldBeVip()` - Never called
- `CalculateStatusChange()` - Never called

### Application Layer Changes

#### InventoryService
Remove the following unused methods:
- `GetCriticalStockItemsAsync()` - Never called
- `UpdateAllLowStockFlagsAsync()` - Never called

#### OrderCancellationService
Remove the following unused methods:
- `CanCancelOrderAsync()` - Never called
- `GetCancellationReasonAsync()` - Never called

#### VipUpgradeService
Remove the following unused methods:
- `CalculateVipDiscountAsync()` - Never called
- `IsUserVipAsync()` - Never called
- `GetUserVipTierAsync()` - Never called
- `GetUserTotalSpendingAsync()` - Never called
- `GetRemainingAmountForNextTierAsync()` - Never called

#### Interfaces to Remove
- `IProductImageService` - ProductImageService is used directly
- `ITransactionManager` - UnitOfWork provides transaction management
- `BusinessValidationResult` class - Never used

### Infrastructure Layer Changes

#### IInventoryRepository and InventoryRepository
Remove the following unused methods:
- `ReserveStockAsync()` - Never called (InventoryService has its own implementation)
- `DecreaseStockWithLockAsync()` - Never called

#### TransactionManager
Remove the entire class - UnitOfWork provides transaction management

### Presentation Layer Changes

#### OrderService.cs
Remove unnecessary using directive:
- `using Shop_ProjForWeb.Infrastructure.Persistent.DbContext;`

#### UsersController.cs
Remove unnecessary using directive:
- `using Shop_ProjForWeb.Core.Domain.Exceptions;`

## Data Models

No data model changes required. This cleanup only removes unused code.

## Correctness Properties

*A property is a characteristic or behavior that should hold true across all valid executions of a system-essentially, a formal statement about what the system should do. Properties serve as the bridge between human-readable specifications and machine-verifiable correctness guarantees.*

Since this is a code cleanup task that removes unused code, the primary correctness property is:

**Property 1: Build Success**
*For any* code removal operation, the solution SHALL compile successfully without errors after the removal.
**Validates: Requirements 1-7**

**Property 2: Test Suite Integrity**
*For any* code removal operation, all existing tests SHALL continue to pass after the removal.
**Validates: Requirements 1-7**

## Error Handling

No new error handling required. The cleanup removes code that is not executed.

## Testing Strategy

### Verification Approach

1. **Build Verification**: After each removal, verify the solution compiles successfully
2. **Test Execution**: Run the existing test suite to ensure no regressions
3. **Diagnostic Check**: Use IDE diagnostics to verify no new warnings or errors are introduced

### Test Commands

```bash
# Build the solution
dotnet build Shop_ProjForWeb.sln

# Run all tests
dotnet test Shop_ProjForWeb.sln
```
