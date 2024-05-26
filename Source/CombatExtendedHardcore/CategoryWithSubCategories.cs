using System;
using System.Collections.Generic;
using Verse;

namespace CombatExtendedHardcore
{
    public struct CategoryWithSubCategories : IEquatable<CategoryWithSubCategories>
    {
        private WeaponCategoryDef mainCategory;
        public WeaponCategoryDef MainCategory => mainCategory;

        private List<WeaponSubCategoryDef> subCategories;
        public List<WeaponSubCategoryDef> SubCategories => subCategories;

        public CategoryWithSubCategories(WeaponCategoryDef mainCategory, List<WeaponSubCategoryDef> subCategories)
        {
            this.mainCategory = mainCategory;
            this.subCategories = subCategories;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is CategoryWithSubCategories))
            {
                return false;
            }
            return Equals((CategoryWithSubCategories)obj);
        }

        public bool Equals(CategoryWithSubCategories other)
        {
            return this == other;
        }

        public static bool operator ==(CategoryWithSubCategories a, CategoryWithSubCategories b)
        {
            if (a.mainCategory == b.mainCategory)
            {
                return a.mainCategory.defName == b.mainCategory.defName;
            }
            return false;
        }

        public static bool operator !=(CategoryWithSubCategories a, CategoryWithSubCategories b)
        {
            return !(a == b);
        }

        public override int GetHashCode()
        {
            return Gen.HashCombine(0, mainCategory);
        }

        public override string ToString()
        {
            return "(" + 0 + "x " + ((mainCategory != null) ? mainCategory.defName : "null") + ")";
        }


        public static implicit operator CategoryWithSubCategories(CategoryWithSubCategoriesClass t)
        {
            if (t == null)
            {
                return new CategoryWithSubCategories(null, null);
            }
            return new CategoryWithSubCategories(t.mainCategory, t.subCategories);
        }
    }
}
