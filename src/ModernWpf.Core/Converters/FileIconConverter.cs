﻿using ModernWpf.Native.Api;
using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace ModernWpf.Converters
{
    /// <summary>
    /// Parse databound value as file path and converts to file icon image.
    /// </summary>
    [ValueConversion(typeof(object), typeof(ImageSource))]
    public class FileIconConverter : IValueConverter
    {
        static readonly FileIconConverter _instance = new FileIconConverter();

        /// <summary>
        /// Gets the singleton instance for this converter.
        /// </summary>
        /// <value>
        /// The instance.
        /// </value>
        public static FileIconConverter Instance { get { return _instance; } }

        #region IValueConverter Members

        /// <summary>
        /// Converts a value.
        /// </summary>
        /// <param name="value">The value produced by the binding source.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>
        /// A converted value. If the method returns null, the valid null value is used.
        /// </returns>
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var para = parameter == null ? string.Empty : parameter.ToString();
            bool large = para.IndexOf("large", StringComparison.OrdinalIgnoreCase) > -1;

            return GetFileIconCore(value, large);
        }

        /// <summary>
        /// Not supported.
        /// </summary>
        /// <param name="value">The value that is produced by the binding target.</param>
        /// <param name="targetType">The type to convert to.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>
        /// A converted value. If the method returns null, the valid null value is used.
        /// </returns>
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }

        #endregion

        /// <summary>
        /// Real method to get the file icon after parsing the parameter.
        /// </summary>
        /// <param name="value">The databound value.</param>
        /// <param name="large">if set to <c>true</c> return large icon.</param>
        /// <returns></returns>
        protected virtual ImageSource GetFileIconCore(object value, bool large)
        {
            if (value == null) { return null; }
            return IconReader.GetFileIcon(value.ToString(), large ? IconReader.IconSize.Large : IconReader.IconSize.Small, false);
        }
    }
}
