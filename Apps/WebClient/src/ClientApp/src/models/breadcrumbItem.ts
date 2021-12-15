export default interface BreadcrumbItem {
    text: string;
    to?: string;
    href?: string;
    active?: boolean;
    dataTestId?: string;
}
