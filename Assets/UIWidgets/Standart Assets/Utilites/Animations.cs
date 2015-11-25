using UnityEngine;
using System.Collections;

namespace UIWidgets {
	/// <summary>
	/// Animations.
	/// </summary>
	public static class Animations
	{
		/// <summary>
		/// Rotate animation.
		/// </summary>
		/// <returns>Animation.</returns>
		/// <param name="rect">Rect.</param>
		/// <param name="time">Time.</param>
		/// <param name="start_angle">Start rotation angle.</param>
		/// <param name="end_angle">End rotation angle.</param>
		static public IEnumerator Rotate(RectTransform rect, float time=0.5f, float start_angle = 0, float end_angle = 90)
		{
			if (rect!=null)
			{
				var start_rotarion = rect.rotation.eulerAngles;

				var end_time = Time.time + time;
				
				while (Time.time <= end_time)
				{
					var rotation_x = Mathf.Lerp(start_angle, end_angle, 1 - (end_time - Time.time) / time);
					
					rect.rotation = Quaternion.Euler(rotation_x, start_rotarion.y, start_rotarion.z);
					yield return null;
				}
				
				//return rotation back for future use
				rect.rotation = Quaternion.Euler(start_rotarion);
			}
		}

		/// <summary>
		/// Rotate animation.
		/// </summary>
		/// <returns>Animation.</returns>
		/// <param name="rect">Rect.</param>
		/// <param name="time">Time.</param>
		/// <param name="start_angle">Start rotation angle.</param>
		/// <param name="end_angle">End rotation angle.</param>
		static public IEnumerator RotateZ(RectTransform rect, float time=0.5f, float start_angle = 0, float end_angle = 90)
		{
			if (rect!=null)
			{
				var start_rotarion = rect.rotation.eulerAngles;
				
				var end_time = Time.time + time;
				
				while (Time.time <= end_time)
				{
					var rotation_z = Mathf.Lerp(start_angle, end_angle, 1 - (end_time - Time.time) / time);
					
					rect.rotation = Quaternion.Euler(start_rotarion.x, start_rotarion.y, rotation_z);
					yield return null;
				}
				
				//return rotation back for future use
				rect.rotation = Quaternion.Euler(start_rotarion);
			}
		}

		/// <summary>
		/// Collapse animation.
		/// </summary>
		/// <returns>Animation.</returns>
		/// <param name="rect">Rect.</param>
		/// <param name="time">Time.</param>
		static public IEnumerator Collapse(RectTransform rect, float time=0.5f)
		{
			if (rect!=null)
			{
				var max_height = rect.rect.height;

				var end_time = Time.time + time;
				
				while (Time.time <= end_time)
				{
					var height = Mathf.Lerp(max_height, 0, 1 - (end_time - Time.time) / time);
					rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);

					yield return null;
				}
				
				//return height back for future use
				rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, max_height);
			}
		}

		/// <summary>
		/// Open animation.
		/// </summary>
		/// <returns>Animation.</returns>
		/// <param name="rect">Rect.</param>
		/// <param name="time">Time.</param>
		static public IEnumerator Open(RectTransform rect, float time=0.5f)
		{
			if (rect!=null)
			{
				var max_height = rect.rect.height;
				
				var end_time = Time.time + time;
				
				while (Time.time <= end_time)
				{
					var height = Mathf.Lerp(0, max_height, 1 - (end_time - Time.time) / time);
					rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);

					yield return null;
				}
				
				//return height back for future use
				rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, max_height);
			}
		}
	}
}