// © 2026 Jong-il Hong
// Licensed under the MIT License. See LICENSE in the project root for license information.
//
// SPDX-License-Identifier: MIT

#nullable enable

using Jih.OpenGames.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Jih.OpenGames.UI
{
    /// <summary>
    /// Manages <see cref="IUILayer"/>s with stack structure.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The stack structure is consists with three parts:
    /// <list type="number">
    /// <item>Top-Most Layer</item>
    /// <item>Layers Stack</item>
    /// <item>Bottom-Least Layer</item>
    /// </list>
    /// The current layer(or very-top layer) is determined by the top-most layer if it exists, otherwise the top layer in stack if it exists, otherwise the bottom-least layer if it exists, otherwise null.<br/>
    /// The current layer will be activated, and the others will be deactivated. When pushing or popping layers, the activation and deactivation will be handled automatically.<br/>
    /// Also supports focusing object in the layer.
    /// </para>
    /// <para>
    /// This class also handles order(sibling index) of the layer's GameObjects in the <see cref="ContainerTransform"/>.<br/>
    /// This class will attach GameObject of the layer to directly under the <see cref="ContainerTransform"/>.<br/>
    /// This class getting the layer's GameObject from the <see cref="IUILayer.GetRootCanvasGroup"/>.<br/>
    /// <br/>
    /// But when the layer removed from the stack structure, this class will call <see cref="IUILayer.Dettach"/> and <b>do nothing</b>.<br/>
    /// Because if dettach the GameObject, RectTransform's information will be lost. So the layer should handle the dettach mechanism by itself.<br/>
    /// Set GameObject.activeSelf or move the GameObject to other backing Canvas may be solution.<br/>
    /// </para>
    /// <para>
    /// This class does not handle the lifecycle of the layers. You need to create and destroy the layers by yourself.<br/>
    /// Also does not handle active state of the layer's GameObject.
    /// </para>
    /// <para>
    /// <see cref="IUILayerComponent"/>s in <see cref="IUILayer"/> can receive the activation and deactivation events of the item.
    /// </para>
    /// <para>
    /// You can get a <see cref="UnityEngine.EventSystems.EventSystem"/> by adding a Canvas to the scene. Unity will create one to the scene.
    /// </para>
    /// </remarks>
    public class UILayerStack
    {
        public Transform ContainerTransform { get; }

        /// <summary>
        /// It requires to handle focus.
        /// </summary>
        public EventSystem EventSystem { get; }

        LayerItem? topMostItem, bottomLeastItem;

        readonly Stack<LayerItem> items = new();

        readonly LayerItemPool _layerItemPool = new();

        public UILayerStack(Transform containerTransform, EventSystem eventSystem)
        {
            ContainerTransform = containerTransform;
            EventSystem = eventSystem;
        }

        /// <summary>
        /// Set or clear the top-most layer.
        /// </summary>
        /// <param name="layer">If <c>null</c>, clear the top-most layer.</param>
        public void SetTopMost(IUILayer? layer)
        {
            if (topMostItem is not null && topMostItem.Same(layer))
            {
                return;
            }

            if (topMostItem is not null)
            {
                Deactivate(topMostItem);
                Detach(topMostItem);

                _layerItemPool.Release(topMostItem);
                topMostItem = null;
            }

            if (layer is not null)
            {
                topMostItem = _layerItemPool.Get();
                topMostItem.Layer = layer;
                topMostItem.IsActive = false;
                SetInputActive(layer, false);

                Attach(topMostItem);
            }

            UpdateItemsActive();
            Sort();
        }

        /// <summary>
        /// Set or clear the bottom-least layer.
        /// </summary>
        /// <param name="layer">If <c>null</c>, clear the bottom-least layer.</param>
        public void SetBottomLeast(IUILayer? layer)
        {
            if (bottomLeastItem is not null && bottomLeastItem.Same(layer))
            {
                return;
            }

            if (bottomLeastItem is not null)
            {
                Deactivate(bottomLeastItem);
                Detach(bottomLeastItem);

                _layerItemPool.Release(bottomLeastItem);
                bottomLeastItem = null;
            }

            if (layer is not null)
            {
                bottomLeastItem = _layerItemPool.Get();
                bottomLeastItem.Layer = layer;
                bottomLeastItem.IsActive = false;
                SetInputActive(layer, false);

                Attach(bottomLeastItem);
            }

            UpdateItemsActive();
            Sort();
        }

        /// <summary>
        /// Push an layer to the layers stack.
        /// </summary>
        /// <param name="layer">Layer to push.</param>
        public void Push(IUILayer layer)
        {
            LayerItem item = _layerItemPool.Get();
            item.Layer = layer;
            item.IsActive = false;
            SetInputActive(layer, false);

            items.Push(item);
            Attach(item);

            UpdateItemsActive();
            Sort();
        }

        /// <summary>
        /// Pop an layer in the layers stack.
        /// </summary>
        /// <param name="layer">Layer to pop.</param>
        /// <exception cref="InvalidOperationException">Throws if current top layer in stack is not the <paramref name="layer"/>.</exception>
        public void Pop(IUILayer layer)
        {
            if (!items.TryPeek(out LayerItem item) || !item.Same(layer))
            {
                throw new InvalidOperationException("Popping UI invalid order.");
            }

            items.Pop();
            Deactivate(item);
            Detach(item);

            _layerItemPool.Release(item);

            UpdateItemsActive();
            Sort();
        }

        /// <summary>
        /// Pops all items until the target layer is on top. If the target layer is null, pops all items.
        /// </summary>
        /// <remarks>
        /// This method does not effect the top-most and the bottom-least layers.<br/>
        /// If the target layer is not in layers stack, pops all layers.
        /// </remarks>
        public void PopAll(IUILayer? targetLayer)
        {
            while (items.TryPeek(out LayerItem lastItem))
            {
                if (targetLayer is not null && lastItem.Same(targetLayer))
                {
                    break;
                }

                items.Pop();
                Deactivate(lastItem);
                Detach(lastItem);

                _layerItemPool.Release(lastItem);
            }

            UpdateItemsActive();
            Sort();
        }

        public void PerformAction(string id, object? args)
        {
            GetCurrent()?.PerformAction(id, args);
        }

        /// <summary>
        /// Gets the very-top layer. If there is no layer, returns <c>null</c>.
        /// </summary>
        /// <remarks>
        /// This method considers the top-most layer, the layers stack and the bottom-least layer.
        /// </remarks>
        public IUILayer? GetCurrent()
        {
            return GetCurrentItem()?.Layer;
        }
        LayerItem? GetCurrentItem()
        {
            return EnumerateAllItems().FirstOrDefault();
        }

        /// <summary>
        /// Enumerates all layers in stack structure order.
        /// </summary>
        /// <remarks>
        /// The top-most layer will be enumerated first, and the bottom-least layer will be enumerated last.<br/>
        /// The items in the layers stack will be enumerated in the order of the stack.
        /// </remarks>
        /// <returns></returns>
        public IEnumerable<IUILayer> EnumerateAllLayers()
        {
            return EnumerateAllItems().Select(item => item.Layer);
        }
        IEnumerable<LayerItem> EnumerateAllItems()
        {
            if (topMostItem is not null)
            {
                yield return topMostItem;
            }
            foreach (var item in items)
            {
                yield return item;
            }
            if (bottomLeastItem is not null)
            {
                yield return bottomLeastItem;
            }
        }

        void UpdateItemsActive()
        {
            bool active = true;
            foreach (var item in EnumerateAllItems())
            {
                if (active)
                {
                    Activate(item);
                    active = false;
                }
                else
                {
                    Deactivate(item);
                }
            }
        }
        void Activate(LayerItem? item)
        {
            if (item is null)
            {
                return;
            }
            if (item.IsActive)
            {
                return;
            }

            try
            {
                IUILayer layer = item.Layer;

                layer.OnActivating();

                foreach (var component in IUILayer.EnumerateLayerComponents(layer))
                {
                    component.OnActivating();
                }

                SetInputActive(layer, true);

                foreach (var component in IUILayer.EnumerateLayerComponents(layer))
                {
                    component.OnActivated();
                }

                layer.OnActivated();

                layer.OnObjectFocusing();

                GameObject? focusObject = layer.GetFocusedObject();
                EventSystem.SetSelectedGameObject(focusObject);

                layer.OnObjectFocused(focusObject);
            }
            finally
            {
                item.IsActive = true;
            }
        }
        void Deactivate(LayerItem? item)
        {
            if (item is null)
            {
                return;
            }
            if (!item.IsActive)
            {
                return;
            }

            try
            {
                IUILayer layer = item.Layer;

                layer.OnDeactivating();

                foreach (var component in IUILayer.EnumerateLayerComponents(layer))
                {
                    component.OnDeactivating();
                }

                SetInputActive(layer, false);

                foreach (var component in IUILayer.EnumerateLayerComponents(layer))
                {
                    component.OnDeactivated();
                }

                layer.OnDeactivated();
            }
            finally
            {
                item.IsActive = false;
            }
        }

        void Attach(LayerItem item)
        {
            IUILayer layer = item.Layer;

            layer.OnAttaching();

            Transform layerTransform = layer.GetRootCanvasGroup().transform;
            if (layerTransform.parent != ContainerTransform)
            {
                layerTransform.SetParent(ContainerTransform, worldPositionStays: false);
            }

            layer.OnAttached();
        }
        void Detach(LayerItem item)
        {
            IUILayer layer = item.Layer;

            layer.Dettach();
        }

        void Sort()
        {
            static void Consume(ref int i, LayerItem? item)
            {
                if (item is null)
                {
                    return;
                }

                int targetIndex = --i;

                Transform transform = item.Layer.GetRootCanvasGroup().transform;
                if (transform.GetSiblingIndex() != targetIndex)
                {
                    transform.SetSiblingIndex(targetIndex);
                }
            }

            int index = items.Count;
            if (topMostItem is not null)
            {
                index++;
            }
            if (bottomLeastItem is not null)
            {
                index++;
            }

            Consume(ref index, topMostItem);
            foreach (var item in items)
            {
                Consume(ref index, item);
            }
            Consume(ref index, bottomLeastItem);
        }

        void SetInputActive(IUILayer layer, bool active)
        {
            CanvasGroup canvasGroup = layer.GetRootCanvasGroup();
            canvasGroup.interactable = active;
            canvasGroup.blocksRaycasts = active;
        }

        class LayerItem
        {
            public IUILayer Layer { get; set; } = null!;
            public bool IsActive { get; set; }

            public bool Same(IUILayer? layer)
            {
                return Layer == layer;
            }
        }

        class LayerItemPool : ObjectPool<LayerItem>
        {
            protected override void Activate(LayerItem obj)
            {
                obj.Layer = null!;
                obj.IsActive = false;
            }

            protected override void Deactivate(LayerItem obj)
            {
                obj.Layer = null!;
                obj.IsActive = false;
            }
        }
    }
}
