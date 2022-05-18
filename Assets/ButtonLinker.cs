using System;
using UnityEngine;

public enum UIButtonType
{
    [InspectorName("Close UI")]
    CloseUI,
    [InspectorName("Inven/Buy")]
    InvenBuy,
    [InspectorName("Inven/EnchantInspectorName")]
    InvenEnchant,
    [InspectorName("Shop/Buy")]
    ShopBuy,
    [InspectorName("Shop/Sell")]
    ShopSell,
    test1,
    test2,
    test3,
    MyMenu,
    Mail,
    Attend,
    Contact,
}

//public class UIEventList : PropertyAttribute { }
[Serializable]
public class UIEventList
{
    public UIButtonType _;
}

public class ButtonLinker : MonoBehaviour
{
    public UIEventList targetEvent;
    public string test;
}
