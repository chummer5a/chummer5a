using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Chummer
{
    /// <summary>
    /// This class allows you to cordinate multiple controls in a group and fire
    /// an event when the mouse leaves that group
    /// </summary>
    public class HoverDisplayCordinator
    {
        //List of controls in the group. A n-tree might do this faster
        private List<Control> _controls;

        //If it have left once we don't want to do it again. This limits flexibility
        //but we don't need general pourpose and this is easier to implement
        private bool _left;

        /// <summary>
        /// This event is raised the first time the mouse leaves the group of controls
        /// </summary>
        public event EventHandler OnAllLeave;

        /// <summary>
        /// Create a new HoverDisplayCordinator
        /// </summary>
        public HoverDisplayCordinator()
        {
            _controls = new List<Control>();
        }

        /// <summary>
        /// Create a new HoverDisplayCordinator
        /// </summary>
        /// <param name="collection">A collection of controls to include</param>
        public HoverDisplayCordinator(IEnumerable<Control> collection)
        {
            _controls = new List<Control>(collection);
        }
        
        /// <summary>
        /// Has the mouse allready left the group of controls once?
        /// </summary>
        public bool Left
        {
            get { return _left; }
        }

        /// <summary>
        /// Add a new control to the list of controls
        /// </summary>
        /// <param name="control">The control to add</param>
        public void AddControl(Control control)
        {
            _controls.Add(control);
            control.MouseLeave += control_MouseLeave;
        }

        public void AddControlRecursive(Control control)
        {
            _controls.Add(control);
            control.MouseLeave += control_MouseLeave;
            if (control.HasChildren)
            {
                foreach (Control child in control.Controls)
                {
                    AddControlRecursive(child);
                }
            }
        }

        //This is our custom event added to each control to check mouse leave
        void control_MouseLeave(object sender, EventArgs e)
        {
            //If it have allready left we don't want to check again
            if(_left) return;

            
            foreach (Control control in _controls)
            {
                //Check if the mouse is inside any control
                if (control.ClientRectangle.Contains(control.PointToClient(Control.MousePosition)))
                {
                    return;
                }
            }

            _left = true;  //Don't do again flag

              //remove everything we have a listner on. Might not be necesary but afraid of GC leak
             //Anybody can test this by uncommenting the lines below and triggering 100000 of those
            //then checking if memory usage changed
            foreach (var control in _controls)
            {
                control.MouseLeave -= control_MouseLeave;
            } 

            //Don't call if nothing suscribed
            if (OnAllLeave != null)
            {
                OnAllLeave(sender, e);
            }

            //As we don't call after this we don't need to store references to potential dead controls
            _controls.Clear();
        }

        
    }
}
