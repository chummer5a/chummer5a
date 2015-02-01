using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace Chummer
{
	public partial class frmUpdate : Form
	{
		private bool _blnSilentMode = false;
		private bool _blnExeDownloaded = false;
		private bool _blnExeDownloadSuccess = true;
		private bool _blnSkip = false;
		private int _intFileCount = 0;
		private int _intDoneCount = 0;
        private XmlDocument _objXmlDocument;
        private CommonFunctions objFunctions = new CommonFunctions();

		#region Control Methods
		public frmUpdate()
		{
            objFunctions.LogWrite(CommonFunctions.LogType.Entering, "frmUpdate", "Instantiate");
            InitializeComponent();
			LanguageManager.Instance.Load(GlobalOptions.Instance.Language, this);
			MoveControls();
            objFunctions.LogWrite(CommonFunctions.LogType.Exiting, "frmUpdate", "Instantiate");
        }

		private void frmUpdate_Load(object sender, EventArgs e)
		{
            objFunctions.LogWrite(CommonFunctions.LogType.Entering, "frmUpdate", "frmUpdate_Load");
            // Count the number of instances of Chummer that are currently running.
            objFunctions.LogWrite(CommonFunctions.LogType.Message, "frmUpdate", "Get process list");
            string strFileName = Process.GetCurrentProcess().MainModule.FileName;
            objFunctions.LogWrite(CommonFunctions.LogType.Message, "frmUpdate", "Get Chummer process count");
            int intCount = 0;
			foreach (Process objProcess in Process.GetProcesses())
			{
				try
				{
					if (objProcess.MainModule.FileName == strFileName)
						intCount++;
				}
				catch
				{
				}
			}

            objFunctions.LogWrite(CommonFunctions.LogType.Content, "frmUpdate", "intCount = " + intCount.ToString());
            // If there is more than 1 instance running, do not let the application be updated.
			if (intCount > 1)
			{
                objFunctions.LogWrite(CommonFunctions.LogType.Message, "frmUpdate", "More than one instance, exiting");
                if (!_blnSilentMode)
					MessageBox.Show(LanguageManager.Instance.GetString("Message_Update_MultipleInstances"), LanguageManager.Instance.GetString("Title_Update"), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                objFunctions.LogWrite(CommonFunctions.LogType.Exiting, "frmUpdate", "frmUpdate_Load");
                this.Close();
			}

			//if (Application.StartupPath.Contains("\\Debug"))
			//{
			//    MessageBox.Show("Cannot run Update from a debug path.");
			//    this.Close();
			//}

            objFunctions.LogWrite(CommonFunctions.LogType.Message, "frmUpdate", "Fetch XML");
            FetchXML();
            objFunctions.LogWrite(CommonFunctions.LogType.Exiting, "frmUpdate", "frmUpdate_Load");
        }

		private void cmdUpdate_Click(object sender, EventArgs e)
		{
            objFunctions.LogWrite(CommonFunctions.LogType.Entering, "frmUpdate", "cmdUpdate_Click");
            // Make sure updates have been selected before attempting to download anything.
            objFunctions.LogWrite(CommonFunctions.LogType.Message, "frmUpdate", "Verify at least one file has been selected");
            bool blnUpdatesSelected = false;
			foreach (TreeNode objNode in treeUpdate.Nodes)
			{
				foreach (TreeNode objChild in objNode.Nodes)
				{
					if (objChild.Checked)
					{
						blnUpdatesSelected = true;
						break;
					}
				}
			}

            objFunctions.LogWrite(CommonFunctions.LogType.Content, "frmUpdate", "blnUpdatesSelected = " + blnUpdatesSelected.ToString());
            // No updates have been selected, so display a message.
			if (!blnUpdatesSelected)
			{
                objFunctions.LogWrite(CommonFunctions.LogType.Message, "frmUpdate", "No files selected");
                MessageBox.Show(LanguageManager.Instance.GetString("Message_Update_NoUpdatesSelected"), LanguageManager.Instance.GetString("Title_Update"), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                objFunctions.LogWrite(CommonFunctions.LogType.Exiting, "frmUpdate", "cmdUpdate_Click");
                return;
			}

			// If we've made it this far, there's stuff to download, so go do it.
            objFunctions.LogWrite(CommonFunctions.LogType.Message, "frmUpdate", "Download updates");
            DownloadUpdates();
            objFunctions.LogWrite(CommonFunctions.LogType.Exiting, "frmUpdate", "cmdUpdate_Click");
        }

		private void cmdSelectAll_Click(object sender, EventArgs e)
		{
            objFunctions.LogWrite(CommonFunctions.LogType.Entering, "frmUpdate", "cmdSelectAll_Click");
            // Select all of the items in the Tree.
			foreach (TreeNode nodItem in treeUpdate.Nodes)
			{
				nodItem.Checked = true;
			}
            objFunctions.LogWrite(CommonFunctions.LogType.Exiting, "frmUpdate", "cmdSelectAll_Click");
        }

		private void treeUpdate_AfterCheck(object sender, TreeViewEventArgs e)
		{
            objFunctions.LogWrite(CommonFunctions.LogType.Entering, "frmUpdate", "treeUpdate_AfterCheck");
            // Select/deselect all of the child nodes if a root node is checked/unchecked.
			TreeNode nodClicked = e.Node;
            objFunctions.LogWrite(CommonFunctions.LogType.Message, "frmUpdate", "Select/deselect child nodes");
            foreach (TreeNode nodNode in nodClicked.Nodes)
			{
				nodNode.Checked = nodClicked.Checked;
			}

			// chummer5.exe and lang/en-us.xml must always match.
            objFunctions.LogWrite(CommonFunctions.LogType.Message, "frmUpdate", "Make sure Chummer5.exe and lang/en-us.xml match");
            if (!_blnSkip)
			{
				_blnSkip = true;
				if (nodClicked.Tag != null)
				{
					if (nodClicked.Tag.ToString() == "chummer5.exe")
					{
						foreach (TreeNode objNode in treeUpdate.Nodes)
						{
							foreach (TreeNode objChild in objNode.Nodes)
							{
								if (objChild.Tag.ToString() == "lang/en-us.xml")
								{
									objChild.Checked = nodClicked.Checked;
									break;
								}
							}
						}
					}
					if (nodClicked.Tag.ToString() == "lang/en-us.xml")
					{
						foreach (TreeNode objNode in treeUpdate.Nodes)
						{
							foreach (TreeNode objChild in objNode.Nodes)
							{
								if (objChild.Tag.ToString() == "chummer5.exe")
								{
									objChild.Checked = nodClicked.Checked;
									break;
								}
							}
						}
					}
				}
				_blnSkip = false;
			}
            objFunctions.LogWrite(CommonFunctions.LogType.Exiting, "frmUpdate", "treeUpdate_AfterCheck");
        }

		private void treeUpdate_AfterSelect(object sender, TreeViewEventArgs e)
		{
            objFunctions.LogWrite(CommonFunctions.LogType.Entering, "frmUpdate", "treeUpdate_AfterSelect");
            // Display the description of the update.
			try
			{
				if (e.Node.ToolTipText != "")
					webNotes.DocumentText = "<span style=\"font-family: verdana, arial; font-size: 12px; line-height: 150%;\">" + e.Node.ToolTipText.Replace("\n", "<br/>") + "</span>";
			}
			catch
			{
			}
            objFunctions.LogWrite(CommonFunctions.LogType.Exiting, "frmUpdate", "treeUpdate_AfterSelect");
        }

		private void cmdRestart_Click(object sender, EventArgs e)
		{
            objFunctions.LogWrite(CommonFunctions.LogType.Entering, "frmUpdate", "cmdRestart_Click");
            // Restart the application.
			Application.Restart();
		}
		#endregion

		#region Custom Methods
		/// <summary>
		/// When running in silent mode, the update window will not be shown.
		/// </summary>
		public bool SilentMode
		{
			get
			{
				return _blnSilentMode;
			}
			set
			{
				_blnSilentMode = value;
			}
		}

		/// <summary>
		/// Retrieve the manifestdata.xml file and determine if any updates are available.
		/// </summary>
		private void FetchXML()
		{
            objFunctions.LogWrite(CommonFunctions.LogType.Entering, "frmUpdate", "FetchXML");
            treeUpdate.Nodes.Clear();

			XmlDocument objXmlDocument = new XmlDocument();
			XmlDocument objXmlFileDocument = new XmlDocument();
			XmlNode objXmlFileNode;
			string strLastType = "";

			XmlDocument objXmlLanguageDocument = new XmlDocument();

            objFunctions.LogWrite(CommonFunctions.LogType.Message, "frmUpdate", "Get manifestdata.xml");
            // Download the manifestdata.xml file which describes all of the files available for download and extract its nodes.
			// Timeout set to 30 seconds.
            HttpWebRequest objRequest = (HttpWebRequest)WebRequest.Create("https://docs.google.com/uc?export=download&id=0B1j5wMS6KHqMQzRxYmlJWEVzQVE");
			//HttpWebRequest objRequest = (HttpWebRequest)WebRequest.Create("http://localhost/manifestdata.xml");
			objRequest.Timeout = 30000;
			HttpWebResponse objResponse;
			StreamReader objReader;

			// Exceptions happen when the request times out or the file is not found. In either case, display a message saying the update
			// information is temporarily unavailable.
			try
			{
				objResponse = (HttpWebResponse)objRequest.GetResponse();
				objReader = new StreamReader(objResponse.GetResponseStream());
                objFunctions.LogWrite(CommonFunctions.LogType.Message, "frmUpdate", "Got manifestdata.xml");
            }
			catch (Exception ex)
			{
                objFunctions.LogWrite(CommonFunctions.LogType.Message, "frmUpdate", "Failed to retrieve manifestdata.xml");
                objFunctions.LogWrite(CommonFunctions.LogType.Error, "frmUpdate", "ERROR Message = " + ex.Message);
                objFunctions.LogWrite(CommonFunctions.LogType.Error, "frmUpdate", "ERROR Source  = " + ex.Source);
                objFunctions.LogWrite(CommonFunctions.LogType.Error, "frmUpdate", "ERROR Trace   = " + ex.StackTrace.ToString());
                // Don't show the error message if we're running in silent mode.
				if (!_blnSilentMode)
					MessageBox.Show(LanguageManager.Instance.GetString("Message_Update_CannotConnect"), LanguageManager.Instance.GetString("Title_Update"), MessageBoxButtons.OK, MessageBoxIcon.Error);

                objFunctions.LogWrite(CommonFunctions.LogType.Exiting, "frmUpdate", "FetchXML");
				this.Close();
				return;
			}

			// Load the downloaded manifestdata.xml file.
			objXmlDocument.Load(objReader);

            objFunctions.LogWrite(CommonFunctions.LogType.Message, "frmUpdate", "Get manifestlang.xml");
            // Download the manifestlang.xml file which describes the language content available for download.
            objRequest = (HttpWebRequest)WebRequest.Create("https://docs.google.com/uc?export=download&id=0B1j5wMS6KHqMUFFTTWo2dC01S0k");
			// objRequest = (HttpWebRequest)WebRequest.Create("http://localhost/manifestlang.xml");
			try
			{
				objResponse = (HttpWebResponse)objRequest.GetResponse();
				objReader = new StreamReader(objResponse.GetResponseStream());
                objFunctions.LogWrite(CommonFunctions.LogType.Message, "frmUpdate", "Got manifestdata.xml");
            }
			catch (Exception ex)
			{
                objFunctions.LogWrite(CommonFunctions.LogType.Message, "frmUpdate", "Failed to retrieve manifestlang.xml");
                objFunctions.LogWrite(CommonFunctions.LogType.Error, "frmUpdate", "ERROR Message = " + ex.Message);
                objFunctions.LogWrite(CommonFunctions.LogType.Error, "frmUpdate", "ERROR Source  = " + ex.Source);
                objFunctions.LogWrite(CommonFunctions.LogType.Error, "frmUpdate", "ERROR Trace   = " + ex.StackTrace.ToString());
                // Don't show the error message if we're running in silent mode.
				if (!_blnSilentMode)
					MessageBox.Show(LanguageManager.Instance.GetString("Message_Update_CannotConnect"), LanguageManager.Instance.GetString("Title_Update"), MessageBoxButtons.OK, MessageBoxIcon.Error);

                objFunctions.LogWrite(CommonFunctions.LogType.Exiting, "frmUpdate", "FetchXML");
                this.Close();
				return;
			}

			// Merge the manifests together into a single usable XmlDocument.
            objFunctions.LogWrite(CommonFunctions.LogType.Message, "frmUpdate", "Merge the two manifests");
            objXmlLanguageDocument.Load(objReader);
			XmlNodeList objXmlList = objXmlLanguageDocument.SelectNodes("/manifest/*");
			foreach (XmlNode objNode in objXmlList)
			{
				XmlNode objImported = objXmlDocument.ImportNode(objNode, true);
				objXmlDocument.DocumentElement.AppendChild(objImported);
			}

            objFunctions.LogWrite(CommonFunctions.LogType.Message, "frmUpdate", "Get the file list");
            XmlNodeList objXmlNodeList = objXmlDocument.SelectNodes("/manifest/file");

			TreeNode nodRoot = new TreeNode();
            _objXmlDocument = objXmlDocument;
            objFunctions.LogWrite(CommonFunctions.LogType.Content, "frmUpdate", "_objXmlDocument = " + _objXmlDocument.InnerXml.ToString());

			foreach (XmlNode objXmlNode in objXmlNodeList)
			{
                objFunctions.LogWrite(CommonFunctions.LogType.Content, "frmUpdate", "objXmlNode = " + objXmlNode["name"].InnerText.ToString());
                // A new type has been found, so attach the current root node to the tree and start a new one.
				if (objXmlNode["type"].InnerText != strLastType)
				{
					if (strLastType != "")
					{
						nodRoot.ExpandAll();
						if (nodRoot.GetNodeCount(false) > 0)
							treeUpdate.Nodes.Add(nodRoot);
						nodRoot = new TreeNode();
					}
					nodRoot.Text = objXmlNode["type"].InnerText;
					strLastType = objXmlNode["type"].InnerText;
				}

				// If we're on the EXE file, check the version numbers.
				bool blnCreateNode = true;
				if (objXmlNode["name"].InnerText.Contains(".exe"))
				{
                    objFunctions.LogWrite(CommonFunctions.LogType.Message, "frmUpdate", "File is EXE");
                    if (objXmlNode["name"].InnerText == "Chummer5.exe")
                    {
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "frmUpdate", "File is Chummer5.exe");
                        objFunctions.LogWrite(CommonFunctions.LogType.Content, "frmUpdate", "Existing Version = " + Convert.ToInt32(Application.ProductVersion.Replace(".", string.Empty)).ToString());
                        objFunctions.LogWrite(CommonFunctions.LogType.Content, "frmUpdate", "New Version = " + Convert.ToInt32(objXmlNode["version"].InnerText.Replace(".", string.Empty)).ToString());
                        if (Convert.ToInt32(objXmlNode["version"].InnerText.Replace(".", string.Empty)) > Convert.ToInt32(Application.ProductVersion.Replace(".", string.Empty)))
						    blnCreateNode = true;
					    else
						    blnCreateNode = false;
                    }
                    else
                    {
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "frmUpdate", "File is another EXE");
                        FileVersionInfo myFileVersionInfo = FileVersionInfo.GetVersionInfo(Application.StartupPath + Path.DirectorySeparatorChar + objXmlNode["name"].InnerText);
                        string strVersion = myFileVersionInfo.FileVersion.ToString().Replace(".", "");
                        int intVersion = Convert.ToInt32(strVersion);
                        objFunctions.LogWrite(CommonFunctions.LogType.Content, "frmUpdate", "Existing Version = " + intVersion.ToString());
                        objFunctions.LogWrite(CommonFunctions.LogType.Content, "frmUpdate", "New Version = " + Convert.ToInt32(objXmlNode["version"].InnerText.Replace(".", string.Empty)).ToString());

                        if (Convert.ToInt32(objXmlNode["version"].InnerText.Replace(".", string.Empty)) > intVersion)
                            blnCreateNode = true;
                        else
                            blnCreateNode = false;
                    }
				}

				// If we're on an XML file, check for the existing file and compare version numbers.
				if (objXmlNode["name"].InnerText.Contains(".xml"))
				{
                    objFunctions.LogWrite(CommonFunctions.LogType.Message, "frmUpdate", "File is XML");
                    try
					{
						objXmlFileDocument.Load(Application.StartupPath + Path.DirectorySeparatorChar + objXmlNode["name"].InnerText.Replace('/', Path.DirectorySeparatorChar));
						objXmlFileNode = objXmlFileDocument.SelectSingleNode("/chummer/version");
                        objFunctions.LogWrite(CommonFunctions.LogType.Content, "frmUpdate", "Existing Version = " + Convert.ToInt32(objXmlFileNode.InnerText).ToString());
                        objFunctions.LogWrite(CommonFunctions.LogType.Content, "frmUpdate", "New Version = " + Convert.ToInt32(objXmlNode["version"].InnerText).ToString());
                        if (Convert.ToInt32(objXmlNode["version"].InnerText) > Convert.ToInt32(objXmlFileNode.InnerText))
							blnCreateNode = true;
						else
							blnCreateNode = false;
					}
					catch (Exception ex)
					{
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "frmUpdate", "Failed to get the existing version number");
                        objFunctions.LogWrite(CommonFunctions.LogType.Error, "frmUpdate", "ERROR Message = " + ex.Message);
                        objFunctions.LogWrite(CommonFunctions.LogType.Error, "frmUpdate", "ERROR Source  = " + ex.Source);
                        objFunctions.LogWrite(CommonFunctions.LogType.Error, "frmUpdate", "ERROR Trace   = " + ex.StackTrace.ToString());
                        blnCreateNode = true;
					}

					// Check for localisation limitations.
					if (GlobalOptions.Instance.LocalisedUpdatesOnly)
					{
						// Only check if we're looking at a non-en-us language file.
						if (objXmlNode["name"].InnerText.StartsWith("lang") && !objXmlNode["name"].InnerText.Contains("en-us"))
						{
							if (!objXmlNode["name"].InnerText.EndsWith(GlobalOptions.Instance.Language.Substring(0, 2) + ".xml") && !objXmlNode["name"].InnerText.EndsWith(GlobalOptions.Instance.Language.Substring(0, 2) + "_data.xml"))
								blnCreateNode = false;
						}
					}
				}

				// If we're on an XSL file, check for the existing file and compare version numbers.
				if (objXmlNode["name"].InnerText.Contains(".xsl"))
				{
                    objFunctions.LogWrite(CommonFunctions.LogType.Message, "frmUpdate", "File is XSL");
                    try
					{
						StreamReader objFile = new StreamReader(Application.StartupPath + Path.DirectorySeparatorChar + objXmlNode["name"].InnerText.Replace('/', Path.DirectorySeparatorChar));
						string strLine = "";
						while ((strLine = objFile.ReadLine()) != null)
						{
							if (strLine.Contains("<!-- Version"))
							{
								int intVersion = Convert.ToInt32(strLine.Replace("<!-- Version ", string.Empty).Replace(" -->", string.Empty));
                                objFunctions.LogWrite(CommonFunctions.LogType.Content, "frmUpdate", "Existing Version = " + intVersion.ToString());
                                objFunctions.LogWrite(CommonFunctions.LogType.Content, "frmUpdate", "New Version = " + Convert.ToInt32(objXmlNode["version"].InnerText).ToString());
                                if (intVersion < Convert.ToInt32(objXmlNode["version"].InnerText))
									blnCreateNode = true;
								else
									blnCreateNode = false;
								break;
							}
						}
						objFile.Close();
					}
					catch(Exception ex)
					{
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "frmUpdate", "Failed to get the existing version number");
                        objFunctions.LogWrite(CommonFunctions.LogType.Error, "frmUpdate", "ERROR Message = " + ex.Message);
                        objFunctions.LogWrite(CommonFunctions.LogType.Error, "frmUpdate", "ERROR Source  = " + ex.Source);
                        objFunctions.LogWrite(CommonFunctions.LogType.Error, "frmUpdate", "ERROR Trace   = " + ex.StackTrace.ToString());
                        blnCreateNode = true;
					}

					// Check for localisation limitations.
					if (GlobalOptions.Instance.LocalisedUpdatesOnly)
					{
						// Only check if we're looking at a non-en-us sheet file.
						string[] strSplit = objXmlNode["name"].InnerText.Split('/');
						if (strSplit.Length > 2)
						{
							if (!objXmlNode["name"].InnerText.Contains("/" + GlobalOptions.Instance.Language.Substring(0, 2) + "/"))
								blnCreateNode = false;
						}
					}
				}

				if (blnCreateNode)
				{
                    objFunctions.LogWrite(CommonFunctions.LogType.Message, "frmUpdate", "Will Create Node");
                    TreeNode nodNode = new TreeNode();
					nodNode.Text = objXmlNode["description"].InnerText;
					nodNode.Tag = objXmlNode["name"].InnerText;
					nodNode.ToolTipText = objXmlNode["notes"].InnerText;
					if (_blnSilentMode)
					{
						if (!objXmlNode["name"].InnerText.Contains("lang/"))
						{
							// Automatically select any file that is not a language file.
							nodNode.Checked = true;
						}
						else
						{
							bool blnChecked = false;
							// Automatically select the default English file.
							if (objXmlNode["name"].InnerText == "lang/en-us.xml")
								blnChecked = true;
							else
							{
								// If this is a non-English language file, only select it if the user already has it installed.
								string strLangPath = Path.Combine(Application.StartupPath, objXmlNode["name"].InnerText.Replace('/', Path.DirectorySeparatorChar));
								if (File.Exists(strLangPath))
									blnChecked = true;
							}
							nodNode.Checked = blnChecked;
						}
					}
                    objFunctions.LogWrite(CommonFunctions.LogType.Message, "frmUpdate", "Create Node - " + nodNode.Tag.ToString());
                    nodRoot.Nodes.Add(nodNode);
				}
			}

            objFunctions.LogWrite(CommonFunctions.LogType.Message, "frmUpdate", "Close the reader");
            objReader.Close();

			// Attach the last root node to the tree.
            objFunctions.LogWrite(CommonFunctions.LogType.Message, "frmUpdate", "Expand all nodes");
            nodRoot.ExpandAll();
			if (nodRoot.GetNodeCount(false) > 0)
				treeUpdate.Nodes.Add(nodRoot);

			if (treeUpdate.GetNodeCount(false) == 0)
			{
                objFunctions.LogWrite(CommonFunctions.LogType.Message, "frmUpdate", "No new updates");
                this.Visible = false;
				if (!_blnSilentMode)
					MessageBox.Show(LanguageManager.Instance.GetString("Message_Update_NoNewUpdates"), LanguageManager.Instance.GetString("Title_Update"), MessageBoxButtons.OK, MessageBoxIcon.Information);
				this.Close();
			}
			else
                if (_blnSilentMode)
                {
                    objFunctions.LogWrite(CommonFunctions.LogType.Message, "frmUpdate", "Download updates");
                    DownloadUpdates();
                }
                else
                {
                    objFunctions.LogWrite(CommonFunctions.LogType.Message, "frmUpdate", "Show frmUpdate with available updates");
                    this.Opacity = 100;
                    this.Show();
                }

			// Close the connection now that we're done with it.
			objResponse.Close();
            objFunctions.LogWrite(CommonFunctions.LogType.Exiting, "frmUpdate", "FetchXML");
        }

		/// <summary>
		/// Download the selected updates and overwrite the existing files.
		/// </summary>
		private void DownloadUpdates()
		{
            objFunctions.LogWrite(CommonFunctions.LogType.Entering, "frmUpdate", "DownloadUpdates");
            cmdUpdate.Enabled = false;
			cmdSelectAll.Enabled = false;

			// Determine the temporary location for the new executable if it is downloaded.
			string strNewPath = Path.Combine(Path.GetTempPath(), "chummer5.exe");
			string strFilePath = Application.StartupPath + Path.DirectorySeparatorChar;
            objFunctions.LogWrite(CommonFunctions.LogType.Content, "frmUpdate", "Temp path = " + strNewPath);

			WebClient wc = new WebClient();

			// Count all of the nodes that have been selected to determine the progress bar's max value.
			pgbOverallProgress.Value = 0;
			pgbOverallProgress.Minimum = 0;
			pgbOverallProgress.Maximum = 0;
			foreach (TreeNode nodRoot in treeUpdate.Nodes)
			{
				foreach (TreeNode nodNode in nodRoot.Nodes)
				{
					if (nodNode.Checked)
					{
						pgbOverallProgress.Maximum++;
						_intFileCount++;
					}
				}
			}

            objFunctions.LogWrite(CommonFunctions.LogType.Content, "frmUpdate", "File Count = " + _intFileCount.ToString());
            if (_blnSilentMode)
			{
				// If nothing is selected (Language files that the user does not have installed), close the window.
				if (pgbOverallProgress.Maximum == 0)
				{
                    objFunctions.LogWrite(CommonFunctions.LogType.Exiting, "frmUpdate", "DownloadUpdates");
                    this.Close();
					return;
				}

				this.Opacity = 100;
				this.Show();
			}

			// Loop through all of the root-level nodes in the update tree.
			foreach (TreeNode nodRoot in treeUpdate.Nodes)
			{
				// Loop through each of the child nodes.
				foreach (TreeNode nodNode in nodRoot.Nodes)
				{
					if (nodNode.Checked)
					{
						// If an item has been checked, download it.
						wc = new WebClient();
						pgbFileProgress.Value = 0;
						wc.DownloadProgressChanged += wc_DownloadProgressChanged;

                        objFunctions.LogWrite(CommonFunctions.LogType.Content, "frmUpdate", "nodNode = " + nodNode.Tag.ToString());
                        if (nodNode.Tag.ToString().Contains(".xml") || nodNode.Tag.ToString().Contains(".xsl"))
						{
                            objFunctions.LogWrite(CommonFunctions.LogType.Message, "frmUpdate", "File is XML or XSL");
                            // Make sure the target directory exists. If it doesn't, create it.
							string[] strCheckDirectory = (strFilePath + nodNode.Tag.ToString().Replace('/', Path.DirectorySeparatorChar)).Split(Path.DirectorySeparatorChar);
							StringBuilder strDirectory = new StringBuilder();
							for (int i = 0; i < strCheckDirectory.Length - 1; i++)
								strDirectory.Append(strCheckDirectory[i]).Append(Path.DirectorySeparatorChar);
							if (!Directory.Exists(strDirectory.ToString()))
								Directory.CreateDirectory(strDirectory.ToString());

							// Downloading an XML or XSL file.
							wc.Encoding = Encoding.UTF8;
							wc.DownloadFileCompleted += wc_DownloadFileCompleted;
                            XmlNode objNode = _objXmlDocument.SelectSingleNode("/manifest/file[name=\"" + nodNode.Tag.ToString() + "\"]/url");
                            string strFile = objNode.InnerText;
                            objFunctions.LogWrite(CommonFunctions.LogType.Content, "frmUpdate", "strFile = " + strFile);
                            wc.DownloadFileAsync(new Uri(strFile), strFilePath + nodNode.Tag.ToString().Replace('/', Path.DirectorySeparatorChar));
						}
						else
						{
							if(nodNode.Tag.ToString().Contains(".exe"))
							{
								// Download the appliation changelog.
								try
								{
									wc.Encoding = Encoding.UTF8;
                                    objFunctions.LogWrite(CommonFunctions.LogType.Message, "frmUpdate", "Download the changelog");
                                    wc.DownloadFile("https://docs.google.com/uc?export=download&id=0B1j5wMS6KHqMODljM1JXdHcxRjg", Path.Combine(Application.StartupPath, "changelog.txt"));
									webNotes.DocumentText = "<font size=\"-1\" face=\"Courier New,Serif\">" + File.ReadAllText(Path.Combine(Application.StartupPath, "changelog.txt")).Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\n", "<br />") + "</font>";
								}
								catch (Exception ex)
								{
									// Not a critical file, so don't freak out if it can't be downloaded.
                                    objFunctions.LogWrite(CommonFunctions.LogType.Message, "frmUpdate", "Failed to download the file");
                                    objFunctions.LogWrite(CommonFunctions.LogType.Error, "frmUpdate", "ERROR Message = " + ex.Message);
                                    objFunctions.LogWrite(CommonFunctions.LogType.Error, "frmUpdate", "ERROR Source  = " + ex.Source);
                                    objFunctions.LogWrite(CommonFunctions.LogType.Error, "frmUpdate", "ERROR Trace   = " + ex.StackTrace.ToString());
                                }

								// Downloading the application executable file.
								try
								{
									wc.Encoding = Encoding.Default;
                                    objFunctions.LogWrite(CommonFunctions.LogType.Message, "frmUpdate", "Set the download complete event");
                                    if (nodNode.Tag.ToString() == "Chummer5.exe")
									    wc.DownloadFileCompleted += wc_DownloadExeFileCompleted;
									wc.DownloadFileCompleted += wc_DownloadFileCompleted;
                                    XmlNode objNode = _objXmlDocument.SelectSingleNode("/manifest/file[name=\"" + nodNode.Tag.ToString() + "\"]/url");
                                    string strFile = objNode.InnerText;
                                    objFunctions.LogWrite(CommonFunctions.LogType.Content, "frmUpdate", "strFile = " + strFile);
                                    wc.DownloadFileAsync(new Uri(strFile), strNewPath);
								}
								catch (Exception ex)
								{
									// The executable couldn't be downloaded, so don't try to replace the current app with something that doesn't exist.
                                    objFunctions.LogWrite(CommonFunctions.LogType.Message, "frmUpdate", "Failed to download the file");
                                    objFunctions.LogWrite(CommonFunctions.LogType.Error, "frmUpdate", "ERROR Message = " + ex.Message);
                                    objFunctions.LogWrite(CommonFunctions.LogType.Error, "frmUpdate", "ERROR Source  = " + ex.Source);
                                    objFunctions.LogWrite(CommonFunctions.LogType.Error, "frmUpdate", "ERROR Trace   = " + ex.StackTrace.ToString());
                                }
							}
						}
						Application.DoEvents();
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "frmUpdate", "Done with this file");
                    }
				}
			}
            objFunctions.LogWrite(CommonFunctions.LogType.Exiting, "frmUpdate", "DownloadUpdates");
        }

		/// <summary>
		/// Run through the files that were downloaded and make sure their size is more than 0 bytes.
		/// </summary>
		private bool ValidateFiles()
		{
            objFunctions.LogWrite(CommonFunctions.LogType.Entering, "frmUpdate", "ValidateFiles");
            bool blnReturn = true;
			string strFilePath = Application.StartupPath + Path.DirectorySeparatorChar;
			
			// Loop through all of the root-level nodes in the update tree.
			foreach (TreeNode nodRoot in treeUpdate.Nodes)
			{
				// Loop through each of the child nodes.
				foreach (TreeNode nodNode in nodRoot.Nodes)
				{
                    if (nodNode.Checked)
                    {
                        objFunctions.LogWrite(CommonFunctions.LogType.Content, "frmUpdate", "nodNode = " + nodNode.Tag.ToString());
                        try
                        {
                            FileInfo objInfo = new FileInfo(strFilePath + nodNode.Tag.ToString().Replace('/', Path.DirectorySeparatorChar));
                            if (objInfo.Length == 0)
                            {
                                objFunctions.LogWrite(CommonFunctions.LogType.Message, "frmUpdate", "The file is zero-length");
                                blnReturn = false;
                                break;
                            }

                            XmlDocument objXmlFileDocument = new XmlDocument();
                            objXmlFileDocument.Load(strFilePath + nodNode.Tag.ToString().Replace('/', Path.DirectorySeparatorChar));
                            blnReturn = true;
                        }
                        catch (Exception ex)
                        {
                            blnReturn = false;
                            objFunctions.LogWrite(CommonFunctions.LogType.Message, "frmUpdate", "Failed to download the file");
                            objFunctions.LogWrite(CommonFunctions.LogType.Error, "frmUpdate", "ERROR Message = " + ex.Message);
                            objFunctions.LogWrite(CommonFunctions.LogType.Error, "frmUpdate", "ERROR Source  = " + ex.Source);
                            objFunctions.LogWrite(CommonFunctions.LogType.Error, "frmUpdate", "ERROR Trace   = " + ex.StackTrace.ToString());
                        }
                    }
				}
			}

            objFunctions.LogWrite(CommonFunctions.LogType.Exiting, "frmUpdate", "ValidateFiles");
            return blnReturn && _blnExeDownloadSuccess;
		}

		/// <summary>
		/// Verify that the executable was successfully downloaded by checking its version number using FileVersionInfo.
		/// </summary>
		private bool ValidateExecutable()
		{
            objFunctions.LogWrite(CommonFunctions.LogType.Entering, "frmUpdate", "ValidateExecutable");
            bool blnReturn = true;

			try
			{
				string strVersion = FileVersionInfo.GetVersionInfo(Path.Combine(Path.GetTempPath(), "chummer5.exe")).ProductVersion;
                objFunctions.LogWrite(CommonFunctions.LogType.Content, "frmUpdate", "strVersion = " + strVersion);
            }
			catch (Exception ex)
			{
				blnReturn = false;
                objFunctions.LogWrite(CommonFunctions.LogType.Error, "frmUpdate", "ERROR Message = " + ex.Message);
                objFunctions.LogWrite(CommonFunctions.LogType.Error, "frmUpdate", "ERROR Source  = " + ex.Source);
                objFunctions.LogWrite(CommonFunctions.LogType.Error, "frmUpdate", "ERROR Trace   = " + ex.StackTrace.ToString());
            }

            objFunctions.LogWrite(CommonFunctions.LogType.Exiting, "frmUpdate", "ValidateExecutable");
            return blnReturn;
		}

		private void MoveControls()
		{
            objFunctions.LogWrite(CommonFunctions.LogType.Entering, "frmUpdate", "MoveControls");
            cmdRestart.Left = this.Width - 19 - cmdRestart.Width;
			cmdSelectAll.Left = cmdUpdate.Left + cmdUpdate.Width + 24;

			int intWidth = Math.Max(lblOverallProgress.Width, lblFileProgress.Width);
			pgbOverallProgress.Left = lblOverallProgress.Left + intWidth + 6;
			pgbOverallProgress.Width = this.Width - pgbOverallProgress.Left - 19;
			pgbFileProgress.Left = lblFileProgress.Left + intWidth + 6;
			pgbFileProgress.Width = this.Width - pgbFileProgress.Left - 19;
            objFunctions.LogWrite(CommonFunctions.LogType.Exiting, "frmUpdate", "MoveControls");
        }
		#endregion

		#region AsyncDownload Events
		/// <summary>
		/// Update the download progress for the file.
		/// </summary>
		private void wc_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
		{
            objFunctions.LogWrite(CommonFunctions.LogType.Entering, "frmUpdate", "wc_DownloadProgressChanged");
            pgbFileProgress.Value = e.ProgressPercentage;
            objFunctions.LogWrite(CommonFunctions.LogType.Exiting, "frmUpdate", "wc_DownloadProgressChanged");
        }

		/// <summary>
		/// The EXE file is down downloading, so replace the old file with the new one.
		/// </summary>
		private void wc_DownloadExeFileCompleted(object sender, AsyncCompletedEventArgs e)
		{
            objFunctions.LogWrite(CommonFunctions.LogType.Entering, "frmUpdate", "wc_DownloadExeFileCompleted");
            string strAppPath = Application.ExecutablePath;
			string strArchive = strAppPath + ".old";
			string strNewPath = Path.Combine(Path.GetTempPath(), "chummer5.exe");

            objFunctions.LogWrite(CommonFunctions.LogType.Message, "frmUpdate", "Validate the EXE");
            if (ValidateExecutable())
			{
                objFunctions.LogWrite(CommonFunctions.LogType.Message, "frmUpdate", "EXE validated");
                _blnExeDownloadSuccess = true;

				// Copy over the executable.
				try
				{
                    objFunctions.LogWrite(CommonFunctions.LogType.Content, "frmUpdate", "strArchive = " + strArchive);
                    objFunctions.LogWrite(CommonFunctions.LogType.Content, "frmUpdate", "strAppPath = " + strAppPath);
                    objFunctions.LogWrite(CommonFunctions.LogType.Content, "frmUpdate", "strNewPath = " + strNewPath);
                    objFunctions.LogWrite(CommonFunctions.LogType.Message, "frmUpdate", "Delete the file");
                    File.Delete(strArchive);
                    objFunctions.LogWrite(CommonFunctions.LogType.Message, "frmUpdate", "Move to archive");
                    File.Move(strAppPath, strArchive);
                    objFunctions.LogWrite(CommonFunctions.LogType.Message, "frmUpdate", "Move to app path");
                    File.Move(strNewPath, strAppPath);
                    objFunctions.LogWrite(CommonFunctions.LogType.Message, "frmUpdate", "Delete the file");
                    File.Delete(strNewPath);
					_blnExeDownloaded = true;
				}
				catch (Exception ex)
				{
                    objFunctions.LogWrite(CommonFunctions.LogType.Error, "frmUpdate", "ERROR Message = " + ex.Message);
                    objFunctions.LogWrite(CommonFunctions.LogType.Error, "frmUpdate", "ERROR Source  = " + ex.Source);
                    objFunctions.LogWrite(CommonFunctions.LogType.Error, "frmUpdate", "ERROR Trace   = " + ex.StackTrace.ToString());
                }
			}
			else
			{
                objFunctions.LogWrite(CommonFunctions.LogType.Message, "frmUpdate", "EXE FAILED validation");
                // If the executable did not validate, delete the downloaded copy and set the download success flag to false so it can be attempted again.
				_blnExeDownloaded = false;
				_blnExeDownloadSuccess = false;
                objFunctions.LogWrite(CommonFunctions.LogType.Message, "frmUpdate", "Delete the file");
                File.Delete(Path.Combine(Path.GetTempPath(), "chummer5.exe"));
                objFunctions.LogWrite(CommonFunctions.LogType.Message, "frmUpdate", "Move from archive");
                File.Move(strArchive, strNewPath);
			}
            objFunctions.LogWrite(CommonFunctions.LogType.Exiting, "frmUpdate", "wc_DownloadExeFileCompleted");
        }

		/// <summary>
		/// A file is done downloading, so increment the overall progress bar and check to see if all downloads are done.
		/// </summary>
		private void wc_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
		{
            objFunctions.LogWrite(CommonFunctions.LogType.Entering, "frmUpdate", "wc_DownloadFileCompleted");
            // Detach the event handlers once they're done so they don't continue to fire and/or consume memory.
			WebClient wc = new WebClient();
			wc = (WebClient)sender;
			wc.DownloadProgressChanged -= wc_DownloadProgressChanged;
			wc.DownloadFileCompleted -= wc_DownloadFileCompleted;

			_intDoneCount++;
			pgbOverallProgress.Value++;
            objFunctions.LogWrite(CommonFunctions.LogType.Content, "frmUpdate", "_intDoneCount = " + _intDoneCount.ToString());
            if (_intDoneCount == _intFileCount)
			{
				bool blnUnzipSuccess = true;

				// Unzip the data files that have been downloaded.
                //OmaeHelper objHelper = new OmaeHelper();
                //foreach (string strFile in Directory.GetFiles(Path.Combine(Application.StartupPath, "data"), "*.zip"))
                //{
                //    objFunctions.LogWrite(CommonFunctions.LogType.Content, "frmUpdate", "strFile = " + strFile);
                //    try
                //    {
                //        byte[] bytFile = File.ReadAllBytes(strFile);
                //        bytFile = objHelper.Decompress(bytFile);
                //        File.WriteAllBytes(strFile.Replace(".zip", ".xml"), bytFile);
                //        if (!strFile.Contains("\\Debug\\"))
                //            File.Delete(strFile);
                //    }
                //    catch (Exception ex)
                //    {
                //        blnUnzipSuccess = false;
                //        objFunctions.LogWrite(CommonFunctions.LogType.Error, "frmUpdate", "ERROR Message = " + ex.Message);
                //        objFunctions.LogWrite(CommonFunctions.LogType.Error, "frmUpdate", "ERROR Source  = " + ex.Source);
                //        objFunctions.LogWrite(CommonFunctions.LogType.Error, "frmUpdate", "ERROR Trace   = " + ex.StackTrace.ToString());
                //    }
                //}

				if (!_blnSilentMode)
				{
					// If the update process is not running in silent mode, display a message informing the user that the update is done.
					pgbOverallProgress.Visible = false;
					pgbFileProgress.Visible = false;
					lblDone.Visible = true;
				}

                objFunctions.LogWrite(CommonFunctions.LogType.Message, "frmUpdate", "Validate the files");
                if (ValidateFiles() && blnUnzipSuccess)
				{
                    objFunctions.LogWrite(CommonFunctions.LogType.Content, "frmUpdate", "_blnExeDownloaded = " + _blnExeDownloaded.ToString());
                    if (_blnExeDownloaded)
					{
                        cmdRestart.Visible = true;
						if (_blnSilentMode)
						{
							// The user is in silent mode, so restart the application.
                            objFunctions.LogWrite(CommonFunctions.LogType.Message, "frmUpdate", "Restart the application");
                            Application.Restart();
						}
					}

					// Close the window when we're done in Silent Mode.
                    if (_blnSilentMode)
                    {
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "frmUpdate", "Close the window");
                        this.Close();
                    }
				}
				else
				{
                    objFunctions.LogWrite(CommonFunctions.LogType.Message, "frmUpdate", "Validation failed");
                    // Show the progress bars again.
					pgbOverallProgress.Visible = true;
					pgbFileProgress.Visible = true;
					lblDone.Visible = false;

					// Something did not download properly.
                    if (_blnSilentMode)
                    {
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "frmUpdate", "Fetch the XML again");
                        FetchXML();
                    }
                    else
                    {
                        // Make a list of each item that is currently checked.
                        List<string> lstStrings = new List<string>();

                        // Loop through all of the root-level nodes in the update tree.
                        foreach (TreeNode nodRoot in treeUpdate.Nodes)
                        {
                            // Loop through each of the child nodes.
                            foreach (TreeNode nodNode in nodRoot.Nodes)
                            {
                                if (nodNode.Checked)
                                {
                                    objFunctions.LogWrite(CommonFunctions.LogType.Content, "frmUpdate", "nodNode.Tag = " + nodNode.Tag.ToString());
                                    lstStrings.Add(nodNode.Tag.ToString());
                                }
                            }
                        }

                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "frmUpdate", "Fetch the XML again");
                        FetchXML();

                        // Select all of the items that were selected before.
                        // Loop through all of the root-level nodes in the update tree.
                        foreach (string strString in lstStrings)
                        {
                            foreach (TreeNode nodRoot in treeUpdate.Nodes)
                            {
                                // Loop through each of the child nodes.
                                foreach (TreeNode nodNode in nodRoot.Nodes)
                                {
                                    if (nodNode.Tag.ToString() == strString)
                                    {
                                        objFunctions.LogWrite(CommonFunctions.LogType.Content, "frmUpdate", "nodNode.Tag = " + nodNode.Tag.ToString());
                                        nodNode.Checked = true;
                                    }
                                }
                            }
                        }

                        // Click the button to do it all again.
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "frmUpdate", "Call cmdUpdate_Click to do it again");
                        cmdUpdate_Click(null, null);
                    }
				}
			}
            objFunctions.LogWrite(CommonFunctions.LogType.Exiting, "frmUpdate", "wc_DownloadFileCompleted");
        }
		#endregion
	}
}