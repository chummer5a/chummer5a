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
				       improvement.SourceName.StartsWith("__SEEKER__")
				select improvement);

			//if neither contains anything, it is safe to exit
			if(impr.Count == 0 && attributes.Count == 0) return;

			//BUG might happen here
			//Calculate bonus from cyberlimbs
			//Taking advantage of the fact only full limbs get a limb slot (it seems) 
			int count = _objCharacter.Cyberware.Count(c => c.LimbSlot != "");
			count = Math.Min((count)/2, 2);

			List<KeyValuePair<string, Improvement>> pairs = new List<KeyValuePair<string, Improvement>>();

			//Merge all pairs where we find coresponding improvement and attribute, removing them from the orginal list
			for (int i = attributes.Count - 1; i >= 0; i--)
			{
				string _ref = attributes[i]; //easy reference, could be refactored out
				Improvement im = impr.FirstOrDefault(x => x.SourceName==$"__SEEKER__{_ref}");
				if(im == null) continue;
				
				pairs.Add(new  KeyValuePair<string, Improvement>(_ref, im));
				impr.Remove(im);
				attributes.RemoveAt(i);
			}

			//we now have a list of pairs, that might have the wrong value
			//a lost of attributes that don't have an improvement
			//and a list of improvements that don't have an attribute

			//Improvement manager defines the functions we need to manipulate improvements
			//When the locals (someday) gets moved to this class, this can be removed and use
			//the local
			Lazy<ImprovementManager> manager = new Lazy<ImprovementManager>(() => new ImprovementManager(_objCharacter));

			foreach (string attribute in attributes)
			{
				manager.Value.CreateImprovement(attribute, Improvement.ImprovementSource.Quality, //Attribute name AGI, BOD etc
					$"__SEEKER__{attribute}",  //Sourcename, as we cannot track dependent improvements, we have to hack it this way
					Improvement.ImprovementType.Attribute,
					Guid.NewGuid().ToString(), (attribute == "BOX" ? count * -3 : 1), 1, 0, 0, count * (attribute == "BOX" ? 0 : 1)); //count is argumented value
			}

			foreach (Improvement improvement in impr)
			{
				manager.Value.RemoveImprovements(improvement.ImproveSource, improvement.SourceName);
			}

			foreach (KeyValuePair<string, Improvement> pair in pairs)
			{
				if(pair.Key == "BOX" ? pair.Value.Value == count * -3 : pair.Value.Augmented == count) continue;

				manager.Value.RemoveImprovements(pair.Value.ImproveSource, pair.Value.SourceName);
				manager.Value.CreateImprovement(
					pair.Key == "BOX" ? "" : pair.Key, Improvement.ImprovementSource.Quality, //Attribute name AGI, BOD etc
					$"__SEEKER__{pair.Key}",  //Sourcename, as we cannot track dependent improvements, we have to hack it this way
					pair.Key == "BOX" ? Improvement.ImprovementType.PhysicalCM : Improvement.ImprovementType.Attribute,
					Guid.NewGuid().ToString(),
					count * (pair.Key == "BOX" ? -3 : 1),
					1, 0, 0,  //because effective improvement is somehow multiplied by rating...?
					count * (pair.Key == "BOX" ? 0 : 1)); //count is argumented value

				
			}

			if (manager.IsValueCreated)
			{
				manager.Value.Commit(); //REFACTOR! WHEN MOVING MANAGER, change this to bool
			}


		}
	}
}
