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
    /// Provides settings for the Health Gateway Light theme.
    /// </summary>
    public class LightTheme : HgTheme
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LightTheme"/> class.
        /// </summary>
        public LightTheme()
        {
            this.Palette = new()
            {
                Black = Colors.Shades.Black,
                White = Colors.Shades.White,
                Primary = "#245FA6",
                PrimaryContrastText = "#FFFFFF",
                Secondary = "#7B5800",
                SecondaryContrastText = "#FFFFFF",
                Tertiary = "#526070",
                TertiaryContrastText = "#FFFFFF",
                Info = "#17A2B8",
                InfoContrastText = Colors.Shades.White,
                Success = "#2E8540",
                SuccessContrastText = Colors.Shades.White,
                Warning = "#FAA500",
                WarningContrastText = "#412D00",
                Error = "#BA1A1A",
                ErrorContrastText = "#FFFFFF",
                Dark = Colors.Grey.Darken3,
                DarkContrastText = Colors.Shades.White,
                TextPrimary = "#0F1D2A",
                TextSecondary = new MudColor(Colors.Shades.Black).SetAlpha(0.54).ToString(MudColorOutputFormats.RGBA),
                TextDisabled = new MudColor(Colors.Shades.Black).SetAlpha(0.38).ToString(MudColorOutputFormats.RGBA),
                ActionDefault = "#003366",
                ActionDisabled = new MudColor(Colors.Shades.Black).SetAlpha(0.26).ToString(MudColorOutputFormats.RGBA),
                ActionDisabledBackground = new MudColor(Colors.Shades.Black).SetAlpha(0.12).ToString(MudColorOutputFormats.RGBA),
                Background = "#FCFCFF",
                BackgroundGrey = Colors.Grey.Lighten2,
                Surface = "#E4EBF5",
                DrawerBackground = "#EBEFF8",
                DrawerText = "#001E30",
                DrawerIcon = "#001E30",
                AppbarBackground = "#EBEFF8",
                AppbarText = "#001E30",
                LinesDefault = new MudColor(Colors.Shades.Black).SetAlpha(0.12).ToString(MudColorOutputFormats.RGBA),
                LinesInputs = Colors.Grey.Lighten1,
                TableLines = new MudColor(Colors.Grey.Lighten2).SetAlpha(1.0).ToString(MudColorOutputFormats.RGBA),
                TableStriped = new MudColor(Colors.Shades.Black).SetAlpha(0.02).ToString(MudColorOutputFormats.RGBA),
                TableHover = new MudColor(Colors.Shades.Black).SetAlpha(0.04).ToString(MudColorOutputFormats.RGBA),
                Divider = Colors.Grey.Lighten2,
                DividerLight = new MudColor(Colors.Shades.Black).SetAlpha(0.8).ToString(MudColorOutputFormats.RGBA),
                HoverOpacity = 0.1,
                GrayDefault = Colors.Grey.Default,
                GrayLight = Colors.Grey.Lighten1,
                GrayLighter = Colors.Grey.Lighten2,
                GrayDark = Colors.Grey.Darken1,
                GrayDarker = Colors.Grey.Darken2,
                OverlayDark = new MudColor("#212121").SetAlpha(0.5).ToString(MudColorOutputFormats.RGBA),
                OverlayLight = new MudColor(Colors.Shades.White).SetAlpha(0.5).ToString(MudColorOutputFormats.RGBA),
            };
        }
    }
}
