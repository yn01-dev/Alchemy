using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

namespace Alchemy.Editor.Elements
{
    public abstract class HashMapFieldBase : VisualElement
    {
        public HashMapFieldBase(object collection, string label)
        {
            styleSheets.Add(Resources.Load<StyleSheet>("Elements/HashMapFieldBase-Styles"));
            
            this.collection = collection;

            var foldout = new Foldout { text = label };
            foldout.AddToClassList("alchemy-hash_map_base-foldout");
            Add(foldout);
            
            contents = new() { name = "contents" };
            contents.AddToClassList("alchemy-hash_map_base-contents");
            foldout.Add(contents);

            inputForm = new() { name = "input-form" };
            foldout.Add(inputForm);

            addButton = new Button(() =>
            {
                if (isInputting) EndInput();
                else StartInput();
            })
            { text = "+ Add" };
            
            addButton.AddToClassList("alchemy-hash_map_base-add_button");
            
            foldout.Add(addButton);

            Rebuild();
        }

        readonly VisualElement inputForm;
        readonly VisualElement contents;
        readonly Button addButton;

        public object Collection => collection;
        readonly object collection;

        bool isInputting;

        public event Action<object> OnValueChanged;

        public void RegisterOnValueChangedCallback(Action<object> callback)
        {
            OnValueChanged += callback;
        }

        void StartInput()
        {
            void ValidateValue(object x)
            {
                var contains = CheckElement(x);
                addButton.SetEnabled(!contains);
                addButton.text = contains ? "(Invalid key)" : "Done";
            }

            if (isInputting) return;
            isInputting = true;

            var initValue = CreateElement();
            var form = CreateItem(collection, initValue, "New Value");
            inputForm.Clear();
            inputForm.Add(form);
            form.OnValueChanged += ValidateValue;
            form.OnClose += () =>
            {
                CancelInput();
            };

            ValidateValue(initValue);
        }

        void EndInput()
        {
            if (!isInputting) return;
            isInputting = false;

            addButton.text = "+ Add";
            addButton.SetEnabled(true);
            AddElement(inputForm.Q<HashMapItemBase>().Value);
            OnValueChanged?.Invoke(collection);
            Rebuild();
        }

        void CancelInput()
        {
            isInputting = false;

            addButton.text = "+ Add";
            addButton.SetEnabled(true);
            Rebuild();
        }

        // Rebuild GUI contents
        public void Rebuild()
        {
            contents.Clear();
            inputForm.Clear();

            if (collection == null) return;

            var i = 0;
            foreach (var item in (IEnumerable)collection)
            {
                var element = CreateItem(collection, item, "Element " + i);
                element.OnClose += () =>
                {
                    if (isInputting) return;
                    var remove = RemoveElement(item);
                    if (remove)
                    {
                        OnValueChanged?.Invoke(collection);
                        Rebuild();
                    }
                };
                element.Lock();

                contents.Add(element);
                i++;
            }

            if (i == 0)
            {
                var box = new Box();
                var label = new Label(CollectionTypeName + " is empty.");
              
                box.Add(label);

                inputForm.Add(box);
            }
        }

        public abstract HashMapItemBase CreateItem(object collection, object elementObj, string label);
        public abstract bool CheckElement(object element);
        public abstract object CreateElement();
        public abstract void AddElement(object element);
        public abstract bool RemoveElement(object element);
        public abstract void ClearElements();
        public abstract string CollectionTypeName { get; }

        public abstract class HashMapItemBase : VisualElement
        {
            public Action OnClose;
            public Action<object> OnValueChanged;

            public abstract object Value { get; }
            public abstract void Lock();
        }
    }
}