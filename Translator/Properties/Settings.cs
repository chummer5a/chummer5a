using System.CodeDom.Compiler;
using System.Configuration;
using System.Runtime.CompilerServices;

namespace Translator.Properties
{
	[CompilerGenerated]
	[GeneratedCode("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "11.0.0.0")]
	internal sealed class Settings : ApplicationSettingsBase
	{
		private static readonly Settings defaultInstance;

		public static Settings Default => defaultInstance;

	    static Settings()
		{
			defaultInstance = (Settings)Synchronized(new Settings());
		}

	    // ReSharper disable once EmptyConstructor
		public Settings()
		{
		}
	}
}
