export const ReminderBody =
    "Find out about your {{program}} screening next steps. You will also get this letter in the mail. Learn more about {{program}} screening.";

export const ResultBody =
    "Download your {{program}} screening result letter. It may include important information about next steps. If you have questions, check the BC Cancer website or talk to your care provider.";

export function fillTemplate(template, values) {
    return template.replace(/{{(\w+)}}/g, (_, key) => values[key] ?? "");
}
