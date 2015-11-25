using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UIWidgets;

namespace UIWidgetsSamples {
	
	[System.Serializable]
	public class ListViewUnderlineSampleItemDescription {
		[SerializeField]
		public Sprite Icon;
		[SerializeField]
		public string Name;
	}
	
	public class ListViewUnderlineSample : ListViewCustom<ListViewUnderlineSampleComponent,ListViewUnderlineSampleItemDescription> {
		bool isStartedListViewCustomSample = false;
		
		protected override void Awake()
		{
			Start();
		}
		
		public override void Start()
		{
			if (isStartedListViewCustomSample)
			{
				return ;
			}
			isStartedListViewCustomSample = true;
			
			SortFunc = (x) => x.OrderBy(y => y.Name).ToList();
			base.Start();
		}
		
		protected override void SetData(ListViewUnderlineSampleComponent component, ListViewUnderlineSampleItemDescription item)
		{
			component.SetData(item);
		}
		
		protected override void HighlightColoring(ListViewUnderlineSampleComponent component)
		{
			component.Underline.color = HighlightedColor;
			component.Text.color = HighlightedColor;
		}
		
		protected override void SelectColoring(ListViewUnderlineSampleComponent component)
		{
			component.Underline.color = SelectedColor;
			component.Text.color = SelectedColor;
		}
		
		protected override void DefaultColoring(ListViewUnderlineSampleComponent component)
		{
			component.Underline.color = DefaultColor;
			component.Text.color = DefaultColor;
		}
	}
}