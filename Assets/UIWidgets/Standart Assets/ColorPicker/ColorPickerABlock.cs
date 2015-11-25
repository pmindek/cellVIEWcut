using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections;

namespace UIWidgets {
	/// <summary>
	/// Color picker Alpha slider block.
	/// </summary>
	public class ColorPickerABlock : MonoBehaviour {
		[SerializeField]
		Slider aSlider;

		/// <summary>
		/// Gets or sets Alpha slider.
		/// </summary>
		/// <value>Alpha slider.</value>
		public Slider ASlider {
			get {
				return aSlider;
			}
			set {
				if (aSlider!=null)
				{
					aSlider.onValueChanged.RemoveListener(SliderValueChanged);
				}
				aSlider = value;
				if (aSlider!=null)
				{
					aSlider.onValueChanged.AddListener(SliderValueChanged);
					UpdateView();
				}
			}
		}

		[SerializeField]
		Spinner aInput;

		/// <summary>
		/// Gets or sets Alpha spinner.
		/// </summary>
		/// <value>Alpha spinner.</value>
		public Spinner AInput {
			get {
				return aInput;
			}
			set {
				if (aInput!=null)
				{
					aInput.onValueChangeInt.RemoveListener(SpinnerValueChanged);
				}
				aInput = value;
				if (aInput!=null)
				{
					aInput.onValueChangeInt.AddListener(SpinnerValueChanged);
					UpdateView();
				}
			}
		}
		
		[SerializeField]
		Image aSliderBackground;

		/// <summary>
		/// Gets or sets Alpha slider background.
		/// </summary>
		/// <value>Alpha slider background.</value>
		public Image ASliderBackground {
			get {
				return aSliderBackground;
			}
			set {
				aSliderBackground = value;
				UpdateMaterial();
			}
		}
		
		[SerializeField]
		Shader defaultShader;

		/// <summary>
		/// Gets or sets the default shader to display alpha gradient in ASliderBackground.
		/// </summary>
		/// <value>The default shader.</value>
		public Shader DefaultShader {
			get {
				return defaultShader;
			}
			set {
				defaultShader = value;
				UpdateMaterial();
			}
		}

		ColorPickerInputMode inputMode;

		/// <summary>
		/// Gets or sets the input mode.
		/// </summary>
		/// <value>The input mode.</value>
		public ColorPickerInputMode InputMode {
			get {
				return inputMode;
			}
			set {
				inputMode = value;
			}
		}
		
		ColorPickerPaletteMode paletteMode;

		/// <summary>
		/// Gets or sets the palette mode.
		/// </summary>
		/// <value>The palette mode.</value>
		public ColorPickerPaletteMode PaletteMode {
			get {
				return paletteMode;
			}
			set {
				paletteMode = value;
			}
		}

		/// <summary>
		/// OnChangeRGB event.
		/// </summary>
		public ColorRGBChangedEvent OnChangeRGB = new ColorRGBChangedEvent();

		/// <summary>
		/// OnChangeHSV event.
		/// </summary>
		public ColorHSVChangedEvent OnChangeHSV = new ColorHSVChangedEvent();

		/// <summary>
		/// OnChangeAlpha event.
		/// </summary>
		public ColorAlphaChangedEvent OnChangeAlpha = new ColorAlphaChangedEvent();

		bool isStarted;

		/// <summary>
		/// Start this instance.
		/// </summary>
		public void Start()
		{
			if (isStarted)
			{
				return ;
			}
			isStarted = true;

			ASlider = aSlider;
			AInput = aInput;
			ASliderBackground = aSliderBackground;
		}

		void OnEnable()
		{
			UpdateMaterial();
		}

		void SpinnerValueChanged(int value)
		{
			ValueChanged();
		}

		void SliderValueChanged(float value)
		{
			ValueChanged();
		}

		bool inUpdateMode;
		void ValueChanged()
		{
			if (inUpdateMode)
			{
				return ;
			}
			OnChangeAlpha.Invoke(GetAlpha());
		}

		byte GetAlpha()
		{
			if (aSlider!=null)
			{
				return (byte)aSlider.value;
			}
			if (aInput!=null)
			{
				return (byte)aInput.Value;
			}
			return currentColor.a;
		}

		Color32 currentColor;

		/// <summary>
		/// Sets the color.
		/// </summary>
		/// <param name="color">Color.</param>
		public void SetColor(Color32 color)
		{
			currentColor = color;
			UpdateView();
		}

		/// <summary>
		/// Sets the color.
		/// </summary>
		/// <param name="color">Color.</param>
		public void SetColor(ColorHSV color)
		{
			currentColor = color;
			UpdateView();
		}

		void UpdateView()
		{
			inUpdateMode = true;
			
			if (aSlider!=null)
			{
				aSlider.value = currentColor.a;
			}
			if (aInput!=null)
			{
				aInput.Value = currentColor.a;
			}
			
			inUpdateMode = false;
		}

		void UpdateMaterial()
		{
			if (defaultShader==null)
			{
				return ;
			}

			if (aSliderBackground==null)
			{
				return ;
			}

			var material = new Material(defaultShader);
			material.SetColor("_ColorLeft", Color.black);
			material.SetColor("_ColorRight", Color.white);
			aSliderBackground.material = material;
		}

		void OnDestroy()
		{
			aSlider = null;
			aInput = null;
		}

	}
}