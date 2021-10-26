﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace RosettaUI
{
    /// <summary>
    ///     Top level interface
    /// </summary>
    public static class UI
    {
        #region Button

        public static ButtonElement Button(LabelElement label, Action onClick)
        {
            return new ButtonElement(label?.getter, onClick);
        }

        #endregion


        #region Label

        public static LabelElement Label(LabelElement label) => label;
        public static LabelElement Label(Func<string> readLabel) => readLabel;

        #endregion


        #region Field

        public static Element Field<T>(Expression<Func<T>> targetExpression, Action<T> onValueChanged = null)
        {
            return Field(ExpressionUtility.CreateLabelString(targetExpression), targetExpression, onValueChanged);
        }

        public static Element Field<T>(LabelElement label, Expression<Func<T>> targetExpression, Action<T> onValueChanged = null)
        {
            var binder = CreateBinder(targetExpression, onValueChanged);
            return Field(label, binder);
        }

        public static Element Field(LabelElement label, IBinder binder)
        {
            var element = BinderToElement.CreateFieldElement(label, binder);
            if (element != null) SetInteractableWithBinder(element, binder);

            return element;
        }

        #endregion


        #region Slider

        public static Element Slider<T>(Expression<Func<T>> targetExpression, T max, Action<T> onValueChanged = null)
        {
            return Slider(targetExpression, default, max, onValueChanged);
        }


        public static Element Slider<T>(Expression<Func<T>> targetExpression, T min, T max,
            Action<T> onValueChanged = null)
        {
            return Slider(ExpressionUtility.CreateLabelString(targetExpression),
                targetExpression,
                ConstGetter.Create(min),
                ConstGetter.Create(max),
                onValueChanged);
        }

        public static Element Slider<T>(Expression<Func<T>> targetExpression, Action<T> onValueChanged = null)
        {
            return Slider(ExpressionUtility.CreateLabelString(targetExpression),
                targetExpression,
                null,
                null,
                onValueChanged);
        }


        public static Element Slider<T>(LabelElement label, Expression<Func<T>> targetExpression, T max,
            Action<T> onValueChanged = null)
        {
            return Slider(label, targetExpression, default, max, onValueChanged);
        }

        public static Element Slider<T>(LabelElement label, Expression<Func<T>> targetExpression, T min, T max,
            Action<T> onValueChanged = null)
        {
            return Slider(label,
                targetExpression,
                ConstGetter.Create(min),
                ConstGetter.Create(max),
                onValueChanged);
        }

        public static Element Slider<T>(LabelElement label, Expression<Func<T>> targetExpression,
            Action<T> onValueChanged = null)
        {
            return Slider(label, targetExpression, null, null, onValueChanged);
        }

        public static Element Slider<T>(LabelElement label,
            Expression<Func<T>> targetExpression,
            IGetter minGetter,
            IGetter maxGetter,
            Action<T> onValueChanged = null)
        {
            var binder = CreateBinder(targetExpression, onValueChanged);
            if (minGetter == null || maxGetter == null)
            {
                var (rangeMinGetter, rangeMaxGetter) = CreateMinMaxGetterFromRangeAttribute(targetExpression);
                minGetter ??= rangeMinGetter;
                maxGetter ??= rangeMaxGetter;
            }

            return Slider(label, binder, minGetter, maxGetter);
        }

        public static Element Slider(LabelElement label, IBinder binder, IGetter minGetter, IGetter maxGetter)
        {
            var contents = BinderToElement.CreateSliderElement(label, binder, minGetter, maxGetter);
            if (contents == null) return null;

            SetInteractableWithBinder(contents, binder);

            return contents;
        }

        #endregion


        #region MinMax Slider

        public static Element MinMaxSlider<T>(Expression<Func<MinMax<T>>> targetExpression, T max,
            Action<MinMax<T>> onValueChanged = null)
        {
            return MinMaxSlider(targetExpression, default, max, onValueChanged);
        }


        public static Element MinMaxSlider<T>(Expression<Func<MinMax<T>>> targetExpression, T min, T max,
            Action<MinMax<T>> onValueChanged = null)
        {
            return MinMaxSlider(ExpressionUtility.CreateLabelString(targetExpression), targetExpression,
                ConstGetter.Create(min), ConstGetter.Create(max), onValueChanged);
        }

        public static Element MinMaxSlider<T>(Expression<Func<MinMax<T>>> targetExpression,
            Action<MinMax<T>> onValueChanged = null)
        {
            return MinMaxSlider(ExpressionUtility.CreateLabelString(targetExpression), targetExpression, null, null,
                onValueChanged);
        }

        public static Element MinMaxSlider<T>(LabelElement label, Expression<Func<MinMax<T>>> targetExpression, T max,
            Action<MinMax<T>> onValueChanged = null)
        {
            return MinMaxSlider(label, targetExpression, default, max, onValueChanged);
        }

        public static Element MinMaxSlider<T>(LabelElement label, Expression<Func<MinMax<T>>> targetExpression, T min,
            T max, Action<MinMax<T>> onValueChanged = null)
        {
            return MinMaxSlider(label, targetExpression, ConstGetter.Create(min), ConstGetter.Create(max),
                onValueChanged);
        }

        public static Element MinMaxSlider<T>(LabelElement label, Expression<Func<MinMax<T>>> targetExpression,
            Action<MinMax<T>> onValueChanged = null)
        {
            return MinMaxSlider(label, targetExpression, null, null, onValueChanged);
        }

        public static Element MinMaxSlider<T>(LabelElement label,
            Expression<Func<MinMax<T>>> targetExpression,
            IGetter<T> minGetter,
            IGetter<T> maxGetter,
            Action<MinMax<T>> onValueChanged = null)
        {
            var binder = CreateBinder(targetExpression, onValueChanged);
            return MinMaxSlider(label, binder, minGetter, maxGetter);
        }

        public static Element MinMaxSlider(LabelElement label, IBinder binder, IGetter minGetter, IGetter maxGetter)
        {
            var contents = BinderToElement.CreateMinMaxSliderElement(label, binder, minGetter, maxGetter);
            if (contents == null) return null;

            SetInteractableWithBinder(contents, binder);

            return contents;
        }

        #endregion


        #region List

        public static Element List<TItem, TValue>(LabelElement label, List<TItem> list,
            Func<TItem, TValue> readItemValue, Action<TItem, TValue> onItemValueChanged,
            Func<TItem, string> createItemLabel = null)
            where TItem : class
        {
            return List(label,
                list,
                (binder, defaultLabelString) =>
                {
                    var childBinder = new ChildBinder<TItem, TValue>(binder,
                        readItemValue,
                        (item, value) =>
                        {
                            onItemValueChanged?.Invoke(item, value);
                            return item;
                        }
                    );

                    var itemLabel = createItemLabel != null && !binder.IsNull
                        ? Label(() => createItemLabel(binder.Get()))
                        : (LabelElement) defaultLabelString;

                    return Field(itemLabel, childBinder);
                }
            );
        }

        public static Element List<T>(LabelElement label, List<T> list,
            Func<BinderBase<T>, string, Element> createItemElement = null)
        {
            Func<IBinder, string, Element> createItemElementIBinder = null;
            if (createItemElement != null)
                createItemElementIBinder =
                    (ibinder, itemLabel) => createItemElement(ibinder as BinderBase<T>, itemLabel);

            var element = BinderToElement.CreateListElement(label, ConstGetter.Create(list), createItemElementIBinder);
            return Fold(label, element);
        }

        #endregion


        #region Dropdown

        public static Element Dropdown(Expression<Func<int>> targetExpression, IEnumerable<string> options,
            Action<int> onValueChanged = null)
        {
            return Dropdown(ExpressionUtility.CreateLabelString(targetExpression), targetExpression, options,
                onValueChanged);
        }

        public static Element Dropdown(LabelElement label, Expression<Func<int>> targetExpression,
            IEnumerable<string> options, Action<int> onValueChanged = null)
        {
            var binder = CreateBinder(targetExpression, onValueChanged);

            Element element = new DropdownElement(label, binder, options);

            SetInteractableWithBinder(element, binder);

            return element;
        }

        #endregion


        #region Row/Column/Box/ScrollView

        public static Row Row(params Element[] elements)
        {
            return new Row(elements);
        }

        public static Row Row(IEnumerable<Element> elements)
        {
            return new Row(elements);
        }

        public static Column Column(params Element[] elements)
        {
            return new Column(elements);
        }

        public static Column Column(IEnumerable<Element> elements)
        {
            return new Column(elements);
        }

        public static BoxElement Box(params Element[] elements)
        {
            return new BoxElement(elements);
        }

        public static BoxElement Box(IEnumerable<Element> elements)
        {
            return new BoxElement(elements);
        }

        public static ScrollViewElement ScrollView(params Element[] elements)
        {
            return new ScrollViewElement(elements);
        }

        public static ScrollViewElement ScrollView(IEnumerable<Element> elements)
        {
            return new ScrollViewElement(elements);
        }

        #endregion


        #region Fold

        public static FoldElement Fold(LabelElement label, params Element[] elements)
        {
            return Fold(label, elements as IEnumerable<Element>);
        }

        public static FoldElement Fold(LabelElement label, IEnumerable<Element> elements)
        {
            return new FoldElement(label, elements);
        }

        #endregion


        #region DynamicElement

        public static DynamicElement DynamicElementIf(Func<bool> trigger, Func<Element> build)
        {
            return DynamicElementOnStatusChanged(
                trigger,
                (t) => !t ? null : build()
            );
        }

        public static DynamicElement DynamicElementOnStatusChanged<T>(Func<T> readStatus, Func<T, Element> build)
            where T : IEquatable<T>
        {
            return DynamicElement.Create(readStatus, build);
        }

        public static DynamicElement DynamicElementOnTrigger(Func<DynamicElement, bool> rebuildIf, Func<Element> build)
        {
            return new DynamicElement(build, rebuildIf);
        }

        #endregion

        #region Window

        public static WindowElement Window(params Element[] elements)
        {
            return Window(null, elements);
        }

        public static WindowElement Window(LabelElement title, params Element[] elements)
        {
            return new WindowElement(title, elements);
        }

        public static WindowElement Window(LabelElement title, IEnumerable<Element> elements)
        {
            return new WindowElement(title, elements);
        }

        #endregion


        #region Window Launcher

        public static WindowLauncherElement WindowLauncher(WindowElement window)
        {
            return WindowLauncher(null, window);
        }

        public static WindowLauncherElement WindowLauncher(LabelElement title, WindowElement window)
        {
            var label = title ?? window.title;
            return new WindowLauncherElement(label, window);
        }

        #endregion


        #region ElementCreator

        public static DynamicElement ElementCreatorWindowLauncher<T>(LabelElement title = null)
            where T : Behaviour, IElementCreator
        {
            return ElementCreatorWindowLauncher(title, typeof(T));
        }

        public static DynamicElement ElementCreatorWindowLauncher(params Type[] types) =>
            ElementCreatorWindowLauncher(null, types);

        public static DynamicElement ElementCreatorWindowLauncher(LabelElement title, params Type[] types)
        {
            Assert.IsTrue(types.Any());

            var elements = types.Select(ElementCreatorInline).ToList();
            title ??= types.First().ToString().Split('.').LastOrDefault();

            return DynamicElementIf(
                trigger: () =>
                {
                    var hasContents = elements.Any(dynamicElement => dynamicElement.Contents.Any());
                    if (!hasContents)
                    {
                        foreach (var e in elements)
                        {
                            e.Update();
                        }
                    }

                    return hasContents;
                },
                build: () => WindowLauncher(Window(title, elements))
            );
        }


        public static DynamicElement ElementCreatorInline<T>()
            where T : Behaviour, IElementCreator
        {
            return ElementCreatorInline(typeof(T));
        }

        public static DynamicElement ElementCreatorInline(Type type)
        {
            Assert.IsTrue(typeof(IElementCreator).IsAssignableFrom(type));

            return DynamicElementFindObject(type, t => ((IElementCreator) t).CreateElement());
        }

        public static DynamicElement DynamicElementFindObject<T>(Func<T, Element> build)
            where T : Behaviour
        {
            return DynamicElementFindObject(typeof(T), (o) => build?.Invoke((T) o));
        }

        public static DynamicElement DynamicElementFindObject(Type type, Func<Object, Element> build)
        {
            Assert.IsTrue(typeof(Object).IsAssignableFrom(type));

            Object target = null;
            var lastCheckTime = Time.realtimeSinceStartup;
            // 起動時に多くのFindObjectObserverElementが呼ばれるとFindObject()を呼ぶタイミングがかぶって重いのでランダムで散らす
            var interval = Random.Range(1f, 1.5f);

            return DynamicElementIf(
                trigger: () =>
                {
                    if (target == null)
                    {
                        var t = Time.realtimeSinceStartup;
                        if (t - lastCheckTime > interval)
                        {
                            lastCheckTime = t;
                            target = Object.FindObjectOfType(type);
                        }
                    }

                    return target != null;
                },
                build: () => build?.Invoke(target)
            );
        }

        #endregion


        static IBinder<T> CreateBinder<T>(Expression<Func<T>> targetExpression, Action<T> onValueChanged)
        {
            var binder = ExpressionUtility.CreateBinder(targetExpression);
            if (binder != null)
            {
                binder.onValueChanged += onValueChanged;
            }

            return binder;
        }


        static (IGetter<T>, IGetter<T>) CreateMinMaxGetterFromRangeAttribute<T>(Expression<Func<T>> targetExpression)
        {
            var rangeAttribute = typeof(IConvertible).IsAssignableFrom(typeof(T))
                ? ExpressionUtility.GetAttribute<T, RangeAttribute>(targetExpression)
                : null;

            return RangeUtility.CreateGetterMinMax<T>(rangeAttribute);
        }


        private static void SetInteractableWithBinder(Element element, IBinder binder)
        {
            element.Interactable = !binder.IsReadOnly;
        }
    }
}