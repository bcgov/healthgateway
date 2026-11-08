import { axe, toHaveNoViolations } from "jest-axe";

/**
 * Web only (jsdom): fail the test on any axe-core violation inside `container`. Rules that only make sense for a whole
 * page (`region`, `page-has-heading-one`) are off for a component render; pass `disabled` to turn off more.
 */
export async function expectNoAxeViolations(
    container: Element,
    disabled: string[] = []
): Promise<void> {
    const rules = Object.fromEntries(
        ["region", "page-has-heading-one", ...disabled].map((rule) => [
            rule,
            { enabled: false },
        ])
    );
    const results = await axe(container, { rules });
    const verdict = toHaveNoViolations.toHaveNoViolations(results);
    if (!verdict.pass) throw new Error(verdict.message());
}
