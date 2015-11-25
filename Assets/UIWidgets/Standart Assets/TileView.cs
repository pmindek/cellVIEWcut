using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System;
using System.Linq;

namespace UIWidgets {
	/// <summary>
	/// Tile view.
	/// </summary>
	public class TileView<TComponent,TItem> : ListViewCustom<TComponent,TItem> where TComponent : ListViewItem {
		int itemsPerRow;
		int itemsPerColumn;

		/// <summary>
		/// Scrolls to item with specifid index.
		/// </summary>
		/// <param name="index">Index.</param>
		protected override void ScrollTo(int index)
		{
			if (!CanOptimize())
			{
				return ;
			}
			
			var first_visible = GetFirstVisibleIndex(true);
			var last_visible = GetLastVisibleIndex(true);

			var block_index = Mathf.FloorToInt((float)index / (float)ItemsPerBlock());
			if (first_visible > block_index)
			{
				SetScrollValue(GetItemPosition(index));
			}
			else if (last_visible < block_index)
			{
				SetScrollValue(GetItemPositionBottom(index));
			}
		}

		/// <summary>
		/// Gets the item position.
		/// </summary>
		/// <returns>The item position.</returns>
		/// <param name="index">Index.</param>
		protected override float GetItemPosition(int index)
		{
			var block_index = Mathf.FloorToInt((float)index / (float)ItemsPerBlock());
			return block_index * GetItemSize();
		}

		/// <summary>
		/// Calculates the max count of visible items.
		/// </summary>
		protected override void CalculateMaxVisibleItems()
		{
			if (IsHorizontal())
			{
				itemsPerRow = Mathf.CeilToInt(scrollWidth / itemWidth) + 1;
				itemsPerRow = Mathf.Max(2, itemsPerRow);
				
				var height = scrollHeight + layout.Spacing.y - layout.GetMarginTop() - layout.GetMarginBottom();
				itemsPerColumn = Mathf.FloorToInt(height / (itemHeight + layout.Spacing.y));
				itemsPerColumn = Mathf.Max(1, itemsPerColumn);
			}
			else
			{
				var width = scrollWidth + layout.Spacing.x - layout.GetMarginLeft() - layout.GetMarginRight();
				itemsPerRow = Mathf.FloorToInt(width / (itemWidth + layout.Spacing.x));
				itemsPerRow = Mathf.Max(1, itemsPerRow);
				
				itemsPerColumn = Mathf.CeilToInt(scrollHeight / itemHeight) + 1;
				itemsPerColumn = Mathf.Max(2, itemsPerColumn);
			}
			
			maxVisibleItems = itemsPerRow * itemsPerColumn;
		}

		/// <summary>
		/// Gets the index of first visible item.
		/// </summary>
		/// <returns>The first visible index.</returns>
		/// <param name="strict">If set to <c>true</c> strict.</param>
		protected override int GetFirstVisibleIndex(bool strict=false)
		{
			return Mathf.Max(0, base.GetFirstVisibleIndex(strict) * ItemsPerBlock());
		}

		/// <summary>
		/// Gets the index of last visible item.
		/// </summary>
		/// <returns>The last visible index.</returns>
		/// <param name="strict">If set to <c>true</c> strict.</param>
		protected override int GetLastVisibleIndex(bool strict=false)
		{
			return (base.GetLastVisibleIndex(strict) + 1) * ItemsPerBlock() - 1;
		}

		/// <summary>
		/// Scrolls the update.
		/// </summary>
		protected override void ScrollUpdate()
		{
			var oldTopHiddenItems = topHiddenItems;
			
			topHiddenItems = GetFirstVisibleIndex();
			if (topHiddenItems > (DataSource.Count - 1))
			{
				topHiddenItems = Mathf.Max(0, DataSource.Count - 2);
			}
			if (DataSource.Count==0)
			{
				SetScrollValue(0f);
			}
			
			if (oldTopHiddenItems==topHiddenItems)
			{
				return ;
			}
			
			if ((CanOptimize()) && (DataSource.Count > 0))
			{
				visibleItems = (maxVisibleItems < DataSource.Count) ? maxVisibleItems : DataSource.Count;
			}
			else
			{
				visibleItems = DataSource.Count;
			}
			if ((topHiddenItems + visibleItems) > DataSource.Count)
			{
				visibleItems = DataSource.Count - topHiddenItems;
				if (visibleItems < ItemsPerBlock())
				{
					visibleItems = Mathf.Min(DataSource.Count, visibleItems + ItemsPerBlock());
					topHiddenItems = DataSource.Count - visibleItems;
				}
			}

			RemoveCallbacks();

			UpdateComponentsCount();

			bottomHiddenItems = Mathf.Max(0, DataSource.Count - visibleItems - topHiddenItems);

			var new_visible_range = Enumerable.Range(topHiddenItems, visibleItems).ToList();
			var current_visible_range = components.ConvertAll<int>(GetComponentIndex);

			var new_indicies_to_change = new_visible_range.Except(current_visible_range).ToList();
			var components_to_change = new Stack<TComponent>(components.Where(x => !new_visible_range.Contains(x.Index)));

			new_indicies_to_change.ForEach(index => {
				var component = components_to_change.Pop();

				component.Index = index;
				SetData(component, DataSource[index]);
				Coloring(component as ListViewItem);
			});

			components.Sort(ComponentsComparer);
			components.ForEach(SetComponentAsLastSibling);

			AddCallbacks();

			if (layout)
			{
				SetFiller();
				layout.UpdateLayout();
			}
		}


		/// <summary>
		/// Raises the item move event.
		/// </summary>
		/// <param name="eventData">Event data.</param>
		/// <param name="item">Item.</param>
		protected override void OnItemMove(AxisEventData eventData, ListViewItem item)
		{
			var block = item.Index % ItemsPerBlock();
			switch (eventData.moveDir)
			{
				case MoveDirection.Left:
					if (block > 0)
					{
						SelectComponentByIndex(item.Index - 1);
					}
					break;
				case MoveDirection.Right:
					if (block < (ItemsPerBlock() - 1))
					{
						SelectComponentByIndex(item.Index + 1);
					}
					break;
				case MoveDirection.Up:
					var index_up = item.Index - ItemsPerBlock();
					if (IsValid(index_up))
					{
						SelectComponentByIndex(index_up);
					}
					break;
				case MoveDirection.Down:
					var index_down = item.Index + ItemsPerBlock();
					if (IsValid(index_down))
					{
						SelectComponentByIndex(index_down);
					}
					break;
			}
		}

		/// <summary>
		/// Count of items the per block.
		/// </summary>
		/// <returns>The per block.</returns>
		int ItemsPerBlock()
		{
			return IsHorizontal() ? itemsPerColumn : itemsPerRow;
		}

		/// <summary>
		/// Gets the blocks count.
		/// </summary>
		/// <returns>The blocks count.</returns>
		/// <param name="items">Items.</param>
		int GetBlocksCount(int items)
		{
			return Mathf.CeilToInt((float)items / (float)ItemsPerBlock());
		}

		/// <summary>
		/// Calculates the size of the bottom filler.
		/// </summary>
		/// <returns>The bottom filler size.</returns>
		protected override float CalculateBottomFillerSize()
		{
			return Mathf.Max(0, GetBlocksCount(bottomHiddenItems) * GetItemSize() - GetItemSpacing());
		}

		/// <summary>
		/// Calculates the size of the top filler.
		/// </summary>
		/// <returns>The top filler size.</returns>
		protected override float CalculateTopFillerSize()
		{
			return Mathf.Max(0, GetBlocksCount(topHiddenItems) * GetItemSize() - GetItemSpacing());
		}
	}
}