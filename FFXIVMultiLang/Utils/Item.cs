using LItem = Lumina.Excel.GeneratedSheets.Item;

namespace FFXIVMultiLang.Utils;

public static class ItemUtil
{
    public static uint GetSellPriceForItem(LItem item)
    {
        var priceLow = item.PriceLow;
        var isHq = item.RowId > 1000000 && item.RowId < 2000000;
        var numMateria = item.MateriaSlotCount;
        var isFilterGroup14 = item.FilterGroup == 14;

        uint result = priceLow;

        if (isHq) result = (11 * priceLow + 9) / 10;
        if (isFilterGroup14) return (11 * result + 9) / 10;
        if (numMateria > 0) return (result * (numMateria + (uint)10) + 9) / 10;

        return result;
    }
}
