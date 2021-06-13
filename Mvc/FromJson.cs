using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;
using SP.StudioCore.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SP.StudioCore.Mvc
{
    /// <summary>
    /// 接收到序列化的JSON字符串，直接反序列化成为对象
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
    public sealed class FromJsonAttribute : ModelBinderAttribute
    {
        public FromJsonAttribute() : base()
        {
            this.BinderType = typeof(FromJsonBinder);
        }
    }

    class FromJsonBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            string data = bindingContext.HttpContext.Request.Form[bindingContext.FieldName];
            if (string.IsNullOrEmpty(data)) return Task.CompletedTask;
            Type type = bindingContext.ModelType;
            // 判断是否包含 string 构造，如果存在则调用构造转换
            ConstructorInfo? constructor = type.GetConstructor(new[] { typeof(string) });
            if (constructor != null)
            {
                bindingContext.Result = ModelBindingResult.Success(constructor.Invoke(new[] { data }));
            }
            else
            {
                bindingContext.Result = ModelBindingResult.Success(JsonConvert.DeserializeObject(data, type));
            }
            return Task.CompletedTask;
        }
    }
}
