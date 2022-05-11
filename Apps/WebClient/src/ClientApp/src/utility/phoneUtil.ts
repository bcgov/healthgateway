export default abstract class PhoneUtil {
    public static formatPhone(phone: string): string {
        phone = phone || "";
        return phone
            .replace(/\D/g, "")
            .replace(/(\d{3})(\d{3})(\d{4})/, "($1) $2-$3");
    }
}
