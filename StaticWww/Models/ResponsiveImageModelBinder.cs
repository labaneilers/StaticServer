using System;
using System.Collections.Specialized;
using System.Drawing;
using System.Globalization;
using System.Web.Mvc;
using StaticWww.Helpers;

namespace StaticWww.Models
{
    public class ResponsiveImageModelBinder : IModelBinder
    {
        private const int MAX_IMAGE_SIZE = 4096;

        public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            var model = new ResponsiveImageModel();

            NameValueCollection queryString = controllerContext.HttpContext.Request.QueryString;

            model.Src = queryString.Get<string>("s", null);
            if (string.IsNullOrWhiteSpace(model.Src))
            {
                throw new Exception("s parameter is required");
            }

            model.Width = queryString.Get("w", 0);
            if (model.Width <= 0)
            {
                throw new Exception("w parameter is required. This is likely because the image doesn't exist: " + model.Src);
            }
            if (model.Width > MAX_IMAGE_SIZE)
            {
                throw new Exception("w parameter is " + model.Width + ", which larger than the maximum of " + MAX_IMAGE_SIZE + ". " + model.Src);
            }

            model.Height = queryString.Get("h", 0);
            if (model.Height <= 0)
            {
                throw new Exception("h parameter is required. This is likely because the image doesn't exist: " + model.Src);
            }

            if (model.Height > MAX_IMAGE_SIZE)
            {
                throw new Exception("h parameter is " + model.Width + ", which larger than the maximum of " + MAX_IMAGE_SIZE + ". " + model.Src);
            }

            model.JpegQuality = queryString.Get("q", 0);
            if (model.JpegQuality < 0 || model.JpegQuality > 100)
            {
                throw new Exception("c parameter should be in range 1-100");
            }

            model.PngColors = queryString.Get("c", -1);
            if (model.PngColors < 0 || model.PngColors > 255)
            {
                throw new Exception("c parameter should be in range 2-255");
            }

            return model;
        }
    }
}