﻿#if !UNITY_2022_2_OR_NEWER
using RosettaUI.UIToolkit.UnityInternalAccess;
#endif
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit.Builder
{
    public partial class UIToolkitBuilder
    {
        private bool Bind_Dropdown(Element element, VisualElement visualElement)
        {
            if (element is not DropdownElement dropdownElement ||
                visualElement is not PopupField<string> popupField) return false;

            var options = dropdownElement.options;
            popupField.choices = options;
            Bind_FieldLabel(dropdownElement, popupField);

            dropdownElement.Bind(popupField,
                elementValueToFieldValue: i => (0 <= i && i < options.Count) ? options[i] : default,
                fieldValueToElementValue: str => options.IndexOf(str)
            );

            return true;
        }
    }
}