using RosettaUI.Builder;
using RosettaUI.Reactive;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit
{
    public class UIToolkitBuilder : BuildFramework<VisualElement>
    {
        readonly Dictionary<Type, Func<Element, VisualElement>> buildFuncs;
        protected override IReadOnlyDictionary<Type, Func<Element, VisualElement>> buildFuncTable => buildFuncs;


        public UIToolkitBuilder()
        {
            buildFuncs = new Dictionary<Type, Func<Element, VisualElement>>()
            {
                [typeof(WindowElement)] = Build_Window,
                /*
                [typeof(Panel)] = (e) => Build_ElementGroup(e, resource.panel),
                [typeof(Row)] = Build_Row,
                [typeof(Column)] = Build_Column,
                */
                [typeof(LabelElement)] = Build_Label,
                [typeof(IntFieldElement)] = Build_Field<int, IntegerField>,
                [typeof(FloatFieldElement)] = Build_Field<float, FloatField>,
                [typeof(StringFieldElement)] = Build_Field<string, TextField>,
                [typeof(BoolFieldElement)] = Build_Field<bool, Toggle>,
                /*
                [typeof(ButtonElement)] = Build_Button,
                [typeof(Dropdown)] = Build_Dropdown,
                [typeof(IntSlider)] = Build_IntSlider,
                [typeof(FloatSlider)] = Build_FloatSlider,
                [typeof(LogSlider)] = Build_LogSlider,
                */
                [typeof(FoldElement)] = Build_Fold,
                /*
                [typeof(DynamicElement)] = (e) => Build_ElementGroup(e, null, true, (go) => AddLayoutGroup<HorizontalLayoutGroup>(go))
                */

            };
        }


        protected override void Initialize(VisualElement ve, Element element)
        {
            element.enableRx.Subscribe((enable) => ve.style.display = enable ? DisplayStyle.Flex : DisplayStyle.None);
            element.interactableRx.Subscribe((interactable) => ve.SetEnabled(interactable));
        }


        VisualElement Build_Window(Element element)
        {
            var windowElement = (WindowElement)element;
            var window = new Window();
            window.closeButton.clicked += () => windowElement.enable = !windowElement.enable;

            return Build_ElementGroup(window, element);
        }

        VisualElement Build_ElementGroup(VisualElement container, Element element)
        {
            var elementGroup = (ElementGroup)element;

            foreach (var e in elementGroup.Elements)
            {
                container.Add(Build(e));
            }

            return container;
        }


        static VisualElement Build_Label(Element element)
        {
            var labelElement = (LabelElement)element;
            var label = new Label(labelElement.GetInitialValue());
            SetupLabelCallback(label, labelElement);

            return label;
        }

        static void SetupLabelCallback(Label label, LabelElement labelElement)
        {
            if (!labelElement.IsConst)
            {
                labelElement.setValueToView += (text) => label.text = text;
            }

        }

        VisualElement Build_Fold(Element element)
        {
            var foldElement = (FoldElement)element;
            var fold = new Foldout();

            var title = foldElement.title;
            fold.text = title.GetInitialValue();

            foreach (var content in foldElement.Contents)
            {
                fold.Add(Build(content));
            }


            foldElement.isOpenRx.Subscribe((isOpen) => fold.value = isOpen);

            return fold;
        }

        static VisualElement Build_Field<T, TField>(Element element)
            where TField : BaseField<T>, new()
        {
            var fieldElement = (FieldBaseElement<T>)element;
            var field = new TField();
            field.label = fieldElement.label.GetInitialValue();
            SetupLabelCallback(field.labelElement, fieldElement.label);
            

            fieldElement.setValueToView += (v) => field.value = v;
            field.RegisterValueChangedCallback(ev => fieldElement.OnViewValueChanged(ev.newValue));
            return field;

        }


    }
}