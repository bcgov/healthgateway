export default interface UserNote {
  // Gets or sets the id.
  id: string;

  // Gets or sets the user hdid.
  hdId: string;

  // Gets or sets the title.
  title: string;

  // Gets or sets the text of the note.
  text: string;

  // Gets or sets the Note timeline datetime.
  journalDateTime: Date;

  // Gets or sets the note db version.
  version: number;
}
