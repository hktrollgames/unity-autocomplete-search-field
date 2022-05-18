using System;
using UnityEngine;

public enum UIButtonType
{
    [InspectorName("Inven/Buy")]
    InvenBuy,
    [InspectorName("Inven/Enchant")]
    InvenEnchant,
    [InspectorName("Shop/Buy")]
    ShopBuy,
    [InspectorName("Shop/Sell")]
    ShopSell,
    [InspectorName("Close UI")]
    CloseUI,
    test1,
    test2,
    test3,
    MyMenu,
    Mail,
    Attend,
    Contact,
}

[Serializable]
public class UIEventList
{
    public UIButtonType _;
}

public class ButtonLinker : MonoBehaviour
{
    public UIEventList target;
}
