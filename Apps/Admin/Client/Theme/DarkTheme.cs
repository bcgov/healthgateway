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
        private const string TranslucentWhite = "rgba(255,255,255, 0.12)";

        /// <summary>
        /// Initializes a new instance of the <see cref="DarkTheme"/> class.
        /// </summary>
        public DarkTheme()
        {
            this.Palette = new()
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
                Dark = Colors.Grey.Darken3,
                DarkContrastText = Colors.Shades.White,
                TextPrimary = "#FFFFFF",
                TextSecondary = "rgba(255,255,255, 0.50)",
                TextDisabled = "rgba(255,255,255, 0.2)",
                ActionDefault = "#91A3BF",
                ActionDisabled = "rgba(255,255,255, 0.26)",
                ActionDisabledBackground = TranslucentWhite,
                Background = "#001E30",
                BackgroundGrey = "#FDBB1B",
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
