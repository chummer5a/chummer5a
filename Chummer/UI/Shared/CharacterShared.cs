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
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Chummer
{
	/// <summary>
	/// Contains functionality shared between frmCreate and frmCareer
	/// </summary>
	[System.ComponentModel.DesignerCategory("")]
	public class CharacterShared : Form
	{
		protected Character _objCharacter;
		protected MainController _objController;
		protected CharacterOptions _objOptions;
		protected CommonFunctions _objFunctions;

		
		/// <summary>
		/// Wrapper for relocating contact forms. 
		/// </summary>
		public class TransportWrapper
		{
			private Control control;

			public TransportWrapper(Control control)
			{
				this.control = control;
			}

			public Control Control
			{
				get { return control; }
			}
		}

		protected void RedlinerCheck()
		{

		    string strSeekerImprovPrefix = "SEEKER";
            //Get attributes affected by redliner/cyber singularity seeker
			var attributes = new List<string>(
				from improvement in _objCharacter.Improvements
				where improvement.ImproveType == Improvement.ImprovementType.Seeker
				select improvement.ImprovedName);

			//And the improvements comming from there
			var impr = new List<Improvement>(
				from improvement in _objCharacter.Improvements
				where (improvement.ImproveType == Improvement.ImprovementType.Attribute ||
				       improvement.ImproveType == Improvement.ImprovementType.PhysicalCM )&&
				       improvement.SourceName.Contains(strSeekerImprovPrefix) //for backwards compability
				select improvement);

			//if neither contains anything, it is safe to exit
		    if (impr.Count == 0 && attributes.Count == 0)
		    {
		        _objCharacter.RedlinerBonus = 0;
                return;
		    }

			//Calculate bonus from cyberlimbs
			int count = Math.Min(_objCharacter.Cyberware.Count(c => c.LimbSlot != "" && c.Name.Contains("Full")) / 2,2);
		    if (impr.Any(x => x.ImprovedName == "STR" || x.ImprovedName == "AGI"))
		    {
		        _objCharacter.RedlinerBonus = count;
		    }
		    else
		    {
                _objCharacter.RedlinerBonus = 0;
            }

		    for (int i = 0; i < attributes.Count; i++)
		    {
		        Improvement objImprove = impr.FirstOrDefault(x => x.SourceName == strSeekerImprovPrefix +"_" + attributes[i] && x.Value == (attributes[i] == "BOX" ? count * -3 : count));
		        if (objImprove != null)
		        {
		            attributes.RemoveAt(i);
		            impr.Remove(objImprove);
		        }
		    }			
			//Improvement manager defines the functions we need to manipulate improvements
			//When the locals (someday) gets moved to this class, this can be removed and use
			//the local
			Lazy<ImprovementManager> manager = new Lazy<ImprovementManager>(() => new ImprovementManager(_objCharacter));

            // Remove which qualites have been removed or which values have changed
            foreach (Improvement improvement in impr)
            {
                manager.Value.RemoveImprovements(improvement.ImproveSource, improvement.SourceName);
            }

            // Add new improvements or old improvements with new values
            foreach (string attribute in attributes)
			{
			    if (attribute == "BOX")
			    {
                    manager.Value.CreateImprovement(attribute, Improvement.ImprovementSource.Quality,
			            strSeekerImprovPrefix + "_" + attribute, Improvement.ImprovementType.PhysicalCM,
			            Guid.NewGuid().ToString(), count*-3);
			    }
			    else
			    {
			        manager.Value.CreateImprovement(attribute, Improvement.ImprovementSource.Quality,
			            strSeekerImprovPrefix +"_" + attribute, Improvement.ImprovementType.Attribute,
			            Guid.NewGuid().ToString(), count, 1, 0, 0, count);
			    }
			}
            if (manager.IsValueCreated)
			{
				manager.Value.Commit(); //REFACTOR! WHEN MOVING MANAGER, change this to bool
			}
        }
	}
}
