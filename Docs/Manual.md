# KaNote manual

## Introduction 

KaNote is a minimalist note and task manager. Tasks or notes are stored in databases and can be organized in thematic folders. These notes can contain different resources (images, attachments, ...), link various alarms and be enriched with custom attributes. KaNote also integrates a minimalist language with which you can automate tasks. In addition to the task manager, you can give KaNote other uses, for example a small content manager.

Two different database engines are supported, Sqlite for personal tasks and SQL server for corporate use.

KaNote also has two different front-ends, an old-school Windows desktop application (WinForm app) and a Web application (Blazor app). 

TODO: ...


## User's Guide 

### Configuration files (desktop app)

The Windows desktop application keeps its configuration in the folder `%LocalAppData%\KNote`, in two XML files:

| File | What it holds | When it changes |
|---|---|---|
| `KNoteData.config` | What you configure: repositories, AI providers, e-mail (SMTP) account, alarm and auto-save options, chat hub and ServerCOM settings. | Only when you change a setting. |
| `KNoteState.config` | What KaNote remembers by itself: last active repository and folder, window positions and sizes, list layout, visible panels and the rows of the alarms panel. | As you use the application. |

Deleting `KNoteState.config` only resets the window layout. Deleting `KNoteData.config` makes KaNote start as if it were the first run: your databases are not deleted, but the list of repositories and AI providers is lost.

**Passwords and API keys.** The e-mail account password, the API keys of the AI providers and the password inside a repository connection string (SQL authentication) are stored encrypted with Windows data protection, tied to your Windows user and this computer. Connection strings without a password (SQLite, Windows authentication) stay readable. If you copy `KNoteData.config` to another computer or user, those secrets cannot be decrypted: KaNote tells you at startup which ones must be entered again, and everything else loads normally. A secret typed in plain text into the file by hand is accepted and gets encrypted the next time the file is saved.

**Backups.** Every time a file is saved, the previous version is kept next to it as `KNoteData.config.bak` / `KNoteState.config.bak`. If a file cannot be read, KaNote loads its `.bak` and tells you.

**Upgrading from a previous version.** Older versions kept everything in a single `KNoteData.config`. The first time this version starts, it converts that file automatically into the two files above, keeping all your values (passwords and API keys are kept, now encrypted). A copy of the old file, **without** passwords and API keys, is kept as `KNoteData.config.v1.bak`.

**Going back to an older version.** Older versions cannot read the new files and will fail at startup. To go back: close KaNote, keep a copy of the current `KNoteData.config` somewhere else, rename `KNoteData.config.v1.bak` to `KNoteData.config`, delete `KNoteState.config`, start the older version and enter your passwords and API keys again.

### Printing and exporting (desktop app)

The **File** menu of the management window has these options, all working on the notes list exactly as you see it: the notes of the selected folder, or the result of a search or a filter, in the order and with the columns shown on screen (including the compact view and the list filter).

| Option | Result |
|---|---|
| **Print notes list ...** | The list as a table, in landscape, headed by the folder path or a summary of the search/filter. All the visible columns fit the page width. The **Print** button of the toolbar does the same. |
| **Print notes list as book ...** | A book: a cover, a table of contents with page numbers and one chapter per note, in the order of the list. The chapter title is the note's topic and its text the note's description. |
| **Print selected note details ...** | Everything about the selected note, as saved: properties, description, attributes, resources (with a thumbnail of the images), tasks and trace notes. The note's script and its alarms are not printed. |
| **Export notes list to CSV ...** | The same content as the printed list, as a CSV file. |

The note editor has its own **Print** button, which prints the details of the note being edited as you are seeing it. If it has changes not saved yet, the report says so.

**Preview, printer and PDF.** Every print opens a preview window first. From it, **Print ...** opens the print dialog (choose the printer, or *Save as PDF*, which also has its own page preview) and **Save as PDF ...** saves the report directly as a PDF file. The book takes a moment longer to open: it is paginated once to know the page numbers of its table of contents.

**CSV files.** The proposed file name describes the folder, search or filter of the list. The file uses the list separator of your regional settings (`;` in Spanish, for instance) and UTF-8 encoding, so Excel opens it directly with a double click. A text that starts with `=`, `+`, `-` or `@` (e.g. a topic like `- Quick links`) is written with a leading apostrophe, so that spreadsheets show it as text instead of trying to run it as a formula.

The save dialogs (PDF and CSV) start in the folder where you last saved one of them.

TODO: ...