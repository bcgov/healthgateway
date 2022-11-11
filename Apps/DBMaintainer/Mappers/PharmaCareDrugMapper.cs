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
namespace HealthGateway.DrugMaintainer
{
    using CsvHelper.Configuration;
    using HealthGateway.Database.Models;

    /// <summary>
    /// Performs a mapping from the read file to the model object.
    /// </summary>
    public class PharmaCareDrugMapper : ClassMap<PharmaCareDrug>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PharmaCareDrugMapper"/> class.
        /// Performs the mapping of read Pharmacare file to the db model.
        /// </summary>
        /// <param name="filedownload">The filedownload to map.</param>
        public PharmaCareDrugMapper(FileDownload filedownload)
        {
            this.Map(m => m.DinPin).Name("DIN/PIN");
            this.Map(m => m.Plan).Name("Plan");
            this.Map(m => m.EffectiveDate).Name("Rec Eff Date");
            this.Map(m => m.EndDate).Name("Rec End Date");
            this.Map(m => m.BenefitGroupList).Name("Ben Grp List");
            this.Map(m => m.LcaIndicator).Name("LCA Ind");
            this.Map(m => m.PayGenericIndicator).Name("Pay Gen Ind");
            this.Map(m => m.BrandName).Name("Brand Nm");
            this.Map(m => m.Manufacturer).Name("Manuf");
            this.Map(m => m.GenericName).Name("Generic Nm");
            this.Map(m => m.DosageForm).Name("Dosage Form");
            this.Map(m => m.TrialFlag).Name("Trial Flg");
            this.Map(m => m.MaximumPrice).Name("Max Price");
            this.Map(m => m.LcaPrice).Name("LCA Price");
            this.Map(m => m.RdpCategory).Name("RDP Cat");
            this.Map(m => m.RdpSubCategory).Name("RDP Sub Cat");
            this.Map(m => m.RdpPrice).Name("RDP Price");
            this.Map(m => m.RdpExcludedPlans).Name("RDP Excl Plans");
            this.Map(m => m.CfrCode).Name("Can Fed Reg Cd");
            this.Map(m => m.PharmaCarePlanDescription).Name("Pcare Plan Desc");
            this.Map(m => m.MaximumDaysSupply).Name("Max Days Supply");
            this.Map(m => m.QuantityLimit).Name("Qty Limit");
            this.Map(m => m.FormularyListDate).Name("Formulary List Date");
            this.Map(m => m.LimitedUseFlag).Name("Ltd Use Flag");

            // Map the Filedownload to each object
            this.Map(m => m.FileDownload).Convert(row => filedownload);
        }
    }
}
