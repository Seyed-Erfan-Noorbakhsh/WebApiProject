# Requirements Document

## Introduction

This specification addresses the cleanup of deprecated, unused, wrongly placed, and problematic code in the Shop_ProjForWeb application. The goal is to improve code quality, maintainability, and adherence to Clean Architecture principles by removing dead code and fixing architectural violations.

## Glossary

- **Clean_Architecture**: A software design pattern that separates concerns into layers (Domain, Application, Infrastructure, Presentation)
- **Dead_Code**: Code that is never executed or called from anywhere in the application
- **Unused_Method**: A method that exists but is never invoked
- **Architecture_Violation**: Code that violates Clean Architecture principles (e.g., Infrastructure references in Application layer)
- **Deprecated_Code**: Code that is outdated and should no longer be used

## Requirements

### Requirement 1: Remove Unused Entity Methods

**User Story:** As a developer, I want to remove unused methods from domain entities, so that the codebase is cleaner and easier to maintain.

#### Acceptance Criteria

1. THE Code_Cleanup SHALL remove the `ValidatePrice()` method from Product entity (validation is already done in `ValidateEntity()`)
2. THE Code_Cleanup SHALL remove the `ValidateDiscountPercent()` method from Product entity (validation is already done in `ValidateEntity()`)
3. THE Code_Cleanup SHALL remove the `ValidateStock()` method from Inventory entity (validation is already done in `ValidateEntity()`)
4. THE Code_Cleanup SHALL remove the `Activate()` method from Product entity (never called)
5. THE Code_Cleanup SHALL remove the `Deactivate()` method from Product entity (never called)
6. THE Code_Cleanup SHALL remove the `UpdatePrice()` method from Product entity (never called)
7. THE Code_Cleanup SHALL remove the `UpdateDiscount()` method from Product entity (never called)
8. THE Code_Cleanup SHALL remove the `Cancel()` method from Order entity (cancellation is handled by OrderCancellationService)
9. THE Code_Cleanup SHALL remove the `UpdateTotalPrice()` method from Order entity (never called)
10. THE Code_Cleanup SHALL remove the `UpdateQuantity()` method from OrderItem entity (never called)
11. THE Code_Cleanup SHALL remove the `UpdateUnitPrice()` method from OrderItem entity (never called)
12. THE Code_Cleanup SHALL remove the `GetTotalPrice()` method from OrderItem entity (never called externally)
13. THE Code_Cleanup SHALL remove the `GetDiscountAmount()` method from OrderItem entity (never called externally)
14. THE Code_Cleanup SHALL remove the `GetFinalPrice()` method from OrderItem entity (never called)
15. THE Code_Cleanup SHALL remove the `UpdateThreshold()` method from Inventory entity (never called)
16. THE Code_Cleanup SHALL remove the `UpdateTotalSpending()` method from User entity (never called, VipUpgradeService handles this)
17. THE Code_Cleanup SHALL remove the `ShouldBeVip()` method from User entity (VipStatusCalculator handles this)
18. THE Code_Cleanup SHALL remove the `UpgradeToVip()` method from User entity (VipUpgradeService handles this)

### Requirement 2: Remove Unused Service Methods

**User Story:** As a developer, I want to remove unused service methods, so that the service layer is focused and maintainable.

#### Acceptance Criteria

1. THE Code_Cleanup SHALL remove the `GetCriticalStockItemsAsync()` method from InventoryService (never called)
2. THE Code_Cleanup SHALL remove the `UpdateAllLowStockFlagsAsync()` method from InventoryService (never called)
3. THE Code_Cleanup SHALL remove the `CanCancelOrderAsync()` method from OrderCancellationService (never called)
4. THE Code_Cleanup SHALL remove the `GetCancellationReasonAsync()` method from OrderCancellationService (never called)
5. THE Code_Cleanup SHALL remove the `CalculateVipDiscountAsync()` method from VipUpgradeService (never called)
6. THE Code_Cleanup SHALL remove the `IsUserVipAsync()` method from VipUpgradeService (never called)
7. THE Code_Cleanup SHALL remove the `GetUserVipTierAsync()` method from VipUpgradeService (never called)
8. THE Code_Cleanup SHALL remove the `GetUserTotalSpendingAsync()` method from VipUpgradeService (never called)
9. THE Code_Cleanup SHALL remove the `GetRemainingAmountForNextTierAsync()` method from VipUpgradeService (never called)

### Requirement 3: Remove Unused Repository Methods

**User Story:** As a developer, I want to remove unused repository methods, so that the data access layer is clean and focused.

#### Acceptance Criteria

1. THE Code_Cleanup SHALL remove the `ReserveStockAsync()` method from IInventoryRepository and InventoryRepository (never called - InventoryService uses its own implementation)
2. THE Code_Cleanup SHALL remove the `DecreaseStockWithLockAsync()` method from IInventoryRepository and InventoryRepository (never called)

### Requirement 4: Remove Unused Interfaces and Classes

**User Story:** As a developer, I want to remove unused interfaces and classes, so that the codebase doesn't have dead abstractions.

#### Acceptance Criteria

1. THE Code_Cleanup SHALL remove the `IProductImageService` interface (ProductImageService is used directly, interface is never referenced)
2. THE Code_Cleanup SHALL remove the `BusinessValidationResult` class from IValidationService.cs (never used)
3. THE Code_Cleanup SHALL remove the `ITransactionManager` interface and `TransactionManager` class (UnitOfWork provides transaction management, TransactionManager is registered but never injected)

### Requirement 5: Remove Unused Interface Methods

**User Story:** As a developer, I want to remove unused interface methods, so that interfaces are minimal and focused.

#### Acceptance Criteria

1. THE Code_Cleanup SHALL remove the `ShouldBeVip()` method from IVipStatusCalculator interface (never called)
2. THE Code_Cleanup SHALL remove the `CalculateStatusChange()` method from IVipStatusCalculator interface (never called)
3. THE Code_Cleanup SHALL remove the corresponding implementations from VipStatusCalculator

### Requirement 6: Fix Unnecessary Using Directives

**User Story:** As a developer, I want to remove unnecessary using directives, so that the code is clean and doesn't have unused imports.

#### Acceptance Criteria

1. THE Code_Cleanup SHALL remove the unnecessary `using Shop_ProjForWeb.Infrastructure.Persistent.DbContext;` from OrderService.cs
2. THE Code_Cleanup SHALL remove the unnecessary `using Shop_ProjForWeb.Core.Domain.Exceptions;` from UsersController.cs

### Requirement 7: Fix Collection Initialization Warnings

**User Story:** As a developer, I want to use modern C# collection initialization syntax, so that the code follows current best practices.

#### Acceptance Criteria

1. THE Code_Cleanup SHALL simplify collection initialization in Product.cs (`new List<OrderItem>()` to `[]`)
2. THE Code_Cleanup SHALL simplify collection initialization in Order.cs (`new List<OrderItem>()` to `[]`)
