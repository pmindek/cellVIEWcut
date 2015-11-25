using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections;

namespace UIWidgets {
	/// <summary>
	/// Color picker RGB block.
	/// </summary>
	public class ColorPickerRGBBlock : MonoBehaviour {
		[SerializeField]
		Slider rSlider;

		/// <summary>
		/// Gets or sets the Red slider.
		/// </summary>
		/// <value>The Red slider.</value>
		public Slider RSlider {
			get {
				return rSlider;
			}
			set {
				if (rSlider!=null)
				{
					rSlider.onValueChanged.RemoveListener(SliderValueChanged);
				}
				rSlider = value;
				if (rSlider!=null)
				{
					rSlider.onValueChanged.AddListener(SliderValueChanged);
				}
			}
		}
		
		[SerializeField]
		Spinner rInput;

		/// <summary>
		/// Gets or sets the Red input.
		/// </summary>
		/// <value>The Red input.</value>
		public Spinner RInput {
			get {
				return rInput;
			}
			set {
				if (rInput!=null)
				{
					rInput.onEndEdit.RemoveListener(SpinnerValueChanged);
				}
				rInput = value;
				if (rInput!=null)
				{
					rInput.onEndEdit.AddListener(SpinnerValueChanged);
				}
			}
		}
		
		[SerializeField]
		Image rSliderBackground;

		/// <summary>
		/// Gets or sets the Red slider background.
		/// </summary>
		/// <value>The Red slider background.</value>
		public Image RSliderBackground {
			get {
				return rSliderBackground;
			}
			set {
				rSliderBackground = value;
				UpdateMaterial();
			}
		}

		[SerializeField]
		Slider gSlider;

		/// <summary>
		/// Gets or sets the Green slider.
		/// </summary>
		/// <value>The Green slider.</value>
		public Slider GSlider {
			get {
				return gSlider;
			}
			set {
				if (gSlider!=null)
				{
					gSlider.onValueChanged.RemoveListener(SliderValueChanged);
				}
				gSlider = value;
				if (gSlider!=null)
				{
					gSlider.onValueChanged.AddListener(SliderValueChanged);
				}
				
			}
		}
		
		[SerializeField]
		Spinner gInput;

		/// <summary>
		/// Gets or sets the Green input.
		/// </summary>
		/// <value>The Green input.</value>
		public Spinner GInput {
			get {
				return gInput;
			}
			set {
				if (gInput!=null)
				{
					gInput.onValueChangeInt.RemoveListener(SpinnerValueChanged);
				}
				gInput = value;
				if (gInput!=null)
				{
					gInput.onValueChangeInt.AddListener(SpinnerValueChanged);
				}
			}
		}
		
		[SerializeField]
		Image gSliderBackground;

		/// <summary>
		/// Gets or sets the Green slider background.
		/// </summary>
		/// <value>The Green slider background.</value>
		public Image GSliderBackground {
			get {
				return gSliderBackground;
			}
			set {
				gSliderBackground = value;
				UpdateMaterial();
			}
		}

		[SerializeField]
		Slider bSlider;

		/// <summary>
		/// Gets or sets the Blue slider.
		/// </summary>
		/// <value>The Blue slider.</value>
		public Slider BSlider {
			get {
				return bSlider;
			}
			set {
				if (bSlider!=null)
				{
					bSlider.onValueChanged.RemoveListener(SliderValueChanged);
				}
				bSlider = value;
				if (bSlider!=null)
				{
					bSlider.onValueChanged.AddListener(SliderValueChanged);
				}
				
			}
		}
		
		[SerializeField]
		Spinner bInput;

		/// <summary>
		/// Gets or sets the Blue input.
		/// </summary>
		/// <value>The Blue input.</value>
		public Spinner BInput {
			get {
				return bInput;
			}
			set {
				if (bInput!=null)
				{
					bInput.onValueChangeInt.RemoveListener(SpinnerValueChanged);
				}
				bInput = value;
				if (bInput!=null)
				{
					bInput.onValueChangeInt.AddListener(SpinnerValueChanged);
				}
			}
		}

		[SerializeField]
		Image bSliderBackground;

		/// <summary>
		/// Gets or sets the Blue slider background.
		/// </summary>
		/// <value>The Blue slider background.</value>
		public Image BSliderBackground {
			get {
				return bSliderBackground;
			}
			set {
				bSliderBackground = value;
				UpdateMaterial();
			}
		}
		
		[SerializeField]
		Shader defaultShader;

		/// <summary>
		/// Gets or sets the default shader to display gradients for sliders background.
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

				gameObject.SetActive(inputMode==ColorPickerInputMode.RGB);
				UpdateView();
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

			RSlider = rSlider;
			RInput = rInput;
			RSliderBackground = rSliderBackground;
			
			GSlider = gSlider;
			GInput = gInput;
			GSliderBackground = gSliderBackground;

			BSlider = bSlider;
			BInput = bInput;
			BSliderBackground = bSliderBackground;
		}

		void OnEnable()
		{
			UpdateMaterial();
		}

		void SpinnerValueChanged(int value)
		{
			ValueChanged();
		}

		void SpinnerValueChanged(string str)
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
			var color = new Color32(
				GetRed(),
				GetGreen(),
				GetBlue(),
				currentColor.a
			);
			OnChangeRGB.Invoke(color);
		}

		byte GetRed()
		{
			if (rSlider!=null)
			{
				return (byte)rSlider.value;
			}
			if (rInput!=null)
			{
				return (byte)rInput.Value;
			}
			return currentColor.r;
		}
		
		byte GetGreen()
		{
			if (gSlider!=null)
			{
				return (byte)gSlider.value;
			}
			if (gInput!=null)
			{
				return (byte)gInput.Value;
			}
			return currentColor.g;
		}

		byte GetBlue()
		{
			if (bSlider!=null)
			{
				return (byte)bSlider.value;
			}
			if (bInput!=null)
			{
				return (byte)bInput.Value;
			}
			return currentColor.b;
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
			#if UNITY_5_2
			UpdateMaterial();
			#else
			UpdateViewReal();
			#endif
		}
		
		void UpdateViewReal()
		{
			inUpdateMode = true;

			if (rSlider!=null)
			{
				rSlider.value = currentColor.r;
			}
			if (rInput!=null)
			{
				rInput.Value = currentColor.r;
			}

			if (gSlider!=null)
			{
				gSlider.value = currentColor.g;
			}
			if (gInput!=null)
			{
				gInput.Value = currentColor.g;
			}

			if (bSlider!=null)
			{
				bSlider.value = currentColor.b;
			}
			if (bInput!=null)
			{
				bInput.Value = currentColor.b;
			}

			if (rSliderBackground!=null)
			{
				rSliderBackground.material.SetColor("_ColorLeft", new Color32(0, currentColor.g, currentColor.b, 255));
				rSliderBackground.material.SetColor("_ColorRight", new Color32(255, currentColor.g, currentColor.b, 255));
			}

			if (gSliderBackground!=null)
			{
				gSliderBackground.material.SetColor("_ColorLeft", new Color32(currentColor.r, 0, currentColor.b, 255));
				gSliderBackground.material.SetColor("_ColorRight", new Color32(currentColor.r, 255, currentColor.b, 255));
			}

			if (bSliderBackground!=null)
			{
				bSliderBackground.material.SetColor("_ColorLeft", new Color32(currentColor.r, currentColor.g, 0, 255));
				bSliderBackground.material.SetColor("_ColorRight", new Color32(currentColor.r, currentColor.g, 255, 255));
			}

			inUpdateMode = false;
		}
		
		void UpdateMaterial()
		{
			if (defaultShader==null)
			{
				return ;
			}

			if (rSliderBackground!=null)
			{
				rSliderBackground.material = new Material(defaultShader);
			}

			if (gSliderBackground!=null)
			{
				gSliderBackground.material = new Material(defaultShader);
			}

			if (bSliderBackground!=null)
			{
				bSliderBackground.material = new Material(defaultShader);
			}

			UpdateViewReal();
		}
		
		void OnDestroy()
		{
			rSlider = null;
			rInput = null;
			
			gSlider = null;
			gInput = null;

			bSlider = null;
			bInput = null;
		}
	}
}