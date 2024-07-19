using System.Diagnostics;
using System.Reflection;
using API.Attributes.HasPermission;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using SharedComponents.Entities;

namespace SharedComponents.Utilities
{
    public class ControllerUtilities
    {
        public static ICollection<(string HttpMethod, string Route, string Permission, PermissionType Type)> GetAllRoutes(params Type[] excludedControllers)
        {
            var routeList = new List<(string HttpMethod, string Route, string Permission, PermissionType Type)>();
            var assemblyContainingControllers = Assembly.GetCallingAssembly();

            var controllerTypes = assemblyContainingControllers.GetTypes()
                .Where(type => typeof(ControllerBase).IsAssignableFrom(type) && !type.IsAbstract && !excludedControllers.Contains(type));

            foreach (var controllerType in controllerTypes)
            {
                var baseRoute = GetBaseRoute(controllerType);

                var methods = controllerType.GetMethods(BindingFlags.Instance | BindingFlags.Public)
                    .Where(m => !m.IsDefined(typeof(NonActionAttribute)) && m.DeclaringType == controllerType);

                foreach (var method in methods)
                {
                    var httpMethodRoutes = GetHttpMethodRoutes(controllerType, method, baseRoute);
                    routeList.AddRange(httpMethodRoutes);
                }
            }
            return routeList.Distinct().ToList();
        }

        public static (string HttpMethod, string Route, string Permission, PermissionType Type) GetRouteFromCallingMethod()
        {
            var callingMethod = GetCallingMethod() as MethodInfo;

            var controllerType = callingMethod.DeclaringType;
            var baseRoute = GetBaseRoute(controllerType);

            var httpMethodAttributes = callingMethod.GetCustomAttributes()
                .Where(attr => attr is HttpMethodAttribute)
                .Cast<HttpMethodAttribute>();

            foreach (var attr in httpMethodAttributes)
            {
                var methodRoute = attr.Template ?? string.Empty;
                methodRoute = methodRoute.Replace("[controller]", controllerType.Name.Replace("Controller", string.Empty));
                var fullRoute = CombineRoutes(baseRoute, methodRoute);
                var httpMethod = attr.HttpMethods.FirstOrDefault() ?? "???";
                var (permission, type) = GetPermissionAttribute(callingMethod);
                if (!string.IsNullOrEmpty(permission))
                {
                    return (httpMethod, fullRoute, permission, type);
                }
            }
            throw new InvalidOperationException("No HTTP method attributes found on the calling method.");
        }

        private static string GetBaseRoute(Type controllerType)
        {
            var controllerName = controllerType.Name.Replace("Controller", string.Empty);
            var baseRoute = controllerType.GetCustomAttribute<RouteAttribute>()?.Template ?? string.Empty;
            return baseRoute.Replace("[controller]", controllerName);
        }

        private static IEnumerable<(string HttpMethod, string Route, string Permission, PermissionType Type)> GetHttpMethodRoutes(Type controllerType, MethodInfo method, string baseRoute)
        {
            var routeList = new List<(string HttpMethod, string Route, string Permission, PermissionType Type)>();
            var controllerName = controllerType.Name.Replace("Controller", string.Empty);

            var httpMethodAttributes = method.GetCustomAttributes()
                .Where(attr => attr is HttpMethodAttribute)
                .Cast<HttpMethodAttribute>();

            foreach (var attr in httpMethodAttributes)
            {
                var methodRoute = attr.Template ?? string.Empty;
                methodRoute = methodRoute.Replace("[controller]", controllerName);
                var fullRoute = CombineRoutes(baseRoute, methodRoute);
                var httpMethod = attr.HttpMethods.FirstOrDefault() ?? "???";
                var (permission, type) = GetPermissionAttribute(method);
                if (!string.IsNullOrEmpty(permission))
                {
                    routeList.Add((httpMethod, fullRoute, permission, type));
                }
            }
            return routeList;
        }

        private static MethodBase GetCallingMethod()
        {
            var stackTrace = new StackTrace();
            MethodBase? callingMethod = null;

            // Traverse the stack trace to find the actual controller method
            for (int i = 1; i < stackTrace.FrameCount; i++)
            {
                var frame = stackTrace.GetFrame(i);
                var method = frame.GetMethod();
                if (method?.DeclaringType != null && typeof(ControllerBase).IsAssignableFrom(method.DeclaringType))
                {
                    callingMethod = method;
                    break;
                }
            }
            if (callingMethod == null)
            {
                throw new InvalidOperationException("Could not identify the calling method.");
            }
            return callingMethod;
        }

        private static string CombineRoutes(string baseRoute, string methodRoute)
        {
            if (string.IsNullOrWhiteSpace(baseRoute)) return methodRoute;
            if (string.IsNullOrWhiteSpace(methodRoute)) return baseRoute;
            return $"{baseRoute}/{methodRoute}".Trim('/');
        }

        private static (string Permission, PermissionType Type) GetPermissionAttribute(MethodInfo method)
        {
            var permissionAttribute = method.GetCustomAttributes(typeof(HasPermissionAttribute), false).FirstOrDefault() as HasPermissionAttribute;
            return permissionAttribute != null ? (permissionAttribute.Permission, permissionAttribute.Type) : (string.Empty, default(PermissionType));
        }
    }
}
