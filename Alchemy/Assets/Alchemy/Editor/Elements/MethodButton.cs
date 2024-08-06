using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Alchemy.Editor.Elements
{
    public sealed class MethodButton : VisualElement
    {
        const string ButtonLabelText = "Invoke";

        public MethodButton(object target, MethodInfo methodInfo)
        {
            StyleSheet styleSheet = Resources.Load<StyleSheet>("Elements/MethodButton-Styles");

            styleSheets.Add(styleSheet);

            var parameters = methodInfo.GetParameters();

            // Create parameterless button
            if (parameters.Length == 0)
            {
                button = new Button(() => methodInfo.Invoke(target, null)) { text = methodInfo.Name };
                Add(button);
                return;
            }

            var parameterObjects = new object[parameters.Length];

            var box = new HelpBox();
            Add(box);

            foldout = new Foldout
            {
                text = methodInfo.Name,
                value = false
            };
            InternalAPIHelper.SetAcceptClicksIfDisabled(
                InternalAPIHelper.GetClickable(foldout.Q<Toggle>()), true
            );

            button = new Button(() => methodInfo.Invoke(target, parameterObjects)) { text = ButtonLabelText };
            
            box.Add(foldout);
            box.Add(button);

            for (int i = 0; i < parameters.Length; i++)
            {
                var index = i;
                var parameter = parameters[index];
                parameterObjects[index] = TypeHelper.CreateDefaultInstance(parameter.ParameterType);
                var element = new GenericField(parameterObjects[index], parameter.ParameterType, ObjectNames.NicifyVariableName(parameter.Name));
                element.OnValueChanged += x => parameterObjects[index] = x;
                foldout.Add(element);
            }
        }

        readonly Foldout foldout;
        readonly Button button;
    }
}