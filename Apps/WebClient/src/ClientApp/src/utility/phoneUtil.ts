export default abstract class PhoneUtil {
    public static formatPhone(phone: string): string {
        return PhoneUtil.stripPhoneMask(phone).replace(
            /(\d{3})(\d{3})(\d{4})/,
            "($1) $2-$3"
        );
    }

    public static stripPhoneMask(phoneNumber: string): string {
        phoneNumber = phoneNumber || "";
        return phoneNumber.replace(/\D/g, "");
    }
}
