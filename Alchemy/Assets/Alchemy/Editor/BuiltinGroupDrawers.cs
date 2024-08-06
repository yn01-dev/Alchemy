using System;
using System.Collections.Generic;
using System.Linq;
using Alchemy.Editor.Elements;
using Alchemy.Inspector;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Alchemy.Editor.Drawers
{
    [CustomGroupDrawer(typeof(GroupAttribute))]
    public sealed class GroupDrawer : AlchemyGroupDrawer
    {
        private readonly StyleSheet _styleSheet = Resources.Load<StyleSheet>("Elements/GroupDrawer-Styles");

        public override VisualElement CreateRootElement(string label)
        {
            Box box = new();
            box.styleSheets.Add(_styleSheet);
            box.AddToClassList("alchemy-group_attribute-group_drawer-box");
            return box;
        }
    }

    [CustomGroupDrawer(typeof(BoxGroupAttribute))]
    public sealed class BoxGroupDrawer : AlchemyGroupDrawer
    {
        private readonly StyleSheet _styleSheet = Resources.Load<StyleSheet>("Elements/BoxGroupDrawer-Styles");

        public override VisualElement CreateRootElement(string label)
        {
            var helpBox = new HelpBox { text = label };
            
            helpBox.styleSheets.Add(_styleSheet);
            helpBox.AddToClassList("alchemy-group_attribute-box_group_drawer-help_box");
            
            return helpBox;
        }
    }

    [CustomGroupDrawer(typeof(TabGroupAttribute))]
    public sealed class TabGroupDrawer : AlchemyGroupDrawer
    {
        private readonly StyleSheet _styleSheet = Resources.Load<StyleSheet>("Elements/TabGroupDrawer-Styles");
        
        VisualElement rootElement;
        private VisualElement pageRoot;
        readonly Dictionary<string, VisualElement> tabElements = new();

        string[] keyArrayCache = new string[0];
        int tabIndex;
        int prevTabIndex;

        sealed class TabItem
        {
            public string name;
            public VisualElement element;
        }

        public override VisualElement CreateRootElement(string label)
        {
            var configKey = UniqueId + "_TabGroup";
            int.TryParse(EditorUserSettings.GetConfigValue(configKey), out tabIndex);
            prevTabIndex = tabIndex;

            rootElement = new HelpBox();
            rootElement.Remove(rootElement.Q<Label>());

            rootElement.styleSheets.Add(_styleSheet);
            rootElement.AddToClassList("alchemy-group_attribute-tab_group_drawer-help_box");
            
            var tabGUIElement = new IMGUIContainer(() =>
            {
                var rect = EditorGUILayout.GetControlRect();
                rect.xMin -= 3.7f;
                rect.xMax += 3.7f;
                rect.yMin -= 3.7f;
                rect.yMax -= 1f;
                tabIndex = GUI.Toolbar(rect, tabIndex, keyArrayCache);
                if (tabIndex != prevTabIndex)
                {
                    EditorUserSettings.SetConfigValue(configKey, tabIndex.ToString());
                    prevTabIndex = tabIndex;
                }

                foreach (var kv in tabElements)
                {
                    kv.Value.style.display = keyArrayCache[tabIndex] == kv.Key ? DisplayStyle.Flex : DisplayStyle.None;
                }
            });
            
            rootElement.Add(tabGUIElement);

            pageRoot = new VisualElement { name = "page-root" };
            rootElement.Add(pageRoot);
            
            return rootElement;
        }

        public override VisualElement GetGroupElement(Attribute attribute)
        {
            var tabGroupAttribute = (TabGroupAttribute)attribute;

            var tabName = tabGroupAttribute.TabName;
            if (!tabElements.TryGetValue(tabName, out var element))
            {
                element = new VisualElement();
                element.AddToClassList("tab-page");
                pageRoot.Add(element);
                tabElements.Add(tabName, element);

                keyArrayCache = tabElements.Keys.ToArray();
            }

            return element;
        }
    }

    [CustomGroupDrawer(typeof(FoldoutGroupAttribute))]
    public sealed class FoldoutGroupDrawer : AlchemyGroupDrawer
    {
        private readonly StyleSheet _styleSheet = Resources.Load<StyleSheet>("Elements/FoldoutGroupDrawer-Styles");
        
        public override VisualElement CreateRootElement(string label)
        {
            var configKey = UniqueId + "_FoldoutGroup";
            bool.TryParse(EditorUserSettings.GetConfigValue(configKey), out var result);

            var foldout = new Foldout
            {
                text = label,
                value = result
            };
            
            foldout.styleSheets.Add(_styleSheet);
            foldout.AddToClassList("alchemy-group_attribute-foldout_group_drawer-foldout");

            foldout.RegisterValueChangedCallback(x =>
            {
                EditorUserSettings.SetConfigValue(configKey, x.newValue.ToString());
            });

            return foldout;
        }
    }

    [CustomGroupDrawer(typeof(HorizontalGroupAttribute))]
    public sealed class HorizontalGroupDrawer : AlchemyGroupDrawer
    {
        private readonly StyleSheet _styleSheet = Resources.Load<StyleSheet>("Elements/HorizontalGroupDrawer-Styles");
        
        public override VisualElement CreateRootElement(string label)
        {
            var root = new VisualElement();
            
            root.styleSheets.Add(_styleSheet);
            root.AddToClassList("alchemy-group_attribute-horizontal_group_drawer-visual_element");

            static void AdjustLabel(PropertyField element, VisualElement inspector, int childCount)
            {
                if (element.childCount == 0) return;
                if (element.Q<Foldout>() != null) return;

                var field = element[0];
                field.RemoveFromClassList("unity-base-field__aligned");

                var labelElement = field.Q<Label>();
                if (labelElement != null)
                    labelElement.style.width = GUIHelper.CalculateLabelWidth(element, inspector) * 0.8f / childCount;
            }

            root.schedule.Execute(() =>
            {
                if (root.childCount <= 1) return;

                var visualTree = root.panel.visualTree;

                foreach (var field in root.Query<PropertyField>().Build())
                {
                    field.schedule.Execute(() => AdjustLabel(field, visualTree, root.childCount));
                }
                foreach (var field in root.Query<GenericField>().Children<PropertyField>().Build())
                {
                    field.schedule.Execute(() => AdjustLabel(field, visualTree, root.childCount));
                }
            });

            return root;
        }
    }
    [CustomGroupDrawer(typeof(InlineGroupAttribute))]
    public sealed class InlineGroupDrawer : AlchemyGroupDrawer
    {
        public override VisualElement CreateRootElement(string label)
        {
            return new VisualElement();
        }
    }
}