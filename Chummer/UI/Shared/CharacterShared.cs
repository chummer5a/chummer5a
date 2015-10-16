using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Chummer
{
	/// <summary>
	/// Contains functionality shared between frmCreate and frmCareer
	/// </summary>
	[System.ComponentModel.DesignerCategory("")]
	public abstract class CharacterShared : Form
	{
		protected Character _objCharacter;
		protected MainController _objController;
		protected CharacterOptions _objOptions;
		protected CommonFunctions _objFunctions;



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
					Guid.NewGuid().ToString(), (attribute == "BOX" ? count * -3 : 1), 0, 0, 0, count * (attribute == "BOX" ? 0 : 1)); //count is argumented value
			}

			foreach (Improvement improvement in impr)
			{
				manager.Value.RemoveImprovements(improvement.ImproveSource, improvement.SourceName);
			}

			foreach (KeyValuePair<string, Improvement> pair in pairs)
			{
				if(pair.Key == "BOX" ? pair.Value.Value == count * -3 : pair.Value.Augmented == count) continue;

				manager.Value.RemoveImprovements(pair.Value.ImproveSource, pair.Value.SourceName);
				manager.Value.CreateImprovement(pair.Key == "BOX" ? "" : pair.Key, Improvement.ImprovementSource.Quality, //Attribute name AGI, BOD etc
					$"__SEEKER__{pair.Key}",  //Sourcename, as we cannot track dependent improvements, we have to hack it this way
					pair.Key == "BOX" ? Improvement.ImprovementType.PhysicalCM : Improvement.ImprovementType.Attribute,
					Guid.NewGuid().ToString(),  (pair.Key == "BOX" ? count * -3 : 1), 0, 0, 0, count * (pair.Key == "BOX" ? 0 : 1)); //count is argumented value
			}



		}
	}
}
