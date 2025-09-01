using System;
using System.Reflection.Emit;
using System.Web.Configuration;
using Sdl.Web.Common.Logging;
using Sdl.Web.Common.Models;

namespace Sdl.Dxa.Modules.Crm
{
    public class TypeRegistrationHelper
    {
        static readonly string ECL_STUB_SCHEMA_PREFIX = "ExternalContentLibraryStubSchema";

        /// <summary>
        /// Build and register configured sub types of an ECL item. 
        /// </summary>
        /// <param name="configName"></param>
        /// <param name="baseEntityClass"></param>
        public static void BuildAndRegisterSubTypes(string configName, Type baseEntityClass)
        {
            var eclTypeNames = WebConfigurationManager.AppSettings[configName];
            if (eclTypeNames == null)
            {
                Log.Warn($"No type names defined for config '{configName}");
            }
            else
            {
                foreach (var eclTypeName in eclTypeNames.Split(new char[] {',', ' '}, StringSplitOptions.RemoveEmptyEntries))
                {
                    Log.Info("Defining image type: " + eclTypeName);
                    BuildAndRegisterSubType(eclTypeName.ToLower(), baseEntityClass);
                }
            }
        }

        /// <summary>
        /// Build and register sub type of an ECL item. Needs to be one per ECL stub schema.
        /// </summary>
        /// <param name="eclName"></param>
        public static void BuildAndRegisterSubType(string eclName, Type baseEntityClass)
        {
            var aName = baseEntityClass.Assembly.GetName(); 
            var ab = AppDomain.CurrentDomain.DefineDynamicAssembly(aName, AssemblyBuilderAccess.Run);
            var mb = ab.DefineDynamicModule(aName.Name);
            var tb = mb.DefineType(baseEntityClass.Name + "Proxy", System.Reflection.TypeAttributes.Public, baseEntityClass);

            var attrCtorParams = new Type[] { typeof(string), typeof(string) };
            var attrCtorInfo = typeof(SemanticEntityAttribute).GetConstructor(attrCtorParams);
            var attrBuilder = new CustomAttributeBuilder(attrCtorInfo, new object[] { ViewModel.CoreVocabulary, ECL_STUB_SCHEMA_PREFIX + eclName });
            tb.SetCustomAttribute(attrBuilder);

            ModelTypeRegistry.RegisterViewModel(new MvcData { ViewName = ECL_STUB_SCHEMA_PREFIX + eclName }, tb.CreateType());
        }
    }
}