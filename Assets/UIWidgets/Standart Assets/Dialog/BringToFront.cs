using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

namespace UIWidgets {

	/// <summary>
	/// Bring to front UI object on click.
	/// Use carefully: it change hierarchy. Objects under layout control will be at another positions.
	/// </summary>
	public class BringToFront : MonoBehaviour, IPointerDownHandler {

		[SerializeField]
		/// <summary>
		/// Bring to front UI object with parents.
		/// </summary>
		public bool WithParents = false;

		/// <summary>
		/// Raises the pointer down event.
		/// </summary>
		/// <param name="eventData">Event data.</param>
		public void OnPointerDown(PointerEventData eventData)
		{
			ToFront();
		}

		/// <summary>
		/// Bring to front UI object.
		/// </summary>
		public void ToFront()
		{
			ToFront(transform);
		}

		/// <summary>
		/// TBring to front specified object.
		/// </summary>
		/// <param name="obj">Object.</param>
		void ToFront(Transform obj)
		{
			obj.SetAsLastSibling();
			if (WithParents && (obj.parent!=null))
			{
				ToFront(obj.parent);
			}
		}
	}
}