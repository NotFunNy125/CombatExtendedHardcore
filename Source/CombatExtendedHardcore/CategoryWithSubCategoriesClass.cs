using System.Collections.Generic;
using System.Xml;
using Verse;

namespace CombatExtendedHardcore
{
    public class CategoryWithSubCategoriesClass
    {
        public WeaponCategoryDef mainCategory = new WeaponCategoryDef();
        public List<WeaponSubCategoryDef> subCategories = new List<WeaponSubCategoryDef>();

        public CategoryWithSubCategoriesClass()
        {
        }

        public CategoryWithSubCategoriesClass(WeaponCategoryDef mainCategory, List<WeaponSubCategoryDef> subCategories)
        {
            this.mainCategory = mainCategory;
            this.subCategories = subCategories;
        }

        public void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "mainCategory", xmlRoot.Name);
            foreach (XmlNode child in xmlRoot.ChildNodes)
            {
                if (child.ChildNodes.Count != 0)
                {
                    foreach (XmlNode child2 in child.ChildNodes)
                    {
                        RegisterListCrossReferences(child2 as XmlElement);
                    }
                }
            }
        }

        private void RegisterListCrossReferences(XmlElement element)
        {
            if (element.ParentNode.Name == "subCategories")
            {
                DirectXmlCrossRefLoader.RegisterListWantsCrossRef(subCategories, element.InnerText);
            }
        }

        public override string ToString()
        {
            return "(" + 0 + "x " + ((mainCategory != null) ? mainCategory.defName : "null") + ")"; ;
        }

        public static implicit operator CategoryWithSubCategoriesClass(CategoryWithSubCategories t)
        {
            return new CategoryWithSubCategoriesClass(t.MainCategory, t.SubCategories);
        }
    }
}