# Implementation Plan: Code Cleanup

## Overview

This implementation plan removes deprecated, unused, and problematic code from the Shop_ProjForWeb application. Tasks are organized by architectural layer and executed in order to maintain build integrity.

## Tasks

- [x] 1. Remove unused methods from Product entity
  - Remove `ValidatePrice()`, `ValidateDiscountPercent()`, `UpdatePrice()`, `UpdateDiscount()`, `Activate()`, `Deactivate()` methods
  - Simplify collection initialization (`new List<OrderItem>()` to `[]`)
  - _Requirements: 1.1, 1.2, 1.6, 1.7, 1.4, 1.5, 7.1_

- [x] 2. Remove unused methods from Order entity
  - Remove `Cancel()` and `UpdateTotalPrice()` methods
  - Simplify collection initialization (`new List<OrderItem>()` to `[]`)
  - _Requirements: 1.8, 1.9, 7.2_

- [x] 3. Remove unused methods from OrderItem entity
  - Remove `GetTotalPrice()`, `GetDiscountAmount()`, `GetFinalPrice()`, `UpdateQuantity()`, `UpdateUnitPrice()` methods
  - _Requirements: 1.12, 1.13, 1.14, 1.10, 1.11_

- [x] 4. Remove unused methods from Inventory entity
  - Remove `ValidateStock()` and `UpdateThreshold()` methods
  - _Requirements: 1.3, 1.15_

- [x] 5. Remove unused methods from User entity
  - Remove `UpdateTotalSpending()`, `ShouldBeVip()`, `UpgradeToVip()` methods
  - _Requirements: 1.16, 1.17, 1.18_

- [x] 6. Remove unused methods from IVipStatusCalculator and VipStatusCalculator
  - Remove `ShouldBeVip()` and `CalculateStatusChange()` from interface and implementation
  - Remove `VipStatusChange` class if no longer needed
  - _Requirements: 5.1, 5.2, 5.3_

- [x] 7. Remove unused methods from InventoryService
  - Remove `GetCriticalStockItemsAsync()` and `UpdateAllLowStockFlagsAsync()` methods
  - _Requirements: 2.1, 2.2_

- [x] 8. Remove unused methods from OrderCancellationService
  - Remove `CanCancelOrderAsync()` and `GetCancellationReasonAsync()` methods
  - _Requirements: 2.3, 2.4_

- [x] 9. Remove unused methods from VipUpgradeService
  - Remove `CalculateVipDiscountAsync()`, `IsUserVipAsync()`, `GetUserVipTierAsync()`, `GetUserTotalSpendingAsync()`, `GetRemainingAmountForNextTierAsync()` methods
  - _Requirements: 2.5, 2.6, 2.7, 2.8, 2.9_

- [x] 10. Remove unused repository methods
  - Remove `ReserveStockAsync()` and `DecreaseStockWithLockAsync()` from IInventoryRepository interface
  - Remove corresponding implementations from InventoryRepository
  - _Requirements: 3.1, 3.2_

- [x] 11. Remove unused interfaces and classes
  - Remove `IProductImageService` interface file
  - Remove `BusinessValidationResult` class from IValidationService.cs
  - Remove `ITransactionManager` interface file
  - Remove `TransactionManager` class file
  - Remove TransactionManager registration from Program.cs
  - _Requirements: 4.1, 4.2, 4.3_

- [x] 12. Fix unnecessary using directives
  - Remove `using Shop_ProjForWeb.Infrastructure.Persistent.DbContext;` from OrderService.cs
  - Remove `using Shop_ProjForWeb.Core.Domain.Exceptions;` from UsersController.cs
  - _Requirements: 6.1, 6.2_

- [x] 13. Checkpoint - Build and test verification
  - Run `dotnet build Shop_ProjForWeb.sln` to verify compilation
  - Run `dotnet test Shop_ProjForWeb.sln` to verify all tests pass
  - Ensure all tests pass, ask the user if questions arise.

## Notes

- Tasks are ordered to maintain build integrity throughout the cleanup process
- Each task should be followed by a build verification to catch issues early
- The final checkpoint ensures the entire solution compiles and all tests pass
