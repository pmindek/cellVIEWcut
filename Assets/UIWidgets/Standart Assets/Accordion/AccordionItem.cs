using UnityEngine;
using System.Collections;

namespace UIWidgets {

	[System.Serializable]
	/// <summary>
	/// Accordion item.
	/// </summary>
	public class AccordionItem {
		/// <summary>
		/// The toggle object.
		/// </summary>
		public GameObject ToggleObject;
		/// <summary>
		/// The content object.
		/// </summary>
		public GameObject ContentObject;
		/// <summary>
		/// Default state of content object.
		/// </summary>
		public bool Open;
		
		[HideInInspector]
		/// <summary>
		/// The current corutine.
		/// </summary>
		public Coroutine CurrentCorutine;
		
		[HideInInspector]
		/// <summary>
		/// The content object RectTransform.
		/// </summary>
		public RectTransform ContentObjectRect;
		
		[HideInInspector]
		/// <summary>
		/// The height of the content object.
		/// </summary>
		public float ContentObjectHeight;
	}
}