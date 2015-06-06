using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Automation.Peers;
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
// OtherCostChanged Event Handler.
public delegate void OtherCostChangedHandler(Object sender);

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
        public event OtherCostChangedHandler OtherCostChanged;

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
            //Uncomment lines belov to make hovering over quick info 
            //label/delete button work
            //cmdDelete.MouseHover += ContactControl_MouseHover;
            //lblQuick.MouseHover += ContactControl_MouseHover;

            //ComboBox in input mode is borked
            //We have to create our own MouseHower using MouseMove
            //cboContactRole.MouseHover += ContactControl_MouseHover;
            cboContactRole.MouseMove += cboContactRole_MouseMove;

            

             //we need to raise some events that are based on this file but
            //now raised somewhere else. This object works as a proxy for that
            cbobj = new ContractControlCallBackObject();
            cbobj.OtherCostChanged += sender =>
            {
                if (OtherCostChanged != null) OtherCostChanged(sender);
            };
            cbobj.FileNameChanged += sender =>
            {
                if (FileNameChanged != null) FileNameChanged(sender);
            };
            cbobj.GroupStatusChanged += cbobj_GroupStatusChanged;
            cbobj.ConnectionRatingChanged += cbobj_ConnectionRatingChanged;
            cbobj.LoyaltyRatingChanged += cbobj_LoyaltyRatingChanged;

        }

        void cboContactRole_MouseMove(object sender, MouseEventArgs e)
        {
            hovertimer.Interval = SystemInformation.MouseHoverTime;
            hovertimer.Stop();
            hovertimer.Start();
        }

        void hovertimer_Tick(object sender, EventArgs e)
        {
            hovertimer.Start();
            //if this gives errors someday we might have to save sender and events
            //from cboContactRole_MouseMove but until then we won't bother
            if (cboContactRole.ClientRectangle.Contains(cboContactRole.PointToClient(Control.MousePosition)))
            {
                ContactControl_MouseHover(sender, e);
            }
        }

        void cbobj_GroupStatusChanged(object sender)
        {
            if (GroupStatusChanged != null) GroupStatusChanged(sender);

            if (IsGroup)
            {
                _objContact.Loyalty = _objContact.MadeMan ? 3 : 1;
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

        public void UpdateQuickText()
        {
            lblQuick.Text = String.Format("({0}/{1})", _objContact.Connection, _objContact.IsGroup ? (_objContact.MadeMan ? "M" : "G") : _objContact.Loyalty.ToString());

        }

        private void ContactControl_MouseHover(object sender, EventArgs e)
        {
            if (_displayCordinator == null)
            {
                _displayCordinator = new HoverDisplayCordinator();

                bool blnContactLocationFocused = txtContactLocation.Focused;
                bool blnContactNameFocused = txtContactName.Focused;
                bool blnContactRoleFocused = cboContactRole.Focused;
                
                _displayCordinator.OnAllLeave += (o, args) => { _displayCordinator = null; };
                ContactAdv _contactAdv = 
                    new ContactAdv(_displayCordinator, this, _objContact, _objCharacter,cbobj);
                _displayCordinator.AddControlRecursive(this);
                _displayCordinator.AddControlRecursive(_contactAdv);
                _contactAdv.Show(ParentForm);
                _contactAdv.DesktopLocation = PointToScreen(new Point(0, (- _contactAdv.Height)));

                if (blnContactLocationFocused) txtContactLocation.Focus();
                if (blnContactNameFocused) txtContactName.Focus();
                if (blnContactRoleFocused) cboContactRole.Focus();
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

                //Init quick text
                //We can't do it in constructor, because there this isen't
                //set because the entire object this is suposed to display
                //is not required in the constructor, for some reason
                UpdateQuickText();
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
        internal event OtherCostChangedHandler OtherCostChanged;

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

        internal void OnOtherCostChanged(Object sender)
        {
            if (OtherCostChanged != null)
            {
                OtherCostChanged(sender);
            }
        }
    }
}