using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.Xml;
namespace Translator
{
	public partial class frmTranslate : Form
	{

        private Button btnSearch;

        private ComboBox cboFile;

        private ComboBox cboSection;

        private CheckBox chkOnlyTranslation;

        private IContainer components;

        private DataGridView dgvSection;

        private DataGridView dgvTranslate;

        private TextBox txtSearch;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }
	}
}

