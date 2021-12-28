using Google.Protobuf;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using SP.StudioCore.Http;
using SP.StudioCore.Protobufs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP.StudioCore.Mvc
{
    /// <summary>
    /// Protobuf 协议的序列化和反序列化
    /// </summary>
    public class FromProtobufAttribute : ModelBinderAttribute
    {
        public FromProtobufAttribute() : base()
        {
            this.BinderType = typeof(FromProtobufBinder);
        }
    }

    class FromProtobufBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            byte[]? data = bindingContext.HttpContext.GetData();
            if(data == null)
            {
                return Task.CompletedTask;
            }
            Type type = bindingContext.ModelType;
            IMessage? message = data.ToObject(type);
            if (message == null) return Task.CompletedTask;
            bindingContext.Result = ModelBindingResult.Success(message);
            return Task.CompletedTask;
        }
    }
}
