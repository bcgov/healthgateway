export function verifyProvincialDrug(drug) {
    expect(drug).to.not.be.null;
    expect(drug.din).to.equal("66999990");
    expect(drug.provincialData.pharmaCareDrug).to.not.be.null;
    expect(drug.provincialData.pharmaCareDrug.dinPin).to.equal("66999990");
}

export function verifyFedDrug(drug) {
    expect(drug).to.not.be.null;
    expect(drug.din).to.equal("02391724");
    expect(drug.federalData.drugProduct.drugIdentificationNumber).to.equal(
        "02391724"
    );
}
