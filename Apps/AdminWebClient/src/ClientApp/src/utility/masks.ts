export type MaskCharacter = string | RegExp;
export type Mask = string | ((value: string) => MaskCharacter[]);

export function zipCodeMask(value: string): MaskCharacter[] {
    const numbers = value.replace(/[^0-9-]/g, "");
    const zipFormat: MaskCharacter[] = [/\d/, /\d/, /\d/, /\d/, /\d/];
    const extendedFormat = zipFormat.concat("-", /\d/, /\d/, /\d/, /\d/);
    return numbers.length > 5 ? extendedFormat : zipFormat;
}

export function phoneNumberMaskTemplate(): MaskCharacter[] {
    return [
        "(",
        /\d/,
        /\d/,
        /\d/,
        ")",
        " ",
        /\d/,
        /\d/,
        /\d/,
        "-",
        /\d/,
        /\d/,
        /\d/,
        /\d/,
    ];
}
const phnMask = "#### ### ###";
const postalCodeMask = "A#A #A#";
export { phnMask, postalCodeMask };
