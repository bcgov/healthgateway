export default abstract class PhoneUtil {
    public static formatPhone(phone: string | null | undefined): string {
        return PhoneUtil.stripPhoneMask(phone).replace(
            /(\d{3})(\d{3})(\d{4})/,
            "($1) $2-$3"
        );
    }

    public static stripPhoneMask(
        phoneNumber: string | null | undefined
    ): string {
        phoneNumber = phoneNumber ?? "";
        return phoneNumber.replace(/\D/g, "");
    }
}
