﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace ModernWpf
{
    /// <summary>
    /// Specifies an accent color and its derived variations.
    /// </summary>
    public sealed class Accent
    {
        #region static stuff


        #region known accent names

        /// <summary>
        /// Pre-defined name for the red accent.
        /// </summary>
        public const string Red = "Red";
        /// <summary>
        /// Pre-defined name for the orange accent.
        /// </summary>
        public const string Orange = "Orange";
        /// <summary>
        /// Pre-defined name for the green accent.
        /// </summary>
        public const string Green = "Green";
        /// <summary>
        /// Pre-defined name for the teal accent.
        /// </summary>
        public const string Teal = "Teal";
        /// <summary>
        /// Pre-defined name for the olive accent.
        /// </summary>
        public const string Olive = "Olive";
        /// <summary>
        /// Pre-defined name for the gold accent.
        /// </summary>
        public const string Gold = "Gold";

        /// <summary>
        /// Pre-defined name for the light blue accent.
        /// </summary>
        public const string LightBlue = "Light Blue";
        /// <summary>
        /// Pre-defined name for the dark blue accent.
        /// </summary>
        public const string DarkBlue = "Dark Blue";
        /// <summary>
        /// Pre-defined name for the light purple accent.
        /// </summary>
        public const string LightPurple = "Light Purple";
        /// <summary>
        /// Pre-defined name for the dark purple accent.
        /// </summary>
        public const string DarkPurple = "Dark Purple";

        #endregion


        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="Accent"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="color">The color.</param>
        public Accent(string name, Color color)
        {
            Name = name;
            Color = color;

            // instead of alpha modify in intensity

            var hsl = (HSLColor)color;
            var lumiStep = (hsl.Luminosity - 0.1) / 5;
            Debug.WriteLine("{0}\t{1} at {2:n2}", name, hsl, lumiStep);
            //var satStep = 0d;
            //if (hsl.Saturation > 0.3)
            //{
            //    satStep = (hsl.Saturation - 0.3) / 5;
            //}

            MainBrush = GetBrush(0xff, color);
            var origLumi = hsl.Luminosity;

            hsl.Luminosity -= lumiStep;
            //hsl.Saturation -= satStep;
            DarkBrush1 = GetBrush(0xff, hsl);

            hsl.Luminosity -= lumiStep;
            //hsl.Saturation -= satStep;
            DarkBrush2 = GetBrush(0xff, hsl);

            hsl.Luminosity -= lumiStep;
            //hsl.Saturation -= satStep;
            DarkBrush3 = GetBrush(0xff, hsl);

            hsl.Luminosity -= lumiStep;
            //hsl.Saturation -= satStep;
            DarkBrush4 = GetBrush(0xff, hsl);

            hsl.Luminosity = origLumi;

            hsl.Luminosity += lumiStep;
            //hsl.Saturation -= satStep;
            LightBrush1 = GetBrush(0xff, hsl);

            hsl.Luminosity += lumiStep;
            //hsl.Saturation -= satStep;
            LightBrush2 = GetBrush(0xff, hsl);

            hsl.Luminosity += lumiStep;
            //hsl.Saturation -= satStep;
            LightBrush3 = GetBrush(0xff, hsl);

            hsl.Luminosity += lumiStep;
            //hsl.Saturation -= satStep;
            LightBrush4 = GetBrush(0xff, hsl);

            // opacity scale
            AlphaBrush1 = GetBrush(0xe5, color);
            AlphaBrush2 = GetBrush(0xcc, color);
            AlphaBrush3 = GetBrush(0xb2, color);
            AlphaBrush4 = GetBrush(0x99, color);
            AlphaBrush5 = GetBrush(0x7f, color);
            AlphaBrush6 = GetBrush(0x66, color);
            AlphaBrush7 = GetBrush(0x4c, color);
            AlphaBrush8 = GetBrush(0x33, color);
            AlphaBrush9 = GetBrush(0x19, color);
        }

        static SolidColorBrush GetBrush(byte alpha, Color color)
        {
            var brush = new SolidColorBrush(Color.FromArgb(alpha, color.R, color.G, color.B));
            brush.Freeze();
            return brush;
        }

        /// <summary>
        /// Gets the accent name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the base color.
        /// </summary>
        /// <value>
        /// The color.
        /// </value>
        public Color Color { get; private set; }

        /// <summary>
        /// Gets the main accent brush.
        /// </summary>
        /// <value>
        /// The main brush.
        /// </value>
        public Brush MainBrush { get; private set; }

        /// <summary>
        /// Gets the brush darker than <see cref="MainBrush" />.
        /// </summary>
        /// <value>
        /// The dark brush.
        /// </value>
        public Brush DarkBrush1 { get; private set; }
        /// <summary>
        /// Gets the brush darker than <see cref="DarkBrush1" />.
        /// </summary>
        /// <value>
        /// The dark brush2.
        /// </value>
        public Brush DarkBrush2 { get; private set; }
        /// <summary>
        /// Gets the brush darker than <see cref="DarkBrush2" />.
        /// </summary>
        /// <value>
        /// The dark brush3.
        /// </value>
        public Brush DarkBrush3 { get; private set; }
        /// <summary>
        /// Gets the brush darker than <see cref="DarkBrush3" />.
        /// </summary>
        /// <value>
        /// The dark brush4.
        /// </value>
        public Brush DarkBrush4 { get; private set; }

        /// <summary>
        /// Gets the brush lighter than <see cref="MainBrush" />.
        /// </summary>
        /// <value>
        /// The light brush.
        /// </value>
        public Brush LightBrush1 { get; private set; }
        /// <summary>
        /// Gets the brush lighter than <see cref="LightBrush1" />.
        /// </summary>
        /// <value>
        /// The light brush2.
        /// </value>
        public Brush LightBrush2 { get; private set; }
        /// <summary>
        /// Gets the brush lighter than <see cref="LightBrush2" />.
        /// </summary>
        /// <value>
        /// The light brush3.
        /// </value>
        public Brush LightBrush3 { get; private set; }
        /// <summary>
        /// Gets the brush lighter than <see cref="LightBrush3" />.
        /// </summary>
        /// <value>
        /// The light brush4.
        /// </value>
        public Brush LightBrush4 { get; private set; }

        /// <summary>
        /// Gets the accent brush that's 10% transparent.
        /// </summary>
        /// <value>
        /// The alpha brush.
        /// </value>
        public Brush AlphaBrush1 { get; private set; }
        /// <summary>
        /// Gets the accent brush that's 20% transparent.
        /// </summary>
        /// <value>
        /// The alpha brush2.
        /// </value>
        public Brush AlphaBrush2 { get; private set; }
        /// <summary>
        /// Gets the accent brush that's 30% transparent.
        /// </summary>
        /// <value>
        /// The alpha brush3.
        /// </value>
        public Brush AlphaBrush3 { get; private set; }
        /// <summary>
        /// Gets the accent brush that's 40% transparent.
        /// </summary>
        /// <value>
        /// The alpha brush4.
        /// </value>
        public Brush AlphaBrush4 { get; private set; }
        /// <summary>
        /// Gets the accent brush that's 50% transparent.
        /// </summary>
        /// <value>
        /// The alpha brush5.
        /// </value>
        public Brush AlphaBrush5 { get; private set; }
        /// <summary>
        /// Gets the accent brush that's 60% transparent.
        /// </summary>
        /// <value>
        /// The alpha brush6.
        /// </value>
        public Brush AlphaBrush6 { get; private set; }
        /// <summary>
        /// Gets the accent brush that's 70% transparent.
        /// </summary>
        /// <value>
        /// The alpha brush7.
        /// </value>
        public Brush AlphaBrush7 { get; private set; }
        /// <summary>
        /// Gets the accent brush that's 80% transparent.
        /// </summary>
        /// <value>
        /// The alpha brush8.
        /// </value>
        public Brush AlphaBrush8 { get; private set; }
        /// <summary>
        /// Gets the accent brush that's 90% transparent.
        /// </summary>
        /// <value>
        /// The alpha brush9.
        /// </value>
        public Brush AlphaBrush9 { get; private set; }
    }
}
