using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Xml;

// ConnectionRatingChanged Event Handler.
public delegate void ConnectionRatingChangedHandler(Object sender);
// GroupRatingChanged Event Handler.
public delegate void ConnectionGroupRatingChangedHandler(Object sender);
// LoyaltyRatingChanged Event Handler.
public delegate void LoyaltyRatingChangedHandler(Object sender);
// DeleteContact Event Handler.
public delegate void DeleteContactHandler(Object sender);
// FileNameChanged Event Handler.
public delegate void FileNameChangedHandler(Object sender);

namespace Chummer
{
    public partial class  ContactControl : UserControl
    {
		private Contact _objContact;
        private readonly Character _objCharacter;
        private string _strContactName;
        private string _strContactRole;
        private string _strContactLocation;
        private bool _blnEnemy = false;
        private HoverDisplayCordinator _displayCordinator;
        private ContractControlCallBackObject cbobj;
        
        // Events.
		public event ConnectionRatingChangedHandler ConnectionRatingChanged;
        public event ConnectionGroupRatingChangedHandler GroupStatusChanged;
        public event LoyaltyRatingChangedHandler LoyaltyRatingChanged;
        public event DeleteContactHandler DeleteContact;
		public event FileNameChangedHandler FileNameChanged;

		#region Control Events
        public ContactControl(Character objCharacter)
        {
            InitializeComponent();
            _objCharacter = objCharacter;
            
            
			LanguageManager.Instance.Load(GlobalOptions.Instance.Language, this);
            
             //Hover dosen't work if there is a control in the way
            //Add same event to most of the controls (base added in init)
            txtContactLocation.MouseHover += ContactControl_MouseHover;
            txtContactName.MouseHover += ContactControl_MouseHover;
            cboContactRole.MouseHover += ContactControl_MouseHover;
            //Uncomment lines belov to make hovering over quick info 
            //label/delete button work
            //cmdDelete.MouseHover += ContactControl_MouseHover;
            //lblQuick.MouseHover += ContactControl_MouseHover;


             //we need to raise some events that are based on this file but
            //now raised somewhere else. This object works as a proxy for that
            cbobj = new ContractControlCallBackObject();
            
            cbobj.FileNameChanged += sender =>
            {
                if (FileNameChanged != null) FileNameChanged(sender);
            };
            cbobj.GroupStatusChanged += cbobj_GroupStatusChanged;
            cbobj.ConnectionRatingChanged += cbobj_ConnectionRatingChanged;
            cbobj.LoyaltyRatingChanged += cbobj_LoyaltyRatingChanged;
        }

        void cbobj_GroupStatusChanged(object sender)
        {
            if (GroupStatusChanged != null) GroupStatusChanged(sender);

            if (IsGroup)
            {
                _objContact.Loyalty = 1;
            }

            UpdateQuickText();
        }

        private void cbobj_LoyaltyRatingChanged(object sender)
        {
            if (LoyaltyRatingChanged != null) LoyaltyRatingChanged(sender);

            UpdateQuickText();
        }

        private void cbobj_ConnectionRatingChanged(object sender)
        {
            if (ConnectionRatingChanged != null) ConnectionRatingChanged(sender);

            UpdateQuickText();
        }

        private void UpdateQuickText()
        {
            lblQuick.Text = String.Format("({0}/{1})", _objContact.Connection, _objContact.IsGroup ? "G" : _objContact.Loyalty.ToString());
        }

        private void ContactControl_MouseHover(object sender, EventArgs e)
        {
            if (_displayCordinator == null)
            {
                _displayCordinator = new HoverDisplayCordinator();
                _displayCordinator.OnAllLeave += (o, args) => { _displayCordinator = null; };
                ContactAdv _contactAdv = 
                    new ContactAdv(_displayCordinator, this, _objContact, _objCharacter,cbobj);
                _displayCordinator.AddControlRecursive(this);
                _displayCordinator.AddControlRecursive(_contactAdv);
                _contactAdv.Show(ParentForm);
                _contactAdv.DesktopLocation = PointToScreen(new Point(0, (- _contactAdv.Height)));
            }
        }

        private void LoadContactList()
        {
            if (_blnEnemy)
            {
                if (_strContactRole != "")
                    cboContactRole.Text = _strContactRole;
                return;
            }

            // Read the list of Categories from the XML file.
            List<ListItem> lstCategories = new List<ListItem>();

            ListItem objBlank = new ListItem();
            objBlank.Value = "";
            objBlank.Name = "";
            lstCategories.Add(objBlank);

            XmlDocument objXmlDocument = new XmlDocument();
            objXmlDocument = XmlManager.Instance.Load("contacts.xml");
            XmlNodeList objXmlSkillList = objXmlDocument.SelectNodes("/chummer/contacts/contact");
            foreach (XmlNode objXmlCategory in objXmlSkillList)
            {
                ListItem objItem = new ListItem();
                objItem.Value = objXmlCategory.InnerText;
                if (objXmlCategory.Attributes["translate"] != null)
                    objItem.Name = objXmlCategory.Attributes["translate"].InnerText;
                else
                    objItem.Name = objXmlCategory.InnerText;
                lstCategories.Add(objItem);
            }
            cboContactRole.ValueMember = "Value";
            cboContactRole.DisplayMember = "Name";
            cboContactRole.DataSource = lstCategories;

            if (_strContactRole != "")
                cboContactRole.Text = _strContactRole;
        }

		private void ContactControl_Load(object sender, EventArgs e)
		{
			this.Width = cmdDelete.Left + cmdDelete.Width;
            LoadContactList();
		}

        private void cmdDelete_Click(object sender, EventArgs e)
        {
            // Raise the DeleteContact Event when the user has confirmed their desire to delete the Contact.
            // The entire ContactControl is passed as an argument so the handling event can evaluate its contents.
            DeleteContact(this);
        }

        private void cboContactRole_TextChanged(object sender, EventArgs e)
		{
			_objContact.Role = cboContactRole.Text;
            ConnectionRatingChanged(this);
		}

        private void txtContactName_TextChanged(object sender, EventArgs e)
        {
            _objContact.Name = txtContactName.Text;
            ConnectionRatingChanged(this);
        }

        private void txtContactLocation_TextChanged(object sender, EventArgs e)
        {
            _objContact.Location = txtContactLocation.Text;
            ConnectionRatingChanged(this);
        }
        #endregion

		#region Properties
		/// <summary>
		/// Contact object this is linked to.
		/// </summary>
		public Contact ContactObject
		{
			get
			{
				return _objContact;
			}
			set
			{
				_objContact = value;
			}
		}

        public bool IsEnemy
        {
            get
            {
                return _blnEnemy;
            }
            set
            {
                _blnEnemy = value;
                cboContactRole.Items.Clear();
            }
        }

        /// <summary>
        /// Contact name.
        /// </summary>
        public string ContactName
        {
            get
            {
				return _objContact.Name;
            }
            set
            {
                txtContactName.Text = value;
                _strContactName = value;
				_objContact.Name = value;
            }
        }

        /// <summary>
        /// Contact role.
        /// </summary>
        public string ContactRole
        {
            get
            {
                return _objContact.Role;
            }
            set
            {
                cboContactRole.Text = value;
                _strContactRole = value;
                _objContact.Role = value;
            }
        }

        /// <summary>
        /// Contact location.
        /// </summary>
        public string ContactLocation
        {
            get
            {
                return _objContact.Location;
            }
            set
            {
                txtContactLocation.Text = value;
                _strContactLocation = value;
                _objContact.Location = value;
            }
        }

        /// <summary>
		/// Indicates if this is a Contact or Enemy.
		/// </summary>
		public ContactType EntityType
		{
			get
			{
				return _objContact.EntityType;
			}
			set
			{
				_objContact.EntityType = value;
			}
		}

        /// <summary>
        /// Connection Rating.
        /// </summary>
        public int ConnectionRating
        {
            get
            {
				return _objContact.Connection;
            }
            set
            {
                //nudConnection.Value = value;
				_objContact.Connection = value;
            }
        }

		/// <summary>
        /// Loyalty Rating.
        /// </summary>
        public int LoyaltyRating
        {
            get
            {
				return _objContact.Loyalty;
            }
            set
            {
                //nudLoyalty.Value = value;
				_objContact.Loyalty = value;
            }
        }

		/// <summary>
		/// Whether or not this is a free Contact.
		/// </summary>
		public bool Free
		{
			get
			{
				return _objContact.Free;
			}
			set
			{
				_objContact.Free = value;
			}
		}

        /// <summary>
        /// Is the contract a group contract
        /// </summary>
        public bool IsGroup
        {
            get
            {
                return _objContact.IsGroup;
            }
            set
            {
                _objContact.IsGroup = value;
            }
        }
		#endregion

        
	}

       //Since events can only be raised from inside a class and we moved some
      //controls that raises events to ContactAdv we need to either move events
     //or give it some way to raise them. This object is giving a way to raise
    //them while maintaning OOP encapsulationg where not everything can raise them
    public class ContractControlCallBackObject
    {
        internal event ConnectionRatingChangedHandler ConnectionRatingChanged;
        internal event ConnectionGroupRatingChangedHandler GroupStatusChanged;
        internal event LoyaltyRatingChangedHandler LoyaltyRatingChanged;
        internal event FileNameChangedHandler FileNameChanged;

        internal void OnConnectionRatingChanged(Object sender)
        {
            if (ConnectionRatingChanged != null)
            {
                ConnectionRatingChanged(sender);
            }
        }

        internal void OnLoyaltyRatingChanged(Object sender)
        {
            if (LoyaltyRatingChanged != null)
            {
                LoyaltyRatingChanged(sender);
            }
        }

        internal void OnGroupStatusChanged(Object sender)
        {
            if (GroupStatusChanged != null)
            {
                GroupStatusChanged(sender);
            }
        }

        internal void OnFileNameChanged(Object sender)
        {
            if (FileNameChanged != null)
            {
                FileNameChanged(sender);
            }
        }
    }
}