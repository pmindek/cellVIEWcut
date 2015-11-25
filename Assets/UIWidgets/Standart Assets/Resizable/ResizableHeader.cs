using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Linq;

namespace UIWidgets
{
	/// <summary>
	/// IResizableItem.
	/// </summary>
	public interface IResizableItem {
		/// <summary>
		/// Gets the objects to resize.
		/// </summary>
		/// <value>The objects to resize.</value>
		GameObject[] ObjectsToResize { get; }
	}

	/// <summary>
	/// ResizableHeader.
	/// </summary>
	[RequireComponent(typeof(LayoutGroup))]
	public class ResizableHeader : MonoBehaviour,
		IInitializePotentialDragHandler, IBeginDragHandler, IEndDragHandler, IDragHandler
	{
		/// <summary>
		/// ListView instance.
		/// </summary>
		[SerializeField]
		public ListViewBase List;
		
		/// <summary>
		/// Update ListView columns width on drag.
		/// </summary>
		[SerializeField]
		public bool OnDragUpdate = true;

		/// <summary>
		/// The active region in points from left or right border where resize allowed.
		/// </summary>
		[SerializeField]
		public float ActiveRegion = 5;

		/// <summary>
		/// The current camera. For Screen Space - Overlay let it empty.
		/// </summary>
		[SerializeField]
		public Camera CurrentCamera;

		/// <summary>
		/// The cursor texture.
		/// </summary>
		[SerializeField]
		public Texture2D CursorTexture;

		/// <summary>
		/// The cursor hot spot.
		/// </summary>
		[SerializeField]
		public Vector2 CursorHotSpot;

		/// <summary>
		/// The default cursor texture.
		/// </summary>
		[SerializeField]
		public Texture2D DefaultCursorTexture;

		/// <summary>
		/// The default cursor hot spot.
		/// </summary>
		[SerializeField]
		public Vector2 DefaultCursorHotSpot;

		RectTransform rectTransform;
		
		/// <summary>
		/// Gets the rect transform.
		/// </summary>
		/// <value>The rect transform.</value>
		public RectTransform RectTransform {
			get {
				if (rectTransform==null)
				{
					rectTransform = transform as RectTransform;
				}
				return rectTransform;
			}
		}
		
		Canvas canvas;
		RectTransform canvasRect;
		
		List<LayoutElement> childrenLayouts = new List<LayoutElement>();
		List<RectTransform> children = new List<RectTransform>();
		LayoutElement leftTarget;
		LayoutElement rightTarget;
		bool processDrag;
		float[] widths;

		LayoutGroup layout;

		void Start()
		{
			layout = GetComponent<LayoutGroup>();
			if (layout!=null)
			{
				Utilites.UpdateLayout(layout);
			}

			Init();
		}

		/// <summary>
		/// Raises the initialize potential drag event.
		/// </summary>
		/// <param name="eventData">Event data.</param>
		public void OnInitializePotentialDrag(PointerEventData eventData)
		{
			Init();
		}

		/// <summary>
		/// Init this instance.
		/// </summary>
		public void Init()
		{
			canvasRect = Utilites.FindCanvas(transform) as RectTransform;
			canvas = canvasRect.GetComponent<Canvas>();
			
			children.Clear();
			childrenLayouts.Clear();
			foreach (Transform child in transform)
			{
				var element = child.GetComponent<LayoutElement>() ?? child.gameObject.AddComponent<LayoutElement>();
				children.Add(child as RectTransform);
				childrenLayouts.Add(element);
			}

			//CalculateWidths();
			//ResetChildren();
			//Resize();
		}

		void LateUpdate()
		{
			if (processDrag)
			{
				return ;
			}
			if (CursorTexture==null)
			{
				return ;
			}
			if (!Input.mousePresent)
			{
				return ;
			}

			Vector2 point;
			bool in_draggable_region = false;
			
			if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(RectTransform, Input.mousePosition, CurrentCamera, out point))
			{
				return ;
			}
			var rect = RectTransform.rect;
			if (!rect.Contains(point))
			{
				return ;
			}
			
			point += new Vector2(rect.width * RectTransform.pivot.x, 0);

			int i = 0;
			foreach (var child in children)
			{
				var is_first = i==0;
				if (!is_first)
				{
					in_draggable_region = CheckLeft(child, point);
					if (in_draggable_region)
					{
						break;
					}
				}
				var is_last = i==(children.Count - 1);
				if (!is_last)
				{
					in_draggable_region = CheckRight(child, point);
					if (in_draggable_region)
					{
						break;
					}
				}
				
				i++;
			}

			if (in_draggable_region)
			{
				Cursor.SetCursor(CursorTexture, CursorHotSpot, Utilites.GetCursorMode());
			}
			else
			{
				Cursor.SetCursor(DefaultCursorTexture, DefaultCursorHotSpot, Utilites.GetCursorMode());
			}
		}

		float widthLimit;

		/// <summary>
		/// Raises the begin drag event.
		/// </summary>
		/// <param name="eventData">Event data.</param>
		public void OnBeginDrag(PointerEventData eventData)
		{
			Vector2 point;
			processDrag = false;

			if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(RectTransform, eventData.pressPosition, eventData.pressEventCamera, out point))
			{
				return ;
			}
			
			var r = RectTransform.rect;
			point += new Vector2(r.width * RectTransform.pivot.x, 0);
			
			int i = 0;
			foreach (var child in children)
			{
				var is_first = i==0;
				if (!is_first)
				{
					processDrag = CheckLeft(child, point);
					if (processDrag)
					{
						leftTarget = childrenLayouts[i - 1];
						rightTarget = childrenLayouts[i];
						widthLimit = children[i - 1].rect.width + children[i].rect.width;
						break;
					}
				}
				var is_last = i==(children.Count - 1);
				if (!is_last)
				{
					processDrag = CheckRight(child, point);
					if (processDrag)
					{
						leftTarget = childrenLayouts[i];
						rightTarget = childrenLayouts[i + 1];
						widthLimit = children[i].rect.width + children[i + 1].rect.width;
						break;
					}
				}
				
				i++;
			}
		}

		/// <summary>
		/// Checks if point in the left region.
		/// </summary>
		/// <returns><c>true</c>, if point in the left region, <c>false</c> otherwise.</returns>
		/// <param name="childRectTransform">RectTransform.</param>
		/// <param name="point">Point.</param>
		bool CheckLeft(RectTransform childRectTransform, Vector2 point)
		{
			var r = childRectTransform.rect;
			r.position += new Vector2(childRectTransform.anchoredPosition.x, 0);
			r.width = ActiveRegion;

			return r.Contains(point);
		}

		/// <summary>
		/// Checks if point in the right region.
		/// </summary>
		/// <returns><c>true</c>, if right was checked, <c>false</c> otherwise.</returns>
		/// <param name="childRectTransform">Child rect transform.</param>
		/// <param name="point">Point.</param>
		bool CheckRight(RectTransform childRectTransform, Vector2 point)
		{
			var r = childRectTransform.rect;
			
			r.position += new Vector2(childRectTransform.anchoredPosition.x, 0);
			r.position = new Vector2(r.position.x + r.width - ActiveRegion, r.position.y);
			r.width = ActiveRegion;
			
			return r.Contains(point);
		}

		/// <summary>
		/// Raises the end drag event.
		/// </summary>
		/// <param name="eventData">Event data.</param>
		public void OnEndDrag(PointerEventData eventData)
		{
			Cursor.SetCursor(DefaultCursorTexture, DefaultCursorHotSpot, Utilites.GetCursorMode());

			CalculateWidths();
			ResetChildren();
			if (!OnDragUpdate)
			{
				Resize();
			}
			processDrag = false;
		}

		/// <summary>
		/// Raises the drag event.
		/// </summary>
		/// <param name="eventData">Event data.</param>
		public void OnDrag(PointerEventData eventData)
		{
			if (!processDrag)
			{
				return ;
			}
			if (canvas==null)
			{
				throw new MissingComponentException(gameObject.name + " not in Canvas hierarchy.");
			}
			Cursor.SetCursor(CursorTexture, CursorHotSpot, Utilites.GetCursorMode());

			Vector2 p1;
			RectTransformUtility.ScreenPointToLocalPointInRectangle(RectTransform, eventData.position, CurrentCamera, out p1);
			Vector2 p2;
			RectTransformUtility.ScreenPointToLocalPointInRectangle(RectTransform, eventData.position - eventData.delta, CurrentCamera, out p2);
			var delta = p1 - p2;

			if (delta.x > 0)
			{
				leftTarget.preferredWidth = Mathf.Min(leftTarget.preferredWidth + delta.x, widthLimit - rightTarget.minWidth);
				rightTarget.preferredWidth = widthLimit - leftTarget.preferredWidth;
			}
			else
			{
				rightTarget.preferredWidth = Mathf.Min(rightTarget.preferredWidth - delta.x, widthLimit - leftTarget.minWidth);
				leftTarget.preferredWidth = widthLimit - rightTarget.preferredWidth;
			}

			if (layout!=null)
			{
				Utilites.UpdateLayout(layout);
			}

			if (OnDragUpdate)
			{
				CalculateWidths();
				Resize();
			}
		}

		float GetRectWidth(RectTransform rect)
		{
			return rect.rect.width;
		}

		/// <summary>
		/// Calculates the widths.
		/// </summary>
		void CalculateWidths()
		{
			widths = children.Select<RectTransform,float>(GetRectWidth).ToArray();
		}

		/// <summary>
		/// Resets the children widths.
		/// </summary>
		void ResetChildren()
		{
			childrenLayouts.ForEach(ResetChildrenWidth);
		}

		void ResetChildrenWidth(LayoutElement element, int index)
		{
			element.preferredWidth = widths[index];
		}

		/// <summary>
		/// Resize this instance.
		/// </summary>
		public void Resize()
		{
			if (List==null)
			{
				return ;
			}
			if (widths.Length < 2)
			{
				return ;
			}
			List.Start();
			List.ForEachComponent(ResizeComponent);
		}

		/// <summary>
		/// Resizes the game object.
		/// </summary>
		/// <param name="go">Go.</param>
		/// <param name="i">The index.</param>
		void ResizeGameObject(GameObject go, int i)
		{
			var layoutElement = go.GetComponent<LayoutElement>();
			if (layoutElement)
			{
				layoutElement.preferredWidth = widths[i];
			}
			else
			{
				(go.transform as RectTransform).SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, widths[i]);
			}
		}

		/// <summary>
		/// Resizes the component.
		/// </summary>
		/// <param name="component">Component.</param>
		void ResizeComponent(ListViewItem component)
		{
			var resizable_item = component as IResizableItem;
			if (resizable_item!=null)
			{
				resizable_item.ObjectsToResize.ForEach(ResizeGameObject);
			}
		}
	}
}