using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;
using UIWidgets;

namespace UIWidgetsSamples {
	public class TestTreeView : MonoBehaviour {
		public TreeView Tree;

		ObservableList<TreeNode<TreeViewItem>> nodes;

		Dictionary<string,ObservableList<TreeNode<TreeViewItem>>> dNodes;
		ObservableList<TreeNode<TreeViewItem>> nodes5k;
		ObservableList<TreeNode<TreeViewItem>> nodes10k;
		ObservableList<TreeNode<TreeViewItem>> nodes50k;

		void Start()
		{
			Tree.Start();
			
			Tree.Nodes = nodes;
		}

		public void Test10K()
		{
			var config = new List<int>() {10, 10, 10, 10};
			nodes = GenerateTreeNodes(config, isExpanded: true);

			Tree.Nodes = nodes;
		}

		public void LongNames()
		{
			var config = new List<int>() {2, 2, 2, 2, 2, 2, 2, 2, 2};
			nodes = GenerateTreeNodes(config, isExpanded: true);
			
			Tree.Nodes = nodes;
		}

		public TestTreeView()
		{
			//var config = new List<int>() {20, 10, 10, 10, 25};
			//var config = new List<int>() {5, 5, 5, 5, 5};
			//var config = new List<int>() {5, 4, 3};
			//var config = new List<int>() {2, 2, 2, 2, 2, 2, 2, 2, 2};
			//var config = new List<int>() {5, 10, 10, 10, };
			//var config = new List<int>() {3, 3 };
			var config = new List<int>() {5, 5, 2 };
			nodes = GenerateTreeNodes(config, isExpanded: true);
		}

		public void PerformanceCheck(string k)
		{
			if (dNodes==null)
			{
				dNodes = new Dictionary<string, ObservableList<TreeNode<TreeViewItem>>>();

				var config1k = new List<int>() {10, 10, 10 };
				dNodes.Add("1k", GenerateTreeNodes(config1k, isExpanded: true));
				
				var config5k = new List<int>() {5, 10, 10, 10 };
				dNodes.Add("5k", GenerateTreeNodes(config5k, isExpanded: true));
				
				var config10k = new List<int>() {10, 10, 10, 10 };
				dNodes.Add("10k", GenerateTreeNodes(config10k, isExpanded: true));
				
				var config50k = new List<int>() {5, 10, 10, 10, 10 };
				dNodes.Add("50k", GenerateTreeNodes(config50k, isExpanded: true));
			}
			nodes = dNodes[k];
			Tree.Nodes = dNodes[k];
		}

		public void SetTreeNodes()
		{
			Tree.Nodes = nodes;

			nodes.BeginUpdate();

			var test_item = new TreeViewItem("added");
			var test_node = new TreeNode<TreeViewItem>(test_item);
			nodes.Add(test_node);
			nodes[1].IsVisible = false;
			nodes[2].Nodes[1].IsVisible = false;

			nodes.EndUpdate();
		}

		public void AddNodes()
		{
			var test_item = new TreeViewItem("New node");
			var test_node = new TreeNode<TreeViewItem>(test_item);
			nodes.Add(test_node);
		}

		public void ToggleNode()
		{
			nodes[0].Nodes[0].IsExpanded = !nodes[0].Nodes[0].IsExpanded;
		}

		public void ChangeNodesName()
		{
			nodes[0].Item.Name = "Node renamed from code";
			nodes[0].Nodes[1].Item.Name = "Another node renamed from code";
		}

		public void ResetFilter()
		{
			nodes.BeginUpdate();

			nodes.ForEach(SetVisible);

			nodes.EndUpdate();
		}

		void SetVisible(TreeNode<TreeViewItem> node)
		{
			if (node.Nodes!=null)
			{
				node.Nodes.ForEach(SetVisible);
			}
			node.IsVisible = true;
		}

		public void Filter(string nameContains)
		{
			nodes.BeginUpdate();

			SampleFilter(nodes, x => x.Name.Contains(nameContains));

			nodes.EndUpdate();
		}

		public void Clear()
		{
			//nodes.Clear();
			nodes = new ObservableList<TreeNode<TreeViewItem>>();
			Tree.Nodes = nodes;
		}

		bool SampleFilter(IObservableList<TreeNode<TreeViewItem>> nodes, Func<TreeViewItem,bool> filterFunc)
		{
			return nodes.Count(x => {
				var have_visible_children = (x.Nodes==null) ? false : SampleFilter(x.Nodes, filterFunc);
				x.IsVisible = have_visible_children || filterFunc(x.Item) ;
				return x.IsVisible;
			}) > 0;
		}

		static public ObservableList<TreeNode<TreeViewItem>> GenerateTreeNodes(List<int> items, string nameStartsWith = "Node ", bool isExpanded = true)
		{
			return Enumerable.Range(1, items[0]).Select(x => {
				var item_name = nameStartsWith + x;
				var item = new TreeViewItem(item_name);
				var nodes = items.Count > 1
					? GenerateTreeNodes(items.GetRange(1, items.Count - 1), item_name + " - ", isExpanded)
					: null;

				return new TreeNode<TreeViewItem>(item, nodes, isExpanded);
			}).ToObservableList();
		}

		public void ReloadScene()
		{
			Application.LoadLevel(Application.loadedLevel);
		}
	}
}