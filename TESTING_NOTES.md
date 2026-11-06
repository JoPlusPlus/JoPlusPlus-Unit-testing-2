# 1. Total tests, passing, failing.
- ## Total = 20, succeeded = 20, failed = 0;
# 2. Line coverage % (from coverage-report/Summary.txt).
- ## 90.5%
# 3. One sentence each: where you used Moq, AutoFixture and Bogus.
- `Moq` -> Task 2, method name : CheckoutAsync_ValidOrderAndAllItemsInStock_SavesOrderAndSendsConfirmation
- `AutoFixture` -> Task 3.1, method name : CheckoutAsync_ValidOrderQuantityAndPrice_CheckoutSucceeds
- `Bogus` -> Task 3.2, method name : CheckoutAsync_BogusCustomerWithFixedSeed_ReceiptShouldHaveSameNameAndEmail

# 4. Look at Examples B and C. Is it fair that a customer who pays 999.99 pays more than one who buys for 1,000? (One sentence — do not change the code.)
- ## Since the business says `subTotal < 1000` dont get discount .. then the business is always right lol :) 
# 5. Any bug you think you found (or "none").
- NONE