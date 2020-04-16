using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Chummer.Datastructures;
using Chummer.Backend.Attributes.OptionAttributes;
using Chummer.Classes;
using Chummer.UI.Options;

//using Mono.CodeGeneration;

namespace Chummer.Backend.Options
{
    public class OptionExtractor
    {
        private readonly List<Predicate<OptionItem>> _supported;

        public OptionExtractor(List<Predicate<OptionItem>> supported)
        {
            //Make copy. I don't event want to think about what happens if somebody changes it while running.
            _supported = new List<Predicate<OptionItem>>(supported);
        }

        public SimpleTree<OptionRenderItem> Extract(object target)
        {
            SimpleTree<OptionRenderItem> root = new SimpleTree<OptionRenderItem> {Tag = "root"};


            Dictionary<string, OptionEntryProxy> proxies = new Dictionary<string, OptionEntryProxy>();



            SimpleTree<OptionEntryProxy> parentTree;
            string[] npath;

            DictionaryList<string, PropertyInfo> propertiesDisplayPath = new DictionaryList<string, PropertyInfo>();
            Dictionary<PropertyInfo, OptionConstraint> constaints =
                new Dictionary<PropertyInfo, OptionConstraint>();
            OptionConstraint currentConstaint = null;
            string currentName = "";
            //Collect all properties in groups based on their option path
            foreach (PropertyInfo info in target.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (typeof(OptionConstraint).IsAssignableFrom(info.PropertyType))
                {
                    if(currentConstaint != null) throw new ArgumentException("Multiple constaints on one property detected.");
                    currentConstaint = (OptionConstraint) info.GetValue(target);
                }

                if(!info.GetMethod.IsPublic) continue;


                if (info.GetCustomAttribute<DisplayIgnoreAttribute>() != null) continue;
                if (info.GetCustomAttribute<OptionAttributes>() != null)
                {
                    currentName = info.GetCustomAttribute<OptionAttributes>().Path;
                }

                propertiesDisplayPath.Add(currentName, info);

                if (currentConstaint != null)
                {
                    constaints.Add(info, currentConstaint);
                    currentConstaint = null;
                }
            }


            var temp = propertiesDisplayPath
                .Where(x => !string.IsNullOrWhiteSpace(x.Key))
                .OrderBy(x => x.Key);

            foreach (KeyValuePair<string, List<PropertyInfo>> group in temp)
            {
                string[] path = group.Key.Split('/');
                SimpleTree<OptionRenderItem> parrent = root;
                string header;
                //find path in option tree, skip last as thats new
                //Breaks if trying to "jump" a path element
                for (int i = 0; i < path.Length - 1; i++)
                {

                    if(!LanguageManager.TryGetString(path[i], out header))
                        header = path[i];

                    //If you got an error and arrived here it is because you made an error with a class you are displaying
                    //OptionAttributesAttribute contains the path in the tree where options should be displayed
                    // and only the last one can be new.
                    parrent = parrent.Children.First(x => (string) x.Tag == header);
                }


                if(!LanguageManager.TryGetString(path.Last(), out header))
                    header = path.Last();

                SimpleTree<OptionRenderItem> newChild = new SimpleTree<OptionRenderItem> {Tag = header};


                foreach (PropertyInfo propertyInfo in group.Value)
                {
                    OptionEntryProxy entryProxy =  CreateOptionEntry(target, propertyInfo);

                    if(entryProxy == null)
                        continue;

                    if (!_supported.Any(x => x(entryProxy)))
                    {
                        Console.WriteLine($"No controlfactory for {entryProxy.TargetProperty.PropertyType}({entryProxy.TargetProperty})");
                        continue;
                    }

                    if (propertyInfo.GetCustomAttribute<HeaderAttribute>() != null)
                    {
                        newChild.Leafs.AddRange(propertyInfo.GetCustomAttributes<HeaderAttribute>().Select(x => new HeaderRenderDirective(x.Title)));
                    }
                    newChild.Leafs.Add(entryProxy);
                    proxies.Add(entryProxy.TargetProperty.Name, entryProxy);
                }
                parrent.Children.Add(newChild);
            }

            SetupConstraints(constaints, proxies);

            return root;
        }

        public List<OptionItem> BookOptions(CharacterOptions characterOptions, ProgramOptions globalOptions)
        {
            List<OptionDictionaryEntryProxy<string, bool>> options = characterOptions
                .Books.Keys
                .Select(bookKey => new OptionDictionaryEntryProxy<string, bool>("Books", characterOptions.Books, bookKey))
                .ToList();

            List<OptionItem> opt = new List<OptionItem>();

            foreach (OptionDictionaryEntryProxy<string,bool> bookProxy in options)
            {
	            if (GlobalOptions.Instance.SourcebookInfo.Any(x => x.Code == bookProxy.Key))
	            {
		            SourcebookInfo info = GlobalOptions.Instance.SourcebookInfo.First(x => x.Code == bookProxy.Key);
		            List<OptionItem> children = new List<OptionItem>
		            {
			            bookProxy
		            };
					
		            children.AddRange(typeof(SourcebookInfo).GetProperties().Select(x => new OptionEntryProxy(info, x)));
		            opt.Add(
			            new OptionGroup("", "Books", "BOOKALLSETTINGS", children)
				            {Tags = {info.Code, info.Name}});
	            }
            }

            return opt;
        }

        private void SetupConstraints(Dictionary<PropertyInfo, OptionConstraint> constaints, Dictionary<string, OptionEntryProxy> proxies)
        {
            foreach (KeyValuePair<PropertyInfo, OptionConstraint> constaint in constaints)
            {
                LambdaExpression ex = constaint.Value.Ex;
                PropertyExtractorExpressionVisitor visitor = new PropertyExtractorExpressionVisitor(constaint.Key.DeclaringType);
                LambdaExpression fix = visitor.Process(ex);
                OptionEntryProxy target = proxies[constaint.Key.Name];
                target.SetConstaint(
                    (Func<List<OptionEntryProxy>, bool>) fix.Compile(),
                    visitor.FoundProperties.Select(x => proxies[x]).ToList());

            }
        }

        private OptionEntryProxy CreateOptionEntry(object target, PropertyInfo arg)
        {
            try
            {
                if (!LanguageManager.TryGetString("Display_" + arg.Name, out var displayString))
                {
                    Console.WriteLine($"No translation found for {arg.DeclaringType?.Name}.{arg.Name}");
                    displayString = $"{{}} Missing String: {arg.Name}";
                    Utils.BreakIfDebug();
                }

                LanguageManager.TryGetString("Tooltip_" + arg.Name, out var toolTip);


                OptionEntryProxy option = new OptionEntryProxy(target, arg, displayString, toolTip:toolTip);

                OptionTagAttribute taga = arg.GetCustomAttribute<OptionTagAttribute>();
                if (taga != null)
                {
                    if(taga.Tags != null) option.Tags.AddRange(taga.Tags);
                    if(taga.TranslatedTags != null) option.Tags.AddRange(taga.TranslatedTags.Select(x => LanguageManager.GetString(x)));
                }
                return option;
            }
            catch (Exception ex)
            {
                Log.Error(new object[]{ex, arg});

                if (Debugger.IsAttached)
                    throw;
                else return null;
            }
        }

        private class PropertyExtractorExpressionVisitor : ExpressionVisitor
        {
            //This would give bugs if trying to access parameters from another class of same type.

            private readonly Type _delclaringType;

            public PropertyExtractorExpressionVisitor(Type delclaringType)
            {
                _delclaringType = delclaringType;
            }

            public List<string> FoundProperties { get; } = new List<string>();
            private ParameterExpression param;

            public LambdaExpression Process(LambdaExpression expression)
            {
                param = Expression.Parameter(typeof(List<OptionEntryProxy>), expression.Parameters[0].Name);

                var bod = Visit(expression.Body);

                return Expression.Lambda<Func<List<OptionEntryProxy>, bool>>(bod, param);
            }

            public override Expression Visit(Expression node)
            {
                if (node.NodeType == ExpressionType.MemberAccess)
                {
                    MemberExpression ex = (MemberExpression) node;
                    PropertyInfo info = ex.Member as PropertyInfo;
                    if (info != null)
                    {
                        if (info.DeclaringType == _delclaringType)
                        {
                            FoundProperties.Add(info.Name);
                            return Expression.Convert(
                                Expression.Property(
                                    Expression.Property(
                                        param, "Item",
                                        Expression.Constant(FoundProperties.Count - 1)),
                                    "Value"
                                ),
                                info.PropertyType
                            );

                        }

                    }

                }
                return base.Visit(node);
            }
        }
    }

    public class HeaderRenderDirective : OptionRenderItem
    {
        public string Title { get; }

        public HeaderRenderDirective(string argTitle)
        {
            Title = argTitle;
        }
    }
}
