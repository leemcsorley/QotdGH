using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace Qotd.Utils
{
    public static class Config
    {
        private static string _uploadImagesUrl;

        public static string UploadImagesUrl
        {
            get
            {
                if (_uploadImagesUrl == null)
                {
                    _uploadImagesUrl = ConfigurationManager.AppSettings["UploadImagesUrl"];
                }
                return _uploadImagesUrl;
            }
        }
    }
}
