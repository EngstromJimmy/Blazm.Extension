using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazmExtension.Extensions
{
    
    internal static class TypeDefinitionExtensions
    {
       static string[] bunitIncludedTestDoubles = new[]
       {
            "Microsoft.AspNetCore.Components.WebAssembly.Hosting.IWebAssemblyHostEnvironment",
            "Microsoft.AspNetCore.Components.Routing.INavigationInterception",
            "Microsoft.AspNetCore.Components.NavigationManager",
            "Microsoft.JSInterop.IJSRuntime",
            "Microsoft.Extensions.Localization.IStringLocalizer",
            "Microsoft.AspNetCore.Authorization.IAuthorizationService"
        };

        //Get properties that has the attribute parameter of a type
        public static IEnumerable<PropertyDefinition> GetParameters(this TypeDefinition typeDefinition)
        {
            return typeDefinition.Properties.Where(x => x.CustomAttributes.Any(c => c.AttributeType.Name == "ParameterAttribute"));
        }
        public static IEnumerable<PropertyDefinition> GetCascadingParameters(this TypeDefinition typeDefinition)
        {
            return typeDefinition.Properties.Where(x => x.CustomAttributes.Any(c => c.AttributeType.Name == "CascadingParameterAttribute"));
        }


        public static string GetParameterDeclarations(this TypeDefinition typeDefinition)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var property in typeDefinition.GetParameters())
            {
                sb.AppendLine(GetPropertyDeclaration(property));
            }
            foreach (var property in typeDefinition.GetCascadingParameters())
            {
                sb.AppendLine(GetPropertyDeclaration(property));
            }
            return sb.ToString();
        }
    
        public static string GetPropertyDeclaration(PropertyDefinition property)
        {
            var friendlyName = GetFriendlyTypeName(property.PropertyType);
            string result = $"            {friendlyName} {FirstCharToLowerCase(property.Name)}";
            var declaration = "default!;";
            if (friendlyName == "RenderFragment")
            {
                declaration = "@<b>render me</b>;";
            }
            else if (friendlyName == "Action")
            {
                declaration = "() => { };";
            }
            else if (friendlyName.StartsWith("Action<"))
            {
                declaration = "_ => { };";
            }

            result += $" = {declaration}";
            return result;
        }

        public static string GetInjectDeclarations(this TypeDefinition typeDefinition)
        {
            StringBuilder sb = new StringBuilder();
            //Add injects
            foreach (var property in typeDefinition.Properties)
            {
                if (property.CustomAttributes.Any(c => c.AttributeType.Name == "InjectAttribute"))
                {
                    if (property.PropertyType.FullName == "System.Net.Http.HttpClient")
                    {
                        sb.AppendLine($"            Services.AddMockHttpClient(); //See <a href=\"https://bunit.dev/docs/test-doubles/mocking-httpclient.html\">this link</a> for more information.");
                    }
                    else if (bunitIncludedTestDoubles.Contains(property.PropertyType.FullName))
                    {
                        //Skip this bUnit has a built in mock for this
                    }
                    else if (property.PropertyType.FullName == "Microsoft.AspNetCore.Components.Authorization.AuthenticationStateProvider")
                    {
                        sb.AppendLine($"            var authContext = this.AddTestAuthorization();");
                        sb.AppendLine($"            //authContext.SetAuthorized(\"TEST USER\", AuthorizationState.Unauthorized);");
                    }
                    else
                    {
                        sb.AppendLine($"            //Services.AddSingleton<{property.PropertyType.Name},/*Add implementation for {property.PropertyType.Name}*/>();");
                    }
                }
            }
            return sb.ToString();
        }

        public static string GetFriendlyTypeName(TypeReference type)
        {
            if (type.IsGenericInstance)
            {
                var genericType = type as GenericInstanceType;
                if (genericType != null && genericType.ElementType.FullName == "System.Nullable`1")
                {
                    return GetFriendlyTypeName(genericType.GenericArguments[0]) + "?";
                }
            }

            switch (type.FullName)
            {
                case "System.Int32": return "int";
                case "System.String": return "string";
                case "System.Single": return "float";
                case "System.Double": return "double";
                case "System.Byte": return "byte";
                case "System.Boolean": return "bool";
                case "System.Int16": return "short";
                case "System.Int64": return "long";
                case "System.UInt16": return "ushort";
                case "System.UInt32": return "uint";
                case "System.UInt64": return "ulong";
                case "System.Char": return "char";
                case "System.Object": return "object";
                case "System.Void": return "void";
                case "System.DateOnly": return "DateOnly";
                case "System.TimeOnly": return "TimeOnly";
                case "System.DateTime": return "DateTime";
                case "System.DateTimeOffset": return "DateTimeOffset";
                case "System.Decimal": return "decimal";
                case "System.Guid": return "Guid";
                case "System.Uri": return "Uri";
                case "Microsoft.AspNetCore.Components.RenderFragment": return "RenderFragment";
                case "Microsoft.AspNetCore.Components.EventCallback":return "Action";
                
                // ... Add more as needed
                default:
                    //These could probably be handled better
                    if (type.FullName.StartsWith("Microsoft.AspNetCore.Components.EventCallback`1"))
                    {
                        return type.FullName.Replace("Microsoft.AspNetCore.Components.EventCallback`1","Action");
                    }
                    if (type.FullName.StartsWith("Microsoft.AspNetCore.Components.RenderFragment`1"))
                    {
                        return type.FullName.Replace("Microsoft.AspNetCore.Components.RenderFragment`1", "RenderFragment");
                    }
                    return type.FullName;
            }
        }


        public static string FirstCharToLowerCase(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            return char.ToLower(input[0]) + input.Substring(1);
        }

    }

}
