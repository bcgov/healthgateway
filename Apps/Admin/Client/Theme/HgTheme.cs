//-------------------------------------------------------------------------
// Copyright © 2019 Province of British Columbia
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//-------------------------------------------------------------------------
namespace HealthGateway.Admin.Client.Theme
{
    using MudBlazor;
    using MudBlazor.Utilities;

    /// <summary>
    /// Provides shared settings for the Health Gateway themes.
    /// </summary>
    public class HgTheme : MudTheme
    {
        private const string CyanBlue = "#001E30";
        private const string TranslucentWhite = "rgba(255,255,255, 0.12)";
        private const string White = "#FFFFFF";

        /// <summary>
        /// Initializes a new instance of the <see cref="HgTheme"/> class.
        /// </summary>
        public HgTheme()
        {
            this.AssignTypography();
            this.AssignPaletteLight();
            this.AssignPaletteDark();
        }

        private void AssignTypography()
        {
            string[] fontFamily = ["BCSans", "Verdana", "Arial", "sans-serif"];

            this.Typography = new()
            {
                Default = { FontFamily = fontFamily },
                H1 =
                {
                    FontFamily = fontFamily,
                    FontSize = "95px",
                },
                H2 =
                {
                    FontFamily = fontFamily,
                    FontSize = "59px",
                },
                H3 = { FontFamily = fontFamily },
                H4 = { FontFamily = fontFamily },
                H5 = { FontFamily = fontFamily },
                H6 = { FontFamily = fontFamily },
                Subtitle1 =
                {
                    FontFamily = fontFamily,
                    FontWeight = "700",
                },
                Subtitle2 = { FontFamily = fontFamily },
                Body1 = { FontFamily = fontFamily },
                Body2 = { FontFamily = fontFamily },
                Button = { FontFamily = fontFamily },
                Caption = { FontFamily = fontFamily },
                Overline = { FontFamily = fontFamily },
            };
        }

        private void AssignPaletteLight()
        {
            this.PaletteLight = new()
            {
                Black = Colors.Shades.Black,
                White = Colors.Shades.White,
                Primary = "#245FA6",
                PrimaryContrastText = White,
                Secondary = "#7B5800",
                SecondaryContrastText = White,
                Tertiary = "#526070",
                TertiaryContrastText = White,
                Info = "#17A2B8",
                InfoContrastText = Colors.Shades.White,
                Success = "#2E8540",
                SuccessContrastText = Colors.Shades.White,
                Warning = "#FAA500",
                WarningContrastText = "#412D00",
                Error = "#BA1A1A",
                ErrorContrastText = White,
                Dark = Colors.Gray.Darken3,
                DarkContrastText = Colors.Shades.White,
                TextPrimary = "#0F1D2A",
                TextSecondary = new MudColor(Colors.Shades.Black).SetAlpha(0.54).ToString(MudColorOutputFormats.RGBA),
                TextDisabled = new MudColor(Colors.Shades.Black).SetAlpha(0.38).ToString(MudColorOutputFormats.RGBA),
                ActionDefault = "#003366",
                ActionDisabled = new MudColor(Colors.Shades.Black).SetAlpha(0.26).ToString(MudColorOutputFormats.RGBA),
                ActionDisabledBackground = new MudColor(Colors.Shades.Black).SetAlpha(0.12).ToString(MudColorOutputFormats.RGBA),
                Background = "#FCFCFF",
                BackgroundGray = Colors.Gray.Lighten2,
                Surface = "#E4EBF5",
                DrawerBackground = "#EBEFF8",
                DrawerText = CyanBlue,
                DrawerIcon = CyanBlue,
                AppbarBackground = "#EBEFF8",
                AppbarText = CyanBlue,
                LinesDefault = new MudColor(Colors.Shades.Black).SetAlpha(0.12).ToString(MudColorOutputFormats.RGBA),
                LinesInputs = Colors.Gray.Lighten1,
                TableLines = new MudColor(Colors.Gray.Lighten2).SetAlpha(1.0).ToString(MudColorOutputFormats.RGBA),
                TableStriped = new MudColor(Colors.Shades.Black).SetAlpha(0.02).ToString(MudColorOutputFormats.RGBA),
                TableHover = new MudColor(Colors.Shades.Black).SetAlpha(0.04).ToString(MudColorOutputFormats.RGBA),
                Divider = Colors.Gray.Lighten2,
                DividerLight = new MudColor(Colors.Shades.Black).SetAlpha(0.8).ToString(MudColorOutputFormats.RGBA),
                HoverOpacity = 0.1,
                GrayDefault = Colors.Gray.Default,
                GrayLight = Colors.Gray.Lighten1,
                GrayLighter = Colors.Gray.Lighten2,
                GrayDark = Colors.Gray.Darken1,
                GrayDarker = Colors.Gray.Darken2,
                OverlayDark = new MudColor("#212121").SetAlpha(0.5).ToString(MudColorOutputFormats.RGBA),
                OverlayLight = new MudColor(Colors.Shades.White).SetAlpha(0.5).ToString(MudColorOutputFormats.RGBA),
            };
        }

        private void AssignPaletteDark()
        {
            this.PaletteDark = new()
            {
                Black = "#27272f",
                White = Colors.Shades.White,
                Primary = "#A7C8FF ",
                PrimaryContrastText = "#003061",
                Secondary = "#FDBB1B",
                SecondaryContrastText = "#412D00",
                Tertiary = "#C4C6CF",
                TertiaryContrastText = "#243140",
                Info = "#D1ECF1",
                InfoContrastText = Colors.Shades.Black,
                Success = "#82DA8A",
                SuccessContrastText = "#003912",
                Warning = "#FCC966",
                WarningContrastText = "#261900",
                Error = "#FFB3B1",
                ErrorContrastText = "#680011",
                Dark = Colors.Gray.Darken3,
                DarkContrastText = Colors.Shades.White,
                TextPrimary = "#FFFFFF",
                TextSecondary = "rgba(255,255,255, 0.50)",
                TextDisabled = "rgba(255,255,255, 0.2)",
                ActionDefault = "#91A3BF",
                ActionDisabled = "rgba(255,255,255, 0.26)",
                ActionDisabledBackground = TranslucentWhite,
                Background = CyanBlue,
                BackgroundGray = "#FDBB1B",
                Surface = "#123147",
                DrawerBackground = "#0D2C41",
                DrawerText = "#CAE6FF",
                DrawerIcon = "#CAE6FF",
                AppbarBackground = "#0D2C41",
                AppbarText = "#CAE6FF",
                LinesDefault = TranslucentWhite,
                LinesInputs = "rgba(255,255,255, 0.3)",
                TableLines = TranslucentWhite,
                TableStriped = new MudColor(Colors.Shades.Black).SetAlpha(0.02).ToString(MudColorOutputFormats.RGBA),
                TableHover = new MudColor(Colors.Shades.Black).SetAlpha(0.04).ToString(MudColorOutputFormats.RGBA),
                Divider = TranslucentWhite,
                DividerLight = "rgba(255,255,255, 0.06)",
                HoverOpacity = .35,
                GrayDefault = Colors.Gray.Default,
                GrayLight = Colors.Gray.Lighten1,
                GrayLighter = Colors.Gray.Lighten2,
                GrayDark = Colors.Gray.Darken1,
                GrayDarker = Colors.Gray.Darken2,
                OverlayDark = new MudColor("#212121").SetAlpha(0.5).ToString(MudColorOutputFormats.RGBA),
                OverlayLight = new MudColor(Colors.Shades.White).SetAlpha(0.5).ToString(MudColorOutputFormats.RGBA),
            };
        }
    }
}
