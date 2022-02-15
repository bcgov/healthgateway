export type MaskCharacter = string | RegExp;
export type Mask = string | ((value: string) => MaskCharacter[]);

export function zipCodeMaskTemplate(value: string): MaskCharacter[] {
    const numbers = value.replace(/[^0-9-]/g, "");
    const zipFormat: MaskCharacter[] = [/\d/, /\d/, /\d/, /\d/, /\d/];
    const extendedFormat = zipFormat.concat("-", /\d/, /\d/, /\d/, /\d/);
    return numbers.length > 5 ? extendedFormat : zipFormat;
}

const phnMaskTemplate: Mask = "#### ### ###";
const phoneNumberMaskTemplate: Mask = "(###) ###-####";
const postalCodeMaskTemplate: Mask = "A#A #A#";

export { phnMaskTemplate, phoneNumberMaskTemplate, postalCodeMaskTemplate };
