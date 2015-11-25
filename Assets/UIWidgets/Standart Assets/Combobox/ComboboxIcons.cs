using UnityEngine;
using UnityEngine.UI;

namespace UIWidgets {
	/// <summary>
	/// Combobox with icons.
	/// </summary>
	[AddComponentMenu("UI/ComboboxIcons", 230)]
	public class ComboboxIcons : ComboboxCustom<ListViewIcons,ListViewIconsItemComponent,ListViewIconsItemDescription>
	{
		void Awake()
		{
			Start();
		}

		[System.NonSerialized]
		private bool is_started;
		
		/// <summary>
		/// Start this instance.
		/// </summary>
		public override void Start()
		{
			if (is_started)
			{
				return ;
			}
			is_started = true;

			base.Start();
		}

		/// <summary>
		/// Updates the current component with selected item.
		/// </summary>
		protected override void UpdateCurrent()
		{
			Current.SetData(ListView.SelectedItem);

			HideList();
		}

		#if UNITY_EDITOR
		[UnityEditor.MenuItem("GameObject/UI/ComboboxIcons", false, 1040)]
		static void CreateObject()
		{
			Utilites.CreateWidgetFromAsset("ComboboxIcons");
		}
		#endif
	}
}