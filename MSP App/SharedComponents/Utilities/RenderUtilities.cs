using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace SharedComponents.Utilities
{
    public static class RenderUtilities
    {
        public static async Task<string> RenderViewToStringAsync(this Controller controller, string viewName, object model)
        {
            return await RenderViewToStringAsync(controller.HttpContext.RequestServices, controller.HttpContext, viewName, model, controller.ViewData, controller.TempData);
        }

        public static async Task<string> RenderViewToStringAsync(this ControllerBase controller, string viewName, object model)
        {
            var serviceProvider = controller.HttpContext.RequestServices;
            var httpContext = controller.HttpContext;
            var viewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary()) { Model = model };
            var tempData = serviceProvider.GetService<ITempDataProvider>();

            return await RenderViewToStringAsync(serviceProvider, httpContext, viewName, model, viewData, new TempDataDictionary(httpContext, tempData));
        }

        private static async Task<string> RenderViewToStringAsync(IServiceProvider serviceProvider, HttpContext httpContext, string viewName, object model, ViewDataDictionary viewData, ITempDataDictionary tempData)
        {
            var viewEngine = serviceProvider.GetService<ICompositeViewEngine>();
            var viewResult = viewEngine.FindView(new ActionContext(httpContext, new RouteData(), new ActionDescriptor()), viewName, false);

            if (viewResult.View == null)
            {
                throw new ArgumentNullException($"View {viewName} was not found.");
            }

            using (var writer = new StringWriter())
            {
                var viewContext = new ViewContext(
                    new ActionContext(httpContext, new RouteData(), new ActionDescriptor()),
                    viewResult.View,
                    viewData,
                    tempData,
                    writer,
                    new HtmlHelperOptions()
                );

                await viewResult.View.RenderAsync(viewContext);
                return writer.GetStringBuilder().ToString();
            }
        }
    }
}
