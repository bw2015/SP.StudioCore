using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using SP.StudioCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP.StudioCore.Mvc
{
    /// <summary>
    /// 从实体类
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
    public class FromModelAttribute : ModelBinderAttribute
    {
        public FromModelAttribute() : base()
        {
            this.BinderType = typeof(FromModelBinder);
        }
    }

    class FromModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            Type type = bindingContext.ModelType;
            bindingContext.Result = ModelBindingResult.Success(bindingContext.HttpContext.Request.Form.Fill(type));
            return Task.CompletedTask;
        }
    }
}
