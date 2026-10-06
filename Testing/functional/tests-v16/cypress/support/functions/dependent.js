export function getCardSelector(hdid) {
    return `[data-testid=dependent-card-${hdid}]`;
}

export function getTabButtonSelector(hdid, tab) {
    return `#${tab}-tab-button-${hdid}`;
}
