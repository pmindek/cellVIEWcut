using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

namespace UIWidgets {

	[AddComponentMenu("UI/Accordion", 350)]
	/// <summary>
	/// Accordion.
	/// </summary>
	public class Accordion : MonoBehaviour {

		/// <summary>
		/// The items.
		/// </summary>
		[SerializeField]
		[FormerlySerializedAs("Items")]
		List<AccordionItem> items = new List<AccordionItem>();

		List<AccordionItem> Items {
			get {
				return items;
			}
			set {
				if (items!=null)
				{
					RemoveCallbacks();
				}
				items = value;
				if (items!=null)
				{
					AddCallbacks();
				}
			}
		}
		
		/// <summary>
		/// Only one item can be opened.
		/// </summary>
		public bool OnlyOneOpen = true;
		
		/// <summary>
		/// Animate open and close.
		/// </summary>
		public bool Animate = true;

		/// <summary>
		/// OnToggleItem event.
		/// </summary>
		public AccordionEvent OnToggleItem = new AccordionEvent();

		List<UnityAction> callbacks = new List<UnityAction>();

		void Start()
		{
			AddCallbacks();
		}

		void AddCallback(AccordionItem item)
		{
			if (item.Open)
			{
				Open(item, false);
			}
			else
			{
				Close(item, false);
			}
			UnityAction callback = () => ToggleItem(item);
			
			item.ToggleObject.AddComponent<AccordionItemComponent>().OnClick.AddListener(callback);
			item.ContentObjectRect = item.ContentObject.transform as RectTransform;
			item.ContentObjectHeight = item.ContentObjectRect.rect.height;
			
			callbacks.Add(callback);
		}

		void AddCallbacks()
		{
			Items.ForEach(AddCallback);
		}

		void RemoveCallback(AccordionItem item, int index)
		{
			if (item==null)
			{
				return ;
			}
			if (item.ToggleObject==null)
			{
				return ;
			}
			
			var component = item.ToggleObject.GetComponent<AccordionItemComponent>();
			if ((component!=null) && (index < callbacks.Count))
			{
				component.OnClick.RemoveListener(callbacks[index]);
			}
		}

		void RemoveCallbacks()
		{
			Items.ForEach(RemoveCallback);
			callbacks.Clear();
		}

		void OnDestroy()
		{
			RemoveCallbacks();
		}

		/// <summary>
		/// Toggles the item.
		/// </summary>
		/// <param name="item">Item.</param>
		void ToggleItem(AccordionItem item)
		{
			if (item.Open)
			{
				if (!OnlyOneOpen)
				{
					Close(item);
				}
			}
			else
			{
				if (OnlyOneOpen)
				{
					Items.Where(IsOpen).ForEach(Close);
				}

				Open(item);
			}
		}

		void Open(AccordionItem item)
		{
			Open(item, Animate);
		}

		void Close(AccordionItem item)
		{
			Close(item, Animate);
		}

		bool IsOpen(AccordionItem item)
		{
			return item.Open;
		}

		/// <summary>
		/// Open the specified item and animate.
		/// </summary>
		/// <param name="item">Item.</param>
		/// <param name="animate">If set to <c>true</c> animate.</param>
		void Open(AccordionItem item, bool animate)
		{
			if (item.CurrentCorutine!=null)
			{
				StopCoroutine(item.CurrentCorutine);
				item.ContentObjectRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, item.ContentObjectHeight);
				item.ContentObject.SetActive(false);
			}
			if (animate)
			{
				item.CurrentCorutine = StartCoroutine(OpenCorutine(item));
			}
			else
			{
				item.ContentObject.SetActive(true);
				OnToggleItem.Invoke(item);
			}

			item.ContentObject.SetActive(true);
			item.Open = true;
		}

		/// <summary>
		/// Close the specified item and animate.
		/// </summary>
		/// <param name="item">Item.</param>
		/// <param name="animate">If set to <c>true</c> animate.</param>
		void Close(AccordionItem item, bool animate)
		{
			if (item.CurrentCorutine!=null)
			{
				StopCoroutine(item.CurrentCorutine);
				item.ContentObject.SetActive(true);
				item.ContentObjectRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, item.ContentObjectHeight);
			}
			if (animate)
			{
				item.CurrentCorutine = StartCoroutine(HideCorutine(item));
			}
			else
			{
				item.ContentObject.SetActive(false);
				item.Open = false;
				OnToggleItem.Invoke(item);
			}

		}

		/// <summary>
		/// Opens the corutine.
		/// </summary>
		/// <returns>The corutine.</returns>
		/// <param name="item">Item.</param>
		IEnumerator OpenCorutine(AccordionItem item)
		{
			item.ContentObject.SetActive(true);
			item.Open = true;

			yield return StartCoroutine(Animations.Open(item.ContentObjectRect));

			OnToggleItem.Invoke(item);
		}

		/// <summary>
		/// Hides the corutine.
		/// </summary>
		/// <returns>The corutine.</returns>
		/// <param name="item">Item.</param>
		IEnumerator HideCorutine(AccordionItem item)
		{
			yield return StartCoroutine(Animations.Collapse(item.ContentObjectRect));

			item.Open = false;
			item.ContentObject.SetActive(false);

			OnToggleItem.Invoke(item);
		}

		#if UNITY_EDITOR
		[UnityEditor.MenuItem("GameObject/UI/Accordion", false, 1000)]
		static void CreateObject()
		{
			Utilites.CreateWidgetFromAsset("Accordion");
		}
		#endif
	}
}
