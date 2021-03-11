using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Text;

namespace SP.StudioCore.Mvc
{
    /// <summary>
    /// 过滤XSS注入的字符串接收
    /// </summary>
    public class FormStringAttribute : Attribute, IBindingSourceMetadata, IModelNameProvider
    {
        public BindingSource BindingSource => BindingSource.Form;

        public string Name { get; set; } 
    }
}
