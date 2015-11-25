using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections;

namespace UIWidgets {
	/// <summary>
	/// OnDragListener.
	/// </summary>
	public class OnDragListener : MonoBehaviour, IDragHandler {

		[SerializeField]
		/// <summary>
		/// OnDragEvent.
		/// </summary>
		public PointerUnityEvent OnDragEvent = new PointerUnityEvent();

		/// <summary>
		/// Raises the OnDragEvent.
		/// </summary>
		/// <param name="eventData">Event data.</param>
		public void OnDrag(PointerEventData eventData)
		{
			OnDragEvent.Invoke(eventData);
		}
	}
}