export default abstract class PhoneUtil {
    public static formatPhone(phone: string) {
        phone = phone || "";
        return phone
            .replace(/[^0-9]/g, "")
            .replace(/(\d{3})(\d{3})(\d{4})/, "($1) $2-$3");
    }
}
