using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections;

namespace UIWidgets {

	/// <summary>
	/// SlideBlock axis.
	/// </summary>
	public enum SlideBlockAxis
	{
		LeftToRight = 0,
		TopToBottom = 1,
		RightToLeft = 2,
		BottomToTop = 3,
	}

	/// <summary>
	/// SlideBlock.
	/// </summary>
	public class SlideBlock : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler {
		[SerializeField]
		[Tooltip("Requirements: start value = 0; end value = 1;")]
		/// <summary>
		/// AnimationCurve.
		/// </summary>
		public AnimationCurve Curve = AnimationCurve.EaseInOut(0, 1, 1, 0);

		/// <summary>
		/// Direction.
		/// </summary>
		public SlideBlockAxis Direction = SlideBlockAxis.LeftToRight;

		[SerializeField]
		bool isOpen;

		/// <summary>
		/// Gets or sets a value indicating whether this instance is open.
		/// </summary>
		/// <value><c>true</c> if this instance is open; otherwise, <c>false</c>.</value>
		public bool IsOpen {
			get {
				return isOpen;
			}
			set {
				isOpen = value;
				ResetPosition();
			}
		}

		[SerializeField]
		GameObject optionalHandle;

		/// <summary>
		/// Gets or sets the optional handle.
		/// </summary>
		/// <value>The optional handle.</value>
		public GameObject OptionalHandle {
			get {
				return optionalHandle;
			}
			set {
				if (optionalHandle!=null)
				{
					var handle = optionalHandle.GetComponent<SlideBlockHandle>();
					if (handle!=null)
					{
						handle.BeginDragEvent.RemoveListener(OnBeginDrag);
						handle.DragEvent.RemoveListener(OnDrag);
						handle.EndDragEvent.RemoveListener(OnEndDrag);
					}
				}
				optionalHandle = value;
				if (optionalHandle!=null)
				{
					var handle = optionalHandle.GetComponent<SlideBlockHandle>() ?? optionalHandle.AddComponent<SlideBlockHandle>();
					handle.BeginDragEvent.AddListener(OnBeginDrag);
					handle.DragEvent.AddListener(OnDrag);
					handle.EndDragEvent.AddListener(OnEndDrag);
				}
			}
		}

		/// <summary>
		/// OnOpen event.
		/// </summary>
		public UnityEvent OnOpen = new UnityEvent();

		/// <summary>
		/// OnClose event.
		/// </summary>
		public UnityEvent OnClose = new UnityEvent();

		RectTransform rectTransform;

		/// <summary>
		/// Gets the RectTransform.
		/// </summary>
		/// <value>RectTransform.</value>
		protected RectTransform RectTransform {
			get {
				if (rectTransform==null)
				{
					rectTransform = transform as RectTransform;
				}
				return rectTransform;
			}
		}

		/// <summary>
		/// Position on Start().
		/// </summary>
		Vector2 initPosition;

		/// <summary>
		/// The current animation.
		/// </summary>
		IEnumerator currentAnimation;
		
		float startTime;

		float startPosition;
		
		float animationLength;
		
		float size;

		float acceleration = 1;

		void Start()
		{
			initPosition = RectTransform.anchoredPosition;
			size = (IsHorizontal()) ? RectTransform.rect.width : RectTransform.rect.height;
			if (IsOpen)
			{
				if (SlideBlockAxis.LeftToRight==Direction)
				{
					initPosition = new Vector2(initPosition.x - size, initPosition.y);
				}
				if (SlideBlockAxis.RightToLeft==Direction)
				{
					initPosition = new Vector2(initPosition.x + size, initPosition.y);
				}
				else if (SlideBlockAxis.TopToBottom==Direction)
				{
					initPosition = new Vector2(initPosition.x, initPosition.y - size);
				}
				else if (SlideBlockAxis.BottomToTop==Direction)
				{
					initPosition = new Vector2(initPosition.x, initPosition.y + size);
				}
			}
			OptionalHandle = optionalHandle;
		}

		void ResetPosition()
		{
			var direction = isOpen ? -1 : 0;
			if (IsHorizontal())
			{
				direction *= -1;
			}

			size = (IsHorizontal()) ? RectTransform.rect.width : RectTransform.rect.height;;

			if (SlideBlockAxis.LeftToRight==Direction)
			{
				RectTransform.anchoredPosition = new Vector2(initPosition.x + (size * direction), RectTransform.anchoredPosition.y);
			}
			else if (SlideBlockAxis.RightToLeft==Direction)
			{
				RectTransform.anchoredPosition = new Vector2(initPosition.x - (size * direction), RectTransform.anchoredPosition.y);
			}
			else if (SlideBlockAxis.TopToBottom==Direction)
			{
				RectTransform.anchoredPosition = new Vector2(RectTransform.anchoredPosition.x, initPosition.y + (size * direction));
			}
			else if (SlideBlockAxis.BottomToTop==Direction)
			{
				RectTransform.anchoredPosition = new Vector2(RectTransform.anchoredPosition.x, initPosition.y - (size * direction));
			}
		}

		/// <summary>
		/// Toggle this instance.
		/// </summary>
		public void Toggle()
		{
			if (IsOpen)
			{
				Close();
			}
			else
			{
				Open();
			}
		}

		/// <summary>
		/// Called by a BaseInputModule before a drag is started.
		/// </summary>
		/// <param name="eventData">Event data.</param>
		public void OnBeginDrag(PointerEventData eventData)
		{
			size = (IsHorizontal()) ? RectTransform.rect.width : RectTransform.rect.height;
		}
		
		/// <summary>
		/// Called by a BaseInputModule when a drag is ended.
		/// </summary>
		/// <param name="eventData">Event data.</param>
		public void OnEndDrag(PointerEventData eventData)
		{
			var pos = (IsHorizontal()) ? RectTransform.anchoredPosition.x : RectTransform.anchoredPosition.y;
			var init_pos = (IsHorizontal()) ? initPosition.x : initPosition.y;
			if (pos==init_pos)
			{
				isOpen = false;
				OnClose.Invoke();
				return ;
			}

			var sign = +1;
			if (IsReverse())
			{
				sign = -1;
				init_pos = -init_pos;
				//pos = -pos;
			}

			if (pos==(init_pos+(size * sign)))
			{
				isOpen = true;
				OnOpen.Invoke();
				return ;
			}
			var k = Mathf.Abs(pos / (init_pos + size));
			if (k >= 0.5)
			{
				isOpen = false;
				Open();
			}
			else
			{
				isOpen = true;
				Close();
			}
		}
		
		/// <summary>
		/// When draging is occuring this will be called every time the cursor is moved.
		/// </summary>
		/// <param name="eventData">Event data.</param>
		public void OnDrag(PointerEventData eventData)
		{
			if (currentAnimation!=null)
			{
				StopCoroutine(currentAnimation);
			}

			Vector2 p1;
			RectTransformUtility.ScreenPointToLocalPointInRectangle(RectTransform, eventData.position, eventData.pressEventCamera, out p1);
			Vector2 p2;
			RectTransformUtility.ScreenPointToLocalPointInRectangle(RectTransform, eventData.position - eventData.delta, eventData.pressEventCamera, out p2);
			var delta = p1 - p2;

			if (SlideBlockAxis.LeftToRight==Direction)
			{
				var x = Mathf.Clamp(RectTransform.anchoredPosition.x + delta.x, initPosition.x, initPosition.x + size);
				RectTransform.anchoredPosition = new Vector2(x, RectTransform.anchoredPosition.y);
			}
			else if (SlideBlockAxis.RightToLeft==Direction)
			{
				var x = Mathf.Clamp(RectTransform.anchoredPosition.x + delta.x, initPosition.x - size, initPosition.x);
				RectTransform.anchoredPosition = new Vector2(x, RectTransform.anchoredPosition.y);
			}
			else if (SlideBlockAxis.TopToBottom==Direction)
			{
				var y = Mathf.Clamp(RectTransform.anchoredPosition.y + delta.y, initPosition.y - size, initPosition.y);
				RectTransform.anchoredPosition = new Vector2(RectTransform.anchoredPosition.x, y);
			}
			else if (SlideBlockAxis.BottomToTop==Direction)
			{
				var y = Mathf.Clamp(RectTransform.anchoredPosition.y + delta.y, initPosition.y, initPosition.y + size);
				RectTransform.anchoredPosition = new Vector2(RectTransform.anchoredPosition.x, y);
			}
		}

		/// <summary>
		/// Open this instance.
		/// </summary>
		public void Open()
		{
			if (IsOpen)
			{
				return ;
			}
			Animate();
		}

		/// <summary>
		/// Close this instance.
		/// </summary>
		public void Close()
		{
			if (!IsOpen)
			{
				return ;
			}
			Animate();
		}

		bool IsHorizontal()
		{
			return SlideBlockAxis.LeftToRight==Direction || SlideBlockAxis.RightToLeft==Direction;
		}

		bool IsReverse()
		{
			return SlideBlockAxis.RightToLeft==Direction || SlideBlockAxis.TopToBottom==Direction;
		}

		void Animate()
		{
			if (currentAnimation!=null)
			{
				StopCoroutine(currentAnimation);
			}

			startTime = Time.time;
			animationLength = Curve.keys[Curve.keys.Length - 1].time;

			size = IsHorizontal() ? RectTransform.rect.width : RectTransform.rect.height;
			startPosition = IsHorizontal() ? RectTransform.anchoredPosition.x : RectTransform.anchoredPosition.y;
			var base_size = size;
			var k = IsReverse() ? -1 : +1;
			if (IsHorizontal())
			{
				size = (isOpen)
					? (k * startPosition) - initPosition.x
					: size - (k * startPosition) + initPosition.x;
			}
			else
			{
				size = (isOpen)
					? (k * startPosition) - initPosition.y
					: size - (k * startPosition) + initPosition.y;
			}
			acceleration = (size==0f) ? 1f : base_size / size;
			
			currentAnimation = RunAnimation();
			StartCoroutine(currentAnimation);
		}

		IEnumerator RunAnimation()
		{
			float delta;
			var direction = isOpen ? -1 : +1;
			if (IsReverse())
			{
				direction = -direction;
			}

			do 
			{
				delta = (Time.time - startTime) * acceleration;
				var value = Curve.Evaluate(delta);
				if (IsHorizontal())
				{
					RectTransform.anchoredPosition = new Vector2(startPosition + (value * size * direction), RectTransform.anchoredPosition.y);
				}
				else
				{
					RectTransform.anchoredPosition = new Vector2(RectTransform.anchoredPosition.x, startPosition + (value * size * direction));
				}
				yield return null;
			}
			while (delta <= animationLength);

			isOpen = !isOpen;
			ResetPosition();

			if (IsOpen)
			{
				OnOpen.Invoke();
			}
			else
			{
				OnClose.Invoke();
			}
		}
	}
}