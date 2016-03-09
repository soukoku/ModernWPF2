﻿using System;
using System.Windows;
using System.Windows.Data;

namespace ModernWpf.Converters
{
    /// <summary>
    /// Convert <see cref="Thickness"/> in a property to single double value for those pesky shape bindings.
    /// </summary>
    [ValueConversion(typeof(Thickness), typeof(double))]
    class ThicknessToDoubleConverter : IValueConverter
    {
        static readonly ThicknessToDoubleConverter _instance = new ThicknessToDoubleConverter();

        /// <summary>
        /// Gets the singleton instance for this converter.
        /// </summary>
        /// <value>
        /// The instance.
        /// </value>
        public static ThicknessToDoubleConverter Instance { get { return _instance; } }

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
            if (value is Thickness)
            {
                var t = ((Thickness)value);

                string para = parameter == null ? string.Empty : parameter.ToString().ToUpperInvariant();
                switch (para)
                {
                    case "LEFT":
                        return t.Left;
                    case "TOP":
                        return t.Top;
                    case "RIGHT":
                        return t.Right;
                    case "BOTTOM":
                        return t.Bottom;
                    default:
                        // default is avg
                        return (t.Left + t.Right + t.Bottom + t.Top) / 4;
                }
            }
            return 0;
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
        /// <exception cref="System.NotSupportedException"></exception>
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }

        #endregion
    }
}
