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
        /// Performs the mapping of read Pharmacare file to the db model.
        /// </summary>
        public PharmaCareDrugMapper(FileDownload filedownload)
        {
            Map(m => m.DINPIN).Name("DIN/PIN");
            Map(m => m.Plan).Name("Plan");
            Map(m => m.EffectiveDate).Name("Rec Eff Date");
            Map(m => m.EndDate).Name("Rec End Date");
            Map(m => m.BenefitGroupList).Name("Ben Grp List");
            Map(m => m.LCAIndicator).Name("LCA Ind");
            Map(m => m.PayGenericIndicator).Name("Pay Gen Ind");
            Map(m => m.BrandName).Name("Brand Nm");
            Map(m => m.Manufacturer).Name("Manuf");
            Map(m => m.GenericName).Name("Generic Nm");
            Map(m => m.DosageForm).Name("Dosage Form");
            Map(m => m.TrialFlag).Name("Trial Flg");
            Map(m => m.MaximumPrice).Name("Max Price");
            Map(m => m.LCAPrice).Name("LCA Price");
            Map(m => m.RDPCategory).Name("RDP Cat");
            Map(m => m.RDPSubCategory).Name("RDP Sub Cat");
            Map(m => m.RDPPrice).Name("RDP Price");
            Map(m => m.RDPExcludedPlans).Name("RDP Excl Plans");
            Map(m => m.CFRCode).Name("Can Fed Reg Cd");
            Map(m => m.PharmaCarePlanDescription).Name("Pcare Plan Desc");
            Map(m => m.MaximumDaysSupply).Name("Max Days Supply");
            Map(m => m.QuantityLimit).Name("Qty Limit");
            Map(m => m.FormularyListDate).Name("Formulary List Date");
            Map(m => m.LimitedUseFlag).Name("Ltd Use Flag");
            // Additional fields in file without documentation
            // Map(m => m.).Name("Max Daily Qty");
            // Map(m => m.).Name("Max Period Qty");
            // Map(m => m.).Name("Max Period Qty Days");
            // Map(m => m.).Name("Max Annual Qty");
            // Map(m => m.).Name("BGTS Cat Cd");
            // Map(m => m.).Name("BGTS Cat Desc");
            // Map(m => m.).Name("BGTS Max Annual Qty");
            // Map the Filedownload to each object
            Map(m => m.FileDownload).ConvertUsing(row => filedownload);
        }
    }
}
