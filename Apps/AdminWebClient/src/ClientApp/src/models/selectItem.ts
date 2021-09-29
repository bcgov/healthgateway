// Interface for an item in a select component
export default interface SelectItem {
    text: string | number;
    value: string | number;
    disabled?: boolean;
    divider?: boolean;
    header?: string;
}
