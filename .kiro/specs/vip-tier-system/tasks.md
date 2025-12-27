# Implementation Plan: VIP Tier System

## Overview

This plan implements the enhanced VIP tier system by modifying existing services to support 4-tier customer loyalty with automated tier upgrades, tier-specific discounts (10%/15%/20%), and comprehensive history logging.

## Tasks

- [x] 1. Enhance VipStatusCalculator with tier calculation
  - [x] 1.1 Add tier threshold constants and CalculateTier method to IVipStatusCalculator interface
    - Add constants: Tier1Threshold=1000, Tier2Threshold=5000, Tier3Threshold=30000
    - Add method: `int CalculateTier(decimal totalSpending)`
    - Add method: `int GetDiscountPercentForTier(int tier)`
    - _Requirements: 1.1, 1.2, 1.3, 1.4, 4.1_
  - [x] 1.2 Implement CalculateTier and GetDiscountPercentForTier in VipStatusCalculator
    - Implement tier calculation logic based on thresholds
    - Implement discount mapping: Tier 1=10%, Tier 2=15%, Tier 3=20%
    - _Requirements: 2.1, 2.2, 2.3, 2.4_
  - [x] 1.3 Write property test for tier calculation correctness
    - **Property 1: Tier Calculation Correctness**
    - **Validates: Requirements 1.1, 1.2, 1.3, 1.4, 4.1, 4.4**
  - [x] 1.4 Write property test for tier-to-discount mapping
    - **Property 2: Tier-to-Discount Mapping**
    - **Validates: Requirements 2.1, 2.2, 2.3, 2.4**

- [x] 2. Update VipStatusHistory entity validation
  - [x] 2.1 Remove single-tier-jump restriction from VipStatusHistory.ValidateEntity
    - Allow multi-tier upgrades (e.g., 0 → 3)
    - Allow tier downgrades (e.g., 2 → 1)
    - Keep validation that tier must change (PreviousTier != NewTier)
    - _Requirements: 4.2, 4.3_

- [x] 3. Enhance VipUpgradeService with multi-tier support
  - [x] 3.1 Update CheckAndUpgradeAsync to use CalculateTier for tier determination
    - Replace hardcoded threshold with VipStatusCalculator.CalculateTier
    - Support tier upgrades from any tier to any higher tier
    - Support tier downgrades when spending decreases
    - _Requirements: 1.5, 4.2, 4.3_
  - [x] 3.2 Ensure VipStatusHistory record is created for all tier changes
    - Create history record with previous tier, new tier, order total, spending, reason
    - Persist history immediately upon tier change
    - _Requirements: 3.1, 3.2, 3.3_
  - [x] 3.3 Write property test for tier transition correctness
    - **Property 5: Tier Transition Correctness**
    - **Validates: Requirements 4.2, 4.3**
  - [x] 3.4 Write property test for history record completeness
    - **Property 4: History Record Completeness**
    - **Validates: Requirements 3.1, 3.2**

- [x] 4. Update AdditiveDiscountCalculator with tier-based discounts
  - [x] 4.1 Inject IVipStatusCalculator into AdditiveDiscountCalculator
    - Add constructor dependency injection
    - Update Program.cs service registration if needed
    - _Requirements: 2.1, 2.2, 2.3, 2.4_
  - [x] 4.2 Update discount calculation to use GetDiscountPercentForTier
    - Replace hardcoded VipDiscountPercent with tier-based lookup
    - Update method signatures to accept vipTier instead of isVip boolean
    - Ensure backward compatibility with existing callers
    - _Requirements: 2.1, 2.2, 2.3, 2.4, 2.5_
  - [x] 4.3 Write property test for additive discount calculation
    - **Property 3: Additive Discount Calculation**
    - **Validates: Requirements 2.5, 5.2**
  - [x] 4.4 Write property test for price rounding and audit trail
    - **Property 6: Price Rounding and Audit Trail**
    - **Validates: Requirements 5.1, 5.3**

- [x] 5. Update PricingService and OrderService integration
  - [x] 5.1 Update PricingService to pass vipTier to discount calculator
    - Modify CalculateFinalPriceWithDiscounts to accept vipTier parameter
    - Update GetDiscountBreakdown to use tier-based discounts
    - _Requirements: 5.4_
  - [x] 5.2 Update OrderService to use user's VipTier for pricing
    - Pass user.VipTier to pricing calculations
    - Store correct VIP discount percentage on order items
    - _Requirements: 5.3, 5.4_

- [x] 6. Checkpoint - Ensure all tests pass
  - Ensure all tests pass, ask the user if questions arise.

- [x] 7. Update VipStatusHistoryRepository query ordering
  - [x] 7.1 Verify GetByUserIdAsync returns records ordered by CreatedAt descending
    - Check existing implementation
    - Update if needed to ensure descending order
    - _Requirements: 3.4_

- [x] 8. Final integration testing
  - [x] 8.1 Write integration test for end-to-end tier upgrade flow
    - Test user progressing through all tiers with orders
    - Verify history records created at each tier change
    - _Requirements: 1.1, 1.2, 1.3, 3.1, 3.2_
  - [x] 8.2 Write integration test for tier downgrade on order cancellation
    - Test tier downgrade when order is cancelled
    - Verify history record created for downgrade
    - _Requirements: 4.3, 3.1_

- [x] 9. Final checkpoint - Ensure all tests pass
  - Ensure all tests pass, ask the user if questions arise.

## Notes

- All tasks are required for comprehensive implementation
- Each task references specific requirements for traceability
- Property tests use FsCheck library for C# property-based testing
- Checkpoints ensure incremental validation
