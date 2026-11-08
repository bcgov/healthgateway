/**
 * Whose records are in the stores while a login is under way. An expired session keeps them on screen behind the login,
 * and the person logging in may choose a different account.
 */
let owner: string | undefined;

export const getRecordOwner = () => owner;
export const setRecordOwner = (hdid: string | undefined) => {
    owner = hdid;
};
