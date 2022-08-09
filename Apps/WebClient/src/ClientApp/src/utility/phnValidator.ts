const PHNsigDigits = [2, 4, 8, 5, 10, 9, 7, 3];

export default abstract class PHNValidator {
    public static IsValid(PHN: string): boolean {
        if (!/^\d{10}$/.test(PHN)) {
            return false;
        } else {
            let ok = false;
            if (PHN[0] == "9") {
                let digit: number;
                let checksum = 0;
                for (let i = 1; i < 9; i++) {
                    digit = parseInt(PHN[i]);
                    checksum += (digit * PHNsigDigits[i - 1]) % 11;
                }
                checksum = 11 - (checksum % 11);
                ok = parseInt(PHN[9]) == checksum;
            }
            if (!ok) {
                return false;
            }
        }

        return true;
    }
}
