using System;
using System.IO;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;

namespace FlowMediaWebMVC.Helpers
{
    public static class UrlImageExtensions
    {
        /// <summary>
        /// Normalizes an image path or URL. If the input is empty, invalid or the file does not exist on disk,
        /// returns Url.Content("~/Imagenes/Nada.png"). Supports absolute http(s) URLs and virtual paths.
        /// </summary>
        public static string NormalizeImagePath(this UrlHelper url, string raw)
        {
            var defaultImage = "~/Imagenes/Nada.png";

            if (string.IsNullOrWhiteSpace(raw))
            {
                return url.Content(defaultImage);
            }

            var trimmed = raw.Trim();

            // absolute http(s) url -> return as-is
            if (trimmed.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                trimmed.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                return trimmed;
            }

            string virtualPath;

            if (trimmed.StartsWith("~"))
            {
                virtualPath = trimmed;
            }
            else if (trimmed.StartsWith("/"))
            {
                virtualPath = "~" + trimmed;
            }
            else if (trimmed.Contains("/"))
            {
                virtualPath = "~/" + trimmed;
            }
            else if (!trimmed.Contains("."))
            {
                return url.Content(defaultImage);
            }
            else
            {
                virtualPath = "~/Imagenes/" + trimmed;
            }

            try
            {
                // Only attempt to map and check files for virtual paths starting with ~
                if (!virtualPath.StartsWith("~"))
                {
                    return url.Content(defaultImage);
                }

                var physical = HostingEnvironment.MapPath(virtualPath);
                if (!string.IsNullOrEmpty(physical) && File.Exists(physical))
                {
                    return url.Content(virtualPath);
                }

                // If the file doesn't exist, return default image
                return url.Content(defaultImage);
            }
            catch
            {
                return url.Content(defaultImage);
            }
        }
    }
}
