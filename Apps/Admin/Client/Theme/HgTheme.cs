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

    /// <summary>
    /// Provides shared settings for the Health Gateway themes.
    /// </summary>
    public class HgTheme : MudTheme
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HgTheme"/> class.
        /// </summary>
        public HgTheme()
        {
            Typography typography = new();

            string[] fontFamily = { "BCSans", "Verdana", "Arial", "sans-serif" };

            typography.Default.FontFamily = fontFamily;
            typography.H1.FontFamily = fontFamily;
            typography.H2.FontFamily = fontFamily;
            typography.H3.FontFamily = fontFamily;
            typography.H4.FontFamily = fontFamily;
            typography.H5.FontFamily = fontFamily;
            typography.H6.FontFamily = fontFamily;
            typography.Subtitle1.FontFamily = fontFamily;
            typography.Subtitle2.FontFamily = fontFamily;
            typography.Body1.FontFamily = fontFamily;
            typography.Body2.FontFamily = fontFamily;
            typography.Button.FontFamily = fontFamily;
            typography.Caption.FontFamily = fontFamily;
            typography.Overline.FontFamily = fontFamily;

            // set font sizes to match the suggestions for Noto Sans from the Type Scale Generator
            // found at https://material.io/design/typography/the-type-system.html
            typography.H1.FontSize = "95px";
            typography.H2.FontSize = "59px";
            typography.Subtitle1.FontWeight = 700;

            this.Typography = typography;
        }
    }
}
