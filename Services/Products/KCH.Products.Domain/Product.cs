using KCH.Products.Domain.Enums;
using KCH.Products.Domain.Exceptions;

namespace KCH.Products.Domain
{
    public class Product
    {
        public const int ItemCodeMaxLength = 10;
        public const int DescriptionMaxLength = 500;

        public int Id { get; private set; }
        public string? ItemCode { get; private set; }
        public string ItemDescription { get; private set; } = string.Empty;
        public ProductCondition Condition { get; private set; }
        public decimal SellingPrice { get; private set; }
        public decimal UnitCost { get; private set; }
        public bool IsActive { get; private set; }

        private Product() { }

        public static Product Create(string? itemCode, string itemDescription, ProductCondition condition, decimal sellingPrice, decimal unitCost)
        {
            var product = new Product { IsActive = true };
            product.SetDetails(itemCode, itemDescription, condition, sellingPrice, unitCost);
            return product;
        }

        public void Update(string? itemCode, string itemDescription, ProductCondition condition, decimal sellingPrice, decimal unitCost)
        {
            if (!IsActive) throw new DomainException("Cannot update an inactive product.");

            SetDetails(itemCode, itemDescription, condition, sellingPrice, unitCost);
        }

        public void Deactivate() => IsActive = false;
        public void Activate() => IsActive = true;

        private void SetDetails(string? itemCode, string itemDescription, ProductCondition condition, decimal sellingPrice, decimal unitCost)
        {
            if (string.IsNullOrWhiteSpace(itemDescription)) throw new DomainException("Item description is required.");
            if (itemDescription.Length > DescriptionMaxLength) throw new DomainException($"Item description cannot exceed {DescriptionMaxLength} characters.");
            if (!Enum.IsDefined(condition)) throw new DomainException("Invalid condition.");
            if (sellingPrice < 0) throw new DomainException("Selling price cannot be negative.");
            if (unitCost < 0) throw new DomainException("Unit cost cannot be negative.");

            SetItemCode(itemCode);
            ItemDescription = itemDescription.Trim();
            Condition = condition;
            SellingPrice = sellingPrice;
            UnitCost = unitCost;
        }

        private void SetItemCode(string? itemCode)
        {
            if (string.IsNullOrWhiteSpace(ItemCode))
            {
                ItemCode = null;
                return;
            }

            if (ItemCode.Length > ItemCodeMaxLength) throw new DomainException($"Item code cannot exceed {ItemCodeMaxLength} characters.");

            ItemCode = itemCode.Trim().ToUpperInvariant();
        }
    }
}
