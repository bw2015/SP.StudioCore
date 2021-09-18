using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace SP.StudioCore.Mvc
{
    /// <summary>
    /// 过滤XSS注入的字符串接收
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
    public sealed class FromStringAttribute : ModelBinderAttribute
    {
        public FromStringAttribute() : base()
        {
            this.BinderType = typeof(FromStringBinder);
        }
    }

    class FromStringBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            string data = bindingContext.HttpContext.Request.Form[bindingContext.FieldName];
            bindingContext.Result = ModelBindingResult.Success(HttpUtility.HtmlEncode(data));
            return Task.CompletedTask;
        }
    }
}
