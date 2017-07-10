using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Chummer.UI.Shared;
using ChummerDataViewer.Model;

namespace ChummerDataViewer
{
	public partial class Mainform : Form
	{
		//Main display
		private readonly BindingList<CrashReport> _crashReports = new BindingList<CrashReport>();
		private BindingListDisplay<CrashReport> _bldCrashReports;

		//Status strip
		private delegate void MainThreadDelegate(INotifyThreadStatus sender, StatusChangedEventArgs args);
		private MainThreadDelegate _mainThreadDelegate;
		private readonly Dictionary<INotifyThreadStatus, ToolStripItem> _statusLabels = new Dictionary<INotifyThreadStatus, ToolStripItem>();

		//background workers
		private DynamoDbLoader _loader;
		private DownloaderWorker _downloader;


		private readonly Dictionary<string, Action<StatusChangedEventArgs>> _specificHandlers;
		

		public Mainform()
		{
			_specificHandlers = new Dictionary<string, Action<StatusChangedEventArgs>>()
			{
				{"DynamoDBConnection" , DynamoDbStatus}
			};
			InitializeComponent();
		}

		private void Mainform_Shown(object sender, EventArgs e)
		{
			if (!PersistentState.Setup)
			{
				SetupForm setupForm = new SetupForm();
				DialogResult result = setupForm.ShowDialog();

				if (result != DialogResult.OK)
				{
					Application.Exit();
					return;
				}

				PersistentState.Initialize(setupForm.Id, setupForm.Key, setupForm.BulkData);

			}

			_loader = new DynamoDbLoader();
			_loader.StatusChanged += OtherThreadNotificationHandler;

			_downloader = new DownloaderWorker();
			_downloader.StatusChanged += OtherThreadNotificationHandler;

			_mainThreadDelegate = MainThreadAction;

			//lstCrashes.View = View.Details;


			foreach (CrashReport crashReport in PersistentState.Database.GetAllCrashes())
			{
				_crashReports.Add(crashReport);
			}
			
			_bldCrashReports = new BindingListDisplay<CrashReport>(_crashReports, c => new CrashReportView(c, _downloader), true)
			{
				Anchor  = AnchorStyles.Bottom | AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Left,
				Location = new Point(12, 69),
				Size = new Size(863, 277),
			};

			_bldCrashReports.Sort(new CrashReportTimeStampFilter());
			
			tabReports.Controls.Add(_bldCrashReports);

			string automation = PersistentState.Database.GetKey("autodownload_automation");
			if (automation != null)
			{
				cboAutomation.SelectedIndex = int.Parse(automation);
			}

			UpdateDBDependantControls();

		}

		private void UpdateDBDependantControls()
		{
			object o = cboBuild.SelectedItem;
			cboBuild.Items.Clear();
			cboBuild.Items.AddRange(PersistentState.Database.GetAllBuildTypes().ToArray());

			if (o != null) cboBuild.SelectedItem = o;

			o = cboVersion.SelectedItem;
			cboVersion.Items.Clear();
			cboVersion.Items.AddRange(PersistentState.Database.GetAllVersions().OrderByDescending(v => v).ToArray());

			if (o != null) cboVersion.SelectedItem = o;

		}

		//This is used to subscribe to an action happening on another thread. Least ugly way i know to re-route it to ui thread
		private void OtherThreadNotificationHandler(INotifyThreadStatus sender, StatusChangedEventArgs args)
		{
			try
			{
				if (Disposing || IsDisposed) return;

				Invoke(_mainThreadDelegate, sender, args);
			}
			catch
			{
				// ignored
			}
		}

		private void MainThreadAction(INotifyThreadStatus sender, StatusChangedEventArgs args)
		{
			ToolStripItem item;
			if (_statusLabels.TryGetValue(sender, out item))
			{
				item.Text =  $"{sender.Name}: {args.Status}";
			}
			else
			{
				item = tsBackground.Items.Add($"{sender.Name}: {args.Status}");
				_statusLabels.Add(sender, item);
			}

			Action<StatusChangedEventArgs> action;
			if (_specificHandlers.TryGetValue(sender.Name, out action))
			{
				action(args);
			}
		}

		private void DynamoDbStatus(StatusChangedEventArgs statusChangedEventArgs)
		{
			List<Guid> list = statusChangedEventArgs.AttachedData;

			if (list == null)
				return;

			foreach (Guid guid in list)
			{
				CrashReport item = PersistentState.Database.GetCrash(guid);
				if(item != null) _crashReports.Add(item);
			}

			UpdateDBDependantControls();
		}

		private void deleteDatabaserequiresRestartToolStripMenuItem_Click(object sender, EventArgs e)
		{
			PersistentState.Database.Delete();
			Application.Restart();
		}

		private void cboAutomation_SelectedIndexChanged(object sender, EventArgs e)
		{
			PersistentState.Database.SetKey("autodownload_automation", cboAutomation.SelectedIndex.ToString());
		}

		private void SearchParameterChanged(object sender, EventArgs e)
		{
			_bldCrashReports.Filter(report => TextFilter(report, txtSearch.Text) && OtherFilter(report), true);
		}

		private bool OtherFilter(CrashReport report)
		{
			bool versionOk = true;
			bool buildOk = true;

			if (cboVersion.SelectedItem != null)
			{
				if (!report.Version.Equals(cboVersion.SelectedItem))
					versionOk = false;
			}

			if (cboBuild.SelectedItem != null)
			{
				if (!report.BuildType.Equals(cboBuild.SelectedItem))
					buildOk = false;
			}

			return buildOk && versionOk;

		}

		private bool TextFilter(CrashReport report, string search)
		{
			if (report.Guid.ToString().Contains(search)) return true;

			if (report.ErrorFrindly.Contains(search)) return true;

			if (report.StackTrace?.Contains(search) ?? false) return true;

			if (report.Userstory?.Contains(search) ?? false) return false;
			
			return false;
		}
	}

	internal class CrashReportTimeStampFilter : IComparer<CrashReport>
	{
		public int Compare(CrashReport x, CrashReport y)
		{
			return y.Timestamp.CompareTo(x.Timestamp);
		}
	}
}
