/*  This file is part of Chummer5a.
 *
 *  Chummer5a is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  Chummer5a is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with Chummer5a.  If not, see <http://www.gnu.org/licenses/>.
 *
 *  You can obtain the full source code for Chummer5a at
 *  https://github.com/chummer5a/chummer5a
 */

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;
using Org.XmlUnit.Builder;
using Org.XmlUnit.Diff;
using NLog;

namespace Chummer
{
    public partial class EditXmlData : Form
    {
        private static readonly Lazy<Logger> s_ObjLogger = new Lazy<Logger>(LogManager.GetCurrentClassLogger);
        private static Logger Log => s_ObjLogger.Value;

        private XmlDocument _objBaseXmlDocument;
        private XmlDocument _objAmendmentXmlDocument;
        private XmlDocument _objResultXmlDocument;
        private string _strBaseXmlContent;
        private string _strResultXmlContent;
        private string _strXmlElementTemplate = string.Empty;
        private bool _blnDirty;
        private bool _blnLoading = true;

        #region Form Events

        public EditXmlData()
        {
            InitializeComponent();
        }

        private async void EditXmlData_Load(object sender, EventArgs e)
        {
            try
            {
                await LoadAvailableXmlFilesAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error loading XML files");
                await Program.ShowScrollableMessageBoxAsync(this, ex.ToString(), LanguageManager.GetString("String_Error"), MessageBoxButtons.OK, MessageBoxIcon.Error).ConfigureAwait(false);
            }
            finally
            {
                _blnLoading = false;
            }
        }

        private async void cmdLoadXml_Click(object sender, EventArgs e)
        {
            try
            {
                await LoadSelectedXmlFileAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error loading XML file");
                await Program.ShowScrollableMessageBoxAsync(this, ex.ToString(), LanguageManager.GetString("String_Error"), MessageBoxButtons.OK, MessageBoxIcon.Error).ConfigureAwait(false);
            }
        }

        private async void cmdApplyAmendment_Click(object sender, EventArgs e)
        {
            try
            {
                await ApplyAmendmentAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error applying amendment");
                await Program.ShowScrollableMessageBoxAsync(this, ex.ToString(), LanguageManager.GetString("String_Error"), MessageBoxButtons.OK, MessageBoxIcon.Error).ConfigureAwait(false);
            }
        }

        private async void cmdSaveAmendment_Click(object sender, EventArgs e)
        {
            try
            {
                await SaveAmendmentAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error saving amendment");
                await Program.ShowScrollableMessageBoxAsync(this, ex.ToString(), LanguageManager.GetString("String_Error"), MessageBoxButtons.OK, MessageBoxIcon.Error).ConfigureAwait(false);
            }
        }

        private void txtAmendmentXml_TextChanged(object sender, EventArgs e)
        {
            if (!_blnLoading)
            {
                _blnDirty = true;
                cmdApplyAmendment.Enabled = _objBaseXmlDocument != null && !string.IsNullOrWhiteSpace(txtAmendmentXml.Text);
            }
        }

        private void EditXmlData_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_blnDirty)
            {
                DialogResult result = MessageBox.Show(
                    LanguageManager.GetString("XmlEditor_UnsavedChanges"),
                    LanguageManager.GetString("XmlEditor_UnsavedChangesTitle"),
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    try
                    {
                        //SaveModifiedXmlAsync().Wait();
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "Error saving on form close");
                        MessageBox.Show(string.Format(LanguageManager.GetString("XmlEditor_ErrorSaving"), ex.Message), LanguageManager.GetString("String_Error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                        e.Cancel = true;
                        return;
                    }
                }
                else if (result == DialogResult.Cancel)
                {
                    e.Cancel = true;
                    return;
                }
            }
        }

        #endregion Form Events

        #region Methods

        private async Task LoadAvailableXmlFilesAsync(CancellationToken token = default)
        {
            try
            {
                string strDataPath = Path.Combine(Utils.GetStartupPath, "data");
                if (!Directory.Exists(strDataPath))
                {
                    await Program.ShowScrollableMessageBoxAsync(this, LanguageManager.GetString("XmlEditor_DataDirectoryNotFound", token: token), LanguageManager.GetString("String_Error", token: token), MessageBoxButtons.OK, MessageBoxIcon.Error, token: token).ConfigureAwait(false);
                    return;
                }

                // Get base XML files (exclude amendment files)
                string[] astrXmlFiles = Directory.GetFiles(strDataPath, "*.xml")
                    .Where(x => !Path.GetFileName(x).StartsWith("amend_", StringComparison.OrdinalIgnoreCase)
                             && !Path.GetFileName(x).StartsWith("custom_", StringComparison.OrdinalIgnoreCase)
                             && !Path.GetFileName(x).StartsWith("override_", StringComparison.OrdinalIgnoreCase))
                    .Select(Path.GetFileName)
                    .OrderBy(x => x)
                    .ToArray();

                await this.DoThreadSafeAsync(x =>
                {
                    x.cboXmlFiles.Items.Clear();
                    x.cboXmlFiles.Items.AddRange(astrXmlFiles);
                    if (x.cboXmlFiles.Items.Count > 0)
                        x.cboXmlFiles.SelectedIndex = 0;
                }, token).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error loading available XML files");
                throw;
            }
        }

        private async Task LoadSelectedXmlFileAsync(CancellationToken token = default)
        {
            try
            {
                string strSelectedFile = cboXmlFiles.SelectedItem?.ToString();
                if (string.IsNullOrEmpty(strSelectedFile))
                {
                    await Program.ShowScrollableMessageBoxAsync(this, LanguageManager.GetString("XmlEditor_NoFileSelected", token: token), LanguageManager.GetString("XmlEditor_NoFileSelectedTitle", token: token), MessageBoxButtons.OK, MessageBoxIcon.Warning, token: token).ConfigureAwait(false);
                    return;
                }

                using (CursorWait.New(this))
                {
                    // Load the base XML document (without amendments)
                    string strFilePath = Path.Combine(Utils.GetStartupPath, "data", strSelectedFile);
                    if (!File.Exists(strFilePath))
                    {
                        await Program.ShowScrollableMessageBoxAsync(this, string.Format(LanguageManager.GetString("XmlEditor_FileNotFound", token: token), strFilePath), LanguageManager.GetString("XmlEditor_FileNotFoundTitle", token: token), MessageBoxButtons.OK, MessageBoxIcon.Error, token: token).ConfigureAwait(false);
                        return;
                    }

                    _objBaseXmlDocument = new XmlDocument();
                    _objBaseXmlDocument.LoadStandard(strFilePath, true, token);
                    _strBaseXmlContent = _objBaseXmlDocument.OuterXml;

                    // Update the UI
                    await this.DoThreadSafeAsync(x =>
                    {
                        x.txtBaseXml.Text = FormatXml(_strBaseXmlContent);
                        x.txtAmendmentXml.Text = GetAmendmentTemplate();
                        x.cmdApplyAmendment.Enabled = false;
                        x.cmdSaveAmendment.Enabled = false;
                        x._blnDirty = false;
                        x.Text = string.Format(LanguageManager.GetString("XmlEditor_Title"), strSelectedFile);
                    }, token).ConfigureAwait(false);

                    // Clear result areas
                    await this.DoThreadSafeAsync(x =>
                    {
                        x.txtResultXml.Text = string.Empty;
                        x.txtDiffPreview.Text = LanguageManager.GetString("XmlEditor_LoadInstructions");
                    }, token).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error loading XML file");
                throw;
            }
        }

        private async Task ApplyAmendmentAsync(CancellationToken token = default)
        {
            try
            {
                if (_objBaseXmlDocument == null)
                {
                    await Program.ShowScrollableMessageBoxAsync(this, LanguageManager.GetString("XmlEditor_NoBaseXml", token: token), LanguageManager.GetString("XmlEditor_NoBaseXmlTitle", token: token), MessageBoxButtons.OK, MessageBoxIcon.Warning, token: token).ConfigureAwait(false);
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtAmendmentXml.Text))
                {
                    await Program.ShowScrollableMessageBoxAsync(this, LanguageManager.GetString("XmlEditor_NoAmendment", token: token), LanguageManager.GetString("XmlEditor_NoAmendmentTitle", token: token), MessageBoxButtons.OK, MessageBoxIcon.Warning, token: token).ConfigureAwait(false);
                    return;
                }

                using (CursorWait.New(this))
                {
                    // Parse the amendment XML
                    try
                    {
                        _objAmendmentXmlDocument = new XmlDocument();
                        _objAmendmentXmlDocument.LoadXml(txtAmendmentXml.Text);
                    }
                    catch (XmlException ex)
                    {
                        await Program.ShowScrollableMessageBoxAsync(this, string.Format(LanguageManager.GetString("XmlEditor_InvalidXml", token: token), ex.Message), LanguageManager.GetString("XmlEditor_InvalidXmlTitle", token: token), MessageBoxButtons.OK, MessageBoxIcon.Error, token: token).ConfigureAwait(false);
                        return;
                    }

                    // Create a copy of the base XML to apply amendments to
                    _objResultXmlDocument = new XmlDocument();
                    _objResultXmlDocument.LoadXml(_strBaseXmlContent);

                    // Apply the amendments using the same logic as XmlManager
                    await ApplyAmendmentOperationsAsync(_objResultXmlDocument, _objAmendmentXmlDocument, token).ConfigureAwait(false);

                    _strResultXmlContent = _objResultXmlDocument.OuterXml;

                    // Update the UI
                    await this.DoThreadSafeAsync(x =>
                    {
                        x.txtResultXml.Text = FormatXml(_strResultXmlContent);
                        x.cmdSaveAmendment.Enabled = true;
                    }, token).ConfigureAwait(false);

                    // Generate diff
                    await UpdateDiffPreviewAsync(token).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error applying amendment");
                throw;
            }
        }

        private async Task SaveAmendmentAsync(CancellationToken token = default)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtAmendmentXml.Text))
                {
                    await Program.ShowScrollableMessageBoxAsync(this, LanguageManager.GetString("XmlEditor_NothingToSave", token: token), LanguageManager.GetString("XmlEditor_NothingToSaveTitle", token: token), MessageBoxButtons.OK, MessageBoxIcon.Warning, token: token).ConfigureAwait(false);
                    return;
                }

                string strSelectedFile = cboXmlFiles.SelectedItem?.ToString();
                if (string.IsNullOrEmpty(strSelectedFile))
                {
                    await Program.ShowScrollableMessageBoxAsync(this, LanguageManager.GetString("XmlEditor_NoBaseFile", token: token), LanguageManager.GetString("XmlEditor_NoBaseFileTitle", token: token), MessageBoxButtons.OK, MessageBoxIcon.Warning, token: token).ConfigureAwait(false);
                    return;
                }

                using (SaveFileDialog dlgSave = new SaveFileDialog())
                {
                    dlgSave.Filter = "XML Files (*.xml)|*.xml|All Files (*.*)|*.*";
                    dlgSave.FileName = $"amend_{strSelectedFile}";
                    dlgSave.Title = LanguageManager.GetString("XmlEditor_SaveTitle", token: token);

                    if (dlgSave.ShowDialog(this) == DialogResult.OK)
                    {
                        using (CursorWait.New(this))
                        {
                            await Task.Run(() => File.WriteAllText(dlgSave.FileName, txtAmendmentXml.Text, Encoding.UTF8), token).ConfigureAwait(false);
                            
                            await Program.ShowScrollableMessageBoxAsync(this, LanguageManager.GetString("XmlEditor_SaveSuccess", token: token), LanguageManager.GetString("XmlEditor_SaveSuccessTitle", token: token), MessageBoxButtons.OK, MessageBoxIcon.Information, token: token).ConfigureAwait(false);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error saving amendment");
                throw;
            }
        }


        private async Task UpdateDiffPreviewAsync(CancellationToken token = default)
        {
            try
            {
                if (_objBaseXmlDocument == null || _objResultXmlDocument == null)
                {
                    await this.DoThreadSafeAsync(x =>
                    {
                        x.txtDiffPreview.Text = LanguageManager.GetString("XmlEditor_DiffInstructions", token: token);
                        x.txtDiffPreview.ForeColor = SystemColors.WindowText;
                    }, token).ConfigureAwait(false);
                    return;
                }

                using (CursorWait.New(this))
                {
                    // Generate a clean, user-friendly diff
                    string strDiffOutput = await GenerateCleanDiffAsync(_strBaseXmlContent, _strResultXmlContent, token).ConfigureAwait(false);

                    await this.DoThreadSafeAsync(x =>
                    {
                        x.txtDiffPreview.Text = strDiffOutput;
                        x.txtDiffPreview.ForeColor = SystemColors.WindowText;
                    }, token).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error updating diff preview");
                await this.DoThreadSafeAsync(x =>
                {
                    x.txtDiffPreview.Text = string.Format(LanguageManager.GetString("XmlEditor_ErrorGeneratingDiff", token: token), ex.Message);
                    x.txtDiffPreview.ForeColor = Color.Red;
                }, token).ConfigureAwait(false);
            }
        }

        private static async Task<string> GenerateCleanDiffAsync(string strBaseXml, string strResultXml, CancellationToken token = default)
        {
            try
            {
                // Parse both XML documents
                XmlDocument objBaseDoc = new XmlDocument();
                objBaseDoc.LoadXml(strBaseXml);

                XmlDocument objResultDoc = new XmlDocument();
                objResultDoc.LoadXml(strResultXml);

                StringBuilder sbdOutput = new StringBuilder();
                sbdOutput.AppendLine(LanguageManager.GetString("XmlEditor_AmendmentChanges", token: token));
                sbdOutput.AppendLine();

                // Compare the documents and generate a clean diff
                await CompareXmlDocumentsAsync(objBaseDoc, objResultDoc, sbdOutput, "", token).ConfigureAwait(false);

                if (sbdOutput.Length <= 30) // Only header was added
                {
                    sbdOutput.AppendLine(LanguageManager.GetString("XmlEditor_NoChanges", token: token));
                    sbdOutput.AppendLine(LanguageManager.GetString("XmlEditor_NoChangesDescription", token: token));
                }

                return sbdOutput.ToString();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error generating clean diff");
                return string.Format(LanguageManager.GetString("XmlEditor_ErrorGeneratingCleanDiff", token: token), ex.Message);
            }
        }

        private static async Task CompareXmlDocumentsAsync(XmlDocument objBaseDoc, XmlDocument objResultDoc, StringBuilder sbdOutput, string strCurrentPath, CancellationToken token = default)
        {
            try
            {
                // Compare root elements
                await CompareXmlNodesAsync(objBaseDoc.DocumentElement, objResultDoc.DocumentElement, sbdOutput, strCurrentPath, token).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error comparing XML documents");
                sbdOutput.AppendLine(string.Format(LanguageManager.GetString("XmlEditor_ErrorComparingDocuments", token: token), ex.Message));
            }
        }

        private static async Task CompareXmlNodesAsync(XmlNode objBaseNode, XmlNode objResultNode, StringBuilder sbdOutput, string strCurrentPath, CancellationToken token = default)
        {
            try
            {
                if (objBaseNode == null && objResultNode == null)
                    return;

                string strNodePath = GetNodePath(objBaseNode ?? objResultNode, strCurrentPath);

                // Node was added
                if (objBaseNode == null && objResultNode != null)
                {
                    sbdOutput.AppendLine(string.Format(LanguageManager.GetString("XmlEditor_Added", token: token), strNodePath));
                    sbdOutput.AppendLine($"  {FormatXmlNode(objResultNode)}");
                    return;
                }

                // Node was removed
                if (objBaseNode != null && objResultNode == null)
                {
                    sbdOutput.AppendLine(string.Format(LanguageManager.GetString("XmlEditor_Removed", token: token), strNodePath));
                    sbdOutput.AppendLine($"  {FormatXmlNode(objBaseNode)}");
                    return;
                }

                // Compare text nodes
                if (objBaseNode.NodeType == XmlNodeType.Text && objResultNode.NodeType == XmlNodeType.Text)
                {
                    if (objBaseNode.Value != objResultNode.Value)
                    {
                        sbdOutput.AppendLine(string.Format(LanguageManager.GetString("XmlEditor_Modified", token: token), strNodePath));
                        sbdOutput.AppendLine(string.Format(LanguageManager.GetString("XmlEditor_OldValue", token: token), FormatXmlNode(objBaseNode)));
                        sbdOutput.AppendLine(string.Format(LanguageManager.GetString("XmlEditor_NewValue", token: token), FormatXmlNode(objResultNode)));
                    }
                    return;
                }

                // Compare element nodes
                if (objBaseNode.NodeType == XmlNodeType.Element && objResultNode.NodeType == XmlNodeType.Element)
                {
                    // Compare attributes
                    if (objBaseNode.Attributes != null || objResultNode.Attributes != null)
                    {
                        var baseAttribs = objBaseNode.Attributes?.Cast<XmlAttribute>().ToDictionary(a => a.Name, a => a.Value) ?? new Dictionary<string, string>();
                        var resultAttribs = objResultNode.Attributes?.Cast<XmlAttribute>().ToDictionary(a => a.Name, a => a.Value) ?? new Dictionary<string, string>();

                        foreach (var kvp in baseAttribs)
                        {
                            if (!resultAttribs.TryGetValue(kvp.Key, out string value))
                            {
                                sbdOutput.AppendLine(string.Format(LanguageManager.GetString("XmlEditor_RemovedAttribute", token: token), strNodePath, kvp.Key, kvp.Value));
                            }
                            else if (value != kvp.Value)
                            {
                                sbdOutput.AppendLine(string.Format(LanguageManager.GetString("XmlEditor_ModifiedAttribute", token: token), strNodePath, kvp.Key));
                                sbdOutput.AppendLine(string.Format(LanguageManager.GetString("XmlEditor_OldValue", token: token), $"\"{kvp.Value}\"") );
                                sbdOutput.AppendLine(string.Format(LanguageManager.GetString("XmlEditor_NewValue", token: token), $"\"{value}\"") );
                                
                            }
                        }

                        foreach (var kvp in resultAttribs)
                        {
                            if (!baseAttribs.ContainsKey(kvp.Key))
                            {
                                sbdOutput.AppendLine(string.Format(LanguageManager.GetString("XmlEditor_AddedAttribute", token: token), strNodePath, kvp.Key, kvp.Value));
                            }
                        }
                    }

                    // Compare text content first (for elements with only text content)
                    var baseTextNodes = objBaseNode.ChildNodes.Cast<XmlNode>().Where(n => n.NodeType == XmlNodeType.Text).ToList();
                    var resultTextNodes = objResultNode.ChildNodes.Cast<XmlNode>().Where(n => n.NodeType == XmlNodeType.Text).ToList();
                    
                    if (baseTextNodes.Count > 0 || resultTextNodes.Count > 0)
                    {
                        string strBaseText = string.Join("", baseTextNodes.Select(n => n.Value));
                        string strResultText = string.Join("", resultTextNodes.Select(n => n.Value));
                        
                        if (strBaseText != strResultText)
                        {
                            sbdOutput.AppendLine(string.Format(LanguageManager.GetString("XmlEditor_Modified", token: token), strNodePath));
                            sbdOutput.AppendLine(string.Format(LanguageManager.GetString("XmlEditor_OldValue", token: token), $"\"{strBaseText}\"") );
                            sbdOutput.AppendLine(string.Format(LanguageManager.GetString("XmlEditor_NewValue", token: token), $"\"{strResultText}\"") );
                            
                        }
                    }

                    // Compare child element nodes
                    var baseChildren = objBaseNode.ChildNodes.Cast<XmlNode>().Where(n => n.NodeType == XmlNodeType.Element).ToList();
                    var resultChildren = objResultNode.ChildNodes.Cast<XmlNode>().Where(n => n.NodeType == XmlNodeType.Element).ToList();

                    // Group children by their identifying attributes or content
                    var baseGroups = GroupChildNodes(baseChildren);
                    var resultGroups = GroupChildNodes(resultChildren);

                    // Find added, removed, and modified nodes
                    foreach (var kvp in resultGroups)
                    {
                        if (!baseGroups.TryGetValue(kvp.Key, out XmlNode value))
                        {
                            string strChildPath = GetNodePath(kvp.Value, strNodePath);
                            sbdOutput.AppendLine(string.Format(LanguageManager.GetString("XmlEditor_Added", token: token), strChildPath));
                            sbdOutput.AppendLine($"  {FormatXmlNode(kvp.Value)}");
                        }
                        else
                        {
                            await CompareXmlNodesAsync(value, kvp.Value, sbdOutput, strNodePath, token).ConfigureAwait(false);
                        }
                    }

                    foreach (var kvp in baseGroups)
                    {
                        if (!resultGroups.ContainsKey(kvp.Key))
                        {
                            string strChildPath = GetNodePath(kvp.Value, strNodePath);
                            sbdOutput.AppendLine(string.Format(LanguageManager.GetString("XmlEditor_Removed", token: token), strChildPath));
                            sbdOutput.AppendLine($"  {FormatXmlNode(kvp.Value)}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error comparing XML nodes");
                sbdOutput.AppendLine(string.Format(LanguageManager.GetString("XmlEditor_ErrorComparingNodes", token: token), strCurrentPath, ex.Message));
            }
        }

        private static Dictionary<string, XmlNode> GroupChildNodes(List<XmlNode> childNodes)
        {
            var groups = new Dictionary<string, XmlNode>();
            
            foreach (var node in childNodes)
            {
                // Try to find an ID attribute first
                string strKey = node.Attributes?["id"]?.Value;
                
                // If no ID, try to use name attribute
                if (string.IsNullOrEmpty(strKey))
                {
                    strKey = node.Attributes?["name"]?.Value;
                }
                
                // If still no key, use the node name with index
                if (string.IsNullOrEmpty(strKey))
                {
                    strKey = $"{node.Name}_{groups.Count}";
                }
                
                groups[strKey] = node;
            }
            
            return groups;
        }

        private static string GetNodePath(XmlNode node, string strCurrentPath)
        {
            if (node == null)
                return strCurrentPath;

            string strNodeName = node.Name;
            
            // For element nodes, try to include ID or name attribute
            if (node.NodeType == XmlNodeType.Element)
            {
                string strId = node.Attributes?["id"]?.Value;
                string strName = node.Attributes?["name"]?.Value;
                
                if (!string.IsNullOrEmpty(strId))
                {
                    strNodeName = $"{strNodeName}[id={strId}]";
                }
                else if (!string.IsNullOrEmpty(strName))
                {
                    strNodeName = $"{strNodeName}[name={strName}]";
                }
            }
            
            return string.IsNullOrEmpty(strCurrentPath) ? strNodeName : $"{strCurrentPath}/{strNodeName}";
        }

        private static string FormatXmlNode(XmlNode node)
        {
            try
            {
                if (node.NodeType == XmlNodeType.Text)
                {
                    return $"\"{node.Value}\"";
                }
                
                if (node.NodeType == XmlNodeType.Element)
                {
                    // Create a temporary document to format just this node
                    XmlDocument tempDoc = new XmlDocument();
                    XmlNode importedNode = tempDoc.ImportNode(node, true);
                    tempDoc.AppendChild(importedNode);
                    
                    // Format the XML and get the inner content
                    using (StringWriter stringWriter = new StringWriter())
                    {
                        using (XmlTextWriter xmlWriter = new XmlTextWriter(stringWriter))
                        {
                            xmlWriter.Formatting = Formatting.Indented;
                            xmlWriter.Indentation = 2;
                            importedNode.WriteTo(xmlWriter);
                        }
                        return stringWriter.ToString().Trim();
                    }
                }
                
                return node.ToString();
            }
            catch (Exception ex)
            {
                Log.Error(ex, LanguageManager.GetString("XmlEditor_ErrorFormattingNode"));
                return node.ToString();
            }
        }

        private static string FormatXml(string strXml)
        {
            try
            {
                XmlDocument objDoc = new XmlDocument();
                objDoc.LoadXml(strXml);
                
                using (StringWriter objStringWriter = new StringWriter())
                {
                    using (XmlTextWriter objXmlWriter = new XmlTextWriter(objStringWriter))
                    {
                        objXmlWriter.Formatting = Formatting.Indented;
                        objXmlWriter.Indentation = 2;
                        objDoc.WriteTo(objXmlWriter);
                    }
                    return objStringWriter.ToString();
                }
            }
            catch
            {
                return strXml; // Return original if formatting fails
            }
        }

        private string GetAmendmentTemplate()
        {
            if (_strXmlElementTemplate == string.Empty)
            {
                _strXmlElementTemplate = LanguageManager.GetString("XmlEditor_AmendmentTemplate");
            }
            return _strXmlElementTemplate;
        }

        private static Task ApplyAmendmentOperationsAsync(XmlDocument xmlTargetDoc, XmlDocument xmlAmendmentDoc, CancellationToken token = default)
        {
            try
            {
                // Use the actual XmlManager.AmendNodeChildren method via reflection.
                // Might be better to make that method public in XmlManager but it's not a common operation.
                var amendNodeChildrenMethod = typeof(XmlManager).GetMethod("AmendNodeChildren", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

                if (amendNodeChildrenMethod != null)
                {
                    using (XmlNodeList xmlNodeList = xmlAmendmentDoc.SelectNodes("/chummer/*"))
                    {
                        if (xmlNodeList?.Count > 0)
                        {
                            foreach (XmlNode objNode in xmlNodeList)
                            {
                                token.ThrowIfCancellationRequested();
                                
                                // Call the actual XmlManager.AmendNodeChildren method
                                object[] parameters = { xmlTargetDoc, objNode, "/chummer", null, token };
                                bool blnResult = (bool)amendNodeChildrenMethod.Invoke(null, parameters);
                                
                                if (blnResult)
                                {
                                    Log.Info($"Applied amendment operation to node: {objNode.Name}");
                                }
                            }
                        }
                    }
                }
                else
                {
                    Log.Warn("Could not access XmlManager.AmendNodeChildren via reflection.");
                    Utils.BreakIfDebug();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error applying amendment operations");
                throw;
            }

            return Task.CompletedTask;
        }

        #endregion Methods
    }
}
