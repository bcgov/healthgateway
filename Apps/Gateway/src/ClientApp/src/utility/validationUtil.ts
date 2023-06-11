const PHNsigDigits = [2, 4, 8, 5, 10, 9, 7, 3];

export default abstract class ValidationUtil {
    /**
     * Validates that a string conforms to the PHN format.
     * @param phn The string to validate.
     * @param ignoreWhitespace Whether to disregard whitespace in the string when validating (defaults to true).
     * @returns true if the string conforms to the PHN format.
     */
    public static validatePhn(phn: string, ignoreWhitespace = true): boolean {
        if (ignoreWhitespace) {
            phn = phn.replace(/\s/g, "");
        }

        if (!/^\d{10}$/.test(phn)) {
            return false;
        }

        let ok = false;
        if (phn[0] == "9") {
            let digit: number;
            let checksum = 0;
            for (let i = 1; i < 9; i++) {
                digit = parseInt(phn[i]);
                checksum += (digit * PHNsigDigits[i - 1]) % 11;
            }
            checksum = 11 - (checksum % 11);
            ok = parseInt(phn[9]) === checksum;
        }

        return ok;
    }
}
