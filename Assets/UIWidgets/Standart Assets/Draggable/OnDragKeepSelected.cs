using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections;

namespace UIWidgets
{
	/// <summary>
	/// Return selection to last selected object after drag.
	/// </summary>
	public class OnDragKeepSelected : MonoBehaviour, IEndDragHandler
	{
		/// <summary>
		/// Raises the end drag event.
		/// </summary>
		/// <param name="eventData">Event data.</param>
		public void OnEndDrag(PointerEventData eventData)
		{
			#if UNITY_5_2 || UNITY_5_3
			#else
			EventSystem.current.SetSelectedGameObject(EventSystem.current.lastSelectedGameObject);
			#endif
		}
	}
}