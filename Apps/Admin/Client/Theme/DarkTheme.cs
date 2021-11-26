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
    /// Provides settings for the Health Gateway Dark theme.
    /// </summary>
    public class DarkTheme : HgTheme
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DarkTheme"/> class.
        /// </summary>
        public DarkTheme()
        {
            this.Palette = new()
            {
                Black = "#27272f",
                White = Colors.Shades.White,
                Primary = "#91A3BF",
                PrimaryContrastText = Colors.Shades.Black,
                Secondary = "#FCBA19",
                SecondaryContrastText = Colors.Shades.Black,
                Tertiary = "#007BFF",
                TertiaryContrastText = Colors.Shades.White,
                Info = "#D1ECF1",
                InfoContrastText = Colors.Shades.Black,
                Success = "#82B68C",
                SuccessContrastText = Colors.Shades.Black,
                Warning = "#FCC966",
                WarningContrastText = Colors.Shades.Black,
                Error = "#E7828B",
                ErrorContrastText = Colors.Shades.Black,
                Dark = Colors.Grey.Darken3,
                DarkContrastText = Colors.Shades.White,
                TextPrimary = "rgba(255,255,255, 0.70)",
                TextSecondary = "rgba(255,255,255, 0.50)",
                TextDisabled = "rgba(255,255,255, 0.2)",
                ActionDefault = "#91A3BF",
                ActionDisabled = "rgba(255,255,255, 0.26)",
                ActionDisabledBackground = "rgba(255,255,255, 0.12)",
                Background = "#111519",
                BackgroundGrey = "#111519",
                Surface = "#111519",
                DrawerBackground = "#0E1923",
                DrawerText = "rgba(255,255,255, 0.70)",
                DrawerIcon = "rgba(255,255,255, 0.70)",
                AppbarBackground = "#0E1923",
                AppbarText = "rgb(255,255,255)",
                LinesDefault = "rgba(255,255,255, 0.12)",
                LinesInputs = "rgba(255,255,255, 0.3)",
                TableLines = "rgba(255,255,255, 0.12)",
                TableStriped = new MudColor(Colors.Shades.Black).SetAlpha(0.02).ToString(MudColorOutputFormats.RGBA),
                TableHover = new MudColor(Colors.Shades.Black).SetAlpha(0.04).ToString(MudColorOutputFormats.RGBA),
                Divider = "rgba(255,255,255, 0.12)",
                DividerLight = "rgba(255,255,255, 0.06)",
                HoverOpacity = .2,
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
