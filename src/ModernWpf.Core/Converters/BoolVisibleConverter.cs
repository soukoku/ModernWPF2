﻿using System;
using System.Windows;
using System.Windows.Data;

namespace ModernWpf.Converters
{
    /// <summary>
    /// Provides conversion of bool values to visibility.
    /// </summary>
    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class BoolVisibleConverter : IValueConverter
    {
        static readonly BoolVisibleConverter _instance = new BoolVisibleConverter();

        /// <summary>
        /// Gets the singleton instance for this converter.
        /// </summary>
        /// <value>
        /// The instance.
        /// </value>
        public static BoolVisibleConverter Instance { get { return _instance; } }
        
        #region IValueConverter Members

        /// <summary>
        /// Converts a value to the string representation.
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
            var visible = value as bool?;

            if (parameter != null && string.Equals("not", parameter.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                visible = !visible.GetValueOrDefault();
            }
            return visible.GetValueOrDefault() ? Visibility.Visible : Visibility.Collapsed;
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
    }
}
