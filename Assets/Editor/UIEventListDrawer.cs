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
    const int margin = 4;  // 그룹좌우 상하 좌우 여백

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
            Debug.Log(item);
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

    bool showSelectUI = false;
    float searchIconWidth = 18;
    float selectIconWidth = 18;
    public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
    {

        EditorGUI.BeginProperty(rect, label, property);


        rect.width -= (selectIconWidth + searchIconWidth);
        rect = EditorGUI.PrefixLabel(rect, GUIUtility.GetControlID(FocusType.Passive), label);

        {
            Rect selectIconRect = new Rect(rect.x + rect.width, rect.y, selectIconWidth, lineHeight);
            if (GUI.Button(selectIconRect, "S"))
            {
                Debug.Log("선택버튼 누름");
            }
        }

        var dropdownRect = rect;
        dropdownRect.height = lineHeight;
        var menuTypePp = property.FindPropertyRelative("_");
        EditorGUI.PropertyField(dropdownRect, menuTypePp);

        rect.y += lineHeight;

        Init(property);
        Rect lRect = rect;
        Rect rRect = rect;
        rRect.width = lRect.width = lRect.width * 0.5f;
        rRect.x += rRect.width;
        lRect.height = lineHeight;

        if (GUI.Button(lRect, showSelectUI ? "Close" : "Select"))
        {
            showSelectUI = !showSelectUI;
            if (showSelectUI == false)
                uiHeight = 0;
        }

        OnSelectGUI(rect);

        var current = Event.current;
        var eventType = current.type;
        autocompleteSearchField.DoSearchField(rRect, true);
        current.type = eventType;

        rRect.y += lineHeight;
        elementRect = autocompleteSearchField.DoResults(rRect);
        elementRect.y -= lineHeight;
        EditorGUI.EndProperty();
    }

    private void OnSelectGUI(Rect rect)
    {
        if (showSelectUI == false)
        {
            return;
        }

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

            //그룹 요소가 한개인 경우 이름을 그룹이름을 같이 표시하자.
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

            rect.y += groupInterval; // 그룹간의 간격
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
            Debug.Log($"{item.Item2} Click");
            showSelectUI = false;
            uiHeight = 0;
            Select(item.Item2);
            return true;
        }
        return false;
    }
    Dictionary<string, List<Tuple<string, UIButtonType>>> groupMenuNames = new Dictionary<string, List<Tuple<string, UIButtonType>>>();


    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return elementRect.y + uiHeight;
    }

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
    }
}