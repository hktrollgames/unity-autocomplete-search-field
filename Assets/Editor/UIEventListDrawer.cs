using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(UIEventList))]
public class UIEventListDrawerDrawer : PropertyDrawer
{
    float uiHeight;

    const int lineHeight = 18;
    const int groupInterval = 8;
    const int margin = 4;           // group border margin

    [SerializeField]
    AutocompleteSearchField.AutocompleteSearchField autocompleteSearchField;
    SerializedProperty menuType;
    Dictionary<string, UIButtonType> menuNames;

    class EnumInfo
    {
        public string displayName;
        public UIButtonType type;

        public EnumInfo(string displayName)
        {
            this.displayName = displayName;
        }
    }

    void Init(SerializedProperty property)
    {
        if (autocompleteSearchField != null)
            return;

        autocompleteSearchField = new AutocompleteSearchField.AutocompleteSearchField();
        autocompleteSearchField.onInputChanged = OnInputChanged;
        autocompleteSearchField.onConfirm = OnConfirm;


        var arrList = Enum.GetValues(typeof(UIButtonType));
        menuNames = (from UIButtonType item in arrList
                    select item)
                    .ToDictionary(x => x.ToString().ToLower(), x => x);

         
        var name = property.displayName;
        property.Next(true);
        menuType = property.Copy();

        List<EnumInfo> enumNames = new List<EnumInfo>();
        foreach (var item in menuType.enumDisplayNames)
        {
            enumNames.Add(new EnumInfo(item.ToString()));
        }

        int i = 0;
        foreach(var item in menuNames.Values)
        {
            enumNames[i++].type = item;
        }


        foreach (var item in enumNames)
        {
            string str = item.displayName.ToString();
            string key = str;
            string displayName = str;
            var temp = str.Split(' ');
            if (temp.Length > 1)
            {
                key = temp[0];
                displayName = temp[1];
            }

            if (groupMenuNames.ContainsKey(key) == false)
            {
                groupMenuNames[key] = new List<Tuple<string, UIButtonType>>();
            }
            groupMenuNames[key].Add(new Tuple<string, UIButtonType>(displayName, item.type));
        }
    }

    Rect elementRect;

    float searchIconWidth = 18;
    float selectIconWidth = 18;

    public enum ViewMode
    {
        DropDownUI,
        SelectUI,
        SearchUI
    }
    ViewMode mode;
    public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(rect, label, property);

        Init(property);


        rect.width -= (selectIconWidth + searchIconWidth);
        rect = EditorGUI.PrefixLabel(rect, GUIUtility.GetControlID(FocusType.Passive), label);
        rect.height = lineHeight;

        OnDropdownGUI(rect, property);

        if (mode != ViewMode.DropDownUI)
        {
            rect.y += lineHeight;
            if (GUI.Button(rect, "Close"))
            {
                mode = ViewMode.DropDownUI;
                uiHeight = 0;
            }
        }

        if (mode == ViewMode.DropDownUI)
        {
            Rect selectIconRect = new Rect(rect.x + rect.width, rect.y, selectIconWidth, lineHeight);

            if (GUI.Button(selectIconRect, "?"))
            {
                mode = ViewMode.SelectUI;
            }
            Rect searchIconRect = new Rect(selectIconRect.x + selectIconRect.width, selectIconRect.y, searchIconWidth, lineHeight);

            if (GUI.Button(searchIconRect, "?"))
            {
                mode = ViewMode.SearchUI;
            }
        }

        switch (mode)
        {
            case ViewMode.SelectUI:
                OnSelectGUI(rect);
                break;
            case ViewMode.SearchUI:
                OnSearchGUI(rect);
                break;
        }


        EditorGUI.EndProperty();
    }

    private void OnDropdownGUI(Rect rect, SerializedProperty property)
    {
        var dropdownRect = rect;
        dropdownRect.height = lineHeight;
        var menuTypePp = property.FindPropertyRelative("_");
        if (menuTypePp == null)
            return;
        EditorGUI.PropertyField(dropdownRect, menuTypePp);
    }

    private void OnSearchGUI(Rect rect)
    {
        rect.y += lineHeight;
        var current = Event.current;
        var eventType = current.type;
        autocompleteSearchField.DoSearchField(rect, true);
        current.type = eventType;

        rect.y += lineHeight;
        elementRect = autocompleteSearchField.DoResults(rect);
        elementRect.y -= lineHeight;
    }

    private void OnSelectGUI(Rect rect)
    {
        rect.height = lineHeight;
        rect.width -= margin;
        float halfMargin = margin * 0.5f;
        rect.x += halfMargin;
        GUIStyle boxStyle = new GUIStyle();
        boxStyle.normal.background = MakeTex(2, 2, new Color(.5f, .5f, .5f, 1f));

        foreach (var groupItem in groupMenuNames)
        {
            rect.y += lineHeight;
            var boxRect = rect;
            boxRect.x -= halfMargin;

            boxRect.height = (groupItem.Value.Count + 1) * lineHeight + halfMargin;
            boxRect.width += margin;

            string groupName = groupItem.Key;
            bool isOnlyOneItem = groupItem.Value.Count == 1;
            string prefix = null;
            if (isOnlyOneItem)
            {
                groupName = "";
                prefix = groupItem.Key;
                boxRect.height -= lineHeight - halfMargin;
                rect.y -= lineHeight - halfMargin;
            }

            GUI.Box(boxRect, groupName, boxStyle);

            foreach (var item in groupItem.Value)
            {
                if (ShowItemButton(item, ref rect, prefix))
                    return;
            }

            rect.y += groupInterval;
        }
        uiHeight = rect.y;

        Texture2D MakeTex(int width, int height, Color col)
        {
            Color[] pix = new Color[width * height];
            for (int i = 0; i < pix.Length; ++i)
            {
                pix[i] = col;
            }
            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }
    }
    bool ShowItemButton(Tuple<string, UIButtonType> item, ref Rect rect, string prefix = null)
    {
        rect.y += lineHeight;
        string buttonDisplayName = prefix += item.Item1;
        if (GUI.Button(rect, buttonDisplayName))
        {
            //Debug.Log($"{item.Item2} Click");
            mode = ViewMode.DropDownUI;
            uiHeight = 0;
            Select(item.Item2);
            return true;
        }
        return false;
    }
    Dictionary<string, List<Tuple<string, UIButtonType>>> groupMenuNames = new Dictionary<string, List<Tuple<string, UIButtonType>>>();


    void OnInputChanged(string searchString)
    {
        autocompleteSearchField.ClearResults();
        if (!string.IsNullOrEmpty(searchString))
        {
            var lowSearchString = searchString.ToLower();
            foreach(var item in menuNames)
            {
                if(item.Key.IndexOf(lowSearchString) >=0)
                    autocompleteSearchField.AddResult(item.Value.ToString());
            }
        }
    }

    void OnConfirm(string result)
    {
        UIButtonType selected = Enum.Parse<UIButtonType>(result);
        Select(selected);
    }

    void Select(UIButtonType selected)
    {
        menuType.enumValueIndex = (int)selected;
        autocompleteSearchField.ClearResults();
        mode = ViewMode.DropDownUI;
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float defaultHeight = EditorGUI.GetPropertyHeight(property, label, true);
        switch (mode)
        {
            case ViewMode.DropDownUI:
                return defaultHeight;
            case ViewMode.SelectUI:
                return defaultHeight + uiHeight;
            case ViewMode.SearchUI:
                return defaultHeight + elementRect.y;
        }

        return defaultHeight;
    }
}