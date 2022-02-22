export type Mask = string | string[];

const phnMaskTemplate: Mask = "#### ### ###";
const phoneNumberMaskTemplate: Mask = "(###) ###-####";
const postalCodeMaskTemplate: Mask = "A#A #A#";
const zipCodeMaskTemplate: Mask = ["#####", "#####-####"];

export {
    phnMaskTemplate,
    phoneNumberMaskTemplate,
    postalCodeMaskTemplate,
    zipCodeMaskTemplate,
};
