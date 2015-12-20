using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace Chummer.UI.Shared
{
	public partial class SkillsDisplay<TType> : UserControl
	{
		private readonly BindingList<TType> _contents;
		private readonly Func<TType, Control> _createFunc;
		private readonly List<Control> _controls = new List<Control>();
		private readonly Dictionary<TType, Control> _map = new Dictionary<TType, Control>(); 

		public SkillsDisplay(BindingList<TType> list, Func<TType, Control> createFunc)
		{
			_contents = list;
			_createFunc = createFunc;

			InitializeComponent();
		}

		private void SkillsDisplay_Load(object sender, EventArgs e)
		{
			foreach (TType content in _contents)
			{
				AddItem(content);
			}

			_contents.RaiseListChangedEvents = true;
			_contents.ListChanged += ContentsOnListChanged;
		}

		private void ContentsOnListChanged(object sender, ListChangedEventArgs listChangedEventArgs)
		{
			ListChangedType type = listChangedEventArgs.ListChangedType;
			if (type == ListChangedType.ItemAdded)
			{
				AddItem(_contents[listChangedEventArgs.NewIndex]);
			}
			else if (type == ListChangedType.ItemDeleted)
			{
				tblContents.Controls.RemoveAt(listChangedEventArgs.NewIndex);
				//tblContents.Controls.Remove(_map[_contents[listChangedEventArgs.NewIndex]]);
			}
		}

		private void AddItem(TType content)
		{
			Control c = _createFunc?.Invoke(content);

			if (c != null)
			{
				_map.Add(content, c);
				_controls.Add(c);
				tblContents.Controls.Add(c);
				SkillsDisplay_Resize(null, null); //Updates size to show new item.
			}
			else if (Debugger.IsAttached) Debugger.Break();

		}

		public void Filter(Func<TType, bool> predicate)
		{
			tblContents.SuspendLayout();
			foreach (KeyValuePair<TType, Control> keyValuePair in _map)
			{
				keyValuePair.Value.Visible = predicate?.Invoke(keyValuePair.Key) ?? true;

			}
			tblContents.ResumeLayout();

		}

		private void ContentOnSizeChanged(object sender, EventArgs eventArgs)
		{
			

		}

		private void SkillsDisplay_Resize(object sender, EventArgs e)
		{
			tblContents.Size = new Size(Width - SystemInformation.VerticalScrollBarWidth, tblContents.PreferredSize.Height);
		}
	}
}
