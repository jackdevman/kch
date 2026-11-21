using KCH.Products.Domain.Enums;
using KCH.Products.Domain.Exceptions;
using Shouldly;

namespace KCH.Products.Domain.Tests
{
    public class ProductTests
    {
        private const string ValidCode = "ABC123";
        private const string ValidDescription = "Hot Wheels '67 Camaro";
        private const ProductCondition ValidCondition = ProductCondition.MISB;
        private const decimal ValidPrice = 150m;
        private const decimal ValidCost = 100m;

        private static Product CreateValid() =>
            Product.Create(ValidCode, ValidDescription, ValidCondition, ValidPrice, ValidCost);

        // ---- Create ----

        [Fact]
        public void Create_WithValidInput_SetsAllProperties()
        {
            var product = CreateValid();

            product.ItemCode.ShouldBe(ValidCode);
            product.ItemDescription.ShouldBe(ValidDescription);
            product.Condition.ShouldBe(ValidCondition);
            product.SellingPrice.ShouldBe(ValidPrice);
            product.UnitCost.ShouldBe(ValidCost);
        }

        [Fact]
        public void Create_IsActiveByDefault()
        {
            CreateValid().IsActive.ShouldBeTrue();
        }

        [Fact]
        public void Create_WithZeroPriceAndCost_IsAllowed()
        {
            var product = Product.Create(ValidCode, ValidDescription, ValidCondition, 0m, 0m);

            product.SellingPrice.ShouldBe(0m);
            product.UnitCost.ShouldBe(0m);
        }

        // ---- Item code ----

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Create_WithBlankItemCode_SetsItemCodeToNull(string? itemCode)
        {
            var product = Product.Create(itemCode, ValidDescription, ValidCondition, ValidPrice, ValidCost);

            product.ItemCode.ShouldBeNull();
        }

        [Fact]
        public void Create_TrimsAndUppercasesItemCode()
        {
            var product = Product.Create("  abc123  ", ValidDescription, ValidCondition, ValidPrice, ValidCost);

            product.ItemCode.ShouldBe("ABC123");
        }

        [Fact]
        public void Create_WithItemCodeAtMaxLength_IsAllowed()
        {
            var code = new string('A', Product.ItemCodeMaxLength);

            var product = Product.Create(code, ValidDescription, ValidCondition, ValidPrice, ValidCost);

            product.ItemCode.ShouldBe(code);
        }

        [Fact]
        public void Create_WithItemCodeAtMaxLengthAfterTrim_IsAllowed()
        {
            var code = new string('A', Product.ItemCodeMaxLength);

            var product = Product.Create($"  {code}  ", ValidDescription, ValidCondition, ValidPrice, ValidCost);

            product.ItemCode.ShouldBe(code);
        }

        [Fact]
        public void Create_WithItemCodeTooLong_Throws()
        {
            var code = new string('A', Product.ItemCodeMaxLength + 1);

            var ex = Should.Throw<DomainException>(() =>
                Product.Create(code, ValidDescription, ValidCondition, ValidPrice, ValidCost));

            ex.Message.ShouldContain("Item code");
        }

        // ---- Description ----

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Create_WithBlankDescription_Throws(string? description)
        {
            var ex = Should.Throw<DomainException>(() =>
                Product.Create(ValidCode, description!, ValidCondition, ValidPrice, ValidCost));

            ex.Message.ShouldBe("Item description is required.");
        }

        [Fact]
        public void Create_TrimsDescription()
        {
            var product = Product.Create(ValidCode, "  Loose figure  ", ValidCondition, ValidPrice, ValidCost);

            product.ItemDescription.ShouldBe("Loose figure");
        }

        [Fact]
        public void Create_WithDescriptionAtMaxLengthAfterTrim_IsAllowed()
        {
            var description = new string('x', Product.DescriptionMaxLength);

            var product = Product.Create(ValidCode, $"  {description}  ", ValidCondition, ValidPrice, ValidCost);

            product.ItemDescription.ShouldBe(description);
        }

        [Fact]
        public void Create_WithDescriptionTooLong_Throws()
        {
            var description = new string('x', Product.DescriptionMaxLength + 1);

            var ex = Should.Throw<DomainException>(() =>
                Product.Create(ValidCode, description, ValidCondition, ValidPrice, ValidCost));

            ex.Message.ShouldContain("Item description cannot exceed");
        }

        // ---- Condition ----

        [Theory]
        [InlineData(ProductCondition.MISB)]
        [InlineData(ProductCondition.MIB)]
        [InlineData(ProductCondition.BIB)]
        [InlineData(ProductCondition.Loose)]
        public void Create_WithDefinedCondition_IsAllowed(ProductCondition condition)
        {
            var product = Product.Create(ValidCode, ValidDescription, condition, ValidPrice, ValidCost);

            product.Condition.ShouldBe(condition);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(99)]
        public void Create_WithUndefinedCondition_Throws(int condition)
        {
            var ex = Should.Throw<DomainException>(() =>
                Product.Create(ValidCode, ValidDescription, (ProductCondition)condition, ValidPrice, ValidCost));

            ex.Message.ShouldBe("Invalid condition.");
        }

        // ---- Prices ----

        [Fact]
        public void Create_WithNegativeSellingPrice_Throws()
        {
            var ex = Should.Throw<DomainException>(() =>
                Product.Create(ValidCode, ValidDescription, ValidCondition, -0.01m, ValidCost));

            ex.Message.ShouldBe("Selling price cannot be negative.");
        }

        [Fact]
        public void Create_WithNegativeUnitCost_Throws()
        {
            var ex = Should.Throw<DomainException>(() =>
                Product.Create(ValidCode, ValidDescription, ValidCondition, ValidPrice, -0.01m));

            ex.Message.ShouldBe("Unit cost cannot be negative.");
        }

        // ---- Update ----

        [Fact]
        public void Update_WithValidInput_ChangesAllProperties()
        {
            var product = CreateValid();

            product.Update("xyz9", "  New description ", ProductCondition.Loose, 200m, 120m);

            product.ItemCode.ShouldBe("XYZ9");
            product.ItemDescription.ShouldBe("New description");
            product.Condition.ShouldBe(ProductCondition.Loose);
            product.SellingPrice.ShouldBe(200m);
            product.UnitCost.ShouldBe(120m);
        }

        [Fact]
        public void Update_WithBlankItemCode_ClearsItemCode()
        {
            var product = CreateValid();

            product.Update(null, ValidDescription, ValidCondition, ValidPrice, ValidCost);

            product.ItemCode.ShouldBeNull();
        }

        [Fact]
        public void Update_WhenInactive_Throws()
        {
            var product = CreateValid();
            product.Deactivate();

            var ex = Should.Throw<DomainException>(() =>
                product.Update(ValidCode, "Changed", ValidCondition, ValidPrice, ValidCost));

            ex.Message.ShouldBe("Cannot update an inactive product.");
            product.ItemDescription.ShouldBe(ValidDescription);
        }

        [Fact]
        public void Update_WithInvalidInput_LeavesProductUnchanged()
        {
            var product = CreateValid();

            Should.Throw<DomainException>(() =>
                product.Update("NEWCODE", "New description", ProductCondition.Loose, 200m, -1m));

            product.ItemCode.ShouldBe(ValidCode);
            product.ItemDescription.ShouldBe(ValidDescription);
            product.Condition.ShouldBe(ValidCondition);
            product.SellingPrice.ShouldBe(ValidPrice);
            product.UnitCost.ShouldBe(ValidCost);
        }

        [Fact]
        public void Update_WithItemCodeTooLong_LeavesProductUnchanged()
        {
            var product = CreateValid();

            Should.Throw<DomainException>(() =>
                product.Update(new string('A', Product.ItemCodeMaxLength + 1), "New description", ProductCondition.Loose, 200m, 120m));

            product.ItemCode.ShouldBe(ValidCode);
            product.ItemDescription.ShouldBe(ValidDescription);
        }

        // ---- Activate / Deactivate ----

        [Fact]
        public void Deactivate_SetsIsActiveFalse()
        {
            var product = CreateValid();

            product.Deactivate();

            product.IsActive.ShouldBeFalse();
        }

        [Fact]
        public void Activate_AfterDeactivate_SetsIsActiveTrue()
        {
            var product = CreateValid();
            product.Deactivate();

            product.Activate();

            product.IsActive.ShouldBeTrue();
        }

        [Fact]
        public void Activate_AfterReactivation_AllowsUpdate()
        {
            var product = CreateValid();
            product.Deactivate();
            product.Activate();

            product.Update(ValidCode, "Changed", ValidCondition, ValidPrice, ValidCost);

            product.ItemDescription.ShouldBe("Changed");
        }
    }
}
