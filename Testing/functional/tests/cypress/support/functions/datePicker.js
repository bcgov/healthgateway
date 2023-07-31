/**
 * Return date in format YYYY-MMM-DD
 */
export function formatToDatePickerInput(incomingDate) {
    // array of months in uppercase format MMM
    const months = [
        "JAN",
        "FEB",
        "MAR",
        "APR",
        "MAY",
        "JUN",
        "JUL",
        "AUG",
        "SEPT",
        "NOV",
        "DEC",
    ];
    const date = new Date(incomingDate);
    const year = date.getFullYear();
    const month = date.getMonth() + 1;
    const day = date.getDate();
    return `${year}-${months[month - 1]}-${day}`;
}
