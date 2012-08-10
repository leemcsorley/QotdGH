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
        private static TimeSpan? _offset = null;

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

        public static DateTime Now
        {
            get
            {
                if (_offset.HasValue) return DateTime.Now + _offset.Value;
                return DateTime.Now;
            }
            set
            {
                _offset = value - DateTime.Now;
            }
        }
    }
}
