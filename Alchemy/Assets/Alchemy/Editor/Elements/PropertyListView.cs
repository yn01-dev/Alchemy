using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UIElements;
using Alchemy.Inspector;

namespace Alchemy.Editor.Elements
{
    /// <summary>
    /// Visual Element that draws SerializedProperty of Array or List
    /// </summary>
    public sealed class PropertyListView : BindableElement
    {
        public PropertyListView(SerializedProperty property)
        {
            Assert.IsTrue(property.isArray);

            var parentObj = property.GetDeclaredObject();
            var events = property.GetAttribute<OnListViewChangedAttribute>(true);
#if UNITY_6000_0_OR_NEWER
            VisualElement lastFocusedElement = null;
#endif

            listView = GUIHelper.CreateListViewFromFieldInfo(parentObj, property.GetFieldInfo());
            listView.headerTitle = ObjectNames.NicifyVariableName(property.displayName);
            listView.bindItem = (element, index) =>
            {
                var arrayElement = property.GetArrayElementAtIndex(index);
                var e = new AlchemyPropertyField(arrayElement, property.GetPropertyType(true), true);
                element.Add(e);
                element.Bind(arrayElement.serializedObject);
#if UNITY_6000_0_OR_NEWER
                var field = lastFocusedElement?.Q<TextField>();
                if (field != null)
                {
                    field.Focus();
                    field.SelectRange(field.value.Length, field.value.Length);
                    using var evt = KeyboardEventBase<KeyDownEvent>.GetPooled('\0', KeyCode.RightArrow, EventModifiers.FunctionKey);
                    field.SendEvent(evt);
                }
#endif
                
                if (events != null)
                {
                    e.TrackPropertyValue(arrayElement, x =>
                    {
                        ReflectionHelper.Invoke(parentObj, events.OnItemChanged,
                            new object[] { index, x.GetValue<object>() });
                    });
                }
            };
            listView.unbindItem = (element, index) =>
            {
#if UNITY_6000_0_OR_NEWER
                lastFocusedElement = element;
#endif
                element.Clear();
                element.Unbind();
            };

            var label = listView.Q<Label>();
            if (label != null) label.style.unityFontStyleAndWeight = FontStyle.Bold;

            listView.BindProperty(property);
            Add(listView);
        }

        readonly ListView listView;

        public string Label
        {
            get => listView.headerTitle;
            set => listView.headerTitle = value;
        }
    }
}